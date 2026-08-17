using System.Diagnostics;
using LN_history.Data.DataStores;
using LN_history.Data.Instrumentation;
using LN_history.Data.Streaming;
using LN_History.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LN_history.Data;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// Registers PostgreSQL access via a singleton <see cref="NpgsqlDataSource"/> and the data stores.
    /// Data stores open short-lived connections from the data source per query.
    /// </summary>
    public static IServiceCollection AddLnHistoryDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? throw new InvalidOperationException("Connection string 'PostgreSQL' is missing.");

        services.AddNpgsqlDataSource(connectionString, dataSource => dataSource
            .ConfigureTracing(tracing => tracing
                // Only trace commands that run inside an ambient Activity, i.e. inside an HTTP
                // request. Without this, the gossip listener's per-notification fetch would emit a
                // parentless root span for every ingested message — thousands per minute of traces
                // that contain a single SELECT and answer no question.
                .ConfigureCommandFilter(_ => Activity.Current is not null)
                // Connection open/close is pool churn, not useful request detail.
                .EnablePhysicalOpenTracing(false)));

        services.AddSingleton<StreamMetrics>();

        services.AddScoped<IChannelDataStore, ChannelDataStore>();
        services.AddScoped<INodeDataStore, NodeDataStore>();
        services.AddScoped<IClosureDataStore, ClosureDataStore>();
        services.AddScoped<ISnapshotDataStore, SnapshotDataStore>();
        services.AddScoped<IStatsDataStore, StatsDataStore>();
        services.AddScoped<IGossipRawStore, GossipRawStore>();
        services.AddScoped<IApiKeyStore, ApiKeyStore>();

        // Live gossip: a singleton in-memory fan-out plus the LISTEN listener that feeds it.
        // The listener is only started when enabled (default true) so environments without the
        // ingest NOTIFY trigger can opt out; the fan-out itself is harmless when idle.
        services.AddSingleton<IGossipStream, GossipStream>();
        var streamEnabled = !bool.TryParse(configuration["GossipStream:Enabled"], out var enabled) || enabled;
        if (streamEnabled)
            services.AddHostedService<GossipNotificationListener>();

        return services;
    }
}
