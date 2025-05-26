using LN_history.Data.Model;

namespace LN_history.Core.Services;

public interface INetworkSnapshotService
{
    Task<NetworkSnapshot> GetSnapshotAsync(DateTime timestamp, CancellationToken cancellationToken);
}