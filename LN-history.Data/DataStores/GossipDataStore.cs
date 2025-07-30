using DuckDB.NET.Data;
using Microsoft.Extensions.Configuration;

namespace LN_history.Data.DataStores;

public class GossipDataStore : IGossipDataStore
{
    private readonly DuckDBConnection _duckDbConnection;
    private readonly IConfiguration _configuration;

    public GossipDataStore(DuckDBConnection duckDbConnection, IConfiguration configuration)
    {
        _duckDbConnection = duckDbConnection;
        _configuration = configuration;
    }
    
    public MemoryStream GetGossipSnapshotByTimestampJoins(DateTime timestamp) {
        using var command = _duckDbConnection.CreateCommand();
        command.CommandText = @"
        WITH valid_channels AS (
            SELECT scid, source_node_id, target_node_id
            FROM channels
            WHERE $timestamp BETWEEN from_timestamp AND COALESCE(to_timestamp, $timestamp)
        ),
        latest_channel_updates AS (
            SELECT scid, MAX(to_timestamp) AS latest_update
            FROM channel_updates
            WHERE scid IN (SELECT scid FROM valid_channels)
            GROUP BY scid
        ),
        channels_with_latest_updates AS (
            SELECT vc.scid, vc.source_node_id, vc.target_node_id, cu.raw_gossip AS channel_update_gossip
            FROM valid_channels vc
            JOIN channel_updates cu ON vc.scid = cu.scid
            JOIN latest_channel_updates lcu ON cu.scid = lcu.scid AND cu.to_timestamp = lcu.latest_update
        ),
        nodes_gossip AS (
            SELECT nrg.node_id, nrg.raw_gossip, MAX(nrg.timestamp) AS latest_gossip_timestamp
            FROM nodes_raw_gossip nrg
            WHERE nrg.timestamp <= $timestamp
              AND nrg.node_id IN (
                  SELECT source_node_id FROM channels_with_latest_updates
                  UNION
                  SELECT target_node_id FROM channels_with_latest_updates
              )
            GROUP BY nrg.node_id, nrg.raw_gossip
        )
        SELECT scid, source_node_id, target_node_id, channel_update_gossip
        FROM channels_with_latest_updates
        UNION ALL
        SELECT node_id, raw_gossip, NULL, NULL
        FROM nodes_gossip;";

        command.Parameters.Add(new DuckDBParameter("timestamp", timestamp));
        return ExecuteQueryAndGetConcatenatedMemoryStream(command);
    }
    
    public MemoryStream GetGossipSnapshotByTimestampCuts(DateTime timestamp)
    {
        using var command = _duckDbConnection.CreateCommand();

        command.CommandText = @"
            SELECT nrg.raw_gossip
            FROM nodes n
            JOIN nodes_raw_gossip nrg ON n.node_id = nrg.node_id
            WHERE $timestamp BETWEEN n.from_timestamp AND n.last_seen
              AND nrg.timestamp <= $timestamp
              AND nrg.timestamp >= $timestamp - INTERVAL '14 days'
            
            UNION ALL
            
            SELECT c.raw_gossip
            FROM channels c
            WHERE $timestamp BETWEEN c.from_timestamp AND c.to_timestamp
            
            UNION ALL
            
            SELECT cu.raw_gossip
            FROM channel_updates cu
            WHERE $timestamp BETWEEN cu.from_timestamp AND cu.to_timestamp
        ";
        
        command.Parameters.Add(new DuckDBParameter("timestamp", timestamp));

        return ExecuteQueryAndGetConcatenatedMemoryStream(command);
    }

    public MemoryStream GetGossipSnapshotDifferenceByTimestamps(DateTime startTimestamp, DateTime endTimestamp)
    {
        using var command = _duckDbConnection.CreateCommand();

        command.CommandText = @"
            SELECT c.raw_gossip
            FROM channels AS c
            WHERE c.from_timestamp <= $end_timestamp
              AND (c.to_timestamp >= $start_timestamp OR c.to_timestamp IS NULL)

            UNION ALL

            SELECT cu.raw_gossip
            FROM channel_updates AS cu
            WHERE cu.from_timestamp <= $end_timestamp
              AND cu.to_timestamp >= $start_timestamp

            UNION ALL

            SELECT nrg.raw_gossip
            FROM nodes_raw_gossip AS nrg
            WHERE nrg.timestamp BETWEEN $start_timestamp AND $end_timestamp;
        ";
        
        command.Parameters.Add(new DuckDBParameter("start_timestamp", startTimestamp));
        command.Parameters.Add(new DuckDBParameter("end_timestamp", endTimestamp));
        
        return ExecuteQueryAndGetConcatenatedMemoryStream(command);
    }

    public MemoryStream GetNodeGossipByNodeId(string nodeId, DateTime timestamp)
    {
        using var command = _duckDbConnection.CreateCommand();

        command.CommandText = @"
            -- Get all nodes_raw_gossip for the node_id up until the given timestamp
            SELECT nrg.raw_gossip
            FROM nodes_raw_gossip nrg
            WHERE nrg.node_id = $node_id
              AND nrg.timestamp <= $timestamp

            UNION ALL

            -- Get channels where the node is either source or target
            SELECT c.raw_gossip
            FROM channels c
            WHERE (c.source_node_id = $node_id OR c.target_node_id = $node_id)
              AND c.from_timestamp <= $timestamp
              AND (c.to_timestamp IS NULL OR c.to_timestamp > $timestamp)

            UNION ALL

            -- Get latest channel_updates for channels involving this node
            SELECT cu.raw_gossip
            FROM channel_updates cu
            WHERE cu.scid IN (
                SELECT c.scid
                FROM channels c
                WHERE (c.source_node_id = $node_id OR c.target_node_id = $node_id)
                  AND c.from_timestamp <= $timestamp
                  AND (c.to_timestamp IS NULL OR c.to_timestamp > $timestamp)
            )
            AND cu.from_timestamp <= $timestamp
            AND cu.to_timestamp > $timestamp;
        ";

        command.Parameters.Add(new DuckDBParameter("node_id", nodeId));
        command.Parameters.Add(new DuckDBParameter("timestamp", timestamp));

        return ExecuteQueryAndGetConcatenatedMemoryStream(command);
    }

    public MemoryStream GetChannelGossipByScid(string scid, DateTime timestamp)
    {
        using var command = _duckDbConnection.CreateCommand();

        command.CommandText = @"
            -- Get the channel by scid
            SELECT c.raw_gossip
            FROM channels c
            WHERE c.scid = $scid
              AND c.from_timestamp <= $timestamp
              AND (c.to_timestamp IS NULL OR c.to_timestamp > $timestamp)

            UNION ALL

            -- Get all channel_updates for this scid up until the given timestamp
            SELECT cu.raw_gossip
            FROM channel_updates cu
            WHERE cu.scid = $scid
              AND cu.from_timestamp <= $timestamp

            UNION ALL

            -- Get latest nodes_raw_gossip for both source and target nodes
            SELECT nrg.raw_gossip
            FROM nodes_raw_gossip nrg
            WHERE nrg.node_id IN (
                SELECT c.source_node_id
                FROM channels c
                WHERE c.scid = $scid
                  AND c.from_timestamp <= $timestamp
                  AND (c.to_timestamp IS NULL OR c.to_timestamp > $timestamp)
                UNION
                SELECT c.target_node_id
                FROM channels c
                WHERE c.scid = $scid
                  AND c.from_timestamp <= $timestamp
                  AND (c.to_timestamp IS NULL OR c.to_timestamp > $timestamp)
            )
            AND nrg.timestamp <= $timestamp
            AND nrg.timestamp = (
                SELECT MAX(nrg2.timestamp)
                FROM nodes_raw_gossip nrg2
                WHERE nrg2.node_id = nrg.node_id
                  AND nrg2.timestamp <= $timestamp
            );
        ";

        command.Parameters.Add(new DuckDBParameter("scid", scid));
        command.Parameters.Add(new DuckDBParameter("timestamp", timestamp));

        return ExecuteQueryAndGetConcatenatedMemoryStream(command);
    }

    private MemoryStream ExecuteQueryAndGetConcatenatedMemoryStream(DuckDBCommand command)
    {
        var concatenatedStream = new MemoryStream();
        _duckDbConnection.Open();
    
        using (var setThreadCommand = _duckDbConnection.CreateCommand())
        {
            var threads = _configuration["Threads"];
            if (!string.IsNullOrEmpty(threads))
            {
                setThreadCommand.CommandText = $"SET threads = {threads};";
                setThreadCommand.ExecuteNonQuery();
            }
        }

        try
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetStream(0) is Stream blobStream)
                {
                    blobStream.CopyTo(concatenatedStream);
                }
            }
        }
        finally
        {
            _duckDbConnection.Close();
        }
    
        concatenatedStream.Position = 0;
        return concatenatedStream;
    }
    
    
}