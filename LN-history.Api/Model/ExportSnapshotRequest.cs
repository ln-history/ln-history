namespace LN_history.Api.Model;

public record ExportSnapshotRequest(
    DateTime Timestamp,
    int PaymentSizeSat)
{
    public bool IsValid() => 
        Timestamp != default && 
        PaymentSizeSat > 0;
}