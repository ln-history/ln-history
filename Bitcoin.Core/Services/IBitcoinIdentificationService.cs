using Bitcoin.Data.Model;

namespace Bitcoin.Core.Services;

public interface IBitcoinIdentificationService
{
    Task<Block?> GetBitcoinBlockByIdentifier(string identifier, CancellationToken cancellationToken);
}