using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal;

public class XuguQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
    private readonly XuguQueryCompilationContext _xuguQueryCompilationContext;

    public XuguQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        XuguQueryCompilationContext relationalQueryCompilationContext,
        IXuguOptions options)
        : base(dependencies, relationalDependencies, relationalQueryCompilationContext)
    {
        _xuguQueryCompilationContext = relationalQueryCompilationContext;
        _ = options;
    }

    protected XuguQueryableMethodTranslatingExpressionVisitor(
        XuguQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor)
        => _xuguQueryCompilationContext = parentVisitor._xuguQueryCompilationContext;

    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new XuguQueryableMethodTranslatingExpressionVisitor(this);

    protected override ShapedQueryExpression? TranslatePrimitiveCollection(
        SqlExpression sqlExpression,
        IProperty? property,
        string tableAlias)
    {
        var elementType = sqlExpression.Type.IsArray
            ? sqlExpression.Type.GetElementType()!
            : sqlExpression.Type.GetGenericArguments().First();
        var elementTypeMapping = (RelationalTypeMapping?)sqlExpression.TypeMapping?.ElementTypeMapping
            ?? RelationalDependencies.TypeMappingSource.FindMapping(elementType);
        var elementNullable = property?.GetElementType()?.IsNullable
            ?? (Nullable.GetUnderlyingType(elementType) is not null || !elementType.IsValueType);
        if (sqlExpression is SqlParameterExpression parameterExpression
            && sqlExpression.TypeMapping is null)
        {
            sqlExpression = parameterExpression.ApplyTypeMapping(
                new XuguPrimitiveCollectionTypeMapping(sqlExpression.Type));
        }

        var keyTypeMapping = RelationalDependencies.TypeMappingSource.FindMapping(typeof(int))!;
        var table = new XuguPrimitiveCollectionTableExpression(
            tableAlias,
            sqlExpression,
            elementType,
            elementTypeMapping);

#pragma warning disable EF1001
        var selectExpression = new SelectExpression(
            [table],
            new ColumnExpression(
                "value",
                tableAlias,
                Nullable.GetUnderlyingType(elementType) ?? elementType,
                elementTypeMapping,
                elementNullable),
            identifier:
            [
                (new ColumnExpression("key", tableAlias, typeof(int), keyTypeMapping, nullable: false),
                    keyTypeMapping.Comparer)
            ],
            _xuguQueryCompilationContext.SqlAliasManager);
#pragma warning restore EF1001
        var keyColumn = selectExpression.CreateColumnExpression(
            table,
            "key",
            typeof(int),
            keyTypeMapping,
            columnNullable: false);
        var collectionLength = new SqlFunctionExpression(
            "JSON_LENGTH",
            [sqlExpression],
            nullable: true,
            argumentsPropagateNullability: [true],
            typeof(int),
            keyTypeMapping);
        selectExpression.ApplyPredicate(
            new SqlBinaryExpression(
                ExpressionType.LessThanOrEqual,
                keyColumn,
                collectionLength,
                typeof(bool),
                RelationalDependencies.TypeMappingSource.FindMapping(typeof(bool))));


        selectExpression.AppendOrdering(
            new OrderingExpression(
                selectExpression.CreateColumnExpression(
                    table,
                    "key",
                    typeof(int),
                    keyTypeMapping,
                    columnNullable: false),
                ascending: true));

        var shaperType = elementType.IsValueType && Nullable.GetUnderlyingType(elementType) is null
            ? typeof(Nullable<>).MakeGenericType(elementType)
            : elementType;
        Expression shaperExpression = new ProjectionBindingExpression(
            selectExpression,
            new ProjectionMember(),
            shaperType);

        if (shaperExpression.Type != elementType)
        {
            shaperExpression = Expression.Convert(shaperExpression, elementType);
        }

        return new ShapedQueryExpression(selectExpression, shaperExpression);
    }

    protected override bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        StructuralTypeShaperExpression shaper,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression.Offset == null
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && (selectExpression.Tables.Count == 1 || selectExpression.Orderings.Count == 0))
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                var projectionBindingExpression = (ProjectionBindingExpression)shaper.ValueBufferExpression;
                var entityProjectionExpression = (StructuralTypeProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = entityProjectionExpression.BindProperty(shaper.StructuralType.GetProperties().First());
                table = selectExpression.GetTable(column).UnwrapJoin();
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    protected override bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        TableExpressionBase targetTable,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression is
            {
                Offset: null,
                IsDistinct: false,
                GroupBy: [],
                Having: null,
                Orderings: []
            }
            && (selectExpression.Tables.Count == 1 || selectExpression.Limit is null))
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                table = targetTable;

                if (selectExpression.Tables.Count > 1
                    && table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }
}
