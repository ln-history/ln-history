using System.Data;
using LN_history.Data.DataStores;
using LN_history.Data.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LN_history.Data;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddGossipMessageData(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.Configure<QuestDbSettings>(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("QuestDb")! + ";Server Compatibility Mode=NoTypeLoading";
        });
        
        // Register IDbConnection factory
        serviceCollection.AddTransient<IDbConnection>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<QuestDbSettings>>().Value;
            return new NpgsqlConnection(options.ConnectionString);
        });
        
        serviceCollection.AddScoped<INodeAnnouncementDataStore, NodeAnnouncementDataStore>();
        serviceCollection.AddScoped<IChannelAnnouncementDataStore, ChannelAnnouncementDataStore>();
        serviceCollection.AddScoped<IChannelUpdateDataStore, ChannelUpdateDataStore>();
        serviceCollection.AddScoped<ILightningNetworkDataStore, LightningNetworkDataStore>();

        serviceCollection.AddScoped<INodeInformationDataStore, NodeInformationDataStore>();
        serviceCollection.AddScoped<IChannelInformationDataStore, ChannelInformationDataStore>();

        return serviceCollection;
    }
}