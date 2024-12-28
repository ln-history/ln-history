
namespace LN_history.Core.Services;

public interface ILightningNetworkSimulationService
{
    Task<ICollection<double>> SimulatePaymentsByPaymentSizeAndTimestampWithMonteCarlo(int paymentSizeSat, DateTime timestamp,
        CancellationToken cancellationToken, int runs = 1_000);
}