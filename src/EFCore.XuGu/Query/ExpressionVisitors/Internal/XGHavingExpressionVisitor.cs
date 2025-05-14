// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    public class XGHavingExpressionVisitor : ExpressionVisitor
    {
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;
        private XGContainsAggregateFunctionExpressionVisitor _containsAggregateFunctionExpressionVisitor;

        public XGHavingExpressionVisitor(XGSqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
            {
                SelectExpression selectExpression => VisitSelect(selectExpression),
                ShapedQueryExpression shapedQueryExpression => shapedQueryExpression.Update(
                    Visit(shapedQueryExpression.QueryExpression), Visit(shapedQueryExpression.ShaperExpression)),
                _ => base.VisitExtension(extensionExpression)
            };

        protected virtual Expression VisitSelect(SelectExpression selectExpression)
        {
            // XG currently do not support complex expressions in HAVING clauses (e.g. function calls).
            // Instead, they want you to reference SELECT aliases for those expressions in the HAVING clause.
            // This is only an issue for HAVING expressions that do not contain any aggregate functions.
            var havingExpression = selectExpression.Having;
            if (havingExpression is not null &&
                havingExpression is not SqlConstantExpression &&
                havingExpression is not XGColumnAliasReferenceExpression)
            {
                _containsAggregateFunctionExpressionVisitor ??= new XGContainsAggregateFunctionExpressionVisitor();
                if (!_containsAggregateFunctionExpressionVisitor.ProcessUntilSelect(havingExpression))
                {
                    selectExpression.PushdownIntoSubquery();
                    var subQuery = (SelectExpression) selectExpression.Tables.Single();

                    var projectionIndex = subQuery.AddToProjection(havingExpression);
                    var alias = subQuery.Projection[projectionIndex].Alias;

                    var columnAliasReferenceExpression = _sqlExpressionFactory.ColumnAliasReference(
                        alias,
                        havingExpression,
                        havingExpression.Type,
                        havingExpression.TypeMapping);

                    // Having expressions, not containing an aggregate function, need to be part of the GROUP BY clause, because they now also
                    // appear as part of the SELECT clause.
                    var groupBy = subQuery.GroupBy.ToList();
                    groupBy.Add(columnAliasReferenceExpression);

                    subQuery = subQuery.Update(
                        subQuery.Projection,
                        subQuery.Tables,
                        subQuery.Predicate,
                        groupBy,
                        columnAliasReferenceExpression,
                        subQuery.Orderings,
                        subQuery.Limit,
                        subQuery.Offset);

                    selectExpression = selectExpression.Update(
                        selectExpression.Projection,
                        new[] {subQuery},
                        selectExpression.Predicate,
                        selectExpression.GroupBy,
                        selectExpression.Having,
                        selectExpression.Orderings,
                        selectExpression.Limit,
                        selectExpression.Offset);
                }
            }

            return base.VisitExtension(selectExpression);
        }
    }
}
