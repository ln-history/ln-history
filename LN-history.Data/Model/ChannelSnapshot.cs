namespace LN_history.Data.Model;

public class ChannelSnapshot
{
    public string Scid { get; set; } = string.Empty;
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public long AmountSat { get; set; }
    public string Features { get; set; } = string.Empty;
    public DateTime FromTimestamp { get; set; }
    public DateTime ToTimestamp { get; set; }
    public ChannelPolicy Policy { get; set; } = new();
}