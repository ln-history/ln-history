using System.Net.WebSockets;
using Asp.Versioning;
using LN_history.Api.Infrastructure;
using LN_history.Core.Auth;
using LN_history.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LN_history.Api.Controllers;

/// <summary>
/// WebSocket endpoint streaming raw BOLT #7 gossip as it is ingested. Each message is a binary
/// frame carrying one raw gossip envelope (varint(len) ++ uint16(type) ++ payload) — the same
/// bytes the snapshot/diff endpoints return, parseable with lnhistoryclient.
/// </summary>
[ApiController]
[ApiVersion(1)]
[Route("ln-history/v{v:apiVersion}/stream")]
[ApiExplorerSettings(IgnoreApi = true)] // WebSocket upgrade — Swagger cannot meaningfully describe it
public class GossipStreamController : ControllerBase
{
    /// <summary>Hard cap on reconnect backfill so a client with a very old cursor can't ask for millions of rows.</summary>
    private const int MaxBackfill = 50_000;

    /// <summary>route_key for charging the reconnect backfill query against the daily budget.</summary>
    private const string BackfillRouteKey = "stream_backfill";
    private const int BackfillDefaultWeight = 40;

    private readonly IGossipStreamService _stream;
    private readonly IApiAccessService _access;
    private readonly ILogger<GossipStreamController> _logger;

    public GossipStreamController(IGossipStreamService stream, IApiAccessService access, ILogger<GossipStreamController> logger)
    {
        _stream = stream;
        _access = access;
        _logger = logger;
    }

    /// <summary>
    /// WebSocket upgrade. Query params: <c>types</c> = comma-separated BOLT #7 types to receive
    /// (e.g. <c>256,257,258</c>; default all); <c>since</c> = replay gossip with internal_id greater
    /// than this value before switching to live (reconnect catch-up). Because browsers cannot set
    /// headers on a WebSocket handshake, the API key may be supplied as the <c>api_key</c> query
    /// param instead of the <c>x-api-key</c> header. A non-WebSocket request receives 400.
    /// </summary>
    [HttpGet]
    public async Task Get(
        [FromQuery(Name = "types")] string? types = null,
        [FromQuery(Name = "since")] long? since = null,
        CancellationToken cancellationToken = default)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsync("This endpoint requires a WebSocket upgrade.", cancellationToken);
            return;
        }

        // Enforce the per-key concurrent-connection cap BEFORE the upgrade (can't send HTTP status
        // after it). record is null only when auth is disabled (Development) => unlimited.
        var record = ApiKeyContext.Get(HttpContext);
        var slotHeld = false;
        if (record is not null)
        {
            if (!_access.TryAcquireStream(record))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await HttpContext.Response.WriteAsync("Too many concurrent stream connections for this key.", cancellationToken);
                return;
            }
            slotHeld = true;
        }

        try
        {
            var typeFilter = ParseTypes(types);
            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            // Charge the reconnect backfill (the only DB-touching part of the stream) against the
            // budget. The live tail is free (in-memory fan-out). Admins bypass.
            if (since is { } cursor)
            {
                if (record is { IsAdmin: false })
                {
                    var weight = _access.ResolveWeight(BackfillRouteKey, BackfillDefaultWeight);
                    var charge = _access.Charge(record, weight);
                    if (!charge.Allowed)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.PolicyViolation,
                            "backfill over budget", CancellationToken.None);
                        return;
                    }
                    _access.RecordActuals(record, BackfillRouteKey, weight, 0, 0);
                }
            }

            // Subscribe FIRST so live frames buffer while we run the backfill query; then dedup the
            // seam by internal_id. This yields no gap (live is buffered) and no duplicate (skip <= lastSent).
            using var subscription = _stream.Subscribe(typeFilter);
            var lastSent = long.MinValue;

            try
            {
                if (since is { } cursor2)
                {
                    var backlog = await _stream.GetBackfillAsync(cursor2, typeFilter, MaxBackfill, cancellationToken);
                    foreach (var frame in backlog)
                    {
                        await SendAsync(socket, frame.RawGossip, cancellationToken);
                        lastSent = frame.InternalId;
                    }
                }

                await foreach (var frame in subscription.ReadAllAsync(cancellationToken))
                {
                    if (frame.InternalId <= lastSent) continue; // already delivered by the backfill above
                    await SendAsync(socket, frame.RawGossip, cancellationToken);
                    lastSent = frame.InternalId;
                }
            }
            catch (OperationCanceledException)
            {
                // Client disconnected or the server is shutting down — normal.
            }
            catch (WebSocketException ex)
            {
                _logger.LogDebug(ex, "Gossip stream WebSocket closed by peer.");
            }
            finally
            {
                await CloseQuietlyAsync(socket);
            }
        }
        finally
        {
            if (slotHeld && record is not null)
                _access.ReleaseStream(record);
        }
    }

    private static ValueTask SendAsync(WebSocket socket, byte[] payload, CancellationToken cancellationToken) =>
        socket.SendAsync(payload.AsMemory(), WebSocketMessageType.Binary, endOfMessage: true, cancellationToken);

    private static async Task CloseQuietlyAsync(WebSocket socket)
    {
        if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
        {
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
            }
            catch (WebSocketException)
            {
                // Peer already gone.
            }
        }
    }

    private static IReadOnlyCollection<int>? ParseTypes(string? types)
    {
        if (string.IsNullOrWhiteSpace(types)) return null;

        var parsed = types
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => int.TryParse(t, out var v) ? v : (int?)null)
            .Where(v => v is not null)
            .Select(v => v!.Value)
            .Distinct()
            .ToArray();

        return parsed.Length > 0 ? parsed : null;
    }
}
