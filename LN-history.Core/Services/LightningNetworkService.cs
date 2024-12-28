using AutoMapper;
using LightningGraph.Core;
using LightningGraph.Model;
using LN_history.Core.Helper;
using LN_history.Data.DataStores;
using LN_history.Data.Model;
using Microsoft.Extensions.Logging;

namespace LN_history.Core.Services;

public class LightningNetworkService : ILightningNetworkService
{
    private readonly IMapper _mapper;
    private readonly ILogger<ILightningNetworkService> _logger;
    private readonly INodeAnnouncementDataStore _nodeAnnouncementDataStore;
    private readonly ILightningNetworkDataStore _lightningNetworkDataStore;
    private readonly IImportLightningNetworkService _importLightningNetworkService;
    private readonly IExportLightningNetworkService _exportLightningNetworkService;
    
    public LightningNetworkService(IMapper mapper, INodeAnnouncementDataStore nodeAnnouncementDataStore, ILightningNetworkDataStore lightningNetworkDataStore, ILogger<ILightningNetworkService> logger, IExportLightningNetworkService exportLightningNetworkService, IImportLightningNetworkService importLightningNetworkService)
    {
        _mapper = mapper;
        _logger = logger;
        _importLightningNetworkService = importLightningNetworkService;
        _exportLightningNetworkService = exportLightningNetworkService;
        _nodeAnnouncementDataStore = nodeAnnouncementDataStore;
        _lightningNetworkDataStore = lightningNetworkDataStore;
    }

    public async Task<LightningFastGraph> GetLightningNetworkAsync(DateTime timestamp, int paymentSizeSat = 10_000, CancellationToken cancellationToken = default)
    {
        var lightningNetwork = await _importLightningNetworkService.ImportLightningNetworkByTimestamp(timestamp, cancellationToken);

        if (lightningNetwork == null)
        {
            _logger.LogInformation($"No topology of Lightning Network at timestamp {timestamp} found");
            lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(timestamp, HelperFunctions.DefaultTimespan, paymentSizeSat, cancellationToken);
            await _exportLightningNetworkService.ExportLightningNetworkCompleteAsync(timestamp, lightningNetwork, cancellationToken);
        }
        else
        {
            _logger.LogInformation($"Lightning network at timestamp {timestamp} with {lightningNetwork.NodeCount} nodes and {lightningNetwork.EdgeCount} channels found");
        }

        if (lightningNetwork.NodeCount == 0)
        {
            throw new Exception($"Lightning Network at timestamp {timestamp} found but errors were encountered during import.");
        }

        return lightningNetwork;       
    }

    public async Task<int> GetNodeCountByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await GetLightningNetworkAsync(timestamp, cancellationToken: cancellationToken);
        
        var result = lightningNetwork.NodeCount;

        return result;
    }

    public async Task<int> GetEdgeCountByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await GetLightningNetworkAsync(timestamp, cancellationToken: cancellationToken);
        
        var result = lightningNetwork.EdgeCount;

        return result;
    }

    public async Task<int> GetDiameterByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        return (await GetNetworkMetricsByTimestampAsync(timestamp, cancellationToken)).Diameter;
    }
    
    public async Task<double> GetAveragePathLengthByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        return (await GetNetworkMetricsByTimestampAsync(timestamp, cancellationToken)).AveragePathLength;
    }
    
    public async Task<double> GetAverageDegreeByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        return (await GetNetworkMetricsByTimestampAsync(timestamp, cancellationToken)).AverageDegree;
    }
    
    public async Task<double> GetGlobalClusteringCoefficientByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        return (await GetNetworkMetricsByTimestampAsync(timestamp, cancellationToken)).ClusteringCoefficient;
    }
    
    public async Task<double> GetDensityByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        return (await GetNetworkMetricsByTimestampAsync(timestamp, cancellationToken)).Density;
    }
    
    private async Task<NetworkMetrics> GetNetworkMetricsByTimestampAsync(
        DateTime timestamp, 
        CancellationToken cancellationToken)
    {
        var lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(
            timestamp, 
            HelperFunctions.DefaultTimespan, 
            10_000,
            cancellationToken);

        _logger.LogInformation($"Start analysis for NetworkMetrics at timestamp {timestamp}");
        
        // var analysis = lightningNetwork.AnalyzeNetwork();
        
        _logger.LogInformation($"Finish analysis for NetworkMetrics at timestamp {timestamp}");
        
        // return analysis;
        
        return new NetworkMetrics();
    }
    
    public async Task<int> GetNumberOfBridgesByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(
            timestamp, 
            HelperFunctions.DefaultTimespan, 
            10_000,
            cancellationToken);

        // var bridgeAnalysis = lightningNetwork.AnalyzeBridges();
        
        // return bridgeAnalysis.BridgeCount;

        return 1;
    }

    public async Task<LightningFastGraph> ConstructLightningFastGraphByTimestampAsync(DateTime timestamp,
        TimeSpan timespan, int paymentSizeSat, CancellationToken cancellationToken)
    {
        var startTime = timestamp - timespan;
        var endTime = timestamp;
    
        _logger.LogInformation($"Constructing FastLightningGraph for gossip messages between {startTime} and {endTime}");
        
        // Parallel fetch of data
        var nodeTask = _nodeAnnouncementDataStore.GetNodeAnnouncementsForTimespanAsync(startTime, endTime, cancellationToken);
        var channelTask = _lightningNetworkDataStore.GetChannelsByTimespanAsync(startTime, endTime, cancellationToken);
        
        await Task.WhenAll(nodeTask, channelTask);

        var nodesMessages = await nodeTask;
        var channelMessages = await channelTask;
        
        _logger.LogInformation($"Fetched {nodesMessages.Count} node announcements and {channelMessages.Count} channel messages.");

        var lightningNetworkGraph = new LightningFastGraph();
        
        // Create lookup set for announced nodes
        var announcedNodeIds = nodesMessages
            .Select(n => n.NodeId)
            .ToHashSet();
        
        lightningNetworkGraph.AddVerticesBatch(_mapper.Map<ICollection<NodeInformation>>(nodesMessages));
        
        var validChannels = channelMessages
            .Where(c => announcedNodeIds.Contains(c.NodeId1) && 
                        announcedNodeIds.Contains(c.NodeId2))
            .Select(c => _mapper.Map<ChannelInformation>(c));
        
        lightningNetworkGraph.AddEdgesBatch(validChannels);
    
        _logger.LogInformation($"Constructed graph with {lightningNetworkGraph.NodeCount} nodes and {lightningNetworkGraph.EdgeCount} channels.");
    
        return lightningNetworkGraph;
    }

    public async Task ExportLightningNetworkByTimestampAsync(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken)
    {
        var lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(timestamp, HelperFunctions.DefaultTimespan, paymentSizeSat, cancellationToken);
        
        await _exportLightningNetworkService.ExportLightningNetworkTopologyByTimestampAsync(timestamp, lightningNetwork, cancellationToken);
    }

    public async Task ExportLightningNodeInformationAsync(DateTime timestamp, LightningFastGraph lightningNetwork, CancellationToken cancellationToken)
    {
        var nodeInformation = lightningNetwork._nodeInformationDict.Values.ToList();

        await _exportLightningNetworkService.ExportLightningNodeInformationByTimestampAsync(timestamp, nodeInformation, cancellationToken);
    }

    public async Task ExportLightningChannelInformationAsync(DateTime timestamp, LightningFastGraph lightningNetwork,
        CancellationToken cancellationToken)
    {
        var channelInformation = lightningNetwork._edgeInformationDict.Values.ToList();
        
        await _exportLightningNetworkService.ExportLightningChannelInformationByTimestampAsync(timestamp, channelInformation, cancellationToken);
    }

    public async Task<double> GetCentralityAnalyticallyByTimestampAsync(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken)
    {
        // var lightningNetwork = await GetLightningNetworkAsync(timestamp, cancellationToken, paymentSizeSat);

        var lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(timestamp, HelperFunctions.DefaultTimespan, paymentSizeSat, cancellationToken);
        
        // return lightningNetwork.AnalyzeCentrality().AverageCentrality;

        return 1.1;
    }

    public async Task<double> GetCentralityEmpiricallyByTimestampAsync(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken)
    {
        // var lightningNetwork = await GetLightningNetworkAsync(timestamp, cancellationToken, paymentSizeSat);

        var lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(timestamp, HelperFunctions.DefaultTimespan, paymentSizeSat, cancellationToken);
        
        // return lightningNetwork.AnalyzeCentralityMonteCarlo().AverageCentrality;

        return 1.1;
    }
}