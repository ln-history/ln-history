using AutoMapper;
using LN_history.Api.Dto;
using LN_history.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LN_history.Api.Controllers;

[ApiController]
[Route("ln-history/[controller]")]
public class ChannelController : ControllerBase
{
    private readonly IChannelService _channelService;
    private readonly IMapper _mapper;
    
    public ChannelController(IChannelService channelService, IMapper mapper)
    {
        _channelService = channelService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Gets Information of a channel by channelId (scid)
    /// </summary>
    /// <param name="scid">paymentSize in satoshis (sats); default value is 0, meaning no cost is calculated</param>
    /// <param name="timestamp">timestamp in ISO 8601 format</param>
    /// <param name="paymentSize"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("channel/{scid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LightningChannelDto>> GetChannelInformationByScidAndTimestamp(string scid, DateTime timestamp, int paymentSize = 0, CancellationToken cancellationToken = default)
    {
        var channelInformation = await _channelService.GetChannelInformationByScid(scid, timestamp, paymentSize, cancellationToken);

        if (channelInformation == null)
        {
            return NotFound($"Channel with scid {scid} not found");
        }
        
        var result = _mapper.Map<LightningChannelDto>(channelInformation);

        return result;
    }
}