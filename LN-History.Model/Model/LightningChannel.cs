using LN_history.Core.Model;

namespace LN_History.Model.Model;

public class LightningChannel
{
    public LightningChannel() { }
    
    public LightningChannel(LightningNode source, LightningNode target)
    {
        Source = source;
        Target = target;
    }
    
    public LightningNode Source { get; set; }
    public LightningNode Target { get; set; }

    public string Scid { get; set; }
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
    public string ChainHash { get; set; }
    
    public double? Cost { get; set; }
}