using AutoMapper;
using LN_history.Api.Dto;
using LN_history.Core.Model;
using LN_History.Model.Model;

namespace LN_history.Api.Mapping;

public class LightningChannelMappingProfile : Profile
{
    public LightningChannelMappingProfile()
    {
        CreateMap<LightningChannel, LightningChannelDto>();
    }
}