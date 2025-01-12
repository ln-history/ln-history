using LightningGraph.Core;
using LightningGraph.Serialization;
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

    public async Task ExportLightningNetworkCompleteAsync(string bucketName, DateTime timestamp, LightningFastGraph lightningNetwork,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Exporting all Information of Lightning Network at timestamp {timestamp}");
        
        await ExportLightningNetworkTopologyByTimestampAsync(bucketName, timestamp, lightningNetwork, cancellationToken);
        // await ExportLightningNodeInformationByTimestampAsync(timestamp, lightningNetwork.NodeInformationDict.Values.ToList(), cancellationToken);
        // await ExportLightningChannelInformationByTimestampAsync(timestamp,lightningNetwork.EdgeInformationDict.Values.ToList(), cancellationToken);
    }

    public async Task ExportLightningNetworkTopologyByTimestampAsync(string bucketName, DateTime timestamp, LightningFastGraph lightningNetwork, CancellationToken cancellationToken)
    {
        var objectName = HelperFunctions.GetFileNameByTimestamp(timestamp, "bin");
        
        _logger.LogInformation($"Serializing the topology of Lightning Network at timestamp {timestamp}");
        var serializedLightningNetwork = LightningFastGraphTopologySerializerService.SerializeTopology(lightningNetwork);
        
        _logger.LogInformation($"Exporting the topology of Lightning Network at timestamp {timestamp}");
        await _cacheService.StoreGraphTopologyUsingRpcAsync(serializedLightningNetwork, bucketName, objectName, cancellationToken);    
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