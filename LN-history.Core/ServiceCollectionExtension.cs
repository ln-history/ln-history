using LN_history.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LN_history.Core;

public static class ServiceCollectionExtension
{
    public static void AddLightningNetworkServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        // Retrieve bucket name from configuration if not existing use "lightning-fast-graph-topology"
        // var defaultBucketName = configuration.GetSection("MinIO:BucketNameLightningFastGraphTopology").Value 
                                // ?? "lightning-fast-graph-topology";

        // Register the bucket name as an option
        // serviceCollection.Configure<LightningNetworkServiceOptions>(options =>
        // {
        //     options.BucketName = defaultBucketName;
        // });
        
        serviceCollection.AddScoped<INetworkSnapshotService, NetworkSnapshotService>();
        serviceCollection.AddScoped<IChannelService, ChannelService>();
        serviceCollection.AddScoped<INodeService, NodeService>();
    }
}