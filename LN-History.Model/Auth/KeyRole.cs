namespace LN_History.Model;

/// <summary>Access tier of an API key.</summary>
public enum KeyRole
{
    /// <summary>Rate-limited tenant (a researcher).</summary>
    Researcher = 0,

    /// <summary>Unlimited access; bypasses all rate/budget limits.</summary>
    Admin = 1,
}
