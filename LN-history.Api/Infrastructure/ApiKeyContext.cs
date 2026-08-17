using LN_History.Model;

namespace LN_history.Api.Infrastructure;

/// <summary>
/// Carries the authenticated key principal through the request via <see cref="HttpContext.Items"/>.
/// Null means auth is disabled (Development) — callers treat that as unlimited/no-limit.
/// </summary>
public static class ApiKeyContext
{
    public const string ItemKey = "__ln_api_key";

    public static void Set(HttpContext context, ApiKeyRecord record) => context.Items[ItemKey] = record;

    public static ApiKeyRecord? Get(HttpContext context) =>
        context.Items.TryGetValue(ItemKey, out var value) ? value as ApiKeyRecord : null;
}
