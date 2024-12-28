using LN_history.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LN_history.Core;

public static class ServiceCollectionExtension
{
    public static void AddLightningNetworkServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<INodeService, NodeService>();
        // serviceCollection.AddScoped<IChannelService, ChannelService>();
        serviceCollection.AddScoped<ILightningNetworkService, LightningNetworkService>();
        serviceCollection.AddScoped<IExportLightningNetworkService, ExportLightningNetworkService>();
        serviceCollection.AddScoped<IImportLightningNetworkService, ImportLightningNetworkService>();
    }
}