namespace LN_history.Data.Model;

public class ChannelPolicy
{
    public DateTime FromTimestamp { get; set; }
    public DateTime ToTimestamp { get; set; }
    public long HtlcMinimumMsat { get; set; }
    public long HtlcMaximumMsat { get; set; }
    public int FeeBaseMsat { get; set; }
    public int FeePpmMsat { get; set; }
    public short CltvExpiryDelta { get; set; }
}