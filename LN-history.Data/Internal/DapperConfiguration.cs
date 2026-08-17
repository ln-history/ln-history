using System.Data;
using Dapper;

namespace LN_history.Data.Internal;

internal static class DapperConfiguration
{
    private static bool _configured;

    /// <summary>
    /// Maps snake_case result columns to PascalCase row-type members so hand-written SQL
    /// materializes into the internal row records without per-column aliases, and teaches Dapper
    /// to bind <see cref="DateOnly"/> parameters (Dapper has no built-in handler for it).
    /// </summary>
    public static void EnsureConfigured()
    {
        if (_configured) return;
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        _configured = true;
    }

    private sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }

        public override DateOnly Parse(object value) =>
            value is DateTime dt ? DateOnly.FromDateTime(dt) : DateOnly.FromDateTime(Convert.ToDateTime(value));
    }
}
