using Bitcoin.Core.Services;

namespace Bitcoin.Core;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddBitcoinServices(this IServiceCollection services)
    {
        services.AddScoped<IBitcoinIdentificationService, BitcoinBlockIdentificationService>();
        
        return services;
    }
}