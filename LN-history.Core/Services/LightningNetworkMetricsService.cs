using LightningGraph.Model;
using LN_History.Model.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LN_history.Core.Services;

public class LightningNetworkMetricsService : ILightningNetworkMetricsService
{
    private readonly ILightningNetworkService _lightningNetworkService;
    private readonly LightningSettings _settings;
    private readonly ILogger<LightningNetworkMetricsService> _logger;
    
    public LightningNetworkMetricsService(IOptions<LightningSettings> settings, ILogger<LightningNetworkMetricsService> logger, ILightningNetworkService lightningNetworkService)
    {
        _settings = settings.Value;
        _logger = logger;
        _lightningNetworkService = lightningNetworkService;
    }

    public async Task<int> GetDiameterByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await _lightningNetworkService.GetLightningNetworkAsync(
            timestamp, 
            _settings.DefaultPaymentSizeSats,
            cancellationToken);
        _logger.LogInformation($"Start analysis for NetworkMetrics.Diameter at timestamp {timestamp}");
        
        return lightningNetwork.GetDiameter();
    }
    
    public async Task<double> GetAveragePathLengthByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await _lightningNetworkService.GetLightningNetworkAsync(
            timestamp, 
            _settings.DefaultPaymentSizeSats,
            cancellationToken);
        _logger.LogInformation($"Start analysis for NetworkMetrics.AveragePathLength at timestamp {timestamp}");
        
        return lightningNetwork.GetAveragePathLength();
    }
    
    public async Task<double> GetAverageDegreeByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await _lightningNetworkService.GetLightningNetworkAsync(
            timestamp, 
            _settings.DefaultPaymentSizeSats,
            cancellationToken);
        _logger.LogInformation($"Start analysis for NetworkMetrics.AverageDegree at timestamp {timestamp}");
        
        return lightningNetwork.GetAverageDegree();
    }
    
    public async Task<double> GetAverageLocalClusteringCoefficientByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await _lightningNetworkService.GetLightningNetworkAsync(
            timestamp, 
            _settings.DefaultPaymentSizeSats,
            cancellationToken);
        _logger.LogInformation($"Start analysis for NetworkMetrics.AverageLocalClusteringCoefficient at timestamp {timestamp}");
        
        return lightningNetwork.GetAverageLocalClusteringCoefficient(); 
    }
    
    public async Task<double> GetGlobalClusteringCoefficientByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await _lightningNetworkService.GetLightningNetworkAsync(
            timestamp, 
            _settings.DefaultPaymentSizeSats,
            cancellationToken);
        _logger.LogInformation($"Start analysis for NetworkMetrics.GlobalClusteringCoefficient at timestamp {timestamp}");
        
        return lightningNetwork.GetGlobalClusteringCoefficient(); 
    }
    
    public async Task<double> GetDensityByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await _lightningNetworkService.GetLightningNetworkAsync(
            timestamp, 
            _settings.DefaultPaymentSizeSats,
            cancellationToken);
        _logger.LogInformation($"Start analysis for NetworkMetrics.Density at timestamp {timestamp}");
        
        return lightningNetwork.GetDensity();
    }
    
    public async Task<NetworkMetrics> GetNetworkMetricsByTimestampAsync(
        DateTime timestamp, 
        CancellationToken cancellationToken)
    {
        var lightningNetwork = await _lightningNetworkService.GetLightningNetworkAsync(
            timestamp, 
            _settings.DefaultPaymentSizeSats,
            cancellationToken);
        _logger.LogInformation($"Start analysis for NetworkMetrics at timestamp {timestamp}");
        
        var networkMetrics = lightningNetwork.GetNetworkMetrics();
        
        _logger.LogInformation($"Finish analysis for NetworkMetrics at timestamp {timestamp}");
        
        return networkMetrics;
    }
}