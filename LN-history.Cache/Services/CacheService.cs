using System.Text;
using LightningGraph.Core;
using LightningGraph.Serialization;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace LN_history.Cache.Services;

public class CacheService : ICacheService
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IMinioClient minioClient, ILogger<CacheService> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
    }

    public async Task<bool> CheckIfObjectExists(string bucketName, string objectName, CancellationToken cancellationToken)
    {
        try
        {
            var statBucketArgs = new BucketExistsArgs()
                .WithBucket(bucketName);
            
            // Check if the bucket exists
            var isBucketExisting = await _minioClient.BucketExistsAsync(statBucketArgs, cancellationToken);

            if (!isBucketExisting)
            {
                _logger.LogWarning($"Bucket {bucketName} does not exist.");
                return false; // Bucket does not exist
            }

            // Check if the object exists in the bucket
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName);

                await _minioClient.StatObjectAsync(statObjectArgs, cancellationToken);

                return true; // Object exists
            }
            catch (ObjectNotFoundException)
            {
                return false; // Object does not exist
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Check if object exists failed for bucketName {bucketName}, objectName {objectName}");
            return false; // Return false in case of any unexpected error
        }
    }

    public async Task<LightningFastGraph> GetGraphAsync(string bucketId, string objectName, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Start retrieving graph from bucket {BucketId}, object {ObjectName}", bucketId, objectName); 
            
            var graphJson = new StringBuilder();
            
            GetObjectArgs getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketId)
                .WithObject(objectName)
                .WithCallbackStream((stream) =>
                {
                    using var reader = new StreamReader(stream);
                    _logger.LogInformation("Reading stream for object {ObjectName}", objectName);

                    var content = reader.ReadToEnd();
                    graphJson.Append(content);
                    
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        _logger.LogWarning("The object content is empty. Cannot deserialize an empty graph.");
                        throw new Exception("Invalid reading from MinIO bucket.");
                    }

                    _logger.LogInformation("Stream read successfully. Content length: {ContentLength}", content.Length);
                });
            await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);
            
            _logger.LogInformation("Object {ObjectName} retrieved successfully from bucket {BucketId}", objectName, bucketId);
            
            // Deserialize the graph from JSON
            var deserializedGraph = LightningFastGraphDeserializationService.Deserialize(graphJson.ToString());
            _logger.LogInformation("Graph deserialized successfully for object {ObjectName}", objectName);

            return deserializedGraph;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving graph. BucketId: {BucketId}, ObjectName: {ObjectName}", bucketId, objectName);
            throw;
        }
    }

    
    public async Task StoreGraphAsync(string bucketId, string objectName, LightningFastGraph graph, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Storing graph in bucket {bucketId} as object: {objectName}");

            // Serialize graph and Convert to byte array
            var graphBytes = Encoding.UTF8.GetBytes(graph.SerializeToJson(objectName));
            using var stream = new MemoryStream(graphBytes);

            // Store object in MinIO
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketId)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(graphBytes.Length)
                .WithContentType("application/json");

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            _logger.LogInformation($"Graph stored successfully in bucket {bucketId}, object: {objectName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing graph. BucketId: {BucketId}, ObjectName: {ObjectName}", 
                bucketId, objectName);
            throw;
        }
    }
}
