using LN_history.Data.Model;

namespace LN_history.Data.DataStores;

public interface INodeInformationDataStore
{
    Task StoreNodeInformationAsync(DateTime timestamp, IEnumerable<NodeInformation> nodeInformation, int batchSize = 100,
        CancellationToken cancellationToken = default);
    
    Task<NodeInformation?> GetNodeInformationByNodeIdAndTimestampAsync(string nodeId, DateTime timestamp, CancellationToken cancellationToken = default);
}