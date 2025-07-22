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
        serviceCollection.Configure<PostgreSqlDbSettings>(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("PostgreSQL")! + ";Server Compatibility Mode=NoTypeLoading";
        });
        
        // Register IDbConnection factory
        serviceCollection.AddTransient<IDbConnection>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PostgreSqlDbSettings>>().Value;
            return new NpgsqlConnection(options.ConnectionString);
        });
        
        serviceCollection.AddScoped<INetworkSnapshotDataStore, NetworkSnapshotDataStore>();
        return serviceCollection;
    }
}