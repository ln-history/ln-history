using LightningGraph.Core;

namespace LN_history.Core.Services;

public interface IImportLightningNetworkService
{
    Task<LightningFastGraph?>
        ImportLightningNetworkByTimestamp(string bucketName, DateTime timestamp, CancellationToken cancellationToken);
}