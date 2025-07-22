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

    public async Task<byte []?> GetSnapshotAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var query = $"""
                    
                    SELECT nrg.raw_gossip 
                    FROM nodes AS n
                    JOIN nodes_raw_gossip AS nrg 
                    ON n.node_id_str = nrg.node_id_str
                    WHERE @timestamp BETWEEN n.from_timestamp AND n.last_seen
                    AND nrg.timestamp <= @timestamp
                    AND nrg.timestamp >= @timestamp - INTERVAL '14 days'
                    AND nrg.raw_gossip IS NOT NULL
                    
                    UNION ALL
                    
                    SELECT c.raw_gossip
                    FROM channels AS c
                    WHERE @timestamp BETWEEN c.from_timestamp AND c.to_timestamp AND c.raw_gossip IS NOT NULL
                    
                    UNION ALL
                    
                    SELECT cu.raw_gossip
                    FROM channel_updates AS cu
                    WHERE @timestamp BETWEEN cu.from_timestamp AND cu.to_timestamp AND cu.raw_gossip IS NOT NULL;
                    """;
        
        var result = await _connection.QueryAsync<byte []?>(query, new { timestamp });

        return result.SelectMany(b => b).ToArray();
    }
}
