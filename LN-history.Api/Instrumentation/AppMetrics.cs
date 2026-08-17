using System.Diagnostics.Metrics;

namespace LN_history.Api.Instrumentation;

public class AppMetrics
{
    public const string MeterName = "LN_history.Api";

    public AppMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        // Histogram: Best for measuring duration (latency)
        // Unit: Seconds (Prometheus standard)
        DbQueryDuration = meter.CreateHistogram<double>(
            "ln_db_query_duration_seconds",
            unit: "s",
            description: "Time taken to execute the SQL query");

        StreamingDuration = meter.CreateHistogram<double>(
            "ln_streaming_duration_seconds",
            unit: "s",
            description: "Time taken to stream the result to the client");

        SnapshotGenerationDuration = meter.CreateHistogram<double>(
            "ln_snapshot_generation_duration_seconds",
            unit: "s",
            description: "Total time for snapshot generation request");

        // --- API-key auth + cost limiting ---------------------------------------------------
        // A 401 or 429 is a client-visible failure that produces no error span (4xx is not an
        // error status in OTel semconv), so without these counters a researcher locked out by a
        // bad key or an exhausted budget is invisible in the dashboards.

        AuthResults = meter.CreateCounter<long>(
            "ln_api_auth_total",
            description: "API-key authentication outcomes, tagged result=ok|missing_key|invalid_key.");

        ChargeResults = meter.CreateCounter<long>(
            "ln_api_charge_total",
            description: "Cost pre-charge outcomes, tagged route_key and outcome=allowed|burst_exceeded|budget_exceeded.");

        ChargeRefunds = meter.CreateCounter<long>(
            "ln_api_charge_refunds_total",
            description: "Budget units refunded because a charged request then failed.");

        StreamRejections = meter.CreateCounter<long>(
            "ln_api_stream_rejections_total",
            description: "WebSocket stream upgrades refused because the key was at its concurrent-connection cap.");

        StreamConnections = meter.CreateCounter<long>(
            "ln_api_stream_connections_total",
            description: "Accepted live-gossip WebSocket connections.");

        StreamDuration = meter.CreateHistogram<double>(
            "ln_api_stream_duration_seconds",
            unit: "s",
            description: "How long a live-gossip WebSocket connection stayed open. Recorded here because the stream is excluded from tracing.");
    }

    public Histogram<double> DbQueryDuration { get; }
    public Histogram<double> StreamingDuration { get; }
    public Histogram<double> SnapshotGenerationDuration { get; }

    public Counter<long> AuthResults { get; }
    public Counter<long> ChargeResults { get; }
    public Counter<long> ChargeRefunds { get; }
    public Counter<long> StreamRejections { get; }
    public Counter<long> StreamConnections { get; }
    public Histogram<double> StreamDuration { get; }
}
