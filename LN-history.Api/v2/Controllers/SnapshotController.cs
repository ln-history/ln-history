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
public class SnapshotController : ControllerBase
{
    private readonly ILogger<SnapshotController> _logger;
    private readonly IGossipDataStore _gossipDataStore;

    public  SnapshotController(ILogger<SnapshotController> logger, IGossipDataStore gossipDataStore)
    {
        _logger = logger;
        _gossipDataStore = gossipDataStore;
    }

    [HttpGet("snapshot/{timestamp}/joins/stream")]
    public IActionResult GetSnapshotStreamViaJoins(DateTime timestamp, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Starting GetSnapshotStream for timestamp: {Timestamp}", timestamp);
        
        try
        {
            // Measure datastore query time
            var datastoreStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var memoryStream = _gossipDataStore.GetGossipSnapshotByTimestampJoins(timestamp);
            datastoreStopwatch.Stop();
            _logger.LogInformation("Datastore query (Joins) completed in {ElapsedMs}ms, returned {StreamLength} bytes", 
                datastoreStopwatch.ElapsedMilliseconds, memoryStream?.Length ?? 0);

            var fileName = $"ln_snapshot_{timestamp:yyyyMMdd_HHmmss}.bin";
            var tempPath = Path.GetTempFileName();

            // Measure file writing time
            var fileWriteStopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                memoryStream!.CopyTo(fileStream);
            }
            fileWriteStopwatch.Stop();
            _logger.LogInformation("File write to temp location completed in {ElapsedMs}ms", fileWriteStopwatch.ElapsedMilliseconds);

            // Measure file read time
            var fileReadStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var fileBytes = System.IO.File.ReadAllBytes(tempPath);
            fileReadStopwatch.Stop();
            _logger.LogInformation("File read from temp location completed in {ElapsedMs}ms, read {FileSize} bytes", 
                fileReadStopwatch.ElapsedMilliseconds, fileBytes.Length);

            stopwatch.Stop();
            _logger.LogInformation("GetSnapshotStream completed successfully in {TotalElapsedMs}ms. Breakdown - Datastore: {DatastoreMs}ms, FileWrite: {FileWriteMs}ms, FileRead: {FileReadMs}ms",
                stopwatch.ElapsedMilliseconds, datastoreStopwatch.ElapsedMilliseconds, fileWriteStopwatch.ElapsedMilliseconds, fileReadStopwatch.ElapsedMilliseconds);

            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetSnapshotStream failed after {ElapsedMs}ms for timestamp: {Timestamp}", 
                stopwatch.ElapsedMilliseconds, timestamp);
            throw;
        }
    }
    
    [HttpGet("snapshot/{timestamp}/joins/stream/compressed")]
    public IActionResult GetSnapshotStreamCompressed(DateTime timestamp, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Starting GetSnapshotStreamCompressed for timestamp: {Timestamp}", timestamp);

        try
        {
            // Measure datastore query time
            var datastoreStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var memoryStream = _gossipDataStore.GetGossipSnapshotByTimestampJoins(timestamp);
            datastoreStopwatch.Stop();
            _logger.LogInformation("Datastore query (Join, compressed) completed in {ElapsedMs}ms, returned {StreamLength} bytes", 
                datastoreStopwatch.ElapsedMilliseconds, memoryStream?.Length ?? 0);
        
            var fileName = $"ln_snapshot_{timestamp:yyyyMMdd_HHmmss}.gz";
    
            // Measure file writing time
            var fileWriteStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var compressedStream = new MemoryStream();
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, leaveOpen: true))
            {
                memoryStream.Position = 0;
                memoryStream.CopyTo(gzipStream);
            }
            fileWriteStopwatch.Stop();
            _logger.LogInformation("File write to compressed file completed in {ElapsedMs}ms", fileWriteStopwatch.ElapsedMilliseconds);
        
            _logger.LogInformation("GetSnapshotStreamViaJoins completed successfully in {TotalElapsedMs}ms. Breakdown - Datastore: {DatastoreMs}ms, FileWrite: {FileWriteMs}ms",
                stopwatch.ElapsedMilliseconds, datastoreStopwatch.ElapsedMilliseconds, fileWriteStopwatch.ElapsedMilliseconds);
            return File(compressedStream.ToArray(), "application/gzip", fileName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetSnapshotStreamViaCuts failed after {ElapsedMs}ms for timestamp: {Timestamp}", 
                stopwatch.ElapsedMilliseconds, timestamp);
            throw;
        }
    }
    
    [HttpGet("snapshot/{timestamp}/joins/stream/optimizedCompression")]
    public IActionResult GetSnapshotStreamOptimizedCompression(DateTime timestamp, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Starting GetSnapshotStreamOptimizedCompression for timestamp: {Timestamp}", timestamp);
        
        try
        {
            // Measure datastore query time
            var datastoreStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var memoryStream = _gossipDataStore.GetGossipSnapshotByTimestampJoins(timestamp);
            datastoreStopwatch.Stop();
            _logger.LogInformation("Datastore query (Join, optimizedCompression) completed in {ElapsedMs}ms, returned {StreamLength} bytes", 
                datastoreStopwatch.ElapsedMilliseconds, memoryStream?.Length ?? 0);
            
            var fileName = $"ln_snapshot_{timestamp:yyyyMMdd_HHmmss}.gz";
            memoryStream.Position = 0;
            
            // Measure file writing time
            var fileWriteStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var compressedFile = new FileCallbackResult("application/gzip", fileName, async (outputStream, _) =>
            {
                await using var gzipStream = new GZipStream(outputStream, CompressionMode.Compress);
                await memoryStream.CopyToAsync(gzipStream, cancellationToken);
            });
            fileWriteStopwatch.Stop();
            _logger.LogInformation("File write to compressed file completed in {ElapsedMs}ms", fileWriteStopwatch.ElapsedMilliseconds);
            
            _logger.LogInformation("GetSnapshotStreamOptimizedCompression completed successfully in {TotalElapsedMs}ms. Breakdown - Datastore: {DatastoreMs}ms, FileWrite: {FileWriteMs}ms",
                stopwatch.ElapsedMilliseconds, datastoreStopwatch.ElapsedMilliseconds, fileWriteStopwatch.ElapsedMilliseconds);

            return compressedFile;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetSnapshotStreamOptimizedCompression failed after {ElapsedMs}ms for timestamp: {Timestamp}", 
                stopwatch.ElapsedMilliseconds, timestamp);
            throw;
        }
    }
    
    [HttpGet("snapshot/{timestamp}/cuts/stream")]
    public IActionResult GetSnapshotStreamViaCuts(DateTime timestamp, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Starting GetSnapshotStreamViaCuts for timestamp: {Timestamp}", timestamp);
        
        try
        {
            // Measure datastore query time
            var datastoreStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var memoryStream = _gossipDataStore.GetGossipSnapshotByTimestampCuts(timestamp);
            datastoreStopwatch.Stop();
            _logger.LogInformation("Datastore query (Cuts) completed in {ElapsedMs}ms, returned {StreamLength} bytes", 
                datastoreStopwatch.ElapsedMilliseconds, memoryStream?.Length ?? 0);

            var fileName = $"ln_snapshot_{timestamp:yyyyMMdd_HHmmss}.bin";
            var tempPath = Path.GetTempFileName();

            // Measure file writing time
            var fileWriteStopwatch = System.Diagnostics.Stopwatch.StartNew();
            using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                memoryStream!.CopyTo(fileStream);
            }
            fileWriteStopwatch.Stop();
            _logger.LogInformation("File write to temp location completed in {ElapsedMs}ms", fileWriteStopwatch.ElapsedMilliseconds);

            // Measure file read time
            var fileReadStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var fileBytes = System.IO.File.ReadAllBytes(tempPath);
            fileReadStopwatch.Stop();
            _logger.LogInformation("File read from temp location completed in {ElapsedMs}ms, read {FileSize} bytes", 
                fileReadStopwatch.ElapsedMilliseconds, fileBytes.Length);

            stopwatch.Stop();
            _logger.LogInformation("GetSnapshotStreamViaCuts completed successfully in {TotalElapsedMs}ms. Breakdown - Datastore: {DatastoreMs}ms, FileWrite: {FileWriteMs}ms, FileRead: {FileReadMs}ms",
                stopwatch.ElapsedMilliseconds, datastoreStopwatch.ElapsedMilliseconds, fileWriteStopwatch.ElapsedMilliseconds, fileReadStopwatch.ElapsedMilliseconds);

            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetSnapshotStreamViaCuts failed after {ElapsedMs}ms for timestamp: {Timestamp}", 
                stopwatch.ElapsedMilliseconds, timestamp);
            throw;
        }
    }
    
    [HttpGet("snapshot/{timestamp}/cuts/stream/compressed")]
    public IActionResult GetSnapshotStreamCompressedViaCuts(DateTime timestamp, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Starting GetSnapshotStreamCompressedViaCuts for timestamp: {Timestamp}", timestamp);

        try
        {
            // Measure datastore query time
            var datastoreStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var memoryStream = _gossipDataStore.GetGossipSnapshotByTimestampCuts(timestamp);
            datastoreStopwatch.Stop();
            _logger.LogInformation("Datastore query (Cuts, compressed) completed in {ElapsedMs}ms, returned {StreamLength} bytes", 
                datastoreStopwatch.ElapsedMilliseconds, memoryStream?.Length ?? 0);

            var fileName = $"ln_snapshot_{timestamp:yyyyMMdd_HHmmss}.gz";

            // Measure file writing time
            var fileWriteStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var compressedStream = new MemoryStream();
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress, leaveOpen: true))
            {
                memoryStream.Position = 0;
                memoryStream.CopyTo(gzipStream);
            }
            fileWriteStopwatch.Stop();
            stopwatch.Stop();

            _logger.LogInformation("File write to compressed file completed in {ElapsedMs}ms", fileWriteStopwatch.ElapsedMilliseconds);
            _logger.LogInformation("GetSnapshotStreamCompressedViaCuts completed in {TotalElapsedMs}ms. Breakdown - Datastore: {DatastoreMs}ms, FileWrite: {FileWriteMs}ms",
                stopwatch.ElapsedMilliseconds, datastoreStopwatch.ElapsedMilliseconds, fileWriteStopwatch.ElapsedMilliseconds);

            return File(compressedStream.ToArray(), "application/gzip", fileName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetSnapshotStreamCompressedViaCuts failed after {ElapsedMs}ms for timestamp: {Timestamp}",
                stopwatch.ElapsedMilliseconds, timestamp);
            throw;
        }
    }

    
    [HttpGet("snapshot/{timestamp}/cuts/stream/optimizedCompression")]
    public IActionResult GetSnapshotStreamOptimizedCompressionViaCuts(DateTime timestamp, CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _logger.LogInformation("Starting GetSnapshotStreamOptimizedCompressionViaCuts for timestamp: {Timestamp}", timestamp);

        try
        {
            // Measure datastore query time
            var datastoreStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var memoryStream = _gossipDataStore.GetGossipSnapshotByTimestampCuts(timestamp);
            datastoreStopwatch.Stop();
            _logger.LogInformation("Datastore query (Cuts, optimizedCompression) completed in {ElapsedMs}ms, returned {StreamLength} bytes", 
                datastoreStopwatch.ElapsedMilliseconds, memoryStream?.Length ?? 0);

            var fileName = $"ln_snapshot_{timestamp:yyyyMMdd_HHmmss}.gz";
            memoryStream.Position = 0;

            // Measure file writing setup time
            var fileWriteStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var compressedFile = new FileCallbackResult("application/gzip", fileName, async (outputStream, _) =>
            {
                await using var gzipStream = new GZipStream(outputStream, CompressionMode.Compress);
                await memoryStream.CopyToAsync(gzipStream, cancellationToken);
            });
            fileWriteStopwatch.Stop();
            stopwatch.Stop();

            _logger.LogInformation("File write to compressed file (callback setup) completed in {ElapsedMs}ms", fileWriteStopwatch.ElapsedMilliseconds);
            _logger.LogInformation("GetSnapshotStreamOptimizedCompressionViaCuts completed in {TotalElapsedMs}ms. Breakdown - Datastore: {DatastoreMs}ms, FileWriteSetup: {FileWriteMs}ms",
                stopwatch.ElapsedMilliseconds, datastoreStopwatch.ElapsedMilliseconds, fileWriteStopwatch.ElapsedMilliseconds);

            return compressedFile;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "GetSnapshotStreamOptimizedCompressionViaCuts failed after {ElapsedMs}ms for timestamp: {Timestamp}",
                stopwatch.ElapsedMilliseconds, timestamp);
            throw;
        }
    }
}