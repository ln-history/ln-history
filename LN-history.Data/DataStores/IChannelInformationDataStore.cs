using LN_history.Data.Model;

namespace LN_history.Data.DataStores;

public interface IChannelInformationDataStore
{
    Task StoreChannelInformationAsync(DateTime timestamp, IEnumerable<ChannelInformation> channelInformation, int batchSize = 100,
        CancellationToken cancellationToken = default);
    
    Task<ChannelInformation?> GetChannelInformationByScidAndTimestampAsync(string scid, DateTime timestamp, CancellationToken cancellationToken = default);
}