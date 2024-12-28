using AutoMapper;
using LN_history.Core.Model;
using LN_history.Data.DataStores;
using LN_history.Data.Model;
using LN_History.Model.Model;

namespace LN_history.Core.Mapping;

public class ChannelMessageMappingProfile : Profile
{
    public ChannelMessageMappingProfile()
    {
        CreateMap<ChannelMessage, LightningChannel>();
        CreateMap<ChannelMessageComplete, LightningChannel>()
            .ConstructUsing(src => new LightningChannel(
                new LightningNode { NodeId = src.NodeId1 },
                new LightningNode { NodeId = src.NodeId2 }
            ));
    }
}