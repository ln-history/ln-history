namespace LN_history.Api.Infrastructure;

/// <summary>
/// Declares an action's rate-limit cost. <see cref="Key"/> is the stable route key looked up in
/// <c>api.endpoint_weights</c> (admin-tunable); <see cref="Default"/> is the fallback weight when the
/// DB has no override. Actions WITHOUT this attribute are not cost-charged (e.g. the WebSocket stream,
/// which manages its own limits).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ResourceCostAttribute : Attribute
{
    public string Key { get; }
    public int Default { get; }

    public ResourceCostAttribute(string key, int @default)
    {
        Key = key;
        Default = @default;
    }
}
