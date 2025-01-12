using LightningGraph.Core;

namespace LN_history.Core.Services;

public interface ILightningNetworkService
{
    Task<LightningFastGraph> GetLightningNetworkAsync(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken = default);
    
    Task<int> GetNodeCountByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken);
    
    Task<int> GetEdgeCountByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken);

    Task<int> GetNumberOfBridgesByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken);

    Task<LightningFastGraph> ConstructLightningFastGraphByTimestampAsync(DateTime timestamp,
        TimeSpan timespan, int paymentSizeSat, CancellationToken cancellationToken);

    Task ExportLightningNetworkByTimestampAsync(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken);

    Task ExportLightningNodeInformationAsync(DateTime timestamp, LightningFastGraph lightningNetwork,
        CancellationToken cancellationToken);

    Task ExportLightningChannelInformationAsync(DateTime timestamp, LightningFastGraph lightningNetwork,
        CancellationToken cancellationToken);
    
    Task<double> GetCentralityAnalyticallyByTimestampAsync(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken);

    Task<double> GetCentralityEmpiricallyByTimestampAsync(DateTime timestamp, int paymentSizeSat,
        CancellationToken cancellationToken);
}