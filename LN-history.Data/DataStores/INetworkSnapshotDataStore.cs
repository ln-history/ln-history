using LN_history.Data.Model;

namespace LN_history.Data.DataStores;

public interface INetworkSnapshotDataStore
{
    Task<byte []?> GetSnapshotAsync(DateTime timestamp, CancellationToken cancellationToken);
}