using LightningGraph.Core;
using LN_history.Cache.Services;
using LN_history.Core.Helper;

namespace LN_history.Core.Services;

public class ImportLightningNetworkService : IImportLightningNetworkService
{
    private readonly ICacheService _cacheService;

    public ImportLightningNetworkService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<LightningFastGraph?> ImportLightningNetworkByTimestamp(DateTime timestamp, CancellationToken cancellationToken)
    {
        var objectName = HelperFunctions.GetFileNameByTimestamp(timestamp, "json");
        
        var isGraphExisting = await _cacheService.CheckIfObjectExists("lightning-fast-graphs", objectName, cancellationToken);

        if (isGraphExisting)
        {
            return await _cacheService.GetGraphAsync("lightning-fast-graphs", objectName, cancellationToken);
        }

        return null;
    }
}