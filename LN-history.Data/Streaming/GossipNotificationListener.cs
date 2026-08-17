using System.Text.Json;
using System.Threading.Channels;
using LN_history.Data.DataStores;
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

    private readonly NpgsqlDataSource _dataSource;
    private readonly IGossipStream _stream;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GossipNotificationListener> _logger;

    private readonly Channel<(long InternalId, int Type)> _pending =
        System.Threading.Channels.Channel.CreateBounded<(long, int)>(new BoundedChannelOptions(16_384)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

    public GossipNotificationListener(
        NpgsqlDataSource dataSource,
        IGossipStream stream,
        IServiceScopeFactory scopeFactory,
        ILogger<GossipNotificationListener> logger)
    {
        _dataSource = dataSource;
        _stream = stream;
        _scopeFactory = scopeFactory;
        _logger = logger;
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

                    // WaitAsync returns whenever a notification arrives (raising Notification); loop to keep waiting.
                    while (!stoppingToken.IsCancellationRequested)
                        await connection.WaitAsync(stoppingToken);
                }
                finally
                {
                    connection.Notification -= OnNotification;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gossip LISTEN connection failed; reconnecting in {Delay}s.", ReconnectDelay.TotalSeconds);
                try { await Task.Delay(ReconnectDelay, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private void OnNotification(object? sender, NpgsqlNotificationEventArgs e)
    {
        if (TryParsePayload(e.Payload, out var internalId, out var type))
            _pending.Writer.TryWrite((internalId, type));
        else
            _logger.LogWarning("Ignoring malformed gossip notification payload: {Payload}", e.Payload);
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
                        _stream.Publish(new GossipFrame(internalId, type, raw));
                    else
                        _logger.LogDebug("No raw gossip for internal_id {InternalId} (type {Type}); skipped.", internalId, type);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch/publish gossip for internal_id {InternalId}.", internalId);
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
