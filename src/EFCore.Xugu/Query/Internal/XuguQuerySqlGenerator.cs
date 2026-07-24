using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Properties;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguQuerySqlGenerator : QuerySqlGenerator
{
    private const ulong LimitUpperBound = 18446744073709551610;

    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private string? _removeTableAliasOld;
    private string? _removeTableAliasNew;

    private static readonly Dictionary<string, string[]> CastMappings =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["INTEGER"] = ["integer", "int", "smallint", "tinyint", "bigint"],
            ["BIGINT"] = ["bigint"],
            ["DOUBLE"] = ["double", "float"],
            ["DECIMAL(18,2)"] = ["decimal", "numeric", "number"],
            ["BOOLEAN"] = ["boolean", "bool"],
            ["DATETIME"] = ["datetime"],
            ["DATE"] = ["date"],
            ["TIME"] = ["time"],
            ["TIMESTAMP"] = ["timestamp", "datetime with time zone"],
            ["VARCHAR(255)"] = ["varchar", "char", "text", "clob"],
            ["JSON"] = ["json"],
        };

    public XuguQuerySqlGenerator(
        QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource)
        : base(dependencies)
        => _typeMappingSource = typeMappingSource;

    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            XuguComplexFunctionArgumentExpression complexFunctionArgument
                => VisitXuguComplexFunctionArgumentExpression(complexFunctionArgument),
            XuguColumnAliasReferenceExpression columnAliasReferenceExpression
                => VisitColumnAliasReference(columnAliasReferenceExpression),
            XuguInlinedParameterExpression inlinedParameterExpression
                => VisitInlinedParameterExpression(inlinedParameterExpression),
            XuguJsonTraversalExpression jsonTraversalExpression
                => VisitJsonPathTraversal(jsonTraversalExpression),
            _ => base.VisitExtension(extensionExpression)
        };

    private Expression VisitInlinedParameterExpression(XuguInlinedParameterExpression inlinedParameterExpression)
    {
        Visit(inlinedParameterExpression.ValueExpression);

        return inlinedParameterExpression;
    }

    private Expression VisitColumnAliasReference(XuguColumnAliasReferenceExpression columnAliasReferenceExpression)
    {
        Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnAliasReferenceExpression.Alias));
        return columnAliasReferenceExpression;
    }

    protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
    {
        // Xugu rejects string `+` (E17003 data format); use CONCAT (docs: string-functions).
        if (sqlBinaryExpression.OperatorType == ExpressionType.Add
            && sqlBinaryExpression.Type == typeof(string))
        {
            Sql.Append("CONCAT(");
            Visit(sqlBinaryExpression.Left);
            Sql.Append(", ");
            Visit(sqlBinaryExpression.Right);
            Sql.Append(")");
            return sqlBinaryExpression;
        }

        return base.VisitSqlBinary(sqlBinaryExpression);
    }

    protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        => sqlUnaryExpression.OperatorType == ExpressionType.Convert
            ? VisitConvert(sqlUnaryExpression)
            : base.VisitSqlUnary(sqlUnaryExpression);

    private SqlUnaryExpression VisitConvert(SqlUnaryExpression sqlUnaryExpression)
    {
        var targetStoreType = GetCastStoreType(sqlUnaryExpression.TypeMapping);

        if (targetStoreType is not null
            && !targetStoreType.Equals(
                sqlUnaryExpression.Operand.TypeMapping?.StoreType,
                StringComparison.OrdinalIgnoreCase))
        {
            Sql.Append("CAST(");
            Visit(sqlUnaryExpression.Operand);
            Sql.Append(" AS ");
            Sql.Append(targetStoreType);
            Sql.Append(")");
        }
        else
        {
            Visit(sqlUnaryExpression.Operand);
        }

        return sqlUnaryExpression;
    }

    private static string? GetCastStoreType(RelationalTypeMapping? typeMapping)
    {
        if (typeMapping is null)
        {
            return null;
        }

        var storeTypeLower = typeMapping.StoreType.ToLowerInvariant();

        foreach (var (castType, storeTypes) in CastMappings)
        {
            if (storeTypes.Any(storeType => storeTypeLower.StartsWith(storeType, StringComparison.Ordinal)))
            {
                return castType;
            }
        }

        return typeMapping.StoreType;
    }

    protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
    {
        if (sqlFunctionExpression.Name.Equals("COUNT", StringComparison.OrdinalIgnoreCase)
            && sqlFunctionExpression.Type is { } type
            && (type == typeof(int) || type == typeof(long)))
        {
            Sql.Append("CAST(");
            base.VisitSqlFunction(sqlFunctionExpression);
            Sql.Append(type == typeof(int) ? " AS INTEGER)" : " AS BIGINT)");

            return sqlFunctionExpression;
        }

        return base.VisitSqlFunction(sqlFunctionExpression);
    }

    protected override void GenerateLimitOffset(SelectExpression selectExpression)
    {
        if (selectExpression.Limit != null)
        {
            Sql.AppendLine().Append("LIMIT ");
            Visit(selectExpression.Limit);
        }

        if (selectExpression.Offset != null)
        {
            if (selectExpression.Limit == null)
            {
                Sql.AppendLine().Append($"LIMIT {LimitUpperBound}");
            }

            Sql.Append(" OFFSET ");
            Visit(selectExpression.Offset);
        }
    }

    /// <summary>
    /// EF Core default emits <c>SELECT … UNION ALL VALUES (…), (…)</c>, which Xugu rejects with
    /// E19132 (unexpected VALUES). Use portable <c>UNION ALL SELECT</c> row expansion instead
    /// (docs: set.md UNION ALL; insert.md multi-row VALUES is DML-only).
    /// </summary>
    protected override void GenerateValues(ValuesExpression valuesExpression)
    {
        if (valuesExpression.RowValues is null)
        {
            throw new InvalidOperationException(
                "ValuesExpression.ValuesParameter must be expanded to constants before SQL generation.");
        }

        var rowValues = valuesExpression.RowValues;
        if (rowValues.Count == 0)
        {
            throw new InvalidOperationException(RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot);
        }

        Sql.Append("SELECT ");

        var firstRowValues = rowValues[0].Values;
        for (var i = 0; i < firstRowValues.Count; i++)
        {
            if (i > 0)
            {
                Sql.Append(", ");
            }

            Visit(firstRowValues[i]);

            Sql
                .Append(AliasSeparator)
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(valuesExpression.ColumnNames[i]));
        }

        for (var r = 1; r < rowValues.Count; r++)
        {
            Sql.Append(" UNION ALL SELECT ");

            var values = rowValues[r].Values;
            for (var i = 0; i < values.Count; i++)
            {
                if (i > 0)
                {
                    Sql.Append(", ");
                }

                Visit(values[i]);
            }
        }
    }

    protected override Expression VisitColumn(ColumnExpression columnExpression)
    {
        if (_removeTableAliasOld is not null
            && columnExpression.TableAlias == _removeTableAliasOld)
        {
            if (_removeTableAliasNew is not null)
            {
                Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(_removeTableAliasNew))
                    .Append(".");
            }

            Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

            return columnExpression;
        }

        return base.VisitColumn(columnExpression);
    }

    protected override Expression VisitTable(TableExpression tableExpression)
    {
        if (_removeTableAliasOld is not null
            && tableExpression.Alias == _removeTableAliasOld)
        {
            if (_removeTableAliasNew is not null)
            {
                Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(_removeTableAliasNew))
                    .Append(AliasSeparator);
            }

            Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableExpression.Name));

            return tableExpression;
        }

        return base.VisitTable(tableExpression);
    }

    /// <summary>
    /// XuguDB has no CROSS APPLY / OUTER APPLY / LATERAL (probe + from.md join grammar).
    /// Fail clearly instead of emitting SQL the server rejects with E19132.
    /// </summary>
    protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        => throw new InvalidOperationException(XuguStrings.ApplyNotSupported);

    /// <inheritdoc cref="VisitCrossApply" />
    protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        => throw new InvalidOperationException(XuguStrings.ApplyNotSupported);

    protected override Expression VisitDelete(DeleteExpression deleteExpression)
    {
        var selectExpression = deleteExpression.SelectExpression;

        if (selectExpression.Offset == null
            && selectExpression.Having == null
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Projection.Count == 0
            && (selectExpression.Tables.Count == 1
                || selectExpression.Orderings.Count == 0 && selectExpression.Limit is null))
        {
            var removeSingleTableAlias = selectExpression.Tables.Count == 1
                && (selectExpression.Orderings.Count > 0 || selectExpression.Limit is not null);

            // Xugu docs (delete.md): `DELETE FROM table WHERE …`；多表为 `DELETE FROM t1 FROM t2 WHERE …`
            Sql.Append("DELETE FROM ");

            if (removeSingleTableAlias)
            {
                _removeTableAliasOld = selectExpression.Tables[0].Alias;
                _removeTableAliasNew = null;
            }

            Visit(deleteExpression.Table);

            if (selectExpression.Tables.Count > 1)
            {
                foreach (var table in selectExpression.Tables)
                {
                    if (table.Equals(deleteExpression.Table)
                        || (table is JoinExpressionBase join && join.Table.Equals(deleteExpression.Table)))
                    {
                        continue;
                    }

                    Sql.AppendLine().Append("FROM ");
                    Visit(table);
                }
            }

            if (selectExpression.Predicate != null)
            {
                Sql.AppendLine().Append("WHERE ");
                Visit(selectExpression.Predicate);
            }

            GenerateOrderings(selectExpression);
            GenerateLimitOffset(selectExpression);

            if (removeSingleTableAlias)
            {
                _removeTableAliasOld = null;
            }

            return deleteExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(EntityFrameworkQueryableExtensions.ExecuteDelete)));
    }

    protected override Expression VisitUpdate(UpdateExpression updateExpression)
    {
        var selectExpression = updateExpression.SelectExpression;

        if (selectExpression.Offset == null
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Projection.Count == 0)
        {
            Sql.Append("UPDATE ");

            if (selectExpression.Tables.Count > 1)
            {
                var tables = selectExpression.Tables;

                if (selectExpression.Tables.All(
                        t => !updateExpression.Table.Equals(t is JoinExpressionBase join ? join.Table : t)))
                {
                    Visit(updateExpression.Table);
                    Sql.AppendLine(",");
                }

                if (tables[0] is not JoinExpressionBase)
                {
                    tables = tables
                        .Skip(1)
                        .Prepend(new CrossJoinExpression(tables[0]))
                        .ToArray();
                }

                for (var i = 0; i < tables.Count; i++)
                {
                    if (i > 0)
                    {
                        Sql.AppendLine();
                    }

                    Visit(tables[i]);
                }
            }
            else
            {
                Visit(updateExpression.Table);
            }

            Sql.AppendLine().Append("SET ");
            Visit(updateExpression.ColumnValueSetters[0].Column);
            Sql.Append(" = ");
            Visit(updateExpression.ColumnValueSetters[0].Value);

            using (Sql.Indent())
            {
                foreach (var columnValueSetter in updateExpression.ColumnValueSetters.Skip(1))
                {
                    Sql.AppendLine(",");
                    Visit(columnValueSetter.Column);
                    Sql.Append(" = ");
                    Visit(columnValueSetter.Value);
                }
            }

            if (selectExpression.Predicate != null)
            {
                Sql.AppendLine().Append("WHERE ");
                Visit(selectExpression.Predicate);
            }

            if (selectExpression.Tables.Count == 1)
            {
                GenerateLimitOffset(selectExpression);
            }

            return updateExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(
                nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate)));
    }

    public virtual Expression VisitXuguComplexFunctionArgumentExpression(
        XuguComplexFunctionArgumentExpression expression)
    {
        for (var i = 0; i < expression.ArgumentParts.Count; i++)
        {
            if (i > 0)
            {
                Sql.Append(expression.Delimiter);
            }

            Visit(expression.ArgumentParts[i]);
        }

        return expression;
    }

    protected override Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
    {
        var path = jsonScalarExpression.Path;
        if (path.Count == 0)
        {
            Visit(jsonScalarExpression.Json);
            return jsonScalarExpression;
        }

        if (!JsonPathNeedsConcat(path))
        {
            Visit(jsonScalarExpression.Json);
            return VisitJsonScalarWithPathOperators(jsonScalarExpression);
        }

        var castStoreType = GetCastStoreType(jsonScalarExpression.TypeMapping);

        if (castStoreType is not null)
        {
            Sql.Append("CAST(");
        }

        Sql.Append("JSON_VALUE(");
        Visit(jsonScalarExpression.Json);
        Sql.Append(", ");
        GenerateJsonPath(path);
        Sql.Append(")");

        if (castStoreType is not null)
        {
            Sql.Append(" AS ");
            Sql.Append(castStoreType);
            Sql.Append(")");
        }

        return jsonScalarExpression;
    }

    private Expression VisitJsonScalarWithPathOperators(JsonScalarExpression jsonScalarExpression)
    {
        var path = jsonScalarExpression.Path;
        var inJsonPathString = false;

        for (var i = 0; i < path.Count; i++)
        {
            var pathSegment = path[i];
            var isLast = i == path.Count - 1;

            switch (pathSegment)
            {
                case { PropertyName: { } propertyName }:
                    if (inJsonPathString)
                    {
                        Sql.Append(".").Append(propertyName);
                        continue;
                    }

                    Sql.Append(" ->> ");

                    if (isLast || path[i + 1] is { ArrayIndex: not null and not SqlConstantExpression })
                    {
                        Sql.Append("'").Append(propertyName).Append("'");
                        continue;
                    }

                    Sql.Append("'$.").Append(propertyName);
                    inJsonPathString = true;
                    continue;

                case { ArrayIndex: SqlConstantExpression arrayIndex }:
                    if (inJsonPathString)
                    {
                        Sql.Append("[");
                        Visit(pathSegment.ArrayIndex);
                        Sql.Append("]");
                        continue;
                    }

                    Sql.Append(" ->> ");

                    if (isLast || path[i + 1] is { ArrayIndex: not null and not SqlConstantExpression })
                    {
                        Sql.Append("'$[");
                        Visit(arrayIndex);
                        Sql.Append("]'");
                        continue;
                    }

                    Sql.Append("'$[");
                    Visit(arrayIndex);
                    Sql.Append("]");
                    inJsonPathString = true;
                    continue;

                default:
                    if (inJsonPathString)
                    {
                        Sql.Append("'");
                        inJsonPathString = false;
                    }

                    Sql.Append(" ->> ");

                    var requiresParentheses = RequiresParentheses(jsonScalarExpression, pathSegment.ArrayIndex!);
                    if (requiresParentheses)
                    {
                        Sql.Append("(");
                    }

                    Visit(pathSegment.ArrayIndex!);

                    if (requiresParentheses)
                    {
                        Sql.Append(")");
                    }

                    continue;
            }
        }

        if (inJsonPathString)
        {
            Sql.Append("'");
        }

        return jsonScalarExpression;
    }

    protected virtual Expression VisitJsonPathTraversal(XuguJsonTraversalExpression expression)
    {
        var isSimplePath = expression.Path.All(
            location => location is SqlConstantExpression ||
                        location is XuguJsonArrayIndexExpression arrayIndex &&
                        arrayIndex.Expression is SqlConstantExpression);

        if (isSimplePath && expression.Path.Count > 0)
        {
            Visit(expression.Expression);
            Sql.Append(expression.ReturnsText ? " ->> " : " -> ");
            Sql.Append("'$");

            foreach (var location in expression.Path)
            {
                if (location is XuguJsonArrayIndexExpression arrayIndexExpression)
                {
                    Sql.Append("[");
                    Visit(arrayIndexExpression.Expression);
                    Sql.Append("]");
                }
                else
                {
                    Sql.Append(".");
                    Visit(location);
                }
            }

            Sql.Append("'");
            return expression;
        }

        if (expression.ReturnsText)
        {
            Sql.Append("JSON_VALUE(");
        }
        else
        {
            Sql.Append("JSON_EXTRACT(");
        }

        Visit(expression.Expression);
        Sql.Append(", ");

        if (!isSimplePath)
        {
            Sql.Append("CONCAT(");
        }

        Sql.Append("'$");

        foreach (var location in expression.Path)
        {
            if (location is XuguJsonArrayIndexExpression arrayIndexExpression)
            {
                var isConstantExpression = arrayIndexExpression.Expression is SqlConstantExpression;

                Sql.Append("[");

                if (!isConstantExpression)
                {
                    Sql.Append("', ");
                }

                Visit(arrayIndexExpression.Expression);

                if (!isConstantExpression)
                {
                    Sql.Append(", '");
                }

                Sql.Append("]");
            }
            else
            {
                Sql.Append(".");
                Visit(location);
            }
        }

        Sql.Append("'");

        if (!isSimplePath)
        {
            Sql.Append(")");
        }

        Sql.Append(")");

        return expression;
    }

    protected virtual bool JsonPathNeedsConcat(IReadOnlyList<PathSegment> path)
        => path.Any(segment => segment.ArrayIndex is not null && segment.ArrayIndex is not SqlConstantExpression);

    protected virtual void GenerateJsonPath(IReadOnlyList<PathSegment> path, bool? needsConcat = null)
    {
        needsConcat ??= JsonPathNeedsConcat(path);

        if (needsConcat.Value)
        {
            Sql.Append("CONCAT(");
        }

        Sql.Append("'$");

        foreach (var pathSegment in path)
        {
            switch (pathSegment)
            {
                case { PropertyName: string propertyName }:
                    Sql.Append(".").Append(propertyName);
                    break;

                case { ArrayIndex: SqlExpression arrayIndex }:
                    Sql.Append("[");

                    if (arrayIndex is SqlConstantExpression)
                    {
                        Visit(arrayIndex);
                    }
                    else
                    {
                        Sql.Append("', ");

                        Visit(
                            new SqlUnaryExpression(
                                ExpressionType.Convert,
                                arrayIndex,
                                typeof(string),
                                _typeMappingSource.GetMapping(typeof(string))));

                        Sql.Append(", '");
                    }

                    Sql.Append("]");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(path));
            }
        }

        Sql.Append("'");

        if (needsConcat.Value)
        {
            Sql.Append(")");
        }
    }
}
