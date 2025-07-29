using Asp.Versioning;
using LN_history.Api.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace LN_history.Api.v1.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("ln-history/v{v:apiVersion}/[controller]")]
public class ChannelController : ControllerBase
{
    private readonly NpgsqlConnection _npgsqlConnection;
    
    public ChannelController(NpgsqlConnection npgsqlConnection)
    {
        _npgsqlConnection = npgsqlConnection;
    }
    
    /// <summary>
    /// Gets Information of a channel by channelId (scid)
    /// </summary>
    /// <param name="scid"></param>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{scid}/info/{timestamp}/stream")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChannelInformationByScidAndTimestamp(string scid, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        await _npgsqlConnection.OpenAsync(cancellationToken);

        var sql = """
            -- Get the channel by scid
            SELECT c.raw_gossip
            FROM channels c
            WHERE c.scid = @scid
              AND c.from_timestamp <= @timestamp
              AND (c.to_timestamp IS NULL OR c.to_timestamp > @timestamp)

            UNION ALL

            -- Get all channel_updates for this scid up until the given timestamp
            SELECT cu.raw_gossip
            FROM channel_updates cu
            WHERE cu.scid = @scid
              AND cu.from_timestamp <= @timestamp

            UNION ALL

            -- Get latest nodes_raw_gossip for both source and target nodes
            SELECT nrg.raw_gossip
            FROM nodes_raw_gossip nrg
            WHERE nrg.node_id IN (
                SELECT c.source_node_id
                FROM channels c
                WHERE c.scid = @scid
                  AND c.from_timestamp <= @timestamp
                  AND (c.to_timestamp IS NULL OR c.to_timestamp > @timestamp)
                UNION
                SELECT c.target_node_id
                FROM channels c
                WHERE c.scid = @scid
                  AND c.from_timestamp <= @timestamp
                  AND (c.to_timestamp IS NULL OR c.to_timestamp > @timestamp)
            )
            AND nrg.timestamp <= @timestamp
            AND nrg.timestamp = (
                SELECT MAX(nrg2.timestamp)
                FROM nodes_raw_gossip nrg2
                WHERE nrg2.node_id = nrg.node_id
                  AND nrg2.timestamp <= @timestamp
            )
        """;

        Response.ContentType = "application/octet-stream";
        Response.Headers.Append("Content-Disposition", 
            $"attachment; filename=channel_{scid}_{timestamp:yyyyMMdd_HHmmss}.bin");

        using var cmd = new NpgsqlCommand(sql, _npgsqlConnection);
        cmd.Parameters.AddWithValue("@scid", scid);
        cmd.Parameters.AddWithValue("@timestamp", timestamp);

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.IsDBNull(0))
            {
                var bytes = (byte[])reader[0];
                await Response.Body.WriteAsync(bytes, cancellationToken);
            }
        }

        return new EmptyResult();
    }
}