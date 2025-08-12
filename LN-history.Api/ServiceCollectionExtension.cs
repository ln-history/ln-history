using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LN_history.Api;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApiServices(this IServiceCollection serviceCollection,
        Assembly?[]? assembliesWithMappingProfile = null)
    {
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