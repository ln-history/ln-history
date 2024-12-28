using AutoMapper;
using LN_history.Core.Model;
using LN_history.Data.Model;

namespace LN_history.Core.Mapping;

public class NodeAnnouncementMappingProfile : Profile
{
    public NodeAnnouncementMappingProfile()
    {
        CreateMap<NodeAnnouncementMessage, LightningNode>();
    }
}