namespace LN_History.Model;

/// <summary>
/// A row of <c>api.api_keys</c> plus the resolved runtime principal. Per-key limit fields are
/// nullable — null means "use the global default". The raw key is never stored; only its sha256.
/// The synthetic emergency-admin principal (from config) is also represented as one of these with
/// <see cref="Role"/> = <see cref="KeyRole.Admin"/> and <see cref="Id"/> = 0.
/// </summary>
public sealed class ApiKeyRecord
{
    public long Id { get; init; }
    public string KeyHash { get; init; } = string.Empty;
    public string DisplayPrefix { get; init; } = string.Empty;
    public KeyRole Role { get; init; }
    public string OwnerLabel { get; init; } = string.Empty;
    public int? DailyBudget { get; init; }
    public int? BurstPerSec { get; init; }
    public int? MaxStreamConns { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool Enabled { get; init; } = true;

    public bool IsAdmin => Role == KeyRole.Admin;
}
