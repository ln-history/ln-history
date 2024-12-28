using System.Globalization;
using CouchDB.Driver.Views;
using Microsoft.Extensions.Logging;

namespace Bitcoin.Data.Datastores;

public class BitcoinTimestampDataStore : IBitcoinTimestampDataStore
{
    private readonly BitcoinBlocksCouchDbContext _bitcoinBlocksDbContext;
    private readonly ILogger<IBitcoinTimestampDataStore> _logger;

    public BitcoinTimestampDataStore(BitcoinBlocksCouchDbContext bitcoinBlocksDbContext, ILogger<IBitcoinTimestampDataStore> logger)
    {
        _bitcoinBlocksDbContext = bitcoinBlocksDbContext;
        _logger = logger;
    }

    public async Task<DateTime?> GetTimestampByHeightAsync(int height, CancellationToken cancellationToken)
    {
        var unixTimeSubsidy = new CouchViewOptions<string>
        {
            Key = height.ToString(),
            IncludeDocs = false
        };

        var unixTimes =
            await _bitcoinBlocksDbContext.Blocks.GetViewAsync<string, long>("unixtime", "byHeight", unixTimeSubsidy,
                cancellationToken);

        var unixTime = unixTimes.SingleOrDefault();

        if (unixTime == null)
        {
            _logger.LogInformation("No timestamp found for block with block height: {height}", height);
            return null;
        }

        try
        {
            var result = DateTimeOffset.FromUnixTimeSeconds(unixTime.Value).UtcDateTime;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed create timestamp for block with height: {height}", height);
            return null;
        }
    }

    public async Task<DateTime?> GetTimestampByHashAsync(string hash, CancellationToken cancellationToken)
    {
        var unixTimeSubsidy = new CouchViewOptions<string>
        {
            Key = hash,
            IncludeDocs = false
        };

        var unixTimes =
            await _bitcoinBlocksDbContext.Blocks.GetViewAsync<string, long>("unixtime", "byHash", unixTimeSubsidy,
                cancellationToken);

        var unixTime = unixTimes.SingleOrDefault();

        if (unixTime == null)
        {
            _logger.LogInformation("No timestamp found for block with block hash: {hash}", hash);
            return null;
        }

        try
        {
            var result = DateTimeOffset.FromUnixTimeSeconds(unixTime.Value).UtcDateTime;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed create timestamp for block with height: {hash}", hash);
            return null;
        }
    }
}