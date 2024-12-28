using LN_history.Data.Model;

namespace LN_history.Data.DataStores;

public interface IChannelUpdateDataStore
{
    Task<ICollection<ChannelUpdateMessage>> GetDistinctLatestChannelUpdateMessagesForTimespanAsync(DateTime startTime,
        DateTime endTime, CancellationToken cancellationToken);

    Task<ChannelUpdateMessage?> GetLatestChannelUpdateMessageForTimespanByScidAsync(string scid, DateTime startTime,
        DateTime endTime, CancellationToken cancellationToken);
}