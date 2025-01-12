using AutoMapper;
using LightningGraph.Core;
using LightningGraph.Model;
using LN_history.Core.Helper;
using LN_history.Core.Settings;
using LN_history.Data.DataStores;
using LN_history.Data.Model;
using LN_History.Model.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LN_history.Core.Services;

public class LightningNetworkService : ILightningNetworkService
{
    private readonly IMapper _mapper;
    private readonly ILogger<ILightningNetworkService> _logger;
    private readonly INodeAnnouncementDataStore _nodeAnnouncementDataStore;
    private readonly ILightningNetworkDataStore _lightningNetworkDataStore;
    private readonly IImportLightningNetworkService _importLightningNetworkService;
    private readonly IExportLightningNetworkService _exportLightningNetworkService;
    private readonly LightningSettings _settings;
    private readonly LightningNetworkServiceOptions _options;
    
    public LightningNetworkService(IMapper mapper, INodeAnnouncementDataStore nodeAnnouncementDataStore, ILightningNetworkDataStore lightningNetworkDataStore, ILogger<ILightningNetworkService> logger, IExportLightningNetworkService exportLightningNetworkService, IImportLightningNetworkService importLightningNetworkService, IOptions<LightningSettings> options, IOptions<LightningNetworkServiceOptions> serviceOptions)
    {
        _mapper = mapper;
        _logger = logger;
        _importLightningNetworkService = importLightningNetworkService;
        _options = serviceOptions.Value;
        _settings = options.Value;
        _exportLightningNetworkService = exportLightningNetworkService;
        _nodeAnnouncementDataStore = nodeAnnouncementDataStore;
        _lightningNetworkDataStore = lightningNetworkDataStore;
    }

    public async Task<LightningFastGraph> GetLightningNetworkAsync(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken = default)
    {
        var bucketName = _options.BucketName;
        var lightningNetwork = await _importLightningNetworkService.ImportLightningNetworkByTimestamp(bucketName, timestamp, cancellationToken);

        if (lightningNetwork == null)
        {
            _logger.LogInformation($"No topology of Lightning Network at timestamp {timestamp} found");
            lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(timestamp, TimeSpan.FromDays(_settings.DefaultTimespanDays), paymentSizeSat, cancellationToken);
            await _exportLightningNetworkService.ExportLightningNetworkCompleteAsync(bucketName, timestamp, lightningNetwork, cancellationToken);
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
        var lightningNetwork = await GetLightningNetworkAsync(timestamp, _settings.DefaultPaymentSizeSats, cancellationToken);
        
        var result = lightningNetwork.NodeCount;

        return result;
    }

    public async Task<int> GetEdgeCountByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await GetLightningNetworkAsync(timestamp, _settings.DefaultPaymentSizeSats, cancellationToken);
        
        var result = lightningNetwork.EdgeCount;

        return result;
    }
    
    public async Task<int> GetNumberOfBridgesByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        var lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(
            timestamp, 
            TimeSpan.FromDays(_settings.DefaultTimespanDays), 
            _settings.DefaultPaymentSizeSats,
            cancellationToken);

        var bridgeAnalysis = lightningNetwork.GetBridgeAnalysis();
        
        return bridgeAnalysis.BridgeCount;
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
        var bucketName = _options.BucketName;
        var lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(timestamp, TimeSpan.FromDays(_settings.DefaultTimespanDays), paymentSizeSat, cancellationToken);
        
        await _exportLightningNetworkService.ExportLightningNetworkTopologyByTimestampAsync(bucketName, timestamp, lightningNetwork, cancellationToken);
    }

    public async Task ExportLightningNodeInformationAsync(DateTime timestamp, LightningFastGraph lightningNetwork, CancellationToken cancellationToken)
    {
        var nodeInformation = lightningNetwork.NodeInformationDict.Values.ToList();

        await _exportLightningNetworkService.ExportLightningNodeInformationByTimestampAsync(timestamp, nodeInformation, cancellationToken);
    }

    public async Task ExportLightningChannelInformationAsync(DateTime timestamp, LightningFastGraph lightningNetwork,
        CancellationToken cancellationToken)
    {
        var channelInformation = lightningNetwork.EdgeInformationDict.Values.ToList();
        
        await _exportLightningNetworkService.ExportLightningChannelInformationByTimestampAsync(timestamp, channelInformation, cancellationToken);
    }

    public async Task<double> GetCentralityAnalyticallyByTimestampAsync(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken)
    {
        var lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(
            timestamp, 
            TimeSpan.FromDays(_settings.DefaultTimespanDays), 
            paymentSizeSat,
            cancellationToken);
        
        return lightningNetwork.GetCentralityMetricsAnalytically().AverageCentrality;
    }

    public async Task<double> GetCentralityEmpiricallyByTimestampAsync(DateTime timestamp, int paymentSizeSat, CancellationToken cancellationToken)
    {
        var lightningNetwork = await ConstructLightningFastGraphByTimestampAsync(
            timestamp, 
            TimeSpan.FromDays(_settings.DefaultTimespanDays), 
            paymentSizeSat,
            cancellationToken);
        
        return lightningNetwork.GetCentralityMetricsEmpirically().AverageCentrality;
    }
}