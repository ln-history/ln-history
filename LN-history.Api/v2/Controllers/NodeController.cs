using System.Diagnostics;
using Asp.Versioning;
using LN_history.Data.DataStores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LN_history.Api.v2.Controllers;

[ApiController]
[ApiVersion(2)]
[Route("ln-history/v{v:apiVersion}/[controller]")]
public class NodeController : ControllerBase
{
    private readonly ILogger<NodeController> _logger;
    private readonly IGossipDataStore _gossipDataStore;

    public NodeController(ILogger<NodeController> logger, IGossipDataStore gossipDataStore)
    {
        _logger = logger;
        _gossipDataStore = gossipDataStore;
    }
    
    [HttpGet("node/{nodeId}/{timestamp}/stream")]
    public IActionResult GetNodeInformation(string nodeId, DateTime timestamp, CancellationToken cancellationToken)
    {
        var stopwatchQuery = Stopwatch.StartNew();
        var memoryStream = _gossipDataStore.GetNodeGossipByNodeId(nodeId, timestamp);
        stopwatchQuery.Stop();
        _logger.LogInformation("Datastore query for node {NodeId} took {ElapsedMilliseconds}ms", nodeId, stopwatchQuery.ElapsedMilliseconds);

        var fileName = $"node_information_{nodeId}-to-{timestamp:yyyyMMdd_HHmmss}.bin";
        var tempPath = Path.GetTempFileName();

        var stopwatchFile = Stopwatch.StartNew();
        using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
        {
            memoryStream.CopyTo(fileStream);
        }

        var fileBytes = System.IO.File.ReadAllBytes(tempPath);
        stopwatchFile.Stop();
        _logger.LogInformation("File creation and read for node {NodeId} took {ElapsedMilliseconds}ms", nodeId, stopwatchFile.ElapsedMilliseconds);

        return File(fileBytes, "application/octet-stream", fileName);
    }

}