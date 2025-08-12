using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace LN_history.Api.ApiKeyMiddleware;

public class ApiKeyTrackingMiddleware
{
    private readonly RequestDelegate _next;

    public ApiKeyTrackingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApiKeyDbContext db)
    {
        if (!context.Request.Headers.TryGetValue("x-api-key", out var apiKeyValues) || StringValues.IsNullOrEmpty(apiKeyValues))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key is missing");
            return;
        }

        var apiKey = apiKeyValues.ToString();

        var entry = await db.ApiKeys.FirstOrDefaultAsync(k => k.ApiKey == apiKey);

        if (entry == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid API Key");
            return;
        }

        // New check: block if usage exceeds limit
        if (entry.Usings > 10000)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests; // 429 is suitable for rate limiting
            await context.Response.WriteAsync("API Key usage limit exceeded");
            return;
        }

        if (!entry.TimeFirstUsage.HasValue)
            entry.TimeFirstUsage = DateTime.UtcNow;

        entry.LastSeen = DateTime.UtcNow;
        entry.Usings++;

        await db.SaveChangesAsync();

        await _next(context);
    }
}