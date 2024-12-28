using System.Globalization;
using CouchDB.Driver.Views;
using Microsoft.Extensions.Logging;

namespace Bitcoin.Data.Datastores;

public class BitcoinMiningFeeDataStore : IBitcoinMiningFeeDataStore
{
    private readonly BitcoinBlocksCouchDbContext _bitcoinBlocksDbContext;
    private readonly ILogger<IBitcoinMiningFeeDataStore> _logger;
    
    public BitcoinMiningFeeDataStore(BitcoinBlocksCouchDbContext bitcoinBlocksDbContext, ILogger<IBitcoinMiningFeeDataStore> logger)
    {
        _bitcoinBlocksDbContext = bitcoinBlocksDbContext;
        _logger = logger;
    }

    public async Task<double?> GetMiningFeeByHeightAsync(int height, CancellationToken cancellationToken)
    {
        var miningFeeOptions = new CouchViewOptions<string>
        {
            Key = height.ToString(),
            IncludeDocs = false
        };

        var miningFees =
            await _bitcoinBlocksDbContext.Blocks.GetViewAsync<string, string>("miningFee", "byHeight", miningFeeOptions,
                cancellationToken);

        var miningFee = miningFees.SingleOrDefault();

        if (miningFee == null)
        {
            _logger.LogInformation("No mining fee found for block with block height: {height}", height);
            return null;
        }

        try
        {
            var result = double.Parse(miningFee.Value, CultureInfo.InvariantCulture);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse mining fee for block with height: {height}", height);
            return null;
        }
    }

    public async Task<double?> GetMiningFeeByHashAsync(string hash, CancellationToken cancellationToken)
    {
        var miningFeeOptions = new CouchViewOptions<string>
        {
            Key = hash,
            IncludeDocs = false
        };

        var miningFees =
            await _bitcoinBlocksDbContext.Blocks.GetViewAsync<string, string>("miningFee", "byHash", miningFeeOptions,
                cancellationToken);

        var miningFee = miningFees.SingleOrDefault();

        if (miningFee == null)
        {
            _logger.LogInformation("No mining fee found for block with hash: {hash}", hash);
            return null;
        }

        try
        {
            var result = double.Parse(miningFee.Value, CultureInfo.InvariantCulture);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse mining fee for block with height: {hash}", hash);
            return null;
        }
    }
}