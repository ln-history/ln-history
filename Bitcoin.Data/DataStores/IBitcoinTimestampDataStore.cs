namespace Bitcoin.Data.Datastores;

public interface IBitcoinTimestampDataStore
{
    Task<DateTime?> GetTimestampByHeightAsync(int height, CancellationToken cancellationToken);

    Task<DateTime?> GetTimestampByHashAsync(string hash, CancellationToken cancellationToken);
}