using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.ExpressionTranslators.Internal;

/// <summary>
/// String Contains/StartsWith/EndsWith via LIKE; Trim/Replace/Pad/IndexOf/Substring/ToLower/ToUpper.
/// Docs: reference/function/string-functions/
/// </summary>
public class XuguStringMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo ContainsMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string)])!;

    private static readonly MethodInfo StartsWithMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!;

    private static readonly MethodInfo EndsWithMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(string)])!;

    private static readonly MethodInfo IndexOfMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.IndexOf), [typeof(string)])!;

    private static readonly MethodInfo IndexOfWithStartMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.IndexOf), [typeof(string), typeof(int)])!;

    private static readonly MethodInfo ReplaceMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.Replace), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo ToLowerMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.ToLower), [])!;

    private static readonly MethodInfo ToUpperMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.ToUpper), [])!;

    private static readonly MethodInfo SubstringOneArgMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.Substring), [typeof(int)])!;

    private static readonly MethodInfo SubstringTwoArgsMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.Substring), [typeof(int), typeof(int)])!;

    private static readonly MethodInfo TrimMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.Trim), [])!;

    private static readonly MethodInfo TrimWithCharMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.Trim), [typeof(char)])!;

    private static readonly MethodInfo TrimStartMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.TrimStart), [])!;

    private static readonly MethodInfo TrimStartWithCharMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.TrimStart), [typeof(char)])!;

    private static readonly MethodInfo TrimEndMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), [])!;

    private static readonly MethodInfo TrimEndWithCharMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.TrimEnd), [typeof(char)])!;

    private static readonly MethodInfo PadLeftOneArgMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.PadLeft), [typeof(int)])!;

    private static readonly MethodInfo PadRightOneArgMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.PadRight), [typeof(int)])!;

    private static readonly MethodInfo PadLeftTwoArgsMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.PadLeft), [typeof(int), typeof(char)])!;

    private static readonly MethodInfo PadRightTwoArgsMethodInfo =
        typeof(string).GetRuntimeMethod(nameof(string.PadRight), [typeof(int), typeof(char)])!;

    private readonly XuguSqlExpressionFactory _sqlExpressionFactory;

    public XuguStringMethodTranslator(XuguSqlExpressionFactory sqlExpressionFactory)
        => _sqlExpressionFactory = sqlExpressionFactory;

    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        // string treated as char sequence: FirstOrDefault/LastOrDefault → SUBSTRING
        // (docs: string-functions/substring; same shape as SQL Server/SQLite providers).
        if (instance is null
            && method.DeclaringType == typeof(Enumerable)
            && method.IsGenericMethod
            && arguments is [var source]
            && source.Type == typeof(string)
            && method.Name is nameof(Enumerable.FirstOrDefault) or nameof(Enumerable.LastOrDefault))
        {
            if (method.Name == nameof(Enumerable.FirstOrDefault))
            {
                return _sqlExpressionFactory.NullableFunction(
                    "SUBSTRING",
                    [source, _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1)],
                    method.ReturnType);
            }

            return _sqlExpressionFactory.NullableFunction(
                "SUBSTRING",
                [
                    source,
                    _sqlExpressionFactory.NullableFunction(
                        "LENGTH",
                        [source],
                        typeof(int),
                        onlyNullWhenAnyNullPropagatingArgumentIsNull: false),
                    _sqlExpressionFactory.Constant(1)
                ],
                method.ReturnType);
        }

        if (instance is null)
        {
            return null;
        }

        if (ContainsMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.Like(
                instance,
                BuildConcatPattern(arguments[0], prefix: "%", suffix: "%"));
        }

        if (StartsWithMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.Like(
                instance,
                BuildConcatPattern(arguments[0], suffix: "%"));
        }

        if (EndsWithMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.Like(
                instance,
                BuildConcatPattern(arguments[0], prefix: "%"));
        }

        if (IndexOfMethodInfo.Equals(method))
        {
            return MakeIndexOf(instance, arguments[0]);
        }

        if (IndexOfWithStartMethodInfo.Equals(method))
        {
            return MakeIndexOf(instance, arguments[0], arguments[1]);
        }

        if (ReplaceMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.NullableFunction(
                "REPLACE",
                [instance, arguments[0], arguments[1]],
                method.ReturnType);
        }

        if (ToLowerMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.NullableFunction("LCASE", [instance], method.ReturnType);
        }

        if (ToUpperMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.NullableFunction("UCASE", [instance], method.ReturnType);
        }

        if (SubstringOneArgMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.NullableFunction(
                "SUBSTRING",
                [
                    instance,
                    _sqlExpressionFactory.Add(arguments[0], _sqlExpressionFactory.Constant(1)),
                    _sqlExpressionFactory.NullableFunction("LENGTH", [instance], typeof(int), onlyNullWhenAnyNullPropagatingArgumentIsNull: false)
                ],
                method.ReturnType);
        }

        if (SubstringTwoArgsMethodInfo.Equals(method))
        {
            return _sqlExpressionFactory.NullableFunction(
                "SUBSTRING",
                [
                    instance,
                    _sqlExpressionFactory.Add(arguments[0], _sqlExpressionFactory.Constant(1)),
                    arguments[1]
                ],
                method.ReturnType);
        }

        if (TrimMethodInfo.Equals(method) || TrimWithCharMethodInfo.Equals(method))
        {
            return ProcessTrim(instance, arguments.Count > 0 ? arguments[0] : null, location: null);
        }

        if (TrimStartMethodInfo.Equals(method) || TrimStartWithCharMethodInfo.Equals(method))
        {
            return ProcessTrim(instance, arguments.Count > 0 ? arguments[0] : null, location: "LEADING");
        }

        if (TrimEndMethodInfo.Equals(method) || TrimEndWithCharMethodInfo.Equals(method))
        {
            return ProcessTrim(instance, arguments.Count > 0 ? arguments[0] : null, location: "TRAILING");
        }

        if (PadLeftOneArgMethodInfo.Equals(method))
        {
            return TranslatePad(true, instance, arguments[0], _sqlExpressionFactory.Constant(" "), method.ReturnType);
        }

        if (PadRightOneArgMethodInfo.Equals(method))
        {
            return TranslatePad(false, instance, arguments[0], _sqlExpressionFactory.Constant(" "), method.ReturnType);
        }

        if (PadLeftTwoArgsMethodInfo.Equals(method))
        {
            return TranslatePad(true, instance, arguments[0], arguments[1], method.ReturnType);
        }

        if (PadRightTwoArgsMethodInfo.Equals(method))
        {
            return TranslatePad(false, instance, arguments[0], arguments[1], method.ReturnType);
        }

        return null;
    }

    private SqlExpression MakeIndexOf(SqlExpression instance, SqlExpression search, SqlExpression? startIndex = null)
    {
        var args = startIndex switch
        {
            null => new[] { search, instance },
            SqlConstantExpression { Value: int idx } => new[]
            {
                search,
                instance,
                _sqlExpressionFactory.Constant(idx + 1)
            },
            _ => new[]
            {
                search,
                instance,
                _sqlExpressionFactory.Add(startIndex, _sqlExpressionFactory.Constant(1))
            }
        };

        return _sqlExpressionFactory.Subtract(
            _sqlExpressionFactory.NullableFunction("LOCATE", args, typeof(int), onlyNullWhenAnyNullPropagatingArgumentIsNull: false),
            _sqlExpressionFactory.Constant(1));
    }

    private SqlExpression? TranslatePad(
        bool leftPad,
        SqlExpression instance,
        SqlExpression length,
        SqlExpression padString,
        Type returnType)
        => length is SqlConstantExpression && padString is SqlConstantExpression
            ? _sqlExpressionFactory.NullableFunction(
                leftPad ? "LPAD" : "RPAD",
                [instance, length, padString],
                returnType,
                onlyNullWhenAnyNullPropagatingArgumentIsNull: false)
            : null;

    private SqlExpression ProcessTrim(SqlExpression instance, SqlExpression? trimChar, string? location)
    {
        var sqlArguments = new List<SqlExpression>();

        if (location is not null)
        {
            sqlArguments.Add(_sqlExpressionFactory.Fragment(location));
        }

        if (trimChar is not null)
        {
            if (trimChar is SqlConstantExpression { Value: char singleChar })
            {
                sqlArguments.Add(_sqlExpressionFactory.Constant(singleChar));
            }
            else
            {
                return _sqlExpressionFactory.NullableFunction("TRIM", [instance], typeof(string));
            }
        }

        if (sqlArguments.Count > 0)
        {
            sqlArguments.Add(_sqlExpressionFactory.Fragment("FROM"));
        }

        sqlArguments.Add(instance);

        return _sqlExpressionFactory.NullableFunction(
            "TRIM",
            [
                _sqlExpressionFactory.ComplexFunctionArgument(
                    sqlArguments,
                    " ",
                    typeof(string))
            ],
            typeof(string));
    }

    private SqlExpression BuildConcatPattern(
        SqlExpression value,
        string? prefix = null,
        string? suffix = null)
    {
        var parts = new List<SqlExpression>();

        if (prefix is not null)
        {
            parts.Add(_sqlExpressionFactory.Constant(prefix));
        }

        parts.Add(value);

        if (suffix is not null)
        {
            parts.Add(_sqlExpressionFactory.Constant(suffix));
        }

        return parts.Count switch
        {
            1 => parts[0],
            _ => _sqlExpressionFactory.Function(
                "CONCAT",
                parts,
                nullable: true,
                argumentsPropagateNullability: parts.Select(_ => true),
                typeof(string))
        };
    }
}
