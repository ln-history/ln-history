using LN_history.Core.Model;

namespace LN_history.Core.Services;

public interface INodeService
{
    Task<LightningNode?> GetNodeInformationByNodeId(string nodeId, CancellationToken cancellationToken);
}