using AutoMapper;
using LN_history.Core.Model;
using LN_history.Data.DataStores;
using Microsoft.Extensions.Logging;

namespace LN_history.Core.Services;

public class NodeService : INodeService
{
    private readonly INodeAnnouncementDataStore _nodeAnnouncementDataStore;
    private readonly IMapper _mapper;
    private readonly ILogger<INodeService> _logger;

    public NodeService(INodeAnnouncementDataStore nodeAnnouncementDataStore, IMapper mapper, ILogger<INodeService> logger)
    {
        _nodeAnnouncementDataStore = nodeAnnouncementDataStore;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LightningNode?> GetNodeInformationByNodeId(string nodeId, CancellationToken cancellationToken)
    {
        var nodeAnnouncementMessage = await _nodeAnnouncementDataStore.GetNodeAnnouncementByIdAsync(nodeId, cancellationToken);

        if (nodeAnnouncementMessage == null)
        {
            _logger.LogWarning($"nodeAnnouncementMessage for node with nodeId: {nodeId} could not be found.");
            return null;
        }
        
        var result = _mapper.Map<LightningNode>(nodeAnnouncementMessage);
        
        return result;
    }
}