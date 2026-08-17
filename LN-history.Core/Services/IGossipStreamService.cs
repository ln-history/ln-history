using LN_History.Model;

namespace LN_history.Core.Services;

/// <summary>
/// Live gossip stream for the API/WebSocket layer: subscribe to new gossip as it is ingested, and
/// optionally backfill the gap since a known <c>internal_id</c> on reconnect.
/// </summary>
public interface IGossipStreamService
{
    /// <summary>Subscribe to live gossip. Dispose the subscription to stop.</summary>
    IGossipSubscription Subscribe(IReadOnlyCollection<int>? types);

    /// <summary>
    /// Frames ingested after <paramref name="sinceInternalId"/> (exclusive), ordered by internal_id,
    /// capped at <paramref name="max"/>. Replays the gap a reconnecting client missed.
    /// </summary>
    Task<IReadOnlyList<GossipFrame>> GetBackfillAsync(
        long sinceInternalId, IReadOnlyCollection<int>? types, int max, CancellationToken cancellationToken);
}
