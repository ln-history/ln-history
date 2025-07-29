using Asp.Versioning;
using LN_history.Api.Authorization;
using LN_history.Data.DataStores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LN_history.Api.v2.Controllers;

[ApiController]
[ApiVersion(2)]
[Route("ln-history/v{v:apiVersion}/[controller]")]
[ApiKeyAuthorize]
public class GossipController : ControllerBase
{
    private readonly ILogger<GossipController> _logger;
    
    private readonly IGossipDataStore _gossipDataStore;

    public  GossipController(ILogger<GossipController> logger, IGossipDataStore gossipDataStore)
    {
        _logger = logger;
        
        _gossipDataStore = gossipDataStore;
    }
    
    [HttpGet("snapshot/{timestamp}/stream")]
    public IActionResult GetSnapshotStream(DateTime timestamp, CancellationToken cancellationToken)
    {
     var memoryStream = _gossipDataStore.GetGossipSnapshotByTimestamp(timestamp);
     var fileName = $"ln_snapshot_{timestamp:yyyyMMdd_HHmmss}.bin";
     var tempPath = Path.GetTempFileName();

     using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
     {
         memoryStream.CopyTo(fileStream);
     }

     var fileBytes = System.IO.File.ReadAllBytes(tempPath);
     return File(fileBytes, "application/octet-stream", fileName);
    }
    
    [HttpGet("snapshot-diff/{timestamp}/stream")]
    public IActionResult GetSnapshotDifferenceStream(DateTime startTimestamp, DateTime endTimestamp, CancellationToken cancellationToken)
    {
        var memoryStream = _gossipDataStore.GetGossipSnapshotDifferenceByTimestamps(startTimestamp, endTimestamp);
        var fileName = $"ln_snapshot-diff_{startTimestamp:yyyyMMdd_HHmmss}-to-{endTimestamp:yyyyMMdd_HHmmss}.bin";
        var tempPath = Path.GetTempFileName();

        using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
        {
            memoryStream.CopyTo(fileStream);
        }

        var fileBytes = System.IO.File.ReadAllBytes(tempPath);
        return File(fileBytes, "application/octet-stream", fileName);
    }
    
    [HttpGet("node/{nodeId}/{timestamp}/stream")]
    public IActionResult GetNodeInformation(string nodeId, DateTime timestamp, CancellationToken cancellationToken)
    {
        var memoryStream = _gossipDataStore.GetNodeGossipByNodeId(nodeId, timestamp);
        var fileName = $"node_information_{nodeId}-to-{timestamp:yyyyMMdd_HHmmss}.bin";
        var tempPath = Path.GetTempFileName();

        using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
        {
            memoryStream.CopyTo(fileStream);
        }

        var fileBytes = System.IO.File.ReadAllBytes(tempPath);
        return File(fileBytes, "application/octet-stream", fileName);
    }
    
    [HttpGet("scid/{scid}/{timestamp}/stream")]
    public IActionResult GetChannelInformation(string scid, DateTime timestamp, CancellationToken cancellationToken)
    {
        var memoryStream = _gossipDataStore.GetChannelGossipByScid(scid, timestamp);
        var fileName = $"channel_information_{scid}-to-{timestamp:yyyyMMdd_HHmmss}.bin";
        var tempPath = Path.GetTempFileName();

        using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
        {
            memoryStream.CopyTo(fileStream);
        }

        var fileBytes = System.IO.File.ReadAllBytes(tempPath);
        return File(fileBytes, "application/octet-stream", fileName);
    }
}