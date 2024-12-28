using Bitcoin.Data.Model;
using CouchDB.Driver.Extensions;

namespace Bitcoin.Data.Datastores;

public class BitcoinBlocksDataStore : IBitcoinBlocksDataStore
{
    private readonly BitcoinBlocksCouchDbContext _bitcoinBlocksCouchDbContext;
    
    public BitcoinBlocksDataStore(BitcoinBlocksCouchDbContext bitcoinBlocksCouchDbContext)
    {
        _bitcoinBlocksCouchDbContext = bitcoinBlocksCouchDbContext;
    }
    
    
    public async Task<Block?> GetBitcoinBlockByTimestampAsync(DateTime timestamp, CancellationToken cancellationToken)
    {
        return await _bitcoinBlocksCouchDbContext.Blocks.FirstOrDefaultAsync(x => x.Time == (long)timestamp.Subtract(DateTime.UnixEpoch).TotalSeconds, cancellationToken);
    }

    public async Task<Block?> GetBitcoinBlockByUnixTimeAsync(long unixTime, CancellationToken cancellationToken)
    {
        return await _bitcoinBlocksCouchDbContext.Blocks.FirstOrDefaultAsync(x => x.Time == unixTime, cancellationToken);
    }

    public async Task<Block?> GetBitcoinBlockByHeightAsync(int height, CancellationToken cancellationToken)
    {
        return await _bitcoinBlocksCouchDbContext.Blocks.FindAsync(height.ToString(), false, cancellationToken);
    }

    public async Task<Block?> GetBitcoinBlockByHashAsync(string hash, CancellationToken cancellationToken)
    {
        return await _bitcoinBlocksCouchDbContext.Blocks.FirstOrDefaultAsync(x => x.Hash == hash, cancellationToken);
    }

    public async Task<ICollection<Block>> GetBitcoinBlocksByHeightAsync(int startHeight, int endHeight, int step, CancellationToken cancellationToken)
    {
        if (step <= 0)
        {
            throw new ArgumentException("Step must be greater than zero.", nameof(step));
        }

        var blockIds = Enumerable
            .Range(0, (endHeight - startHeight) / step + 1)
            .Select(i => (startHeight + i * step).ToString())
            .ToList();

        return await _bitcoinBlocksCouchDbContext.Blocks
            .FindManyAsync(blockIds, cancellationToken);
    }
}