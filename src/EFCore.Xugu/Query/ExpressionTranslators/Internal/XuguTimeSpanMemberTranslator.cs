using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// TimeSpan.Hours/Minutes/Seconds/Milliseconds via HOUR/MINUTE/SECOND/MICROSECOND.
/// Docs: reference/function/date-and-time-functions/hour.md, minute.md, second.md, microsecond.md
/// </summary>
public class XuguTimeSpanMemberTranslator : IMemberTranslator
{
    private static readonly Dictionary<string, (string Function, int Divisor)> DatePartMapping =
        new(StringComparer.Ordinal)
        {
            [nameof(TimeSpan.Hours)] = ("HOUR", 1),
            [nameof(TimeSpan.Minutes)] = ("MINUTE", 1),
            [nameof(TimeSpan.Seconds)] = ("SECOND", 1),
            [nameof(TimeSpan.Milliseconds)] = ("MICROSECOND", 1000),
        };

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguTimeSpanMemberTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (instance is null
            || member.DeclaringType != typeof(TimeSpan)
            || !DatePartMapping.TryGetValue(member.Name, out var datePart))
        {
            return null;
        }

        // MICROSECOND() returns high-precision numeric; driver GetInt32 throws E34412.
        // Project INTEGER (same mitigation as COUNT/TIMESTAMPDIFF).
        var extract = _sqlExpressionFactory.NullableFunction(
            datePart.Function,
            [instance],
            typeof(long),
            onlyNullWhenAnyNullPropagatingArgumentIsNull: false);

        SqlExpression value = extract;
        if (datePart.Divisor != 1)
        {
            value = _sqlExpressionFactory.Divide(
                extract,
                _sqlExpressionFactory.Constant(datePart.Divisor));
        }

        return _sqlExpressionFactory.Convert(value, typeof(int));
    }
}
