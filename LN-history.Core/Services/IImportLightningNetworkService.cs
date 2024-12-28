using LightningGraph.Core;

namespace LN_history.Core.Services;

public interface IImportLightningNetworkService
{
    Task<LightningFastGraph?>
        ImportLightningNetworkByTimestamp(DateTime timestamp, CancellationToken cancellationToken);
}