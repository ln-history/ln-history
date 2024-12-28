using AutoMapper;
using LN_history.Core.Model;
using LN_history.Data.Model;
using LN_History.Model.Model;

namespace LN_history.Core.Mapping;

public class ChannelUpdateMappingProfile : Profile
{
    public ChannelUpdateMappingProfile()
    {
        CreateMap<ChannelUpdateMessage, LightningChannel>()
            .ForMember(x => x.NodeId1, opt => opt.Ignore())
            .ForMember(x => x.NodeId2, opt => opt.Ignore())
            .ForMember(x => x.Features, opt => opt.Ignore());
    }
}