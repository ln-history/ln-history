namespace LN_history.Core.Model;

public class LightningNode
{
    public string NodeId { get; set; }
    public string Features { get; set; }
    public DateTime Timestamp { get; set; }
    public string RgbColor { get; set; }
    public string Addresses { get; set; }
}