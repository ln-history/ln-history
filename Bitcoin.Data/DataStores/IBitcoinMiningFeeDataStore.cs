namespace Bitcoin.Data.Datastores;

public interface IBitcoinMiningFeeDataStore
{
    Task<double?> GetMiningFeeByHeightAsync(int height, CancellationToken cancellationToken);

    Task<double?> GetMiningFeeByHashAsync(string hash, CancellationToken cancellationToken);
}