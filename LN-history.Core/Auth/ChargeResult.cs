namespace LN_history.Core.Auth;

public enum ChargeOutcome
{
    Allowed,
    BurstExceeded,
    BudgetExceeded,
}

/// <summary>Outcome of a pre-charge attempt against a key's burst bucket and daily budget.</summary>
public sealed record ChargeResult(
    ChargeOutcome Outcome,
    long RemainingBudget,
    long ResetUnixSeconds,
    int RetryAfterSeconds)
{
    public bool Allowed => Outcome == ChargeOutcome.Allowed;

    /// <summary>Admins bypass limits entirely — nothing charged, no headers meaningful.</summary>
    public static readonly ChargeResult AdminAllowed = new(ChargeOutcome.Allowed, long.MaxValue, 0, 0);
}
