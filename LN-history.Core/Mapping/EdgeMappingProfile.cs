using AutoMapper;
using LightningGraph.Serialization;
using LN_history.Data.Model;

namespace LN_history.Core.Mapping;

public class EdgeMappingProfile : Profile
{
    public EdgeMappingProfile()
    {
        CreateMap<ChannelMessageComplete, Edge>()
            .ForMember(x => x.Scid, opt => opt.MapFrom(y =>y.Scid))
            .ForMember(x => x.From, opt => opt.MapFrom(y =>y.NodeId1))
            .ForMember(x => x.To, opt => opt.MapFrom(y =>y.NodeId2))
            .ForMember(x => x.Weight, opt => opt.MapFrom(y => new Weight(){ BaseMSat =  y.FeeBaseMSat, ProportionalMillionths = y.FeeProportionalMillionths}));
    }
}