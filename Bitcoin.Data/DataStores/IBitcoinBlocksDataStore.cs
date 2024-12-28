using Bitcoin.Data.Model;

namespace Bitcoin.Data.Datastores;

public interface IBitcoinBlocksDataStore
{
    Task<Block?> GetBitcoinBlockByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken);
    
    Task<Block?> GetBitcoinBlockByUnixTimeAsync(long unixTime, CancellationToken cancellationToken);
    
    Task<Block?> GetBitcoinBlockByHeightAsync(int height, CancellationToken cancellationToken);
    
    Task<Block?> GetBitcoinBlockByHashAsync(string hash, CancellationToken cancellationToken);
    
    Task<ICollection<Block>> GetBitcoinBlocksByHeightAsync(int startHeight, int endHeight, int step, CancellationToken cancellationToken);
}