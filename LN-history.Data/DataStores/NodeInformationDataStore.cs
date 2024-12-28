using System.Data;
using System.Text;
using Dapper;
using LN_history.Data.Model;
using Microsoft.Extensions.Logging;

namespace LN_history.Data.DataStores;


public class NodeInformationDataStore : INodeInformationDataStore
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<INodeInformationDataStore> _logger;

    public NodeInformationDataStore(IDbConnection dbConnection, ILogger<INodeInformationDataStore> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }

    public async Task StoreNodeInformationAsync(DateTime timestamp, IEnumerable<NodeInformation> nodeInformation, int batchSize = 100,
        CancellationToken cancellationToken = default)
    {
        var batches = nodeInformation
            .Select((node, index) => new { node, index })
            .GroupBy(x => x.index / batchSize)
            .Select(group => group.Select(x => x.node).ToList());

        foreach (var batch in batches)
        {
            var queryBuilder =
                new StringBuilder(
                    "INSERT INTO NodeInformation (node_id, timestamp, features, rgb_color, addresses) VALUES ");
            var parameters = new DynamicParameters();

            for (int i = 0; i < batch.Count; i++)
            {
                var nodeInfo = batch[i];
                queryBuilder.Append($"(@NodeId{i}, @Timestamp{i}, @Features{i}, @RgbColor{i}, @Addresses{i}),");
                parameters.Add($"NodeId{i}", nodeInfo.NodeId);
                parameters.Add($"Timestamp{i}", timestamp);
                parameters.Add($"Features{i}", nodeInfo.Features);
                parameters.Add($"RgbColor{i}", nodeInfo.RgbColor);
                parameters.Add($"Addresses{i}", nodeInfo.Addresses);
            }

            // Remove the last comma
            queryBuilder.Length--;

            var query = queryBuilder.ToString();

            var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken,
                commandTimeout: 120);

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

    public async Task<NodeInformation?> GetNodeInformationByNodeIdAndTimestampAsync(string nodeId, DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        var query = "SELECT * FROM NodeInformation WHERE NodeId = @NodeId AND Timestamp = @Timestamp";
        
        var parameters = new
        {
            NodeId = nodeId,
            Timestamp = timestamp
        };
        
        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken, commandTimeout: 120);

        try
        {
            var results = await _dbConnection.QueryAsync<NodeInformation>(command);
            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error executing batch query: {ex.Message}");
            _logger.LogCritical($"Query: {query}");
            _logger.LogCritical($"Parameters: NodeId: {nodeId}, Timestamp: {timestamp}");
            return null;
        }
    }
}