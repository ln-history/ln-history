namespace LN_history.Core.Services;

public class LightningNetworkAnalyticalService : ILightningNetworkAnalyticalService
{
    public Task<ICollection<double>> CalculateAllShortestPathCostsByPaymentSizeAndTimestamp(int paymentSizeSat, DateTime timestamp,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}