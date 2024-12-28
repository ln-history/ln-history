using AutoMapper;

using LN_history.Api.Dto;
using LN_history.Core.Model;

namespace LN_history.Api.Mapping;

public class LightningNodeMappingProfile : Profile
{
    public LightningNodeMappingProfile()
    {
        CreateMap<LightningNode, LightningNodeDto>();
    }
}