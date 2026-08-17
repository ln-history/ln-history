using System.Diagnostics;
using System.Text.Json;
using LN_history.Api.Instrumentation;
using LN_history.Core.Auth;
using Microsoft.Extensions.Logging;

namespace LN_history.Api.Infrastructure;

/// <summary>
/// Authenticates each request against the in-memory key cache and attaches the principal to
/// <see cref="HttpContext.Items"/> for the cost filter. Swagger is always reachable. Browsers cannot
/// set headers on a WebSocket handshake, so for upgrade requests the key may be supplied as the
/// <c>api_key</c> query parameter instead of <c>x-api-key</c>. Only added to the pipeline when
/// <c>ApiKeyMiddleware:Enabled</c> is true (off in Development ⇒ no auth, no limits).
/// </summary>
public sealed class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;

    public ApiKeyAuthMiddleware(RequestDelegate next, ILogger<ApiKeyAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IApiAccessService access, AppMetrics metrics)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        var presented = ExtractKey(context.Request);
        if (string.IsNullOrEmpty(presented))
        {
            metrics.AuthResults.Add(1, new KeyValuePair<string, object?>("result", "missing_key"));
            _logger.LogInformation("Rejected {Method} {Path}: no API key presented.",
                context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, "API key is missing.");
            return;
        }

        var record = access.Authenticate(presented);
        if (record is null)
        {
            metrics.AuthResults.Add(1, new KeyValuePair<string, object?>("result", "invalid_key"));
            // The key itself is never logged — only the fact that one was presented and rejected.
            _logger.LogWarning("Rejected {Method} {Path}: unknown, disabled or expired API key.",
                context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, "Invalid or expired API key.");
            return;
        }

        metrics.AuthResults.Add(1, new KeyValuePair<string, object?>("result", "ok"));

        // Tag the server span so a trace in Tempo can be attributed to a tenant. Cardinality is the
        // number of issued keys (a handful), and these are span attributes only — Tempo's
        // span-metrics dimensions are configured separately, so no metric series are added.
        var activity = Activity.Current;
        if (activity is not null)
        {
            activity.SetTag("lnhistory.key.owner", record.OwnerLabel);
            activity.SetTag("lnhistory.key.role", record.IsAdmin ? "admin" : "researcher");
        }

        ApiKeyContext.Set(context, record);
        await _next(context);
    }

    private static string? ExtractKey(HttpRequest request)
    {
        if (request.Headers.TryGetValue("x-api-key", out var header))
            return header.ToString();

        var isWebSocket = request.Headers.TryGetValue("Upgrade", out var upgrade)
                          && upgrade.ToString().Equals("websocket", StringComparison.OrdinalIgnoreCase);
        if (isWebSocket && request.Query.TryGetValue("api_key", out var queryKey))
            return queryKey.ToString();

        return null;
    }

    private static async Task WriteProblemAsync(HttpContext context, string detail)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/problem+json";
        var problem = new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            title = "Unauthorized",
            status = StatusCodes.Status401Unauthorized,
            detail
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
