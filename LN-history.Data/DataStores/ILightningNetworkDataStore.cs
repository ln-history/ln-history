using LN_history.Data.Model;

namespace LN_history.Data.DataStores;

public interface ILightningNetworkDataStore
{
    Task<ICollection<ChannelMessageComplete>> GetChannelsByTimespanAsync(DateTime startTime, DateTime endTime, CancellationToken cancellationToken);

    Task<ChannelMessageComplete?> GetChannelInformationCompleteByScidAndTimespanAsync(string scid, DateTime startTime, DateTime endTime, CancellationToken cancellationToken);
}