using AutoMapper;
using LN_history.Data.Model;

namespace LN_history.Core.Mapping;

public class ChannelInformationMappingProfile : Profile
{
    public ChannelInformationMappingProfile()
    {
        CreateMap<ChannelMessageComplete, ChannelInformation>();
    }
}