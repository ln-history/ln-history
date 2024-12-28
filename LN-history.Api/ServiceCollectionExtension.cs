using System.Reflection;
using LN_history.Api.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace LN_history.Api;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApiServices(this IServiceCollection serviceCollection,
        Assembly?[]? assembliesWithMappingProfile = null)
    {
        serviceCollection.AddScoped<ApiKeyAuthorizationFilter>();
            
        serviceCollection.AddAutoMapper(opt =>
        {
            if (assembliesWithMappingProfile != null)
            {
                opt.AddMaps(assembliesWithMappingProfile);
            }
        });
        
        return serviceCollection;
    }
}