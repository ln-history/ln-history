namespace Bitcoin.Data.Datastores;

public interface IBitcoinBlockSubsidyDataStore
{
    Task<double?> GetSubsidyByHeightAsync(int height, CancellationToken cancellationToken);

    Task<double?> GetSubsidyByHashAsync(string hash, CancellationToken cancellationToken);
}