using System.Globalization;
using CouchDB.Driver.Views;
using Microsoft.Extensions.Logging;

namespace Bitcoin.Data.Datastores;

public class BitcoinBlockSubsidyDataStore : IBitcoinBlockSubsidyDataStore
{
    private readonly BitcoinBlocksCouchDbContext _bitcoinBlocksDbContext;
    private readonly ILogger<BitcoinBlockSubsidyDataStore> _logger;

    public BitcoinBlockSubsidyDataStore(BitcoinBlocksCouchDbContext bitcoinBlocksDbContext, ILogger<BitcoinBlockSubsidyDataStore> logger)
    {
        _bitcoinBlocksDbContext = bitcoinBlocksDbContext;
        _logger = logger;
    }

    public async Task<double?> GetSubsidyByHeightAsync(int height, CancellationToken cancellationToken)
    {
        var subsidyOptions = new CouchViewOptions<string>
        {
            Key = height.ToString(),
            IncludeDocs = false
        };

        var subsidies =
            await _bitcoinBlocksDbContext.Blocks.GetViewAsync<string, string>("subsidy", "byHeight", subsidyOptions,
                cancellationToken);

        var subsidy = subsidies.SingleOrDefault();

        if (subsidy == null)
        {
            _logger.LogInformation("No subsidy found for block with block height: {height}", height);
            return null;
        }

        try
        {
            var result = double.Parse(subsidy.Value, CultureInfo.InvariantCulture);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse subsidy for block with height: {height}", height);
            return null;
        }
    }

    public async Task<double?> GetSubsidyByHashAsync(string hash, CancellationToken cancellationToken)
    {
        var subsidyOptions = new CouchViewOptions<string>
        {
            Key = hash,
            IncludeDocs = false
        };

        var subsidies =
            await _bitcoinBlocksDbContext.Blocks.GetViewAsync<string, string>("subsidy", "byHash", subsidyOptions,
                cancellationToken);

        var subsidy = subsidies.SingleOrDefault();

        if (subsidy == null)
        {
            _logger.LogInformation("No subsidy found for block with hash: {hash}", hash);
            return null;
        }

        try
        {
            var result = double.Parse(subsidy.Value, CultureInfo.InvariantCulture);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse subsidy for block with height: {hash}", hash);
            return null;
        }
    }
}