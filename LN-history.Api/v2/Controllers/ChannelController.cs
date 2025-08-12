using System.Diagnostics;
using Asp.Versioning;
using LN_history.Data.DataStores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LN_history.Api.v2.Controllers;

[ApiController]
[ApiVersion(2)]
[Route("ln-history/v{v:apiVersion}/[controller]")]
public class ChannelController : ControllerBase
{
    private readonly ILogger<ChannelController> _logger;
    
    private readonly IGossipDataStore _gossipDataStore;

    public ChannelController(ILogger<ChannelController> logger, IGossipDataStore gossipDataStore)
    {
        _logger = logger;
        _gossipDataStore = gossipDataStore;
    }
    
    [HttpGet("scid/{scid}/{timestamp}/stream")]
    public IActionResult GetChannelInformation(string scid, DateTime timestamp, CancellationToken cancellationToken)
    {
        var stopwatchQuery = Stopwatch.StartNew();
        var memoryStream = _gossipDataStore.GetChannelGossipByScid(scid, timestamp);
        stopwatchQuery.Stop();
        _logger.LogInformation("Datastore query for SCID {Scid} took {ElapsedMilliseconds}ms", scid, stopwatchQuery.ElapsedMilliseconds);

        var fileName = $"channel_information_{scid}-to-{timestamp:yyyyMMdd_HHmmss}.bin";
        var tempPath = Path.GetTempFileName();

        var stopwatchFile = Stopwatch.StartNew();
        using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
        {
            memoryStream.CopyTo(fileStream);
        }

        var fileBytes = System.IO.File.ReadAllBytes(tempPath);
        stopwatchFile.Stop();
        _logger.LogInformation("File creation and read for SCID {Scid} took {ElapsedMilliseconds}ms", scid, stopwatchFile.ElapsedMilliseconds);

        return File(fileBytes, "application/octet-stream", fileName);
    }

}