using System.Data;
using Dapper;
using LN_history.Data.Model;
using Microsoft.Extensions.Logging;

namespace LN_history.Data.DataStores;

public class ChannelAnnouncementDataStore : IChannelAnnouncementDataStore
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger<IChannelAnnouncementDataStore> _logger;

    public ChannelAnnouncementDataStore(IDbConnection dbConnection, ILogger<IChannelAnnouncementDataStore> logger)
    {
        _dbConnection = dbConnection;
        _logger = logger;
    }
    
    public async Task<ChannelAnnouncementMessage?> GetChannelAnnouncementByScidAsync(string scid,
        CancellationToken cancellationToken)
    {
        var query = "SELECT DISTINCT(scid), node_id_1, node_id_2, features, chain_hash FROM channel_announcements WHERE scid = @Scid;";

        var parameters = new { Scid = scid };

        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken, commandTimeout: 120);

        try
        {
            var result = await _dbConnection.QueryFirstOrDefaultAsync<ChannelAnnouncementMessage>(command);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogCritical($"Error executing query: {ex.Message}");
            _logger.LogCritical($"Query: {query}");
            _logger.LogCritical($"Parameters: Scid = {parameters.Scid}");
            throw;
        }
    }

    public async Task<ICollection<ChannelAnnouncementMessage>> GetChannelAnnouncementsAsync(CancellationToken cancellationToken)
    {
        var query = "SELECT DISTINCT(scid), node_id_1, node_id_2, features, chain_hash FROM channel_announcements;";

        var command = new CommandDefinition(query, cancellationToken: cancellationToken, commandTimeout: 120);

        var result = await _dbConnection.QueryAsync<ChannelAnnouncementMessage>(command);

        return result.ToList();
    }
}