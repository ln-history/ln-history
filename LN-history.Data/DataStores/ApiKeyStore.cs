using Dapper;
using LN_history.Data.Internal;
using LN_History.Model;
using Npgsql;

namespace LN_history.Data.DataStores;

public class ApiKeyStore : IApiKeyStore
{
    private readonly NpgsqlDataSource _dataSource;

    public ApiKeyStore(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
        DapperConfiguration.EnsureConfigured();
    }

    public async Task<IReadOnlyList<ApiKeyRecord>> LoadKeysAsync(CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        const string sql = """
            SELECT id, key_hash, display_prefix, role, owner_label,
                   daily_budget, burst_per_sec, max_stream_conns, expires_at, enabled
            FROM api.api_keys
            WHERE enabled = true
            """;
        var rows = await connection.QueryAsync<ApiKeyRow>(new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.Select(r => r.ToRecord()).ToList();
    }

    public async Task<IReadOnlyDictionary<string, int>> LoadWeightsAsync(CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<(string route_key, int weight)>(
            new CommandDefinition("SELECT route_key, weight FROM api.endpoint_weights", cancellationToken: cancellationToken));
        return rows.ToDictionary(r => r.route_key, r => r.weight);
    }

    public async Task<IReadOnlyDictionary<long, long>> LoadTodaySpendAsync(DateOnly utcDay, CancellationToken cancellationToken)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<(long key_id, long spent)>(new CommandDefinition(
            "SELECT key_id, sum(sum_cost) AS spent FROM api.usage WHERE usage_day = @utcDay GROUP BY key_id",
            new { utcDay }, cancellationToken: cancellationToken));
        return rows.ToDictionary(r => r.key_id, r => r.spent);
    }

    public async Task FlushUsageAsync(IReadOnlyCollection<UsageDelta> deltas, CancellationToken cancellationToken)
    {
        if (deltas.Count == 0) return;

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string upsert = """
            INSERT INTO api.usage (key_id, usage_day, endpoint, sum_cost, req_count, sum_duration_ms, sum_bytes)
            VALUES (@KeyId, @UsageDay, @Endpoint, @Cost, @Count, @DurationMs, @Bytes)
            ON CONFLICT (key_id, usage_day, endpoint) DO UPDATE SET
                sum_cost        = api.usage.sum_cost        + EXCLUDED.sum_cost,
                req_count       = api.usage.req_count       + EXCLUDED.req_count,
                sum_duration_ms = api.usage.sum_duration_ms + EXCLUDED.sum_duration_ms,
                sum_bytes       = api.usage.sum_bytes       + EXCLUDED.sum_bytes
            """;
        // Dapper executes the upsert once per delta on the same connection/transaction.
        await connection.ExecuteAsync(new CommandDefinition(upsert, deltas, transaction, cancellationToken: cancellationToken));

        var activeIds = deltas.Where(d => d.KeyId > 0).Select(d => d.KeyId).Distinct().ToArray();
        if (activeIds.Length > 0)
            await connection.ExecuteAsync(new CommandDefinition(
                "UPDATE api.api_keys SET last_used_at = now() WHERE id = ANY(@activeIds)",
                new { activeIds }, transaction, cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
    }

    private sealed class ApiKeyRow
    {
        public long Id { get; set; }
        public string KeyHash { get; set; } = string.Empty;
        public string DisplayPrefix { get; set; } = string.Empty;
        public string Role { get; set; } = "researcher";
        public string OwnerLabel { get; set; } = string.Empty;
        public int? DailyBudget { get; set; }
        public int? BurstPerSec { get; set; }
        public int? MaxStreamConns { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool Enabled { get; set; }

        public ApiKeyRecord ToRecord() => new()
        {
            Id = Id,
            KeyHash = KeyHash.Trim(),
            DisplayPrefix = DisplayPrefix,
            Role = string.Equals(Role, "admin", StringComparison.OrdinalIgnoreCase) ? KeyRole.Admin : KeyRole.Researcher,
            OwnerLabel = OwnerLabel,
            DailyBudget = DailyBudget,
            BurstPerSec = BurstPerSec,
            MaxStreamConns = MaxStreamConns,
            ExpiresAt = ExpiresAt,
            Enabled = Enabled,
        };
    }
}
