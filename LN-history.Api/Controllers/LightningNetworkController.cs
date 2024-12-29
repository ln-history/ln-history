using AutoMapper;
using LN_history.Api.Authorization;
using LN_history.Api.Model;
using LN_history.Cache.Services;
using LN_history.Core.Helper;
using LN_history.Core.Services;
using LN_History.Model.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LN_history.Api.Controllers;

[ApiController]
[Route("ln-history/[controller]")]
[ApiKeyAuthorize]
public class LightningNetworkController : ControllerBase
{
    private readonly ILightningNetworkService _lightningNetworkService;
    private readonly ILogger<LightningNetworkController> _logger;
    private readonly LightningSettings _settings;
    

    public LightningNetworkController(ILightningNetworkService lightningNetworkService, ILogger<LightningNetworkController> logger, IOptions<LightningSettings> options)
    {
        _lightningNetworkService = lightningNetworkService;
        _logger = logger;
        _settings = options.Value;
    }

    /// <summary>
    /// Gets number of nodes in the Lightning Network by timestamp
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="int"/></returns>
    [HttpGet("nodes/count/fast/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetNodeCountByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    {
        var result =  await _lightningNetworkService.GetNodeCountByTimestampAsync(timestamp, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets number of channels in the Lightning Network by timestamp
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="int"/></returns>
    [HttpGet("edgeCount/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetEdgeCountByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    {
        var result =  await _lightningNetworkService.GetEdgeCountByTimestampAsync(timestamp, cancellationToken);
        return Ok(result);
    }

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
    //     var results = await _lightningNetworkSimulationService.SimulatePaymentsByPaymentSizeAndTimestampWithMonteCarlo(paymentSizeSat, timestamp, cancellationToken);
    //     var result = HelperFunctions.CalculateMedian(results.ToList());
    //     
    //     return Ok(result);
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
    //     var results = await _lightningNetworkSimulationService.SimulatePaymentsByPaymentSizeAndTimestampWithMonteCarlo(paymentSizeSat, timestamp, cancellationToken);
    //     var result = HelperFunctions.CalculateAverage(results.ToList());
    //     
    //     return Ok(result);
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
    //     var results = await _lightningNetworkAnalyticalService.CalculateAllShortestPathCostsByPaymentSizeAndTimestamp(paymentSizeSat, timestamp, cancellationToken);
    //     var result = HelperFunctions.CalculateMedian(results.ToList());
    //     
    //     return Ok(result);
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
    //     var results = await _lightningNetworkAnalyticalService.CalculateAllShortestPathCostsByPaymentSizeAndTimestamp(paymentSizeSat, timestamp, cancellationToken);
    //     var result = HelperFunctions.CalculateAverage(results.ToList());
    //     
    //     return Ok(result);
    // }
    
    /// <summary>
    /// Gets the diameter of the Lightning Network by timestamp.
    /// The diameter is defined as the longest shortest path of a graph
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="int"/></returns>
    [HttpGet("diameter/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<double>> GetDiameterOfLightningNetworkByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    {
        var result = await _lightningNetworkService.GetDiameterByTimestampAsync(timestamp, cancellationToken);
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets the average shortest path length of the Lightning Network by timestamp.
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="int"/></returns>
    [HttpGet("avgPathLength/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<double>> GetAveragePathLengthOfLightningNetworkByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    {
        var result = await _lightningNetworkService.GetAveragePathLengthByTimestampAsync(timestamp, cancellationToken);
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets the average degree of the nodes in the Lightning Network by timestamp.
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="int"/></returns>
    [HttpGet("avgDegree/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<double>> GetAverageDegreeOfLightningNetworkByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    {
        var result = await _lightningNetworkService.GetAverageDegreeByTimestampAsync(timestamp, cancellationToken);
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets the global clustering coefficient of the Lightning Network by timestamp.
    /// The global clustering coefficient is defined as the ratio of actual connections between a node’s neighbors to the maximum possible connections between them.
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="double"/></returns>
    [HttpGet("clusteringCOefficient/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<double>> GetGlobalClusteringCoefficientOfLightningNetworkByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var result = await _lightningNetworkService.GetGlobalClusteringCoefficientByTimestampAsync(timestamp, cancellationToken);

        return Ok(result);
    }
    
    /// <summary>
    /// Gets the density of the Lightning Network by timestamp.
    /// The density is defined as the ratio of the number of edges (E) to the maximum possible number of edges (M) in a graph.
    /// In the case of the Lightning Network - an undirected *multi* graph - allowing parallel edges this metric should *not* be taken too seriously.
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="double"/></returns>
    [HttpGet("density/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<double>> GetDensityOfLightningNetworkByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var result = await _lightningNetworkService.GetDensityByTimestampAsync(timestamp, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets number of bridges in the Lightning Network by timestamp.
    /// A bridge is defined as an edge, that if removed the graph is split into two connected components.
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="int"/></returns>
    [HttpGet("bridges/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<double>> GetNumberOfBridgesInLightningNetworkByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    {
        var result = await _lightningNetworkService.GetNumberOfBridgesByTimestampAsync(timestamp, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Calculates the centrality of the Lightning Network analytically 
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="paymentSizeSat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="int"/></returns>
    [HttpGet("centrality/analytical/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<double>> GetCentralityAnalyticallyOfLightningNetworkByTimestamp(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken)
    {
        var result = await _lightningNetworkService.GetCentralityAnalyticallyByTimestampAsync(timestamp, paymentSizeSat, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Calculates the centrality of the Lightning Network empirically using a Monte-Carlo-Simulation  
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="paymentSizeSat"></param>
    /// <param name="cancellationToken"></param>
    /// <returns><see cref="int"/></returns>
    [HttpGet("centrality/empirical/{timestamp}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<double>> GetCentralityEmpiricallyOfLightningNetworkByTimestamp(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken)
    {
        var result = await _lightningNetworkService.GetCentralityEmpiricallyByTimestampAsync(timestamp, paymentSizeSat, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Exports a snapshot of the Lightning Network graph at a specific timestamp
    /// </summary>
    /// <remarks>
    /// Creates a point-in-time snapshot of the Lightning Network, including:
    /// - Node information and attributes
    /// - Channel data and capacities
    /// - Routing fees calculated for the specified payment size
    /// 
    /// The snapshot is stored in MinIO with the key format: `ln-{timestamp}`
    /// </remarks>
    /// <param name="request">Export parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Graph exported successfully</response>
    /// <response code="400">Invalid parameters provided</response>
    /// <response code="500">Internal server error during export</response>
    [HttpPost("snapshots")]
    [ProducesResponseType(typeof(ExportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExportResponse>> ExportSnapshot(
        [FromBody] ExportSnapshotRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _lightningNetworkService.ExportLightningNetworkByTimestampAsync(
                request.Timestamp,
                request.PaymentSizeSat,
                cancellationToken);

            return Ok(new ExportResponse(
                Message: $"Lightning Network snapshot created for {request.Timestamp:O}",
                Timestamp: request.Timestamp));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid export parameters");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Parameters",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export Lightning Network snapshot");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ProblemDetails
                {
                    Title = "Export Failed",
                    Detail = "Failed to create Lightning Network snapshot",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("export/nodeInformation/{timestamp}")]
    [ProducesResponseType(typeof(ExportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExportResponse>> ExportNodeInformationByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        try
        {
            var lightningNetwork = await _lightningNetworkService.GetLightningNetworkAsync(timestamp, _settings.DefaultPaymentSizeSats, cancellationToken: cancellationToken);
            
            await _lightningNetworkService.ExportLightningNodeInformationAsync(timestamp, lightningNetwork, cancellationToken);
            
            return Ok(new ExportResponse(
                Message: $"Successfully exported NodeInformation of Lightning Network at {timestamp:O}",
                Timestamp: timestamp));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to export Lightning Network NodeInformation");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ProblemDetails
                {
                    Title = "Export Failed",
                    Detail = "Failed to export Lightning Network NodeInformation",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpPost("export/channelInformation/{timestamp}")]
    [ProducesResponseType(typeof(ExportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExportResponse>> ExportChannelInformationByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        try
        {
            var lightningNetwork = await _lightningNetworkService.GetLightningNetworkAsync(timestamp, paymentSizeSat: 10_000, cancellationToken: cancellationToken);
            
            await _lightningNetworkService.ExportLightningChannelInformationAsync(timestamp, lightningNetwork, cancellationToken);
            
            return Ok(new ExportResponse(
                Message: $"Successfully exported ChannelInformation of Lightning Network at {timestamp:O}",
                Timestamp: timestamp));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to export Lightning Network ChannelInformation");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ProblemDetails
                {
                    Title = "Export Failed",
                    Detail = "Failed to export Lightning Network NodeInformation",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }
}