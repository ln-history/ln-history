using LN_History.Model;

namespace LN_history.Data.DataStores;

/// <summary>
/// Reads raw gossip envelopes by inventory <c>internal_id</c>. Backs the live-stream listener
/// (single-row lookup on each NOTIFY) and the stream reconnect backfill (rows since an internal_id).
/// </summary>
public interface IGossipRawStore
{
    /// <summary>
    /// The raw gossip bytes for one message, looked up in the table implied by its BOLT #7
    /// <paramref name="type"/>. Null if the type is unknown or the row/bytes are absent.
    /// </summary>
    Task<byte[]?> GetRawByInternalIdAsync(long internalId, int type, CancellationToken cancellationToken);

    /// <summary>
    /// Frames with <c>internal_id</c> greater than <paramref name="sinceInternalId"/>, ordered by
    /// internal_id ascending, capped at <paramref name="max"/>. Used to replay the gap a reconnecting
    /// client missed. Optionally restricted to <paramref name="types"/>.
    /// </summary>
    Task<IReadOnlyList<GossipFrame>> GetSinceAsync(
        long sinceInternalId, IReadOnlyCollection<int>? types, int max, CancellationToken cancellationToken);
}
