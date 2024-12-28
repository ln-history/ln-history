using System.Data;
using Dapper;
using LN_history.Data.Model;
using Microsoft.Extensions.Logging;

namespace LN_history.Data.DataStores;

/// <inheritdoc />
public class NodeAnnouncementDataStore : INodeAnnouncementDataStore
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<INodeAnnouncementDataStore> _logger;

    public NodeAnnouncementDataStore(IDbConnection dbConnection, ILogger<INodeAnnouncementDataStore> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    /// <summary>
    /// Gets some value
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<NodeAnnouncementMessage?> GetNodeAnnouncementByIdAsync(string nodeId, CancellationToken cancellationToken)
    {
        const string query = "SELECT * FROM node_announcements WHERE node_id = @NodeId";
        
        var parameters = new
        {
            NodeId = nodeId
        };
        
        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken, commandTimeout: 120);
        
        try
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<NodeAnnouncementMessage>(command);
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error executing query: {ex.Message}");
            _logger.LogCritical($"Query: {query}");
            _logger.LogCritical($"Parameters: NodeId = {parameters.NodeId}");
            throw;
        }
    }

    public async Task<ICollection<NodeAnnouncementMessage>> GetNodeAnnouncementsForTimespanAsync(DateTime startTime, DateTime endTime,
        CancellationToken cancellationToken)
    {
        const string query = "SELECT * FROM node_announcements LATEST BY node_id WHERE (TIMESTAMP BETWEEN @startTime AND @endTime)";
        // Formatting DateTime to string in ISO 8601 format expected by QuestDB
        var parameters = new
        {
            StartTime = startTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"),
            EndTime = endTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ")
        };
        
        try
        {
            var result = await _dbConnection.QueryAsync<NodeAnnouncementMessage>(
                query, parameters, commandTimeout: 120);

            return result.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error executing query: {ex.Message}");
            _logger.LogCritical($"Query: {query}");
            _logger.LogCritical($"Parameters: StartTime = {parameters.StartTime}, EndTime = {parameters.EndTime}");
            return new List<NodeAnnouncementMessage>();
        }
    }
}
