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
        serviceCollection.AddTransient<DuckDBConnection>(sp => {
            var duckDbPath = configuration.GetConnectionString("DuckDB")!;
            var connectionString = $"Data Source={duckDbPath}";
            var connection = new DuckDBConnection(connectionString);

            var threads = configuration["Threads"];

            try {
                connection.Open();

                using (var setThreadsCommand = connection.CreateCommand()) {
                    // Set number of threads
                    if (!string.IsNullOrEmpty(threads)) {
                        setThreadsCommand.CommandText = "SET threads = ?";
                        setThreadsCommand.Parameters.Add(new DuckDBParameter("threads", threads));
                        setThreadsCommand.ExecuteNonQuery();
                    }
                }

                using (var warmUpCommand = connection.CreateCommand()) {
                    // Preloading data by executing queries that effectively load data into cache
                    warmUpCommand.CommandText = @"
                SELECT * FROM channel_updates LIMIT 1;
                SELECT * FROM channels LIMIT 1;
                SELECT * FROM nodes_raw_gossip LIMIT 1;
                SELECT * FROM nodes LIMIT 1;";
            
                    // If you want to ensure everything is loaded, you might need to execute separately
                    warmUpCommand.ExecuteNonQuery();
                }
            } catch (Exception ex) {
                // Handle exceptions
                Console.WriteLine("Error during initializing DuckDB connection: " + ex.Message);
            } finally {
                connection.Close();
            }

            // Initialize DuckDB connection
            return connection;
        });
        
        serviceCollection.AddScoped<INetworkSnapshotDataStore, NetworkSnapshotDataStore>();
        serviceCollection.AddScoped<IGossipDataStore, GossipDataStore>();
        return serviceCollection;
    }
}