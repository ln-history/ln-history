using System.Text.Json;
using LN_history.Core.Auth;

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

    public ApiKeyAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IApiAccessService access)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        var presented = ExtractKey(context.Request);
        if (string.IsNullOrEmpty(presented))
        {
            await WriteProblemAsync(context, "API key is missing.");
            return;
        }

        var record = access.Authenticate(presented);
        if (record is null)
        {
            await WriteProblemAsync(context, "Invalid or expired API key.");
            return;
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
