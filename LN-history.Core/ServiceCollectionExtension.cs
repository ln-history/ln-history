using LN_history.Core.Auth;
using LN_history.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LN_history.Core;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Registers Core-layer services (channel, node, snapshot, block, stats) that orchestrate
    /// the data stores and Bitcoin RPC access.
    /// </summary>
    public static IServiceCollection AddLightningNetworkServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IChannelService, ChannelService>();
        services.AddScoped<INodeService, NodeService>();
        services.AddScoped<ISnapshotService, SnapshotService>();
        services.AddScoped<IBlockService, BlockService>();
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IGossipStreamService, GossipStreamService>();

        // API-key auth + cost-based rate limiting. The in-memory service is always registered
        // (harmless when the middleware is off); the DB-touching maintenance loop runs only when
        // the API-key middleware is enabled.
        var accessOptions = new ApiAccessOptions();
        configuration.GetSection("ApiAccess").Bind(accessOptions);
        accessOptions.EmergencyAdminKey ??= configuration["ApiKey"];
        services.AddSingleton(accessOptions);
        services.AddSingleton<IApiAccessService, ApiAccessService>();
        if (configuration.GetValue<bool>("ApiKeyMiddleware:Enabled"))
            services.AddHostedService<ApiAccessMaintenanceService>();

        return services;
    }
}
