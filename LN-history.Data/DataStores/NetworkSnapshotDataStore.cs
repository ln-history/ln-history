using System.Data;
using Dapper;
using LN_history.Data.Model;

namespace LN_history.Data.DataStores;

public class NetworkSnapshotDataStore : INetworkSnapshotDataStore
{
    private readonly IDbConnection _connection;

    public NetworkSnapshotDataStore(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<(IEnumerable<Node>, IEnumerable<Channel>)> GetSnapshotAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        // var ts = timestamp.ToString("yyyy-MM-dd");

        var channelQuery = @"
            SELECT c.scid, c.source_node_id, c.target_node_id, c.amount_sat, c.features,
                   c.from_timestamp AS channel_from, c.to_timestamp AS channel_to,
                   cu.from_timestamp AS update_from, cu.to_timestamp AS update_to,
                   cu.htlc_minimum_msat, cu.htlc_maximum_msat, cu.fee_base_msat, cu.fee_ppm_msat, cu.cltv_expiry_delta
            FROM channels c
            JOIN channelupdates cu ON c.latest_update_id = cu.update_id
            WHERE cu.from_timestamp <= @timestamp AND cu.to_timestamp >= @timestamp
              AND c.from_timestamp <= @timestamp AND c.to_timestamp >= @timestamp";

        var nodeQuery = @"
            SELECT DISTINCT n.*
            FROM nodes n
            WHERE n.from_timestamp <= @timestamp AND n.to_timestamp >= @timestamp
              AND n.id IN (
                SELECT c.source_node_id FROM channels c
                WHERE c.latest_update_id IN (
                    SELECT cu.update_id FROM channelupdates cu
                    WHERE cu.from_timestamp <= @timestamp AND cu.to_timestamp >= @timestamp)
                AND c.from_timestamp <= @timestamp AND c.to_timestamp >= @timestamp
                UNION
                SELECT c.target_node_id FROM channels c
                WHERE c.latest_update_id IN (
                    SELECT cu.update_id FROM channelupdates cu
                    WHERE cu.from_timestamp <= @timestamp AND cu.to_timestamp >= @timestamp)
                AND c.from_timestamp <= @timestamp AND c.to_timestamp >= @timestamp)";

        var channels = await _connection.QueryAsync<Channel>(channelQuery, new { timestamp });
        var nodes = await _connection.QueryAsync<Node>(nodeQuery, new { timestamp });

        return (nodes, channels);
    }
}
