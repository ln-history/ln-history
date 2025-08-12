using Microsoft.EntityFrameworkCore;

namespace LN_history.Api.ApiKeyMiddleware;

public class ApiKeyDbContext : DbContext
{
    public DbSet<ApiKeyEntry> ApiKeys { get; set; }

    public ApiKeyDbContext(DbContextOptions<ApiKeyDbContext> options) : base(options) { }
}