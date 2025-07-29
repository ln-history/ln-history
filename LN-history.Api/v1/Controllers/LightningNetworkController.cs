using Asp.Versioning;
using LN_history.Api.Authorization;
using LN_history.Data.DataStores;
using LN_History.Model.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LN_history.Api.v1.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("ln-history/{v:apiVersion}/[controller]")]
[ApiKeyAuthorize]
public class LightningNetworkController : ControllerBase
{
    private readonly ILogger<LightningNetworkController> _logger;
    private readonly LightningSettings _settings;
    
    private readonly INetworkSnapshotDataStore _networkSnapshotDataStore;
    private readonly NpgsqlConnection _dbConnection;

    public LightningNetworkController(ILogger<LightningNetworkController> logger, IOptions<LightningSettings> options, INetworkSnapshotDataStore networkSnapshotDataStore, NpgsqlConnection dbConnection)
    {
        _logger = logger;
        _settings = options.Value;
        
        _networkSnapshotDataStore = networkSnapshotDataStore;
        _dbConnection = dbConnection;
    }
    
    [HttpGet("snapshotv2/{timestamp}/stream")]
    public async Task<IActionResult> GetSnapshotv2Stream(DateTime timestamp, CancellationToken cancellationToken)
    {
        await _dbConnection.OpenAsync(cancellationToken);
    
        var sql = """
            SELECT nrg.raw_gossip
            FROM nodes n
            JOIN nodes_raw_gossip nrg ON n.node_id_str = nrg.node_id_str
            WHERE @timestamp BETWEEN n.from_timestamp AND n.last_seen
              AND nrg.timestamp <= @timestamp
              AND nrg.timestamp >= @timestamp - INTERVAL '14 days'
              AND nrg.raw_gossip IS NOT NULL
            
            UNION ALL
            
            SELECT c.raw_gossip
            FROM channels c
            WHERE @timestamp BETWEEN c.from_timestamp AND c.to_timestamp
              AND c.raw_gossip IS NOT NULL
            
            UNION ALL
            
            SELECT cu.raw_gossip
            FROM channel_updates cu
            WHERE @timestamp BETWEEN cu.from_timestamp AND cu.to_timestamp
              AND cu.raw_gossip IS NOT NULL
        """; 
    
        Response.ContentType = "application/octet-stream";
        Response.Headers.Add("Content-Disposition", 
            $"attachment; filename=snapshot_{timestamp:yyyyMMdd_HHmmss}.bin");
    
        using var cmd = new NpgsqlCommand(sql, _dbConnection);
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
    
    //TODO: Fix this method
    [HttpGet("snapshotv2/{timestamp}/compressed")]
    public async Task<IActionResult> GetSnapshotv2Compressed(DateTime timestamp, CancellationToken cancellationToken)
    {
        // var sql = """
        //               SELECT nrg.raw_gossip
        //               FROM nodes n
        //               JOIN nodes_raw_gossip nrg ON n.node_id_str = nrg.node_id_str
        //               WHERE @timestamp BETWEEN n.from_timestamp AND n.last_seen
        //                 AND nrg.timestamp <= @timestamp
        //                 AND nrg.timestamp >= @timestamp - INTERVAL '14 days'
        //                 AND nrg.raw_gossip IS NOT NULL
        //               
        //               UNION ALL
        //               
        //               SELECT c.raw_gossip
        //               FROM channels c
        //               WHERE @timestamp BETWEEN c.from_timestamp AND c.to_timestamp
        //                 AND c.raw_gossip IS NOT NULL
        //               
        //               UNION ALL
        //               
        //               SELECT cu.raw_gossip
        //               FROM channel_updates cu
        //               WHERE @timestamp BETWEEN cu.from_timestamp AND cu.to_timestamp
        //                 AND cu.raw_gossip IS NOT NULL
        //           """; 
        //
        // Response.ContentType = "application/gzip";
        // Response.Headers.Add("Content-Disposition", 
        //     $"attachment; filename=snapshot_{timestamp:yyyyMMdd_HHmmss}.bin.gz");
        //
        // await _dbConnection.OpenAsync(cancellationToken);
        //
        // using var gzipStream = new GZipStream(Response.Body, CompressionLevel.Fastest);
        // using var cmd = new NpgsqlCommand(sql, _dbConnection);
        // cmd.Parameters.AddWithValue("@timestamp", timestamp);
        //
        // using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        //
        // while (await reader.ReadAsync(cancellationToken))
        // {
        //     if (!reader.IsDBNull(0))
        //     {
        //         var bytes = (byte[])reader[0];
        //         await gzipStream.WriteAsync(bytes, cancellationToken);
        //     }
        // }
        //
        // return new EmptyResult();
        throw new NotImplementedException();
    }

    // /// <summary>
    // /// Gets number of nodes in the Lightning Network by timestamp
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="int"/></returns>
    // [HttpGet("nodes/count/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<int>> GetNodeCountByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    // {
    //     // var result =  await _lightningNetworkService.GetNodeCountByTimestampAsync(timestamp, cancellationToken);
    //     // return Ok(result);
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets number of channels in the Lightning Network by timestamp
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="int"/></returns>
    // [HttpGet("edges/count/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<int>> GetEdgeCountByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    // {
    //     // var result =  await _lightningNetworkService.GetEdgeCountByTimestampAsync(timestamp, cancellationToken);
    //     // return Ok(result);
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets median transaction cost for a payment of size paymentSizeSat in the Lightning Network by timestamp using a Monte Carlo Simulation
    // /// </summary>
    // /// <param name="paymentSizeSat">paymentSize in satoshis (sats)</param>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="double"/></returns>
    // [HttpGet("simulateMedian/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetMedianTransactionCostByTimestampUsingSimulation(int paymentSizeSat, DateTime timestamp,
    //     CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets average transaction cost for a payment of size paymentSizeSat in the Lightning Network by timestamp using a Monte Carlo Simulation
    // /// </summary>
    // /// <param name="paymentSizeSat">paymentSize in satoshis (sats)</param>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="double"/></returns>
    // [HttpGet("simulateAverage{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetAverageTransactionCostByTimestampUsingSimulation(int paymentSizeSat, DateTime timestamp,
    //     CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets median transaction cost for a payment of size paymentSizeSat in the Lightning Network by timestamp using a formula
    // /// </summary>
    // /// <param name="paymentSizeSat">paymentSize in satoshis (sats)</param>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="double"/></returns>
    // [HttpGet("calculateMedian/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetMedianTransactionCostByTimestampUsingCalculation(int paymentSizeSat, DateTime timestamp,
    //     CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets average transaction cost for a payment of size paymentSizeSat in the Lightning Network by timestamp using a formula
    // /// </summary>
    // /// <param name="paymentSizeSat">paymentSize in satoshis (sats)</param>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="double"/></returns>
    // [HttpGet("calculateAverage/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetAverageTransactionCostByTimestampUsingCalculation(int paymentSizeSat, DateTime timestamp,
    //     CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets the diameter of the Lightning Network by timestamp.
    // /// The diameter is defined as the longest shortest path of a graph
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="int"/></returns>
    // [HttpGet("diameter/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetDiameterOfLightningNetworkByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets the average shortest path length of the Lightning Network by timestamp.
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="int"/></returns>
    // [HttpGet("avgPathLength/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetAveragePathLengthOfLightningNetworkByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets the average degree of the nodes in the Lightning Network by timestamp.
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="int"/></returns>
    // [HttpGet("avgDegree/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetAverageDegreeOfLightningNetworkByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets the global clustering coefficient of the Lightning Network by timestamp.
    // /// The local clustering coefficient of a node quantifies how close its neighbours are to being a clique.
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="double"/></returns>
    // [HttpGet("clusteringCoefficient/local/avg/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetAverageLocalClusteringCoefficientOfLightningNetworkByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets the global clustering coefficient of the Lightning Network by timestamp.
    // /// The global clustering coefficient is defined as the ratio of actual connections between a node’s neighbors to the maximum possible connections between them.
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="double"/></returns>
    // [HttpGet("clusteringCoefficient/global/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetGlobalClusteringCoefficientOfLightningNetworkByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets the density of the Lightning Network by timestamp.
    // /// The density is defined as the ratio of the number of edges (E) to the maximum possible number of edges (M) in a graph.
    // /// In the case of the Lightning Network - an undirected *multi* graph - allowing parallel edges this metric should *not* be taken too seriously.
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="double"/></returns>
    // [HttpGet("density/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetDensityOfLightningNetworkByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets <see cref="NetworkMetrics"/> of the Lightning Network by timestamp.
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="double"/></returns>
    // [HttpGet("metrics/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetNetworkMetricsOfLightningNetworkByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Gets number of bridges in the Lightning Network by timestamp.
    // /// A bridge is defined as an edge, that if removed the graph is split into two connected components.
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="int"/></returns>
    // [HttpGet("bridges/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetNumberOfBridgesInLightningNetworkByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Calculates the centrality of the Lightning Network analytically 
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="paymentSizeSat"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="int"/></returns>
    // [HttpGet("centrality/analytical/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetCentralityAnalyticallyOfLightningNetworkByTimestamp(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }
    //
    // /// <summary>
    // /// Calculates the centrality of the Lightning Network empirically using a Monte-Carlo-Simulation  
    // /// </summary>
    // /// <param name="timestamp">timestamp in ISO 8601 format</param>
    // /// <param name="paymentSizeSat"></param>
    // /// <param name="cancellationToken"></param>
    // /// <returns><see cref="int"/></returns>
    // [HttpGet("centrality/empirical/{timestamp}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<ActionResult<double>> GetCentralityEmpiricallyOfLightningNetworkByTimestamp(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken)
    // {
    //     throw new NotImplementedException();
    // }

    /// <summary>
    /// Get all raw_gossip necessary to construct the network topology of the Lightning Networks at a specific timestamp
    /// </summary>
    /// <remarks>
    /// raw_gossip is bytes, take a look at client libraries to parse them into something useful.
    /// </remarks>
    /// <param name="timestamp"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Successfully got raw_gossip</response>
    /// <response code="400">Invalid parameters provided</response>
    /// <response code="500">Internal server error when getting raw_gossip data</response>
    [HttpGet("snapshot/{timestamp}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSnapshotByTimestamp(
        DateTime timestamp,
        CancellationToken cancellationToken)
    {
        try
        {
            var bytes = await _networkSnapshotDataStore.GetSnapshotAsync(timestamp, cancellationToken: cancellationToken);
            
            var fileName = $"ln_snapshot_{timestamp:yyyyMMdd_HHmmss}.bin";
            if (bytes == null || bytes.Length == 0)
                return NotFound("No snapshot data available for the given timestamp.");

            return File(bytes, "application/octet-stream", fileName);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid parameters provided",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal server error during export",
                Detail = ex.Message,
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }


    
}