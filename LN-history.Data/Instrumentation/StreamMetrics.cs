using System.Diagnostics.Metrics;

namespace LN_history.Data.Instrumentation;

/// <summary>
/// Meter for the live-gossip path: Postgres LISTEN/NOTIFY → in-memory fan-out → WebSocket
/// subscribers. The WebSocket request is deliberately excluded from tracing (a span would stay open
/// for the whole connection), so these instruments are the only telemetry the stream produces.
/// Registered as a singleton and exported under <see cref="MeterName"/>.
/// </summary>
public sealed class StreamMetrics
{
    public const string MeterName = "LN_history.Stream";

    private int _subscribers;
    private int _listenerConnected;
    private int _fetchQueueDepth;

    public StreamMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        NotificationsReceived = meter.CreateCounter<long>(
            "ln_gossip_notifications_received_total",
            description: "NOTIFY payloads received on the dedicated LISTEN connection.");

        NotificationsDiscarded = meter.CreateCounter<long>(
            "ln_gossip_notifications_discarded_total",
            description: "NOTIFY payloads thrown away before fetch (malformed, or evicted because the fetch queue was full).");

        FramesPublished = meter.CreateCounter<long>(
            "ln_gossip_frames_published_total",
            description: "Gossip frames fanned out from the listener to subscribers.");

        FramesDropped = meter.CreateCounter<long>(
            "ln_gossip_frames_dropped_total",
            description: "Frames evicted from a subscriber's buffer because that subscriber could not keep up.");

        FetchFailures = meter.CreateCounter<long>(
            "ln_gossip_fetch_failures_total",
            description: "Failures fetching raw_gossip for a notified internal_id.");

        ListenerReconnects = meter.CreateCounter<long>(
            "ln_gossip_listener_reconnects_total",
            description: "Times the dedicated LISTEN connection dropped and had to be re-established.");

        // The single most useful stream signal: if this is 0, live gossip is silently dead even
        // though the API is otherwise healthy and answering requests.
        meter.CreateObservableGauge("ln_gossip_listener_connected",
            () => Volatile.Read(ref _listenerConnected),
            description: "1 when the dedicated Postgres LISTEN connection is established, 0 otherwise.");

        meter.CreateObservableGauge("ln_gossip_stream_subscribers",
            () => Volatile.Read(ref _subscribers),
            description: "Currently connected live-gossip WebSocket subscribers.");

        meter.CreateObservableGauge("ln_gossip_fetch_queue_depth",
            () => Volatile.Read(ref _fetchQueueDepth),
            description: "Notifications waiting to have their raw gossip fetched. Sustained growth means the fetch loop is behind the ingest rate.");
    }

    public Counter<long> NotificationsReceived { get; }
    public Counter<long> NotificationsDiscarded { get; }
    public Counter<long> FramesPublished { get; }
    public Counter<long> FramesDropped { get; }
    public Counter<long> FetchFailures { get; }
    public Counter<long> ListenerReconnects { get; }

    public void SubscriberAdded() => Interlocked.Increment(ref _subscribers);
    public void SubscriberRemoved() => Interlocked.Decrement(ref _subscribers);
    public void SetListenerConnected(bool connected) => Volatile.Write(ref _listenerConnected, connected ? 1 : 0);
    public void SetFetchQueueDepth(int depth) => Volatile.Write(ref _fetchQueueDepth, depth);
}
