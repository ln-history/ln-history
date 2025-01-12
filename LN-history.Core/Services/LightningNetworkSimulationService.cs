namespace LN_history.Core.Services;

public class LightningNetworkSimulationService : ILightningNetworkSimulationService
{
    public Task<ICollection<double>> SimulatePaymentsByPaymentSizeAndTimestampWithMonteCarlo(int paymentSizeSat, DateTime timestamp,
        CancellationToken cancellationToken, int runs = 1000)
    {
        throw new NotImplementedException();
    }
}