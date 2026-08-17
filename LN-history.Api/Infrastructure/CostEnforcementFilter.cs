using System.Diagnostics;
using LN_history.Core.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LN_history.Api.Infrastructure;

/// <summary>
/// Global resource filter that pre-charges a request's endpoint weight against the key's burst bucket
/// and daily budget BEFORE the action runs (so an over-budget heavy query is rejected without touching
/// the DB), records measured actuals afterwards, and refunds the weight when the request fails. No-ops
/// when there is no principal (auth disabled) or the action declares no <see cref="ResourceCostAttribute"/>.
/// </summary>
public sealed class CostEnforcementFilter : IAsyncResourceFilter
{
    private readonly IApiAccessService _access;

    public CostEnforcementFilter(IApiAccessService access) => _access = access;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var http = context.HttpContext;
        var record = ApiKeyContext.Get(http);
        var cost = ResolveCost(http);

        // No principal (auth off) or no cost attribute (e.g. the self-managed WS stream) -> pass through.
        if (record is null || cost is null)
        {
            await next();
            return;
        }

        var (routeKey, weight) = cost.Value;
        var charge = _access.Charge(record, weight);

        if (!record.IsAdmin)
        {
            http.Response.Headers["X-RateLimit-Remaining"] = charge.RemainingBudget.ToString();
            http.Response.Headers["X-RateLimit-Reset"] = charge.ResetUnixSeconds.ToString();
        }

        if (!charge.Allowed)
        {
            http.Response.Headers.RetryAfter = charge.RetryAfterSeconds.ToString();
            context.Result = TooMany(charge);
            return; // handler never runs; DB untouched
        }

        var sw = Stopwatch.StartNew();
        var executed = await next();
        sw.Stop();

        var failed = executed.Exception is not null || http.Response.StatusCode >= 400;
        if (failed)
            _access.Refund(record, weight);

        var bytes = http.Response.ContentLength ?? 0;
        _access.RecordActuals(record, routeKey, failed ? 0 : weight, sw.ElapsedMilliseconds, bytes);
    }

    private (string RouteKey, int Weight)? ResolveCost(HttpContext http)
    {
        var attr = http.GetEndpoint()?.Metadata.GetMetadata<ResourceCostAttribute>();
        if (attr is null) return null;

        var routeKey = attr.Key;
        // A snapshot with channel updates is materially heavier -> charge it as a distinct key.
        if (routeKey == "snapshot"
            && http.Request.Query.TryGetValue("withUpdates", out var wu)
            && bool.TryParse(wu, out var on) && on)
            routeKey = "snapshot_with_updates";

        return (routeKey, _access.ResolveWeight(routeKey, attr.Default));
    }

    private static ObjectResult TooMany(ChargeResult charge)
    {
        var detail = charge.Outcome == ChargeOutcome.BurstExceeded
            ? "Burst rate exceeded. Slow down and retry."
            : "Daily cost budget exhausted. Retry after the reset.";
        var problem = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.29",
            Title = "Too Many Requests",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = detail,
        };
        problem.Extensions["remaining_budget"] = charge.RemainingBudget;
        problem.Extensions["reset_unix"] = charge.ResetUnixSeconds;
        return new ObjectResult(problem) { StatusCode = StatusCodes.Status429TooManyRequests };
    }
}
