using LN_history.Core.Model;
using LN_History.Model.Model;

namespace LN_history.Core.Services;

public interface IChannelService
{
    Task<LightningChannel?> GetChannelInformationByScid(string scid, DateTime timestamp, int paymentSize, CancellationToken cancellationToken);
}