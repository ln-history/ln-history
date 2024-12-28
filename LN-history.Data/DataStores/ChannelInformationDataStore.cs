using System.Data;
using System.Text;
using Dapper;
using LN_history.Data.Model;
using Microsoft.Extensions.Logging;

namespace LN_history.Data.DataStores;

public class ChannelInformationDataStore : IChannelInformationDataStore
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<IChannelInformationDataStore> _logger;

    public ChannelInformationDataStore(IDbConnection dbConnection, ILogger<IChannelInformationDataStore> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task StoreChannelInformationAsync(DateTime timestamp, IEnumerable<ChannelInformation> channelInformation, int batchSize = 100, CancellationToken cancellationToken = default)
    {
        var batches = channelInformation
            .Select((channel, index) => new { channel, index })
            .GroupBy(x => x.index / batchSize)
            .Select(group => group.Select(x => x.channel).ToList());

        foreach (var batch in batches)
        {
            var queryBuilder = new StringBuilder("INSERT INTO ChannelInformation (scid, timestamp, features, node_id_1, node_id_2, message_flags, channel_flags, cltv_expiry_delta, htlc_minimum_msat, fee_base_msat, fee_proportional_millionths, htlc_maximum_msat, chain_hash) VALUES ");
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                var channelInfo = batch[i];
                queryBuilder.Append($"(@Scid{i}, @Timestamp{i}, @Features{i}, @NodeId1{i}, @NodeId2{i}, @MessageFlags{i}, @ChannelFlags{i}, @CltvExpiryDelta{i}, @HtlcMinimumMSat{i}, @FeeBaseMSat{i}, @FeeProportionalMillionths{i}, @HtlcMaximumMSat{i}, @ChainHash{i}),");
                parameters.Add($"Scid{i}", channelInfo.Scid);
                parameters.Add($"Features{i}", channelInfo.Features);
                parameters.Add($"NodeId1{i}", channelInfo.NodeId1);
                parameters.Add($"NodeId2{i}", channelInfo.NodeId2);
                parameters.Add($"Timestamp{i}", timestamp);
                parameters.Add($"MessageFlags{i}", channelInfo.MessageFlags);
                parameters.Add($"ChannelFlags{i}", channelInfo.ChannelFlags);
                parameters.Add($"CltvExpiryDelta{i}", channelInfo.CltvExpiryDelta);
                parameters.Add($"HtlcMinimumMSat{i}", channelInfo.HtlcMinimumMSat);
                parameters.Add($"HtlcMaximumMSat{i}", channelInfo.HtlcMaximumMSat);
                parameters.Add($"FeeBaseMSat{i}", channelInfo.FeeBaseMSat);
                parameters.Add($"FeeProportionalMillionths{i}", channelInfo.FeeProportionalMillionths);
                parameters.Add($"ChainHash{i}", channelInfo.ChainHash);
            }

            // Remove the last comma
            queryBuilder.Length--;

            var query = queryBuilder.ToString();

            var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken, commandTimeout: 120);

            try
            {
                await _dbConnection.ExecuteAsync(command);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Error executing batch query: {ex.Message}");
                _logger.LogCritical($"Query: {query}");
                _logger.LogCritical($"Parameters: {string.Join(", ", parameters.ParameterNames)}");
            }
        }
    }

    public async Task<ChannelInformation?> GetChannelInformationByScidAndTimestampAsync(string scid, DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var query = $"SELECT * FROM ChannelInformation WHERE Scid = @Scid AND Timestamp = @Timestamp";

        var parameters = new
        {
            Scid = scid,
            Timestamp = timestamp
        };
        
        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);

        try
        {
            var result = await _dbConnection.QueryAsync(command);
            return result.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error executing batch query: {ex.Message}");
            _logger.LogCritical($"Query: {query}");
            _logger.LogCritical($"Parameters: Scid: {scid}, Timestamp: {timestamp}");
            return null;
        }
    }
}