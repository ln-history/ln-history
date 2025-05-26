namespace LN_history.Data.Model;

public class NetworkSnapshot
{
    public DateTime Timestamp { get; set; }
    public List<NodeSnapshot> Nodes { get; set; } = new();
    public List<ChannelSnapshot> Channels { get; set; } = new();
}