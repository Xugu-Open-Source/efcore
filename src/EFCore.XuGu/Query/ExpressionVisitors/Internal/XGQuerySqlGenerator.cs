// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using EntityFrameworkCore.XuGu.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EFCore.XuGu.Query.ExpressionVisitors.Internal;

namespace EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGQuerySqlGenerator : QuerySqlGenerator
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private IRelationalCommandBuilder _relationalCommandBuilder;
        private readonly List<Type> NumTypes= new List<Type> { typeof(int), typeof(long), typeof(short), typeof(byte), typeof(decimal), typeof(double), typeof(float) };
        // The order in which the types are specified matters, because types get matched by using StartsWith.

        private static readonly Dictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " <> " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },
            { ExpressionType.Add, " + " },
            { ExpressionType.Subtract, " - " },
            { ExpressionType.Multiply, " * " },
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },
            { ExpressionType.And, " & " },
            { ExpressionType.Or, " | " },
            { ExpressionType.Negate, "-" },
        };
        private static readonly Dictionary<string, string[]> _castMappings = new Dictionary<string, string[]>
        {
            { "signed", new []{ "tinyint", "smallint", "mediumint", "int", "bigint", "bit" }},
            { "decimal(38,17)", new []{ "decimal" } },
            { "double", new []{ "double" } },
            { "float", new []{ "float" } },
            { "binary", new []{ "binary", "blob" } },
            { "datetime(6)", new []{ "datetime(6)" } },
            { "datetime", new []{ "datetime" } },
            { "date", new []{ "date" } },
            { "timestamp(6)", new []{ "timestamp(6)" } },
            { "timestamp", new []{ "timestamp" } },
            { "time(3)", new []{ "time(3)" } },
            { "time", new []{ "time" } },
            { "json", new []{ "json" } },
            { "char", new []{ "char", "varchar", "clob" } },
            { "nchar", new []{ "char", "varchar" } },
        };

        private const ulong LimitUpperBound = 18446744073709551610;

        [NotNull] private readonly XGSqlExpressionFactory _sqlExpressionFactory;
        private readonly IXGOptions _options;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XGQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] XGSqlExpressionFactory sqlExpressionFactory,
            [CanBeNull] IXGOptions options)
            : base(dependencies)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _options = options;
            Dependencies = dependencies;

            _relationalCommandBuilderFactory = dependencies.RelationalCommandBuilderFactory;
            _sqlGenerationHelper = dependencies.SqlGenerationHelper;
        }

        protected override QuerySqlGeneratorDependencies Dependencies { get; }

        public override IRelationalCommand GetCommand([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            _relationalCommandBuilder = _relationalCommandBuilderFactory.Create();

            GenerateTagsHeaderComment(selectExpression);

            if (selectExpression.IsNonComposedFromSql())
            {
                GenerateFromSql((FromSqlExpression)selectExpression.Tables[0]);
            }
            else
            {
                VisitSelect(selectExpression);
            }

            return _relationalCommandBuilder.Build();
        }

        private bool IsNonComposedSetOperation(SelectExpression selectExpression)
            => selectExpression.Offset == null
                && selectExpression.Limit == null
                && !selectExpression.IsDistinct
                && selectExpression.Predicate == null
                && selectExpression.Having == null
                && selectExpression.Orderings.Count == 0
                && selectExpression.GroupBy.Count == 0
                && selectExpression.Tables.Count == 1
                && selectExpression.Tables[0] is SetOperationBase setOperation
                && selectExpression.Projection.Count == setOperation.Source1.Projection.Count
                && selectExpression.Projection.Select(
                        (pe, index) => pe.Expression is ColumnExpression column
                            && string.Equals(column.Table.Alias, setOperation.Alias, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(
                                column.Name, setOperation.Source1.Projection[index].Alias, StringComparison.OrdinalIgnoreCase))
                    .All(e => e);

        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (IsNonComposedSetOperation(selectExpression))
            {
                // Naked set operation
                GenerateSetOperation((SetOperationBase)selectExpression.Tables[0]);

                return selectExpression;
            }

            IDisposable subQueryIndent = null;

            if (selectExpression.Alias != null)
            {
                _relationalCommandBuilder.AppendLine("(");
                subQueryIndent = _relationalCommandBuilder.Indent();
            }

            _relationalCommandBuilder.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                _relationalCommandBuilder.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);

            if (selectExpression.Projection.Any())
            {
                GenerateSelectList(selectExpression.Projection, e => Visit(e));
            }
            else
            {
                _relationalCommandBuilder.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                _relationalCommandBuilder.AppendLine().Append("FROM ");

                GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());
            }
            else
            {
                GeneratePseudoFromClause();
            }

            if (selectExpression.Predicate != null)
            {
                bool isCollate = false;
                if (selectExpression.Predicate is SqlBinaryExpression binaryExpression && (binaryExpression.Left is CollateExpression || binaryExpression.Right is CollateExpression))
                {
                    isCollate = true;
                }
                else
                {
                    isCollate = false;
                }
                if(!isCollate) _relationalCommandBuilder.AppendLine().Append("WHERE ");
                if (selectExpression.Predicate is SqlConstantExpression constantExpression && constantExpression.Type == typeof(bool))
                {
                    _relationalCommandBuilder.Append((bool)constantExpression.Value? "TRUE ": "FALSE ");
                }
                else if(selectExpression.Predicate is SqlBinaryExpression sqlBinaryExpression && sqlBinaryExpression.Left is ExistsExpression && (sqlBinaryExpression.OperatorType==ExpressionType.Equal || sqlBinaryExpression.OperatorType==ExpressionType.NotEqual))
                {
                    VisitExistsSql(sqlBinaryExpression.Left as ExistsExpression);
                    _relationalCommandBuilder.Append(sqlBinaryExpression.OperatorType == ExpressionType.Equal ? "=" : "!=");
                    if (sqlBinaryExpression.Right is ExistsExpression)
                    {
                        VisitExistsSql(sqlBinaryExpression.Right as ExistsExpression);
                    }
                }
                else
                {
                    if(!isCollate)
                    {
                        Visit(selectExpression.Predicate);
                    }
                    
                }
                
            }

            if (selectExpression.GroupBy.Count > 0)
            {
                _relationalCommandBuilder.AppendLine().Append("GROUP BY ");

                GenerateGroupByList(selectExpression.GroupBy, e => Visit(e));
            }

            if (selectExpression.Having != null)
            {
                _relationalCommandBuilder.AppendLine().Append("HAVING ");

                Visit(selectExpression.Having);
            }

            GenerateOrderings(selectExpression);
            GenerateLimitOffset(selectExpression);

            if (selectExpression.Alias != null)
            {
                subQueryIndent.Dispose();

                _relationalCommandBuilder.AppendLine()
                    .Append(")" + AliasSeparator + _sqlGenerationHelper.DelimitIdentifier(selectExpression.Alias));
            }

            return selectExpression;
        }

        protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            Check.NotNull(fromSqlExpression, nameof(fromSqlExpression));

            _relationalCommandBuilder.AppendLine("(");

            CheckComposableSql(fromSqlExpression.Sql);

            using (_relationalCommandBuilder.Indent())
            {
                GenerateFromSql(fromSqlExpression);
            }

            _relationalCommandBuilder.Append(")")
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(fromSqlExpression.Alias));

            return fromSqlExpression;
        }

        protected override Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            Check.NotNull(rowNumberExpression, nameof(rowNumberExpression));

            _relationalCommandBuilder.Append("ROW_NUMBER() OVER(");
            if (rowNumberExpression.Partitions.Any())
            {
                _relationalCommandBuilder.Append("PARTITION BY ");
                GenerateList(rowNumberExpression.Partitions, e => Visit(e));
                _relationalCommandBuilder.Append(" ");
            }

            _relationalCommandBuilder.Append("ORDER BY ");
            GenerateOrderList(rowNumberExpression.Orderings, e => Visit(e));
            _relationalCommandBuilder.Append(")");

            return rowNumberExpression;
        }

        protected override void GenerateSetOperation([NotNull] SetOperationBase setOperation)
        {
            Check.NotNull(setOperation, nameof(setOperation));

            GenerateSetOperationOperand(setOperation, setOperation.Source1);
            _relationalCommandBuilder.AppendLine();
            _relationalCommandBuilder.AppendLine($"{GetSetOperation(setOperation)}{(setOperation.IsDistinct ? "" : " ALL")}");
            GenerateSetOperationOperand(setOperation, setOperation.Source2);

            static string GetSetOperation(SetOperationBase operation)
                => operation switch
                {
                    ExceptExpression _ => "EXCEPT",
                    IntersectExpression _ => "INTERSECT",
                    UnionExpression _ => "UNION",
                    _ => throw new InvalidOperationException(CoreStrings.UnknownEntity("SetOperationType")),
                };
        }

        protected override Expression VisitCollate(CollateExpression collateExpresion)
        {
            //Check.NotNull(collateExpresion, nameof(collateExpresion));

            //Visit(collateExpresion.Operand);

            //_relationalCommandBuilder
            //    .Append(" COLLATE ")
            //    .Append(collateExpresion.Collation);

            return collateExpresion;
        }

        protected override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            Check.NotNull(crossJoinExpression, nameof(crossJoinExpression));

            _relationalCommandBuilder.Append("CROSS JOIN ");
            Visit(crossJoinExpression.Table);

            return crossJoinExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            Check.NotNull(crossApplyExpression, nameof(crossApplyExpression));

            //_relationalCommandBuilder.Append("INNER JOIN ");
            _relationalCommandBuilder.Append("INNER JOIN ");
            //var tableExpression = crossApplyExpression.Table as SelectExpression;
            //var joinExpression=new InnerJoinExpression(crossApplyExpression.Table, tableExpression.Predicate);
            Visit(crossApplyExpression.Table);
            //Visit(joinExpression);

            return crossApplyExpression;
            //return joinExpression;
        }
        protected override void GenerateOrderings([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Orderings.Any())
            {
                var orderings = selectExpression.Orderings.ToList();

                if (selectExpression.Limit == null
                    && selectExpression.Offset == null)
                {
                    orderings.RemoveAll(oe => oe.Expression is SqlConstantExpression || oe.Expression is SqlParameterExpression);
                }

                if (orderings.Count > 0)
                {
                    _relationalCommandBuilder.AppendLine()
                        .Append("ORDER BY ");

                    GenerateOrderList(orderings, e => Visit(e));
                }
            }
        }
        private void GenerateOrderList<T>(
                    IReadOnlyList<T> items,
                    Action<T> generationAction,
                    Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                if (items[i] is OrderingExpression orderingExpression && orderingExpression.Expression is SqlBinaryExpression sqlBinaryExpression && sqlBinaryExpression.Type==typeof(bool))
                {
                    _relationalCommandBuilder.Append("CASE WHEN ");
                    Visit(sqlBinaryExpression);
                    _relationalCommandBuilder.Append(" THEN TRUE ELSE FALSE END");
                }
                else if (items[i] is OrderingExpression pExpression && pExpression.Expression is ExistsExpression existsExpression)
                {
                    _relationalCommandBuilder.Append("CASE WHEN ");
                    Visit(existsExpression);
                    _relationalCommandBuilder.Append(" THEN TRUE ELSE FALSE END");
                }
                else if (items[i] is OrderingExpression prExpression && prExpression.Expression is LikeExpression likeExpression)
                {
                    _relationalCommandBuilder.Append("CASE WHEN ");
                    Visit(likeExpression);
                    _relationalCommandBuilder.Append(" THEN TRUE ELSE FALSE END");
                }
                else
                {
                    generationAction(items[i]);
                }
            }
        }
        protected override Expression VisitLeftJoin(LeftJoinExpression leftJoinExpression)
        {
            Check.NotNull(leftJoinExpression, nameof(leftJoinExpression));

            _relationalCommandBuilder.Append("LEFT JOIN ");
            Visit(leftJoinExpression.Table);
            _relationalCommandBuilder.Append(" ON ");
            Visit(leftJoinExpression.JoinPredicate);

            return leftJoinExpression;
        }

        protected override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            Check.NotNull(innerJoinExpression, nameof(innerJoinExpression));

            _relationalCommandBuilder.Append("INNER JOIN ");
            Visit(innerJoinExpression.Table);
            _relationalCommandBuilder.Append(" ON ");
            Visit(innerJoinExpression.JoinPredicate);

            return innerJoinExpression;
        }

        protected override Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Name, tableExpression.Schema))
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }
        protected override Expression VisitColumn(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            _relationalCommandBuilder
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Table.Alias))
                .Append(".")
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

            return columnExpression;
        }

        private void GenerateSelectList<T>(
                    IReadOnlyList<T> items,
                    Action<T> generationAction,
                    Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                if(items[i] is ProjectionExpression projectionExpression && projectionExpression.Expression is SqlBinaryExpression sqlBinaryExpression && (sqlBinaryExpression.OperatorType==ExpressionType.Or || sqlBinaryExpression.OperatorType==ExpressionType.And || sqlBinaryExpression.OperatorType==ExpressionType.AndAlso||sqlBinaryExpression.OperatorType==ExpressionType.OrElse))
                {
                    _relationalCommandBuilder.Append("CASE WHEN(");
                    Visit(sqlBinaryExpression);
                    _relationalCommandBuilder.Append(") THEN TRUE ELSE FALSE END");
                    if (!string.IsNullOrEmpty(projectionExpression.Alias))
                    {
                        _relationalCommandBuilder.Append(" AS ").Append(_sqlGenerationHelper.DelimitIdentifier(projectionExpression.Alias));
                    }
                }
                //else if (items[i] is ProjectionExpression pExpression && pExpression.Expression is ExistsExpression existsExpression)
                //{
                //    _relationalCommandBuilder.Append("CASE WHEN ");
                //    Visit(existsExpression);
                //    _relationalCommandBuilder.Append(" THEN TRUE ELSE FALSE END");
                //    if (!string.IsNullOrEmpty(pExpression.Alias))
                //    {
                //        _relationalCommandBuilder.Append(" AS ").Append(_sqlGenerationHelper.DelimitIdentifier(pExpression.Alias));
                //    }
                //}
                //else if (items[i] is ProjectionExpression prExpression && prExpression.Expression is LikeExpression likeExpression)
                //{
                //    _relationalCommandBuilder.Append("CASE WHEN ");
                //    Visit(likeExpression);
                //    _relationalCommandBuilder.Append(" THEN TRUE ELSE FALSE END");
                //    if (!string.IsNullOrEmpty(prExpression.Alias))
                //    {
                //        _relationalCommandBuilder.Append(" AS ").Append(_sqlGenerationHelper.DelimitIdentifier(prExpression.Alias));
                //    }
                //}
                else if (items[i] is ProjectionExpression pExpression && pExpression.Type==typeof(bool) && (pExpression.Expression is ExistsExpression || pExpression.Expression is LikeExpression || (pExpression.Expression is SqlUnaryExpression && ((SqlUnaryExpression)pExpression.Expression).OperatorType!=ExpressionType.Negate && ((SqlUnaryExpression)pExpression.Expression).OperatorType != ExpressionType.NegateChecked)))
                {
                    _relationalCommandBuilder.Append("CASE WHEN ");
                    Visit(pExpression);
                    _relationalCommandBuilder.Append(" THEN TRUE ELSE FALSE END");
                    if (!string.IsNullOrEmpty(pExpression.Alias))
                    {
                        _relationalCommandBuilder.Append(" AS ").Append(_sqlGenerationHelper.DelimitIdentifier(pExpression.Alias));
                    }
                }
                else if(items[i] is ProjectionExpression projectionExpression1 && projectionExpression1.Expression is SqlBinaryExpression sqlBinaryExpression1 && (sqlBinaryExpression1.OperatorType==ExpressionType.Add || sqlBinaryExpression1.OperatorType==ExpressionType.Subtract) && NumTypes.Contains(sqlBinaryExpression1.Type))
                {
                    _relationalCommandBuilder.Append(" NVL(");
                    Visit(sqlBinaryExpression1.Left);
                    _relationalCommandBuilder.Append(",0)");
                    if (sqlBinaryExpression1.OperatorType == ExpressionType.Subtract)
                    {
                        _relationalCommandBuilder.Append(" - ");
                    }
                    else
                    {
                        _relationalCommandBuilder.Append(" + ");
                    }
                    _relationalCommandBuilder.Append(" NVL(");
                    Visit(sqlBinaryExpression1.Right);
                    _relationalCommandBuilder.Append(",0)");
                }
                else
                {
                    generationAction(items[i]);
                }
            }
        }

        private void GenerateList<T>(
                    IReadOnlyList<T> items,
                    Action<T> generationAction,
                    Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                generationAction(items[i]);
            }
        }
        private void GenerateGroupByList<T>(
                    IReadOnlyList<T> items,
                    Action<T> generationAction,
                    Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                if (items[i] is SqlBinaryExpression sqlBinaryExpression && (sqlBinaryExpression.OperatorType == ExpressionType.Or || sqlBinaryExpression.OperatorType == ExpressionType.And || sqlBinaryExpression.OperatorType == ExpressionType.AndAlso || sqlBinaryExpression.OperatorType == ExpressionType.OrElse))
                {
                    _relationalCommandBuilder.Append("CASE WHEN(");
                    Visit(sqlBinaryExpression);
                    _relationalCommandBuilder.Append(") THEN TRUE ELSE FALSE END");
                }
                else if (items[i] is ExistsExpression existsExpression)
                {
                    _relationalCommandBuilder.Append("CASE WHEN ");
                    Visit(existsExpression);
                    _relationalCommandBuilder.Append(" THEN TRUE ELSE FALSE END");
                }
                else if (items[i] is LikeExpression likeExpression)
                {
                    _relationalCommandBuilder.Append("CASE WHEN ");
                    Visit(likeExpression);
                    _relationalCommandBuilder.Append(" THEN TRUE ELSE FALSE END");
                }
                else if (items[i] is ProjectionExpression projectionExpression1 && projectionExpression1.Expression is SqlBinaryExpression sqlBinaryExpression1 && (sqlBinaryExpression1.OperatorType == ExpressionType.Add || sqlBinaryExpression1.OperatorType == ExpressionType.Subtract) && NumTypes.Contains(sqlBinaryExpression1.Type))
                {
                    _relationalCommandBuilder.Append(" NVL(");
                    Visit(sqlBinaryExpression1.Left);
                    _relationalCommandBuilder.Append(",0)");
                    if (sqlBinaryExpression1.OperatorType == ExpressionType.Subtract)
                    {
                        _relationalCommandBuilder.Append(" - ");
                    }
                    else
                    {
                        _relationalCommandBuilder.Append(" + ");
                    }
                    _relationalCommandBuilder.Append(" NVL(");
                    Visit(sqlBinaryExpression1.Right);
                    _relationalCommandBuilder.Append(",0)");
                }
                else
                {
                    generationAction(items[i]);
                }
            }
        }

        private void GenerateFromSql(FromSqlExpression fromSqlExpression)
        {
            var sql = fromSqlExpression.Sql;
            string[] substitutions = null;

            switch (fromSqlExpression.Arguments)
            {
                case ConstantExpression constantExpression
                    when constantExpression.Value is CompositeRelationalParameter compositeRelationalParameter:
                    {
                        var subParameters = compositeRelationalParameter.RelationalParameters;
                        substitutions = new string[subParameters.Count];
                        for (var i = 0; i < subParameters.Count; i++)
                        {
                            substitutions[i] = GenerateParameterName(subParameters[i].InvariantName);
                        }

                        _relationalCommandBuilder.AddParameter(compositeRelationalParameter);

                        break;
                    }

                case ConstantExpression constantExpression
                    when constantExpression.Value is object[] constantValues:
                    {
                        substitutions = new string[constantValues.Length];
                        for (var i = 0; i < constantValues.Length; i++)
                        {
                            var value = constantValues[i];
                            if (value is RawRelationalParameter rawRelationalParameter)
                            {
                                substitutions[i] = GenerateParameterName(rawRelationalParameter.InvariantName);
                                _relationalCommandBuilder.AddParameter(rawRelationalParameter);
                            }
                            else if (value is SqlConstantExpression sqlConstantExpression)
                            {
                                substitutions[i] = sqlConstantExpression.TypeMapping.GenerateSqlLiteral(sqlConstantExpression.Value);
                            }
                        }

                        break;
                    }
            }

            if (substitutions != null)
            {
                // ReSharper disable once CoVariantArrayConversion
                // InvariantCulture not needed since substitutions are all strings
                sql = string.Format(sql, substitutions);
            }

            _relationalCommandBuilder.AppendLines(sql);
        }

        //protected override Expression VisitExtension(Expression extensionExpression)
        //    => extensionExpression switch
        //    {
        //        XGJsonTraversalExpression jsonTraversalExpression => VisitJsonPathTraversal(jsonTraversalExpression),
        //        _ => base.VisitExtension(extensionExpression)
        //    };

        protected override Expression VisitIn(InExpression inExpression)
        {
            Check.NotNull(inExpression, nameof(inExpression));

            if (inExpression.Values != null)
            {
                Visit(inExpression.Item);
                _relationalCommandBuilder.Append(inExpression.IsNegated ? " NOT IN " : " IN ");
                _relationalCommandBuilder.Append("(");
                var valuesConstant = (SqlConstantExpression)inExpression.Values;
                var valuesList = ((IEnumerable<object>)valuesConstant.Value)
                    .Select(v => new SqlConstantExpression(Expression.Constant(v), valuesConstant.TypeMapping)).ToList();
                GenerateList(valuesList, e => Visit(e));
                _relationalCommandBuilder.Append(")");
            }
            else
            {
                Visit(inExpression.Item);
                _relationalCommandBuilder.Append(inExpression.IsNegated ? " NOT IN " : " IN ");
                _relationalCommandBuilder.AppendLine("(");

                using (_relationalCommandBuilder.Indent())
                {
                    Visit(inExpression.Subquery);
                }

                _relationalCommandBuilder.AppendLine().Append(")");
            }

            return inExpression;
        }

        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            Check.NotNull(orderingExpression, nameof(orderingExpression));

            if (orderingExpression.Expression is SqlConstantExpression
                || orderingExpression.Expression is SqlParameterExpression)
            {
                _relationalCommandBuilder.Append("(SELECT 1)");
            }
            else
            {
                Visit(orderingExpression.Expression);
            }

            if (!orderingExpression.IsAscending)
            {
                _relationalCommandBuilder.Append(" DESC");
            }

            return orderingExpression;
        }

        protected virtual Expression VisitJsonPathTraversal(XGJsonTraversalExpression expression)
        {
            // If the path contains parameters, then the -> and ->> aliases are not supported by XG, because
            // we need to concatenate the path and the parameters.
            // We will use JSON_EXTRACT (and JSON_UNQUOTE if needed) only in this case, because the aliases
            // are much more readable.
            var isSimplePath = expression.Path.All(
                l => l is SqlConstantExpression ||
                     l is XGJsonArrayIndexExpression e && e.Expression is SqlConstantExpression);

            if (expression.ReturnsText)
            {
                Sql.Append("JSON_UNQUOTE(");
            }

            if (expression.Path.Count > 0)
            {
                Sql.Append("JSON_EXTRACT(");
            }

            Visit(expression.Expression);

            if (expression.Path.Count > 0)
            {
                Sql.Append(", ");

                if (!isSimplePath)
                {
                    Sql.Append("CONCAT(");
                }

                Sql.Append("'$");

                foreach (var location in expression.Path)
                {
                    if (location is XGJsonArrayIndexExpression arrayIndexExpression)
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
            }

            if (expression.ReturnsText)
            {
                Sql.Append(")");
            }

            return expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void GenerateLimitOffset([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            
            if (selectExpression.Limit != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("LIMIT ");

                Visit(selectExpression.Limit);
            }
            if (selectExpression.Offset != null)
            {
                if (selectExpression.Limit == null)
                {
                    _relationalCommandBuilder.AppendLine().Append("LIMIT 9999999");
                }
                _relationalCommandBuilder.AppendLine()
                    .Append("OFFSET ");

                Visit(selectExpression.Offset);
            }
        }

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.Name.StartsWith("@@", StringComparison.Ordinal))
            {
                _relationalCommandBuilder.Append(sqlFunctionExpression.Name);

                return sqlFunctionExpression;
            }

            if (sqlFunctionExpression.IsBuiltIn)
            {
                if (sqlFunctionExpression.Instance != null)
                {
                    Visit(sqlFunctionExpression.Instance);
                    _relationalCommandBuilder.Append(".");
                }

                _relationalCommandBuilder.Append(sqlFunctionExpression.Name);
            }
            else
            {
                if (!string.IsNullOrEmpty(sqlFunctionExpression.Schema))
                {
                    _relationalCommandBuilder
                        .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.Schema))
                        .Append(".");
                }

                _relationalCommandBuilder
                    .Append(_sqlGenerationHelper.DelimitIdentifier(sqlFunctionExpression.Name));
            }

            if (!sqlFunctionExpression.IsNiladic)
            {
                _relationalCommandBuilder.Append("(");
                GenerateList(sqlFunctionExpression.Arguments, e => Visit(e));
                _relationalCommandBuilder.Append(")");
            }

            return sqlFunctionExpression;
        }

        protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        {
            _relationalCommandBuilder.Append("LEFT JOIN ");

            Visit(outerApplyExpression.Table);

            _relationalCommandBuilder.Append(" ON TRUE");

            return outerApplyExpression;
        }

        protected override Expression VisitExists(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, nameof(existsExpression));

            
            bool isSelect=_relationalCommandBuilder.ToString().EndsWith("SELECT ")|| _relationalCommandBuilder.ToString()=="SELECT ";

            if (isSelect)
            {
                _relationalCommandBuilder.AppendLine("CASE WHEN ");
                if (existsExpression.IsNegated)
                {
                    _relationalCommandBuilder.Append("NOT ");
                }
                _relationalCommandBuilder.AppendLine("EXISTS (");
            }
            else
            {
                if (existsExpression.IsNegated)
                {
                    _relationalCommandBuilder.Append("NOT ");
                }
                _relationalCommandBuilder.Append("EXISTS (");
            }

            using (_relationalCommandBuilder.Indent())
            {
                Visit(existsExpression.Subquery);
            }

            if (isSelect)
            {
                _relationalCommandBuilder.Append(") THEN true ELSE false END AS Exists_Flag FROM dual");
            }
            else
            {
                _relationalCommandBuilder.Append(")");
            }

            return existsExpression;
        }
        protected virtual Expression VisitExistsSql(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, nameof(existsExpression));
            _relationalCommandBuilder.AppendLine("(SELECT CASE WHEN EXISTS (");
            using (_relationalCommandBuilder.Indent())
            {
                Visit(existsExpression.Subquery);
            }

            _relationalCommandBuilder.Append(") THEN true ELSE false END AS Exists_Flag FROM dual)");

            return existsExpression;
        }

        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            Check.NotNull(sqlBinaryExpression, nameof(sqlBinaryExpression));

            var requiresBrackets = RequiresBrackets(sqlBinaryExpression.Left);

            if (requiresBrackets)
            {
                _relationalCommandBuilder.Append("(");
            }

            Visit(sqlBinaryExpression.Left);

            if (requiresBrackets)
            {
                _relationalCommandBuilder.Append(")");
            }

            _relationalCommandBuilder.Append(GetOperator(sqlBinaryExpression));

            requiresBrackets = RequiresBrackets(sqlBinaryExpression.Right);

            if (requiresBrackets)
            {
                _relationalCommandBuilder.Append("(");
            }

            Visit(sqlBinaryExpression.Right);

            if (requiresBrackets)
            {
                _relationalCommandBuilder.Append(")");
            }

            return sqlBinaryExpression;
        }

        protected override string GetOperator([NotNull] SqlBinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            if (binaryExpression.Type== typeof(bool) && binaryExpression.OperatorType == ExpressionType.And)
            {
                return " AND ";
            }
            if (binaryExpression.Type == typeof(bool) && binaryExpression.OperatorType == ExpressionType.Or)
            {
                return " OR ";
            }

            return _operatorMap[binaryExpression.OperatorType];
        }

        private static bool RequiresBrackets(SqlExpression expression)
            => expression is SqlBinaryExpression ||
               expression is LikeExpression;

        public virtual Expression VisitXGRegexp(XGRegexpExpression XGRegexpExpression)
        {
            Check.NotNull(XGRegexpExpression, nameof(XGRegexpExpression));

            Visit(XGRegexpExpression.Match);
            _relationalCommandBuilder.Append(" REGEXP ");
            Visit(XGRegexpExpression.Pattern);

            return XGRegexpExpression;
        }

        public Expression VisitXGMatch(XGMatchExpression XGMatchExpression)
        {
            Check.NotNull(XGMatchExpression, nameof(XGMatchExpression));

            _relationalCommandBuilder.Append("CONTAINS ");
            _relationalCommandBuilder.Append("(");
            Visit(XGMatchExpression.Match);
            _relationalCommandBuilder.Append(" , ");
            Visit(XGMatchExpression.Against);

            _relationalCommandBuilder.Append(")");

            return XGMatchExpression;
        }

        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            Check.NotNull(projectionExpression, nameof(projectionExpression));

            Visit(projectionExpression.Expression);

            if (!string.Equals(string.Empty, projectionExpression.Alias)
                && !(projectionExpression.Expression is ColumnExpression column
                    && string.Equals(column.Name, projectionExpression.Alias)))
            {
                _relationalCommandBuilder.Append(AliasSeparator + _sqlGenerationHelper.DelimitIdentifier(projectionExpression.Alias));
            }

            return projectionExpression;
        }

        protected override Expression VisitLike(LikeExpression likeExpression)
        {
            Check.NotNull(likeExpression, nameof(likeExpression));

            Visit(likeExpression.Match);
            _relationalCommandBuilder.Append(" LIKE ");
            Visit(likeExpression.Pattern);

            if (likeExpression.EscapeChar != null)
            {
                _relationalCommandBuilder.Append(" ESCAPE ");
                Visit(likeExpression.EscapeChar);
            }

            return likeExpression;
        }

        protected override Expression VisitScalarSubquery(ScalarSubqueryExpression scalarSubqueryExpression)
        {
            Check.NotNull(scalarSubqueryExpression, nameof(scalarSubqueryExpression));

            _relationalCommandBuilder.AppendLine("(");
            using (_relationalCommandBuilder.Indent())
            {
                Visit(scalarSubqueryExpression.Subquery);
            }

            _relationalCommandBuilder.Append(")");

            return scalarSubqueryExpression;
        }

        protected override Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        {
            Check.NotNull(sqlFragmentExpression, nameof(sqlFragmentExpression));

            _relationalCommandBuilder.Append(sqlFragmentExpression.Sql);

            return sqlFragmentExpression;
        }

        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        {
            Check.NotNull(sqlUnaryExpression, nameof(sqlUnaryExpression));

            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Convert:
                    {
                        _relationalCommandBuilder.Append("CAST(");
                        var requiresBrackets = RequiresBrackets(sqlUnaryExpression.Operand);
                        if (requiresBrackets)
                        {
                            _relationalCommandBuilder.Append("(");
                        }

                        Visit(sqlUnaryExpression.Operand);
                        if (requiresBrackets)
                        {
                            _relationalCommandBuilder.Append(")");
                        }

                        _relationalCommandBuilder.Append(" AS ");
                        _relationalCommandBuilder.Append(sqlUnaryExpression.TypeMapping.StoreType);
                        _relationalCommandBuilder.Append(")");
                        break;
                    }

                case ExpressionType.Not
                    when sqlUnaryExpression.Type == typeof(bool):
                    {
                        _relationalCommandBuilder.Append("NOT (");
                        Visit(sqlUnaryExpression.Operand);
                        _relationalCommandBuilder.Append(")");
                        break;
                    }

                case ExpressionType.Not:
                    {
                        _relationalCommandBuilder.Append("~");
                        Visit(sqlUnaryExpression.Operand);
                        break;
                    }

                case ExpressionType.Equal:
                    {
                        Visit(sqlUnaryExpression.Operand);
                        _relationalCommandBuilder.Append(" IS NULL");
                        break;
                    }

                case ExpressionType.NotEqual:
                    {
                        Visit(sqlUnaryExpression.Operand);
                        _relationalCommandBuilder.Append(" IS NOT NULL");
                        break;
                    }

                case ExpressionType.Negate:
                    {
                        _relationalCommandBuilder.Append("-");
                        var requiresBrackets = RequiresBrackets(sqlUnaryExpression.Operand);
                        if (requiresBrackets)
                        {
                            _relationalCommandBuilder.Append("(");
                        }

                        Visit(sqlUnaryExpression.Operand);
                        if (requiresBrackets)
                        {
                            _relationalCommandBuilder.Append(")");
                        }

                        break;
                    }
            }

            return sqlUnaryExpression;
        }


        private SqlUnaryExpression VisitConvert(SqlUnaryExpression sqlUnaryExpression)
        {
            var castMapping = GetCastStoreType(sqlUnaryExpression.TypeMapping);

            if (castMapping == "binary")
            {
                _relationalCommandBuilder.Append("UNHEX(HEX(");
                Visit(sqlUnaryExpression.Operand);
                _relationalCommandBuilder.Append("))");
                return sqlUnaryExpression;
            }

            // There needs to be no CAST() applied between the exact same store type. This could happen, e.g. if
            // `System.DateTime` and `System.DateTimeOffset` are used in conjunction, because both use different type
            // mappings, but map to the same store type (e.g. `datetime(6)`).
            //
            // There also is no need for a double CAST() to the same type. Due to only rudimentary CAST() support in
            // XG, the final store type of a CAST() operation might be different than the store type of the type
            // mapping of the expression (e.g. "float" will be cast to "double"). So we optimize these cases too.
            //
            // An exception is the JSON data type, when used in conjunction with a parameter (like `JsonDocument`).
            // JSON parameters like that will be serialized to string and supplied as a string parameter to XG
            // (at least this seems to be the case currently with XuguClient). To make assignments and comparisons
            // between JSON columns and JSON parameters (supplied as string) work, the string needs to be explicitly
            // converted to JSON.

            var sameInnerCastStoreType = sqlUnaryExpression.Operand is SqlUnaryExpression operandUnary &&
                                         operandUnary.OperatorType == ExpressionType.Convert &&
                                         castMapping.Equals(GetCastStoreType(operandUnary.TypeMapping), StringComparison.OrdinalIgnoreCase);

            if (castMapping == "json" && !_options.ServerVersion.Supports.JsonDataTypeEmulation ||
                !castMapping.Equals(sqlUnaryExpression.Operand.TypeMapping.StoreType, StringComparison.OrdinalIgnoreCase) &&
                !sameInnerCastStoreType)
            {
                var useDecimalToDoubleWorkaround = false;

                if (castMapping.StartsWith("double") &&
                    !_options.ServerVersion.Supports.DoubleCast)
                {
                    useDecimalToDoubleWorkaround = true;
                    castMapping = "decimal(38,17)";
                }

                if (useDecimalToDoubleWorkaround)
                {
                    _relationalCommandBuilder.Append("(");
                }

                _relationalCommandBuilder.Append("CAST(");
                Visit(sqlUnaryExpression.Operand);
                _relationalCommandBuilder.Append(" AS ");
                _relationalCommandBuilder.Append(castMapping);
                _relationalCommandBuilder.Append(")");

                if (useDecimalToDoubleWorkaround)
                {
                    _relationalCommandBuilder.Append(" + 0e0)");
                }
                else if (castMapping.EndsWith("char"))
                {
                    // Expressions like `"mystring" + 1` can lead to collation mismatches.
                    // We force `utf8mb4_bin` here, that should always work. It might however change the case sensitivity of
                    // operations it is part of.
                    _relationalCommandBuilder.Append(" COLLATE utf8mb4_bin");
                }
            }
            else
            {
                Visit(sqlUnaryExpression.Operand);
            }

            return sqlUnaryExpression;
        }

        private string GetCastStoreType(RelationalTypeMapping typeMapping)
        {
            var storeTypeLower = typeMapping.StoreType.ToLower();
            string castMapping = null;
            foreach (var kvp in _castMappings)
            {
                foreach (var storeType in kvp.Value)
                {
                    if (storeTypeLower.StartsWith(storeType))
                    {
                        castMapping = kvp.Key;
                        break;
                    }
                }

                if (castMapping != null)
                {
                    break;
                }
            }

            if (castMapping == null)
            {
                throw new InvalidOperationException($"Cannot cast from type '{typeMapping.StoreType}'");
            }

            if (castMapping == "signed" && storeTypeLower.Contains("unsigned"))
            {
                castMapping = "unsigned";
            }

            // FLOAT and DOUBLE are supported by CAST() as of XG 8.0.17.
            // For server versions before that, a workaround is applied, that casts to a DECIMAL,
            // that is then added to 0e0, which results in a DOUBLE.
            // As of XG 8.0.18, a FLOAT cast might unnecessarily drop decimal places and round,
            // so we just keep casting to double instead. XuguClient ensures, that a System.Single
            // will be returned if expected, even if we return a DOUBLE.
            // REF: https://stackoverflow.com/a/32991084/2618319

            if (castMapping.StartsWith("float") &&
                !_options.ServerVersion.Supports.FloatCast)
            {
                castMapping = "double";
            }

            return castMapping;
        }
        protected override Expression VisitDistinct(DistinctExpression distinctExpression)
        {
            Check.NotNull(distinctExpression, nameof(distinctExpression));

            _relationalCommandBuilder.Append("DISTINCT (");
            Visit(distinctExpression.Operand);
            _relationalCommandBuilder.Append(")");

            return distinctExpression;
        }
        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            Check.NotNull(sqlParameterExpression, nameof(sqlParameterExpression));

            var parameterNameInCommand = GenerateParameterName(sqlParameterExpression.Name);

            if (_relationalCommandBuilder.Parameters
                .All(p => p.InvariantName != sqlParameterExpression.Name))
            {
                _relationalCommandBuilder.AddParameter(
                    sqlParameterExpression.Name,
                    parameterNameInCommand,
                    sqlParameterExpression.TypeMapping,
                    sqlParameterExpression.IsNullable);
            }

            _relationalCommandBuilder
                .Append(GenerateParameterName(sqlParameterExpression.Name));

            return sqlParameterExpression;
        }

        public virtual string GenerateParameterName(string name)
            => name.StartsWith(":", StringComparison.Ordinal)
                ? name
                : ":" + name;
        protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        {
            Check.NotNull(sqlConstantExpression, nameof(sqlConstantExpression));

            _relationalCommandBuilder
                .Append(sqlConstantExpression.TypeMapping.GenerateSqlLiteral(sqlConstantExpression.Value));

            return sqlConstantExpression;
        }
        public Expression VisitXGComplexFunctionArgumentExpression(XGComplexFunctionArgumentExpression XGComplexFunctionArgumentExpression)
        {
            Check.NotNull(XGComplexFunctionArgumentExpression, nameof(XGComplexFunctionArgumentExpression));

            var first = true;
            foreach (var argument in XGComplexFunctionArgumentExpression.ArgumentParts)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    _relationalCommandBuilder.Append(XGComplexFunctionArgumentExpression.Delimiter);
                }

                Visit(argument);
            }

            return XGComplexFunctionArgumentExpression;
        }

        public Expression VisitXGCollateExpression(XGCollateExpression XGCollateExpression)
        {
            Check.NotNull(XGCollateExpression, nameof(XGCollateExpression));

            Visit(XGCollateExpression.ValueExpression);

            return XGCollateExpression;
        }

        protected override Expression VisitCase(CaseExpression caseExpression)
        {
            Check.NotNull(caseExpression, nameof(caseExpression));

            _relationalCommandBuilder.Append("CASE");

            if (caseExpression.Operand != null)
            {
                _relationalCommandBuilder.Append(" ");
                Visit(caseExpression.Operand);
            }

            using (_relationalCommandBuilder.Indent())
            {
                foreach (var whenClause in caseExpression.WhenClauses)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("WHEN ");
                    Visit(whenClause.Test);
                    _relationalCommandBuilder.Append(" THEN ");
                    Visit(whenClause.Result);
                }

                if (caseExpression.ElseResult != null)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("ELSE ");
                    Visit(caseExpression.ElseResult);
                }
            }

            _relationalCommandBuilder
                .AppendLine()
                .Append("END");

            return caseExpression;
        }

        public Expression VisitXGBinaryExpression(XGBinaryExpression XGBinaryExpression)
        {
            if (XGBinaryExpression.OperatorType == XGBinaryExpressionOperatorType.NonOptimizedEqual)
            {
                Visit(_sqlExpressionFactory.Equal(XGBinaryExpression.Left, XGBinaryExpression.Right));
            }
            else
            {
                _relationalCommandBuilder.Append("(");
                Visit(XGBinaryExpression.Left);
                _relationalCommandBuilder.Append(")");

                switch (XGBinaryExpression.OperatorType)
                {
                    case XGBinaryExpressionOperatorType.IntegerDivision:
                        _relationalCommandBuilder.Append(" DIV ");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _relationalCommandBuilder.Append("(");
                Visit(XGBinaryExpression.Right);
                _relationalCommandBuilder.Append(")");
            }

            return XGBinaryExpression;
        }
        private void GenerateSetOperationHelper(SetOperationBase setOperation)
        {
            _relationalCommandBuilder.AppendLine("(");
            using (_relationalCommandBuilder.Indent())
            {
                GenerateSetOperation(setOperation);
            }

            _relationalCommandBuilder.AppendLine()
                .Append(")")
                .Append(AliasSeparator)
                .Append(_sqlGenerationHelper.DelimitIdentifier(setOperation.Alias));
        }
        protected override Expression VisitExcept(ExceptExpression exceptExpression)
        {
            Check.NotNull(exceptExpression, nameof(exceptExpression));

            GenerateSetOperationHelper(exceptExpression);

            return exceptExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitIntersect(IntersectExpression intersectExpression)
        {
            Check.NotNull(intersectExpression, nameof(intersectExpression));

            GenerateSetOperationHelper(intersectExpression);

            return intersectExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitUnion(UnionExpression unionExpression)
        {
            Check.NotNull(unionExpression, nameof(unionExpression));

            GenerateSetOperationHelper(unionExpression);

            return unionExpression;
        }

    }
}
