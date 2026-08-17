using Dapper;
using LN_history.Data.Internal;
using LN_History.Model;
using Npgsql;

namespace LN_history.Data.DataStores;

public class GossipRawStore : IGossipRawStore
{
    private readonly NpgsqlDataSource _dataSource;

    /// <summary>BOLT #7 message type -&gt; the table carrying its <c>raw_gossip</c> and <c>internal_id</c>.</summary>
    private static readonly IReadOnlyDictionary<int, string> TableByType = new Dictionary<int, string>
    {
        [256] = "channels",
        [257] = "node_announcements_complete",
        [258] = "channel_updates",
    };

    /// <summary>Guard for the reconnect-backfill scan (it can range over the internal_id indexes).</summary>
    private const int BackfillTimeoutMs = 30_000;

    public GossipRawStore(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
        DapperConfiguration.EnsureConfigured();
    }

    public async Task<byte[]?> GetRawByInternalIdAsync(long internalId, int type, CancellationToken cancellationToken)
    {
        if (!TableByType.TryGetValue(type, out var table))
            return null;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var sql = $"SELECT raw_gossip FROM {table} WHERE internal_id = @internalId LIMIT 1";
        return await connection.QuerySingleOrDefaultAsync<byte[]?>(
            new CommandDefinition(sql, new { internalId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<GossipFrame>> GetSinceAsync(
        long sinceInternalId, IReadOnlyCollection<int>? types, int max, CancellationToken cancellationToken)
    {
        var wanted = (types is { Count: > 0 } ? types : TableByType.Keys)
            .Where(TableByType.ContainsKey).Distinct().ToArray();
        if (wanted.Length == 0) return Array.Empty<GossipFrame>();

        // Message type is constant per table, so we inject it as a literal in each UNION branch.
        var unions = wanted.Select(t =>
            $"SELECT internal_id, {t} AS type, raw_gossip FROM {TableByType[t]} " +
            "WHERE internal_id > @since AND raw_gossip IS NOT NULL");
        var sql = $"SELECT internal_id, type, raw_gossip FROM ({string.Join(" UNION ALL ", unions)}) e " +
                  "ORDER BY internal_id LIMIT @max";

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await connection.ExecuteAsync(new CommandDefinition(
            $"SET LOCAL statement_timeout = {BackfillTimeoutMs}", transaction: transaction, cancellationToken: cancellationToken));

        var rows = await connection.QueryAsync<GossipFrameRow>(new CommandDefinition(
            sql, new { since = sinceInternalId, max }, transaction, cancellationToken: cancellationToken));
        return rows.Select(r => new GossipFrame(r.InternalId, r.Type, r.RawGossip)).ToList();
    }

    private sealed class GossipFrameRow
    {
        public long InternalId { get; set; }
        public int Type { get; set; }
        public byte[] RawGossip { get; set; } = Array.Empty<byte>();
    }
}
