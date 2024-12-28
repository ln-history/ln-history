using AutoMapper;
using LN_history.Core.Model;
using LN_history.Data.Model;
using LN_History.Model.Model;

namespace LN_history.Core.Mapping;

public class ChannelAnnouncementMappingProfile : Profile
{
    public ChannelAnnouncementMappingProfile()
    {
        CreateMap<ChannelAnnouncementMessage, LightningChannel>()
            .ForMember(x => x.ChannelFlags, opt => opt.Ignore())
            .ForMember(x => x.Timestamp, opt => opt.Ignore())
            .ForMember(x => x.MessageFlags, opt => opt.Ignore())
            .ForMember(x => x.CltvExpiryDelta, opt => opt.Ignore())
            .ForMember(x => x.FeeProportionalMillionths, opt => opt.Ignore())
            .ForMember(x => x.FeeBaseMSat, opt => opt.Ignore())
            .ForMember(x => x.HtlcMaximumMSat, opt => opt.Ignore())
            .ForMember(x => x.HtlcMinimumMSat, opt => opt.Ignore());
    }
}