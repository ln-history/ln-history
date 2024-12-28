using System.Data;
using Dapper;
using LN_history.Core.Comparer;
using LN_history.Data.Model;
using Microsoft.Extensions.Logging;

namespace LN_history.Data.DataStores;

public class ChannelUpdateDataStore : IChannelUpdateDataStore
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<IChannelUpdateDataStore> _logger;

    public ChannelUpdateDataStore(IDbConnection dbConnection, ILogger<IChannelUpdateDataStore> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task<ICollection<ChannelUpdateMessage>> GetDistinctLatestChannelUpdateMessagesForTimespanAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        var query = "SELECT * FROM channel_updates LATEST BY scid WHERE timestamp BETWEEN @startTime AND @endTime;";
        
        var parameters = new
        {
            StartTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"),
            EndTime = endTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ")
        };

        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken, commandTimeout: 120);

        try
        {
            var result = await _dbConnection.QueryAsync<ChannelUpdateMessage>(command);
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

    public async Task<ChannelUpdateMessage?> GetLatestChannelUpdateMessageForTimespanByScidAsync(string scid, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        var query = "SELECT MAX(timestamp) FROM channel_updates WHERE scid = @scid AND (TIMESTAMP BETWEEN @startTime AND @endTime)";
        
        // Formatting DateTime to string in ISO 8601 format expected by QuestDB
        var parameters = new
        {
            StartTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"),
            EndTime = endTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"),
            Scid = scid
        };
        
        try
        {
            var result = await _dbConnection.QueryFirstOrDefaultAsync<ChannelUpdateMessage>(
                query, parameters, commandTimeout: 120);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error executing query: {ex.Message}");
            _logger.LogCritical($"Query: {query}");
            _logger.LogCritical($"Parameters: StartTime = {parameters.StartTime}, EndTime = {parameters.EndTime}");
            throw;
        }
    }
}