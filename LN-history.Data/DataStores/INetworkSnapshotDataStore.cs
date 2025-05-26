using LN_history.Data.Model;

namespace LN_history.Data.DataStores;

public interface INetworkSnapshotDataStore
{
    Task<(IEnumerable<Node>, IEnumerable<Channel>)> GetSnapshotAsync(DateTime timestamp, CancellationToken cancellationToken);
}