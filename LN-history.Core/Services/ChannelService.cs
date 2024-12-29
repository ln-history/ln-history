using AutoMapper;
using LN_history.Core.Helper;
using LN_history.Data.DataStores;
using LN_History.Model.Model;
using LN_History.Model.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LN_history.Core.Services;

public class ChannelService : IChannelService
{
    private readonly ILightningNetworkDataStore _lightningNetworkDataStore;
    private readonly IMapper _mapper;
    private readonly ILogger<IChannelService> _logger;
    private readonly LightningSettings _settings;
    
    public ChannelService(ILightningNetworkDataStore lightningNetworkDataStore, IMapper mapper, ILogger<IChannelService> logger, IOptions<LightningSettings> options)
    {
        _lightningNetworkDataStore = lightningNetworkDataStore;
        _mapper = mapper;
        _logger = logger;
        _settings = options.Value;
    }
    public async Task<LightningChannel?> GetChannelInformationByScid(string scid, DateTime timestamp, int paymentSize, CancellationToken cancellationToken)
    {
        var startTime = timestamp - TimeSpan.FromDays(_settings.DefaultTimespanDays);
        var endTime = timestamp;

        var channel = await _lightningNetworkDataStore.GetChannelInformationCompleteByScidAndTimespanAsync(scid, startTime, endTime, cancellationToken);

        if (channel == null)
        {
            _logger.LogInformation($"Channel with scid {scid} not found");
            return null;
        }

        var lightningChannel = _mapper.Map<LightningChannel>(channel);

        lightningChannel.Cost = HelperFunctions.CalculateCost(lightningChannel.FeeBaseMSat, lightningChannel.FeeProportionalMillionths, paymentSize);
        
        return lightningChannel;
    }
}