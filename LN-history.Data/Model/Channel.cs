namespace LN_history.Data.Model;

public class Channel
{
    public string Scid { get; set; } = string.Empty;
    public string Source_Node_Id { get; set; }
    public string Target_Node_Id { get; set; }
    public long Amount_Sat { get; set; }
    public byte[] Features { get; set; } = Array.Empty<byte>();
    public DateTime Channel_From { get; set; }
    public DateTime Channel_To { get; set; }
    public DateTime Update_From { get; set; }
    public DateTime Update_To { get; set; }
    public long Htlc_Minimum_Msat { get; set; }
    public long Htlc_Maximum_Msat { get; set; }
    public int Fee_Base_Msat { get; set; }
    public int Fee_Ppm_Msat { get; set; }
    public short Cltv_Expiry_Delta { get; set; }
}