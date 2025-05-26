namespace LN_history.Data.Model;

public class NodeSnapshot
{
    public string NodeId { get; set; } = string.Empty;
    public string? Alias { get; set; }
    public string RgbColor { get; set; } = string.Empty;
    public DateTime FromTimestamp { get; set; }
    public DateTime ToTimestamp { get; set; }
}