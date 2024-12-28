using System.ComponentModel.DataAnnotations.Schema;
using LN_history.Data.DataStores;

namespace LN_history.Data.Model;

[Table("channel_announcements")]
public class ChannelUpdateMessage : ChannelMessage
{
    public DateTime Timestamp { get; set; }
    public string MessageFlags { get; set; }
    public string ChannelFlags { get; set; }
    public long CltvExpiryDelta { get; set; }
    public long HtlcMinimumMSat { get; set; }
    public long HtlcMaximumMSat { get; set; }
    public long FeeBaseMSat { get; set; }
    public long FeeProportionalMillionths { get; set; }
}