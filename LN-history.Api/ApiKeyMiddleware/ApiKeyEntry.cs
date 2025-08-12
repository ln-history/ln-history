namespace LN_history.Api.ApiKeyMiddleware;

public class ApiKeyEntry
{
    public int Id { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Usings { get; set; }
    public DateTime? TimeFirstUsage { get; set; }
    public DateTime? LastSeen { get; set; }
}