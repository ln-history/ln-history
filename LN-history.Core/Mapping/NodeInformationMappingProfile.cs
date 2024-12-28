using AutoMapper;
using LN_history.Data.Model;

namespace LN_history.Core.Mapping;

public class NodeInformationMappingProfile : Profile
{
    public NodeInformationMappingProfile()
    {
        CreateMap<NodeAnnouncementMessage, NodeInformation>();
    }
}