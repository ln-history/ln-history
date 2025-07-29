using System.Data;
using DuckDB.NET.Data;
using LN_history.Data.DataStores;
using LN_history.Data.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;

namespace LN_history.Data;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddLnHistoryDatabase(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        // Configure PostgreSQL connection
        serviceCollection.Configure<PostgreSqlDbSettings>(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("PostgreSQL")! + ";Server Compatibility Mode=NoTypeLoading";
        });
        
        // Register PostgreSQL IDbConnection factory
        serviceCollection.AddTransient<NpgsqlConnection>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PostgreSqlDbSettings>>().Value;
            return new NpgsqlConnection(options.ConnectionString);
        });
        
        // Register DuckDB IDbConnection factory
        serviceCollection.AddTransient<DuckDBConnection>(sp =>
        {
            var duckDbPath = configuration.GetConnectionString("DuckDB")!;
            var connectionString = $"Data Source={duckDbPath}";

            // Initialize DuckDB connection
            return new DuckDBConnection(connectionString);
        });
        
        serviceCollection.AddScoped<INetworkSnapshotDataStore, NetworkSnapshotDataStore>();
        serviceCollection.AddScoped<IGossipDataStore, GossipDataStore>();
        return serviceCollection;
    }
}