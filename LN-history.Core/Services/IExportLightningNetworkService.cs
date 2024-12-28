using LightningGraph.Core;
using LN_history.Data.Model;

namespace LN_history.Core.Services;

public interface IExportLightningNetworkService
{
    Task ExportLightningNetworkCompleteAsync(DateTime timestamp, LightningFastGraph lightningNetwork,
        CancellationToken cancellationToken);
    
    Task ExportLightningNetworkTopologyByTimestampAsync(DateTime timestamp, LightningFastGraph lightningNetwork,
        CancellationToken cancellationToken);

    Task ExportLightningNodeInformationByTimestampAsync(DateTime timestamp, IEnumerable<NodeInformation> nodes,
        CancellationToken cancellationToken = default);
    
    Task ExportLightningChannelInformationByTimestampAsync(DateTime timestamp, IEnumerable<ChannelInformation> channels,
        CancellationToken cancellationToken = default);
}