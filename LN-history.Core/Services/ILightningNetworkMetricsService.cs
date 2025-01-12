using LightningGraph.Model;

namespace LN_history.Core.Services;

public interface ILightningNetworkMetricsService
{
    Task<int> GetDiameterByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken);
    Task<double> GetAveragePathLengthByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken);
    Task<double> GetAverageDegreeByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken);
    Task<double> GetAverageLocalClusteringCoefficientByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken);
    Task<double> GetGlobalClusteringCoefficientByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken);
    Task<double> GetDensityByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken);

    Task<NetworkMetrics> GetNetworkMetricsByTimestampAsync(
        DateTime timestamp,
        CancellationToken cancellationToken);
}