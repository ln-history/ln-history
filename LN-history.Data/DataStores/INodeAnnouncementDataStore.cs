using LN_history.Data.Model;

namespace LN_history.Data.DataStores;

public interface INodeAnnouncementDataStore
{
    Task<NodeAnnouncementMessage?> GetNodeAnnouncementByIdAsync(string nodeId, CancellationToken cancellationToken);

    Task<ICollection<NodeAnnouncementMessage>> GetNodeAnnouncementsForTimespanAsync(DateTime startTime, DateTime endTime,
        CancellationToken cancellationToken);
}