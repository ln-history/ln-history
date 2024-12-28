using Bitcoin.Data.Datastores;
using Bitcoin.Data.Model;

namespace Bitcoin.Core.Services;

public class BitcoinBlockIdentificationService : IBitcoinIdentificationService
{
    private readonly IBitcoinBlocksDataStore _bitcoinBlocksDataStore;
    private readonly ILogger<IBitcoinIdentificationService> _logger;

    public BitcoinBlockIdentificationService(IBitcoinBlocksDataStore bitcoinBlocksDataStore, ILogger<IBitcoinIdentificationService> logger)
    {
        _bitcoinBlocksDataStore = bitcoinBlocksDataStore;
        _logger = logger;
    }

    public async Task<Block?> GetBitcoinBlockByIdentifier(string identifier, CancellationToken cancellationToken)
    {
        try
        {
            if (long.TryParse(identifier, out var numericIdentifier))
            {
                if (numericIdentifier > 1231006505) // Unix Timestamp for Genesis Block at 03.01.2009 19:15:05
                {
                    var unixTimestamp = DateTimeOffset.FromUnixTimeSeconds(numericIdentifier).UtcDateTime;
                    _logger.LogInformation("Identifier parsed as Unix timestamp: {UnixTimestamp}", unixTimestamp);
                    return await _bitcoinBlocksDataStore.GetBitcoinBlockByTimestampAsync(unixTimestamp, cancellationToken);
                }

                _logger.LogInformation("Identifier parsed as block height: {Height}", numericIdentifier);
                return await _bitcoinBlocksDataStore.GetBitcoinBlockByHeightAsync((int)numericIdentifier, cancellationToken);
            }

            if (DateTime.TryParse(identifier, out var timestamp))
            {
                _logger.LogInformation("Identifier parsed as timestamp: {Timestamp}", timestamp);
                return await _bitcoinBlocksDataStore.GetBitcoinBlockByTimestampAsync(timestamp, cancellationToken);
            }

            // Assume the identifier is a hash if it doesn't match the above cases
            _logger.LogInformation("Identifier treated as hash: {Hash}", identifier);
            return await _bitcoinBlocksDataStore.GetBitcoinBlockByHashAsync(identifier, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Bitcoin block with identifier: {Identifier}", identifier);
            throw;
        }
        finally
        {
            _logger.LogInformation("GetBitcoinBlockByIdentifier completed for identifier: {Identifier}", identifier);
        }
    }
}