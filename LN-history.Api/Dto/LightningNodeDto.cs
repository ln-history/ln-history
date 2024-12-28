namespace LN_history.Api.Dto;

public class LightningNodeDto
{
    public string NodeId { get; set; }

    public string Features { get; set; }
    
    public DateTime Timestamp { get; set; }

    public string RgbColor { get; set; }
    
    public string Addresses { get; set; }
}