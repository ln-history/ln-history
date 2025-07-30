using System.Diagnostics;
using System.IO.Compression;
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
public class SnapshotDiffController : ControllerBase
{
    private readonly ILogger<SnapshotDiffController> _logger;
    
    private readonly IGossipDataStore _gossipDataStore;

    public SnapshotDiffController(ILogger<SnapshotDiffController> logger, IGossipDataStore gossipDataStore)
    {
        _logger = logger;
        _gossipDataStore = gossipDataStore;
    }
    
    [HttpGet("snapshot-diff/{startTimestamp}/{endTimestamp}/stream")]
    public IActionResult GetSnapshotDifferenceStream(DateTime startTimestamp, DateTime endTimestamp, CancellationToken cancellationToken)
    {
        var stopwatchQuery = Stopwatch.StartNew();
        var memoryStream = _gossipDataStore.GetGossipSnapshotDifferenceByTimestamps(startTimestamp, endTimestamp);
        stopwatchQuery.Stop();
        _logger.LogInformation("Snapshot diff query time: {ElapsedMilliseconds} ms", stopwatchQuery.ElapsedMilliseconds);

        var fileName = $"ln_snapshot-diff_{startTimestamp:yyyyMMdd_HHmmss}-to-{endTimestamp:yyyyMMdd_HHmmss}.bin";
        var tempPath = Path.GetTempFileName();

        var stopwatchFile = Stopwatch.StartNew();
        using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
        {
            memoryStream.CopyTo(fileStream);
        }

        var fileBytes = System.IO.File.ReadAllBytes(tempPath);
        stopwatchFile.Stop();
        _logger.LogInformation("Snapshot diff file creation time: {ElapsedMilliseconds} ms", stopwatchFile.ElapsedMilliseconds);

        return File(fileBytes, "application/octet-stream", fileName);
    }

    
    [HttpGet("snapshot-diff/{startTimestamp}/{endTimestamp}/stream/compressed")]
    public IActionResult GetSnapshotDifferenceStreamCompressed(DateTime startTimestamp, DateTime endTimestamp, CancellationToken cancellationToken)
    {
        var stopwatchQuery = Stopwatch.StartNew();
        var memoryStream = _gossipDataStore.GetGossipSnapshotDifferenceByTimestamps(startTimestamp, endTimestamp);
        stopwatchQuery.Stop();
        _logger.LogInformation("Snapshot diff query time: {ElapsedMilliseconds} ms", stopwatchQuery.ElapsedMilliseconds);

        var fileName = $"ln_snapshot-diff_{startTimestamp:yyyyMMdd_HHmmss}-to-{endTimestamp:yyyyMMdd_HHmmss}.gz";
        var compressedStream = new MemoryStream();

        var stopwatchCompression = Stopwatch.StartNew();
        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, leaveOpen: true))
        {
            memoryStream.Position = 0;
            memoryStream.CopyTo(gzipStream);
        }
        compressedStream.Position = 0;
        stopwatchCompression.Stop();
        _logger.LogInformation("Snapshot diff compression time: {ElapsedMilliseconds} ms", stopwatchCompression.ElapsedMilliseconds);

        return File(compressedStream.ToArray(), "application/gzip", fileName);
    }

    
    [HttpGet("snapshot-diff/{startTimestamp}/{endTimestamp}/stream/optimizedCompression")]
    public IActionResult GetSnapshotDifferenceStreamOptimizedCompression(DateTime startTimestamp, DateTime endTimestamp, CancellationToken cancellationToken)
    {
        var stopwatchQuery = Stopwatch.StartNew();
        var memoryStream = _gossipDataStore.GetGossipSnapshotDifferenceByTimestamps(startTimestamp, endTimestamp);
        stopwatchQuery.Stop();
        _logger.LogInformation("Snapshot diff query time: {ElapsedMilliseconds} ms", stopwatchQuery.ElapsedMilliseconds);

        var fileName = $"ln_snapshot-diff_{startTimestamp:yyyyMMdd_HHmmss}-to-{endTimestamp:yyyyMMdd_HHmmss}.gz";
        memoryStream.Position = 0;

        return new FileCallbackResult("application/gzip", fileName, async (outputStream, _) =>
        {
            var stopwatchCompression = Stopwatch.StartNew();
            await using var gzipStream = new GZipStream(outputStream, CompressionMode.Compress);
            await memoryStream.CopyToAsync(gzipStream, cancellationToken);
            stopwatchCompression.Stop();
            _logger.LogInformation("Snapshot diff optimized compression time: {ElapsedMilliseconds} ms", stopwatchCompression.ElapsedMilliseconds);
        });
    }
}