using LightningGraph.Core;
using LightningGraph.Serialization;
using LN_history.Cache.Services;
using LN_history.Core.Helper;
using Microsoft.Extensions.Logging;

namespace LN_history.Core.Services;

public class ImportLightningNetworkService : IImportLightningNetworkService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<ImportLightningNetworkService> _logger;

    public ImportLightningNetworkService(ICacheService cacheService, ILogger<ImportLightningNetworkService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<LightningFastGraph?> ImportLightningNetworkByTimestamp(string bucketName, DateTime timestamp, CancellationToken cancellationToken)
    {
        var objectName = HelperFunctions.GetFileNameByTimestamp(timestamp, "bin");
        
        var isGraphExisting = await _cacheService.CheckIfObjectExists(bucketName, objectName, cancellationToken);

        if (!isGraphExisting) return null;
        
        _logger.LogInformation($"Found lightning network at {timestamp} in cache.");
        
        var deserializedLightningNetwork =  await _cacheService.GetGraphTopologyUsingRpcAsync(bucketName, objectName, cancellationToken);

        if (deserializedLightningNetwork != null)
        {
            return LightningFastGraphTopologySerializerService.DeserializeTopology(deserializedLightningNetwork);
        }

        return null;
    }
}