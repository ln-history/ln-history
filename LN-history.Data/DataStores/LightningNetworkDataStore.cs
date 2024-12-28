using System.Data;
using Dapper;
using LN_history.Data.Model;
using Microsoft.Extensions.Logging;

namespace LN_history.Data.DataStores;

public class LightningNetworkDataStore : ILightningNetworkDataStore
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<ILightningNetworkDataStore> _logger;
    
    public LightningNetworkDataStore(ILogger<ILightningNetworkDataStore> logger, IDbConnection dbConnection)
    {
        _logger = logger;
        _dbConnection = dbConnection;
    }

    public async Task<ICollection<ChannelMessageComplete>> GetChannelsByTimespanAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        var query = """
                    WITH RankedUpdates AS (
                        SELECT  
                            up.scid AS scid, 
                            up.timestamp AS timestamp, 
                            up.message_flags AS message_flags, 
                            up.channel_flags AS channel_flags, 
                            up.cltv_expiry_delta AS cltv_expiry_delta, 
                            up.htlc_minimum_msat AS htlc_minimum_msat, 
                            up.fee_base_msat AS fee_base_msat, 
                            up.fee_proportional_millionths AS fee_proportional_millionths, 
                            up.htlc_maximum_msat AS htlc_maximum_msat, 
                            up.chain_hash AS chain_hash, 
                            an.features AS features, 
                            an.node_id_1 AS node_id_1, 
                            an.node_id_2 AS node_id_2, 
                            ROW_NUMBER() OVER (PARTITION BY up.scid ORDER BY up.timestamp DESC) AS rn
                        FROM 
                            channel_updates AS up 
                        JOIN 
                            channel_announcements AS an ON up.scid = an.scid
                        WHERE 
                            up.timestamp BETWEEN @startTime AND @endTime
                    )
                    SELECT 
                        scid,
                        timestamp,
                        message_flags,
                        channel_flags,
                        cltv_expiry_delta,
                        htlc_minimum_msat,
                        fee_base_msat,
                        fee_proportional_millionths,
                        htlc_maximum_msat,
                        chain_hash,
                        features,
                        node_id_1,
                        node_id_2
                    FROM 
                        RankedUpdates
                    WHERE 
                        rn = 1
                    """;
        
        var parameters = new
        {
            StartTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"),
            EndTime = endTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ")
        };

        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken, commandTimeout: 300);

        try
        {
            var result = await _dbConnection.QueryAsync<ChannelMessageComplete>(command);
            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error executing query: {ex.Message}");
            _logger.LogCritical($"Query: {query}");
            _logger.LogCritical($"Parameters: StartTime = {parameters.StartTime}, EndTime = {parameters.EndTime}");
            throw;
        }
    }

    public async Task<ChannelMessageComplete?> GetChannelInformationCompleteByScidAndTimespanAsync(string scid, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        var query =
            """
            SELECT DISTINCT(up.scid), timestamp, message_flags, channel_flags, cltv_expiry_delta, htlc_minimum_msat, fee_base_msat, fee_proportional_millionths, htlc_maximum_msat, up.chain_hash, features, node_id_1, node_id_2
            FROM (SELECT * 
                  FROM channel_updates
                  WHERE timestamp BETWEEN @startTime AND @endTime) AS up, channel_announcements AS an
            WHERE up.scid = an.scid
            """;

        var parameters = new
        {
            StartTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"),
            EndTime = endTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"),
            Scid = scid
        };

        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken, commandTimeout: 120);

        try
        {
            var result = await _dbConnection.QueryFirstOrDefaultAsync<ChannelMessageComplete>(command);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error executing query: {ex.Message}");
            _logger.LogCritical($"Query: {query}");
            _logger.LogCritical($"Parameters: StartTime = {parameters.Scid}");
            throw;
        }
    }
}