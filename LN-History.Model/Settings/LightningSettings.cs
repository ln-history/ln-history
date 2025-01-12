namespace LN_History.Model.Settings;

public class LightningSettings
{
    public int DefaultTimespanDays { get; set; } = 30; // Number of days into the past where channel messages get retrieved
    public int DefaultPaymentSizeSats { get; set; } = 10_000;
}