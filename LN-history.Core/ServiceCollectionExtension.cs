using LN_history.Core.Services;
using LN_history.Core.Settings;
using LN_History.Model.Settings;
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

        // serviceCollection.Configure<LightningSettings>(options =>
        // {
        //     options.DefaultTimespanDays = Convert.ToInt32(configuration.GetSection("LightningSettings:DefaultTimespanDays").Value);
        //     options.DefaultPaymentSizeSats = Convert.ToInt32(configuration.GetSection("LightningSettings:DefaultPaymentSizeSats").Value);
        // });
        
        serviceCollection.AddScoped<INetworkSnapshotService, NetworkSnapshotService>();
        serviceCollection.AddScoped<IChannelService, ChannelService>();
        serviceCollection.AddScoped<INodeService, NodeService>();
    }
}