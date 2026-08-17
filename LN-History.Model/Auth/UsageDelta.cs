namespace LN_History.Model;

/// <summary>
/// An accumulated usage increment for one (key, UTC day, endpoint), flushed into <c>api.usage</c>
/// via an additive upsert. <see cref="Cost"/> is the charged weight; <see cref="DurationMs"/> and
/// <see cref="Bytes"/> are the measured actuals recorded alongside for reporting/retuning.
/// </summary>
public sealed class UsageDelta
{
    public long KeyId { get; set; }
    public DateOnly UsageDay { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public long Cost { get; set; }
    public long Count { get; set; }
    public long DurationMs { get; set; }
    public long Bytes { get; set; }
}
