using System.Collections.Concurrent;
using System.Threading.Channels;
using LN_history.Data.Instrumentation;
using LN_History.Model;
using Microsoft.Extensions.Logging;

namespace LN_history.Data.Streaming;

/// <summary>
/// In-memory <see cref="IGossipStream"/>: a thread-safe subscriber registry over per-subscriber
/// bounded channels. Publish is non-blocking; a subscriber that fills its buffer drops the oldest
/// frame so one slow WebSocket can never stall the shared database listener or other subscribers.
/// Registered as a singleton.
/// </summary>
public sealed class GossipStream : IGossipStream
{
    /// <summary>Per-subscriber buffer depth before drop-oldest kicks in.</summary>
    private const int SubscriberQueueCapacity = 2048;

    private readonly ConcurrentDictionary<Guid, Subscriber> _subscribers = new();
    private readonly ILogger<GossipStream> _logger;
    private readonly StreamMetrics _metrics;

    public GossipStream(ILogger<GossipStream> logger, StreamMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public void Publish(GossipFrame frame)
    {
        foreach (var subscriber in _subscribers.Values)
            if (subscriber.Accepts(frame.Type))
                subscriber.Enqueue(frame);
    }

    public IGossipSubscription Subscribe(IReadOnlyCollection<int>? types)
    {
        var subscriber = new Subscriber(types, _logger, _metrics);

        // Wire the unsubscribe callback BEFORE publishing the subscriber to the registry. If it were
        // set afterwards, a Dispose racing in between would find OnDispose still null and leave the
        // subscriber in the dictionary forever — a permanent leak that keeps receiving every frame.
        subscriber.OnDispose = id =>
        {
            if (_subscribers.TryRemove(id, out _))
                _metrics.SubscriberRemoved();
        };

        _subscribers[subscriber.Id] = subscriber;
        _metrics.SubscriberAdded();

        // Covers the race above: if Dispose already ran, its TryRemove found nothing, so undo here.
        if (subscriber.IsDisposed && _subscribers.TryRemove(subscriber.Id, out _))
            _metrics.SubscriberRemoved();

        return subscriber;
    }

    private sealed class Subscriber : IGossipSubscription
    {
        private readonly Channel<GossipFrame> _channel;
        private readonly HashSet<int>? _types;
        private readonly ILogger _logger;
        private readonly StreamMetrics _metrics;
        private long _dropped;
        private int _disposed;

        public Guid Id { get; } = Guid.NewGuid();
        public Action<Guid>? OnDispose { get; set; }
        public long Dropped => Interlocked.Read(ref _dropped);
        public bool IsDisposed => Volatile.Read(ref _disposed) == 1;

        public Subscriber(IReadOnlyCollection<int>? types, ILogger logger, StreamMetrics metrics)
        {
            _types = types is { Count: > 0 } ? new HashSet<int>(types) : null;
            _logger = logger;
            _metrics = metrics;
            _channel = System.Threading.Channels.Channel.CreateBounded<GossipFrame>(new BoundedChannelOptions(SubscriberQueueCapacity)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });
        }

        public bool Accepts(int type) => _types is null || _types.Contains(type);

        public void Enqueue(GossipFrame frame)
        {
            // Drop-oldest: TryWrite always succeeds by evicting the oldest buffered frame when full.
            // Counting the eviction is best-effort — a full buffer means this consumer is lagging.
            if (_channel.Reader.Count >= SubscriberQueueCapacity)
            {
                var total = Interlocked.Increment(ref _dropped);
                _metrics.FramesDropped.Add(1);
                if (total == 1 || total % 1000 == 0)
                    _logger.LogWarning("Gossip stream subscriber {Id} is lagging; {Dropped} frames dropped.", Id, total);
            }
            _channel.Writer.TryWrite(frame);
        }

        public IAsyncEnumerable<GossipFrame> ReadAllAsync(CancellationToken cancellationToken) =>
            _channel.Reader.ReadAllAsync(cancellationToken);

        public void Dispose()
        {
            // Idempotent: the controller disposes via `using`, and Subscribe may also unwind a losing
            // registration race. Either way the subscriber count must only ever be decremented once.
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;
            OnDispose?.Invoke(Id);
            _channel.Writer.TryComplete();
        }
    }
}
