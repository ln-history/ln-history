using Asp.Versioning;
using Bitcoin.Core.Services;
using Bitcoin.Data.Datastores;
using Bitcoin.Data.Model;
using LN_history.Api.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LN_history.Api.v1.Controllers;

[ApiController]
[ApiVersion(1)]
[Route("ln-history/{v:apiVersion}/[controller]")]
[ApiKeyAuthorize]
public class BitcoinController : ControllerBase
{
    private readonly IBitcoinBlocksDataStore _bitcoinBlocksDataStore;
    private readonly IBitcoinMiningFeeDataStore _bitcoinMiningFeeDataStore;
    private readonly IBitcoinBlockSubsidyDataStore _bitcoinBlockSubsidyDataStore;
    private readonly IBitcoinIdentificationService _bitcoinIdentificationService;
    private readonly IBitcoinTimestampDataStore _bitcoinTimestampDataStore;

    public BitcoinController(IBitcoinBlocksDataStore bitcoinBlocksDataStore, IBitcoinIdentificationService bitcoinIdentificationService, IBitcoinMiningFeeDataStore bitcoinMiningFeeDataStore, IBitcoinBlockSubsidyDataStore bitcoinBlockSubsidyDataStore, IBitcoinTimestampDataStore bitcoinTimestampDataStore)
    {
        _bitcoinBlocksDataStore = bitcoinBlocksDataStore;
        _bitcoinIdentificationService = bitcoinIdentificationService;
        _bitcoinMiningFeeDataStore = bitcoinMiningFeeDataStore;
        _bitcoinBlockSubsidyDataStore = bitcoinBlockSubsidyDataStore;
        _bitcoinTimestampDataStore = bitcoinTimestampDataStore;
    }

    /// <summary>
    /// Gets a Bitcoin block by an identifier.
    /// The identifier can be the block height, unixTimestamp, a timestamp in ISO 8601 format or block hash of the block
    /// </summary>
    /// <param name="identifier">The height, timestamp or hash of a block</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("blocks/{identifier}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Block>> GetBitcoinBlockByIdentifier(string identifier,
        CancellationToken cancellationToken)
    {
        var result = await _bitcoinIdentificationService.GetBitcoinBlockByIdentifier(identifier, cancellationToken);

        if (result == null) { return NotFound($"Block matching identifier {identifier} not found."); }

        return result;
    }

    /// <summary>
    /// Gets a Bitcoin block by DateTime
    /// </summary>
    /// <param name="timestamp">timestamp in ISO 8601 format (e. g. 2021-01-01)</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("blocks/timestamp/{timestamp}")]
    [ProducesResponseType( StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Block>> GetBitcoinBlockByDateTime(DateTime timestamp,
        CancellationToken cancellationToken)
    {
        var result = await _bitcoinBlocksDataStore.GetBitcoinBlockByTimestampAsync(timestamp, cancellationToken);
        
        if (result == null) { return NotFound($"Block at DateTime {timestamp} not found."); }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets a Bitcoin block by unix time
    /// </summary>
    /// <param name="unixTime">Seconds since 01.01.1970</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("blocks/unixTime/{unixTime}")]
    [ProducesResponseType( StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Block>> GetBitcoinBlockByDateTime(long unixTime,
        CancellationToken cancellationToken)
    {
        var result = await _bitcoinBlocksDataStore.GetBitcoinBlockByUnixTimeAsync(unixTime, cancellationToken);
        
        if (result == null) { return NotFound($"Block at unix time {unixTime} not found."); }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets a Bitcoin block by block Hash
    /// </summary>
    /// <param name="hash">Block hash</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("blocks/hash/{hash}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Block>> GetBitcoinBlockByHash(string hash,
        CancellationToken cancellationToken)
    {
        var result = await _bitcoinBlocksDataStore.GetBitcoinBlockByHashAsync(hash, cancellationToken);
        
        if (result == null) { return NotFound($"Block with hash {hash} not found."); }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets a Bitcoin block by block height
    /// </summary>
    /// <param name="height">Block height</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("blocks/height/{height}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Block>> GetBitcoinBlockByHeight(int height,
        CancellationToken cancellationToken)
    {
        var result = await _bitcoinBlocksDataStore.GetBitcoinBlockByHeightAsync(height, cancellationToken);

        if (result == null) { return NotFound($"Block with height {height} not found."); }

        return Ok(result);
    }
    
    /// <summary>
    /// Gets Bitcoin blocks by block height range
    /// </summary>
    /// <param name="startHeight"></param>
    /// <param name="endHeight"></param>
    /// <param name="step"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("blocks/range/{startHeight}/{endHeight}/{step?}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ICollection<Block>>> GetBitcoinBlocksByHeight(int startHeight, int endHeight,
        CancellationToken cancellationToken, int step = 1)
    {
        var result = await _bitcoinBlocksDataStore.GetBitcoinBlocksByHeightAsync(startHeight, endHeight, step, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Gets the mining reward of a Bitcoin block by block height
    /// The mining reward equals the sum of the mining fee and the block subsidy.
    /// </summary>
    /// <param name="height"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("mining/reward/height/{height}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<double>> GetBitcoinMiningRewardByHeight(int height,
        CancellationToken cancellationToken)
    {
        var miningFee = await _bitcoinMiningFeeDataStore.GetMiningFeeByHeightAsync(height, cancellationToken);
        if (miningFee == null) { return NotFound($"Mining fee for height {height} not found."); }
        
        var subsidy = await _bitcoinBlockSubsidyDataStore.GetSubsidyByHeightAsync(height, cancellationToken);
        if (subsidy == null) { return NotFound($"Block subsidy for height {height} not found."); }

        var result = miningFee + subsidy;
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets the mining reward of a Bitcoin block by block hash
    /// The mining reward equals the sum of the mining fee and the block subsidy.
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("mining/reward/hash/{hash}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<double>> GetBitcoinMiningRewardByHash(string hash,
        CancellationToken cancellationToken)
    {
        var miningFee = await _bitcoinMiningFeeDataStore.GetMiningFeeByHashAsync(hash, cancellationToken);
        if (miningFee == null) { return NotFound($"Mining fee for hash {hash} not found."); }
        
        var subsidy = await _bitcoinBlockSubsidyDataStore.GetSubsidyByHashAsync(hash, cancellationToken);
        if (subsidy == null) { return NotFound($"Block subsidy for hash {hash} not found."); }

        var result = miningFee + subsidy;
        
        return Ok(result);
    }

    /// <summary>
    /// Gets the mining fee of a bitcoin block by block height
    /// </summary>
    /// <param name="height"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("mining/fee/height/{height}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<double>> GetBitcoinMiningFeeByHeight(int height, CancellationToken cancellationToken)
    {
        var result = await _bitcoinMiningFeeDataStore.GetMiningFeeByHeightAsync(height, cancellationToken);
        
        if (result == null) { return NotFound($"Mining fee for height {height} not found."); }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets the mining fee of a bitcoin block by block hash
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("mining/fee/hash/{hash}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<double>> GetBitcoinMiningFeeByHash(string hash, CancellationToken cancellationToken)
    {
        var result = await _bitcoinMiningFeeDataStore.GetMiningFeeByHashAsync(hash, cancellationToken);

        if (result == null) { return NotFound($"Mining fee for hash {hash} not found."); }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets the block subsidy of a bitcoin block by block height
    /// </summary>
    /// <param name="height"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("mining/subsidy/height/{height}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<double>> GetBlockSubsidyByHeight(int height, CancellationToken cancellationToken)
    {
        var result = await _bitcoinBlockSubsidyDataStore.GetSubsidyByHeightAsync(height, cancellationToken);
        
        if (result == null) { return NotFound($"Block subsidy for height {height} not found."); }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets the block subsidy of a bitcoin block by block hash
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("mining/subsidy/hash/{hash}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<double>> GetSubsidyFeeByHash(string hash, CancellationToken cancellationToken)
    {
        var result = await _bitcoinBlockSubsidyDataStore.GetSubsidyByHashAsync(hash, cancellationToken);

        if (result == null) { return NotFound($"Block subsidy for hash {hash} not found."); }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets the unix block timestamp of a bitcoin block by block height
    /// </summary>
    /// <param name="height"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("timestamp/height/{height}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<double>> GetBlockTimestampByHeight(int height, CancellationToken cancellationToken)
    {
        var result = await _bitcoinTimestampDataStore.GetTimestampByHeightAsync(height, cancellationToken);
        
        if (result == null) { return NotFound($"Block timestamp for height {height} not found."); }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Gets the unix block timestamp of a bitcoin block by block hash
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("timestamp/hash/{hash}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<double>> GetBlockTimestampFeeByHash(string hash, CancellationToken cancellationToken)
    {
        var result = await _bitcoinTimestampDataStore.GetTimestampByHashAsync(hash, cancellationToken);

        if (result == null) { return NotFound($"Block timestamp for hash {hash} not found."); }
        
        return Ok(result);
    }
}