namespace LN_history.Data.Model;

public class ChannelMessageComplete
{
    public string Scid { get; set; }
    public string ChainHash { get; set; }
    public string Features { get; set; }
    public string NodeId1 { get; set; }
    public string NodeId2 { get; set; }
    public DateTime Timestamp { get; set; }
    public string MessageFlags { get; set; }
    public string ChannelFlags { get; set; }
    public long CltvExpiryDelta { get; set; }
    public long HtlcMinimumMSat { get; set; }
    public long HtlcMaximumMSat { get; set; }
    public long FeeBaseMSat { get; set; }
    public long FeeProportionalMillionths { get; set; }
}