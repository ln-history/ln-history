using LightningGraph.Core;
using LN_history.Cache.Services;
using LN_history.Core.Helper;
using LN_history.Data.DataStores;
using LN_history.Data.Model;
using Microsoft.Extensions.Logging;

namespace LN_history.Core.Services;

public class ExportLightningNetworkService : IExportLightningNetworkService
{
    private readonly ILogger<ExportLightningNetworkService> _logger;
    private readonly ICacheService _cacheService;
    private readonly INodeInformationDataStore _nodeInformationDataStore;
    private readonly IChannelInformationDataStore _channelInformationDataStore;

    public ExportLightningNetworkService(ICacheService cacheService, INodeInformationDataStore nodeInformationDataStore, IChannelInformationDataStore channelInformationDataStore, ILogger<ExportLightningNetworkService> logger)
    {
        _cacheService = cacheService;
        _nodeInformationDataStore = nodeInformationDataStore;
        _channelInformationDataStore = channelInformationDataStore;
        _logger = logger;
    }


    public async Task ExportLightningNetworkCompleteAsync(DateTime timestamp, LightningFastGraph lightningNetwork,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Exporting all Information of Lightning Network at timestamp {timestamp}");
        
        await ExportLightningNetworkTopologyByTimestampAsync(timestamp, lightningNetwork, cancellationToken);
        await ExportLightningNodeInformationByTimestampAsync(timestamp, lightningNetwork._nodeInformationDict.Values.ToList(), cancellationToken);
        await ExportLightningChannelInformationByTimestampAsync(timestamp,lightningNetwork._edgeInformationDict.Values.ToList(), cancellationToken);
    }

    public async Task ExportLightningNetworkTopologyByTimestampAsync(DateTime timestamp, LightningFastGraph lightningNetwork, CancellationToken cancellationToken)
    {
        var objectName = HelperFunctions.GetFileNameByTimestamp(timestamp, "json");
        
        _logger.LogInformation($"Exporting the topology of Lightning Network at timestamp {timestamp}");
        
        await _cacheService.StoreGraphAsync("lightning-fast-graphs", objectName, lightningNetwork, cancellationToken);    
    }

    public async Task ExportLightningNodeInformationByTimestampAsync(DateTime timestamp, IEnumerable<NodeInformation> nodes,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Exporting NodeInformation of Lightning Network at timestamp {DateTime.Now}");
        
        await _nodeInformationDataStore.StoreNodeInformationAsync(timestamp, nodes, cancellationToken: cancellationToken);
    }

    public async Task ExportLightningChannelInformationByTimestampAsync(DateTime timestamp, IEnumerable<ChannelInformation> channels,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Exporting ChannelInformation of Lightning Network at timestamp {DateTime.Now}");
        
        await _channelInformationDataStore.StoreChannelInformationAsync(timestamp, channels, cancellationToken: cancellationToken);
    }
}