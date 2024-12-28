using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace LN_history.Api.Authorization;

public class ApiKeyAuthorizationFilter : IAsyncAuthorizationFilter
{
    private const string ApiKeyHeaderName = "x-api-key";
    private readonly string _configuredApiKey;

    public ApiKeyAuthorizationFilter(IConfiguration configuration)
    {
        _configuredApiKey = configuration["ApiKey"] ?? throw new ArgumentNullException(nameof(configuration), "ApiKey is not configured");
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!string.Equals(_configuredApiKey, extractedApiKey, StringComparison.Ordinal))
        {
            context.Result = new ForbidResult();
            return;
        }

        await Task.CompletedTask;
    }
}