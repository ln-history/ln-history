using System.Data;
using LN_history.Data.Model;

namespace LN_history.Data.DataStores;

public interface IChannelAnnouncementDataStore
{
    Task<ChannelAnnouncementMessage?> GetChannelAnnouncementByScidAsync(string scid,
        CancellationToken cancellationToken);

    Task<ICollection<ChannelAnnouncementMessage>> GetChannelAnnouncementsAsync(CancellationToken cancellationToken);
}