using System.Text.Json;
using System.Threading.Channels;
using LN_history.Data.DataStores;
using LN_history.Data.Instrumentation;
using LN_History.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace LN_history.Data.Streaming;

/// <summary>
/// Owns a single dedicated PostgreSQL connection doing <c>LISTEN</c> on the gossip notification
/// channel. Each NOTIFY payload is <c>{"i":&lt;internal_id&gt;,"t":&lt;type&gt;}</c>, emitted on COMMIT
/// by the ingest trigger. On receipt it fetches the raw gossip bytes (on a separate pooled
/// connection) and publishes them to <see cref="IGossipStream"/>.
///
/// The LISTEN callback is decoupled from the fetch via an internal queue: the callback must not
/// block or run a query on the listening connection, so it only enqueues (internal_id, type) and a
/// pump loop does the async fetch + publish.
/// </summary>
public sealed class GossipNotificationListener : BackgroundService
{
    /// <summary>Postgres LISTEN/NOTIFY channel. Must match the ingest trigger's <c>pg_notify()</c>.</summary>
    public const string NotifyChannel = "lnhistory_gossip";

    private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(5);

    /// <summary>Capacity of the fetch queue; kept as a field so depth can be reported as a metric.</summary>
    private const int PendingCapacity = 16_384;

    private readonly NpgsqlDataSource _dataSource;
    private readonly IGossipStream _stream;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GossipNotificationListener> _logger;
    private readonly StreamMetrics _metrics;

    private readonly Channel<(long InternalId, int Type)> _pending =
        System.Threading.Channels.Channel.CreateBounded<(long, int)>(new BoundedChannelOptions(PendingCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

    public GossipNotificationListener(
        NpgsqlDataSource dataSource,
        IGossipStream stream,
        IServiceScopeFactory scopeFactory,
        ILogger<GossipNotificationListener> logger,
        StreamMetrics metrics)
    {
        _dataSource = dataSource;
        _stream = stream;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _metrics = metrics;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(ListenLoopAsync(stoppingToken), PumpLoopAsync(stoppingToken));
    }

    private async Task ListenLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync(stoppingToken);
                connection.Notification += OnNotification;
                try
                {
                    await using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = $"LISTEN {NotifyChannel}";
                        await cmd.ExecuteNonQueryAsync(stoppingToken);
                    }
                    _logger.LogInformation("Listening for gossip notifications on '{Channel}'.", NotifyChannel);
                    _metrics.SetListenerConnected(true);

                    // WaitAsync returns whenever a notification arrives (raising Notification); loop to keep waiting.
                    while (!stoppingToken.IsCancellationRequested)
                        await connection.WaitAsync(stoppingToken);
                }
                finally
                {
                    connection.Notification -= OnNotification;
                    _metrics.SetListenerConnected(false);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _metrics.ListenerReconnects.Add(1);
                _logger.LogError(ex, "Gossip LISTEN connection failed; reconnecting in {Delay}s.", ReconnectDelay.TotalSeconds);
                try { await Task.Delay(ReconnectDelay, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private void OnNotification(object? sender, NpgsqlNotificationEventArgs e)
    {
        _metrics.NotificationsReceived.Add(1);
        if (!TryParsePayload(e.Payload, out var internalId, out var type))
        {
            _metrics.NotificationsDiscarded.Add(1, new KeyValuePair<string, object?>("reason", "malformed"));
            _logger.LogWarning("Ignoring malformed gossip notification payload: {Payload}", e.Payload);
            return;
        }

        // The queue is drop-oldest, so TryWrite never reports the loss. Sample the depth first so a
        // backed-up fetch loop is visible instead of silently shedding gossip.
        var depth = _pending.Reader.Count;
        _metrics.SetFetchQueueDepth(depth);
        if (depth >= PendingCapacity)
            _metrics.NotificationsDiscarded.Add(1, new KeyValuePair<string, object?>("reason", "queue_full"));

        _pending.Writer.TryWrite((internalId, type));
    }

    private async Task PumpLoopAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var (internalId, type) in _pending.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var store = scope.ServiceProvider.GetRequiredService<IGossipRawStore>();
                    var raw = await store.GetRawByInternalIdAsync(internalId, type, stoppingToken);
                    if (raw is { Length: > 0 })
                    {
                        _stream.Publish(new GossipFrame(internalId, type, raw));
                        _metrics.FramesPublished.Add(1, new KeyValuePair<string, object?>("msg_type", type));
                    }
                    else
                    {
                        _metrics.NotificationsDiscarded.Add(1, new KeyValuePair<string, object?>("reason", "no_raw_gossip"));
                        _logger.LogDebug("No raw gossip for internal_id {InternalId} (type {Type}); skipped.", internalId, type);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _metrics.FetchFailures.Add(1);
                    _logger.LogError(ex, "Failed to fetch/publish gossip for internal_id {InternalId}.", internalId);
                }
                finally
                {
                    _metrics.SetFetchQueueDepth(_pending.Reader.Count);
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private static bool TryParsePayload(string payload, out long internalId, out int type)
    {
        internalId = 0;
        type = 0;
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            if (!root.TryGetProperty("i", out var i) || !root.TryGetProperty("t", out var t))
                return false;
            internalId = i.GetInt64();
            type = t.GetInt32();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
