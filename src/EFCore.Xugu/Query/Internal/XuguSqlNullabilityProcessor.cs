using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguSqlNullabilityProcessor : SqlNullabilityProcessor
{
    public XuguSqlNullabilityProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        RelationalParameterBasedSqlProcessorParameters parameters)
        : base(dependencies, parameters)
    {
    }

    /// <summary>
    /// Must call base so <see cref="ValuesExpression"/> parameter collections expand to constants
    /// before SQL generation. Identity override previously left ValuesParameter unexpanded.
    /// </summary>
    protected override TableExpressionBase Visit(TableExpressionBase tableExpressionBase)
    {
        if (tableExpressionBase is XuguPrimitiveCollectionTableExpression primitiveCollectionTable)
        {
            // CollectionExpression is a SqlExpression (parameter/column/json); only parameters need
            // nullability/parameter processing via the SqlExpression Visit path.
            if (primitiveCollectionTable.CollectionExpression is SqlExpression sqlCollection)
            {
                var visited = Visit(sqlCollection, allowOptimizedExpansion: false, out _);
                if (!ReferenceEquals(visited, sqlCollection))
                {
                    return primitiveCollectionTable.Update(visited);
                }
            }

            return primitiveCollectionTable;
        }

        return base.Visit(tableExpressionBase);
    }

#pragma warning disable EF1001
    protected override bool IsCollectionTable(TableExpressionBase table, [NotNullWhen(true)] out Expression? collection)
    {
        if (table is XuguPrimitiveCollectionTableExpression primitiveCollectionTable)
        {
            collection = primitiveCollectionTable.CollectionExpression;
            return true;
        }

        return base.IsCollectionTable(table, out collection);
    }

    protected override TableExpressionBase UpdateParameterCollection(
        TableExpressionBase table,
        SqlParameterExpression newCollectionParameter)
        => table is XuguPrimitiveCollectionTableExpression primitiveCollectionTable
            ? primitiveCollectionTable.Update(newCollectionParameter)
            : base.UpdateParameterCollection(table, newCollectionParameter);
#pragma warning restore EF1001

    protected override SqlExpression VisitCustomSqlExpression(
        SqlExpression sqlExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
        => sqlExpression switch
        {
            XuguComplexFunctionArgumentExpression complexFunctionArgument
                => VisitComplexFunctionArgument(complexFunctionArgument, allowOptimizedExpansion, out nullable),
            XuguColumnAliasReferenceExpression columnAliasReferenceExpression
                => VisitColumnAliasReference(columnAliasReferenceExpression, allowOptimizedExpansion, out nullable),
            XuguJsonTraversalExpression jsonTraversalExpression
                => VisitJsonTraversal(jsonTraversalExpression, allowOptimizedExpansion, out nullable),
            XuguJsonArrayIndexExpression jsonArrayIndexExpression
                => VisitJsonArrayIndex(jsonArrayIndexExpression, allowOptimizedExpansion, out nullable),
            _ => base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable)
        };

    private SqlExpression VisitColumnAliasReference(
        XuguColumnAliasReferenceExpression columnAliasReferenceExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        var expression = Visit(
            columnAliasReferenceExpression.Expression,
            allowOptimizedExpansion,
            out nullable);

        return columnAliasReferenceExpression.Update(columnAliasReferenceExpression.Alias, expression);
    }

    private SqlExpression VisitComplexFunctionArgument(
        XuguComplexFunctionArgumentExpression complexFunctionArgument,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        nullable = false;

        var argumentParts = new SqlExpression[complexFunctionArgument.ArgumentParts.Count];

        for (var i = 0; i < argumentParts.Length; i++)
        {
            argumentParts[i] = Visit(
                complexFunctionArgument.ArgumentParts[i],
                allowOptimizedExpansion,
                out var argumentPartNullable);
            nullable |= argumentPartNullable;
        }

        return complexFunctionArgument.Update(argumentParts, complexFunctionArgument.Delimiter);
    }

    private SqlExpression VisitJsonTraversal(
        XuguJsonTraversalExpression jsonTraversalExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        var expression = Visit(jsonTraversalExpression.Expression, allowOptimizedExpansion, out nullable);

        var path = new SqlExpression[jsonTraversalExpression.Path.Count];
        for (var i = 0; i < path.Length; i++)
        {
            path[i] = Visit(jsonTraversalExpression.Path[i], allowOptimizedExpansion, out var pathNullable);
            nullable |= pathNullable;
        }

        return jsonTraversalExpression.Update(expression, path);
    }

    private SqlExpression VisitJsonArrayIndex(
        XuguJsonArrayIndexExpression jsonArrayIndexExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        var expression = Visit(jsonArrayIndexExpression.Expression, allowOptimizedExpansion, out nullable);
        return jsonArrayIndexExpression.Update(expression);
    }
}
