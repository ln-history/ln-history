namespace LN_history.Data.Model;

public class Node
{
    public byte[] Node_Id { get; set; } = Array.Empty<byte>();
    public string? Alias { get; set; }
    public byte[] Rgb_Color { get; set; } = Array.Empty<byte>();
    public DateTime From_Timestamp { get; set; }
    public DateTime To_Timestamp { get; set; }
}