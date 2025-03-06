// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.Sql.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XuGuDbQuerySqlGenerator : DefaultQuerySqlGenerator, IXuGuDbExpressionVisitor
    {
        private readonly IRelationalCommandBuilderFactory _relationalCommandBuilderFactory;
        private readonly ISqlGenerationHelper _sqlGenerationHelper;
        private readonly IParameterNameGeneratorFactory _parameterNameGeneratorFactory;
        private readonly IRelationalTypeMapper _relationalTypeMapper;

        private IRelationalCommandBuilder _relationalCommandBuilder;
        private IReadOnlyDictionary<string, object> _parametersValues;
        private ParameterNameGenerator _parameterNameGenerator;
        private RelationalTypeMapping _typeMapping;
        private int _limitCount = 999999999;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XuGuDbQuerySqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalTypeMapper relationalTypeMapper,
            [NotNull] SelectExpression selectExpression)
            : base(
                relationalCommandBuilderFactory,
                sqlGenerationHelper,
                parameterNameGeneratorFactory,
                relationalTypeMapper,
                selectExpression)
        {
            Check.NotNull(relationalCommandBuilderFactory, nameof(relationalCommandBuilderFactory));
            Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper));
            Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory));
            Check.NotNull(relationalTypeMapper, nameof(relationalTypeMapper));
            Check.NotNull(selectExpression, nameof(selectExpression));

            _relationalCommandBuilderFactory = relationalCommandBuilderFactory;
            _sqlGenerationHelper = sqlGenerationHelper;
            _parameterNameGeneratorFactory = parameterNameGeneratorFactory;
            _relationalTypeMapper = relationalTypeMapper;

            SelectExpression = selectExpression;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this SQL query is cacheable.
        /// </summary>
        /// <value>
        ///     true if this SQL query is cacheable, false if not.
        /// </value>
        public new bool IsCacheable { get; private set; }

        /// <summary>
        ///     Gets the select expression.
        /// </summary>
        /// <value>
        ///     The select expression.
        /// </value>
        protected override SelectExpression SelectExpression { get; }

        /// <summary>
        ///     Gets the SQL generation helper.
        /// </summary>
        /// <value>
        ///     The SQL generation helper.
        /// </value>
        protected override ISqlGenerationHelper SqlGenerator => _sqlGenerationHelper;

        /// <summary>
        ///     Gets the parameter values.
        /// </summary>
        /// <value>
        ///     The parameter values.
        /// </value>
        protected override IReadOnlyDictionary<string, object> ParameterValues => _parametersValues;

        /// <summary>
        ///     The generated SQL.
        /// </summary>
        protected override IRelationalCommandBuilder Sql => _relationalCommandBuilder;

        /// <summary>
        ///     The default string concatenation operator SQL.
        /// </summary>
        protected override string ConcatOperator => "||";

        /// <summary>
        ///     The default true literal SQL.
        /// </summary>
        protected override string TypedTrueLiteral => "CAST('1' AS BIT)";

        /// <summary>
        ///     The default false literal SQL.
        /// </summary>
        protected override string TypedFalseLiteral => "CAST('0' AS BIT)";

        /// <summary>
        ///     Visit a top-level SelectExpression.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

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

            

            var projectionAdded = false;

            if (selectExpression.IsProjectStar)
            {
                _relationalCommandBuilder
                    .Append(_sqlGenerationHelper.DelimitIdentifier(selectExpression.Tables.Last().Alias))
                    .Append(".*");

                projectionAdded = true;
            }

            if (selectExpression.Projection.Any())
            {
                if (selectExpression.IsProjectStar)
                {
                    _relationalCommandBuilder.Append(", ");
                }

                VisitProjection(selectExpression.Projection);

                projectionAdded = true;
            }

            if (!projectionAdded)
            {
                _relationalCommandBuilder.Append("1");
            }

            if (selectExpression.Tables.Any())
            {
                _relationalCommandBuilder.AppendLine()
                    .Append(" FROM ");

                VisitJoin(selectExpression.Tables, sql => sql.AppendLine());
                //VisitTempTable(selectExpression);
            }

            if (selectExpression.Predicate != null)
            {
                var optimizedPredicate = ApplyOptimizations(selectExpression.Predicate, searchCondition: true);
                if (optimizedPredicate != null)
                {
                    _relationalCommandBuilder.AppendLine()
                        .Append(" WHERE ");

                    Visit(optimizedPredicate);
                }
            }

            if (selectExpression.OrderBy.Any())
            {
                _relationalCommandBuilder.AppendLine();

                GenerateOrderBy(selectExpression.OrderBy);
            }

            GenerateLimitOffset(selectExpression);

            GenerateTop(selectExpression);

            if (subQueryIndent != null)
            {
                subQueryIndent.Dispose();

                _relationalCommandBuilder.AppendLine()
                    .Append(")");

                if (selectExpression.Alias.Length > 0)
                {
                    _relationalCommandBuilder.Append(" ")
                        .Append(_sqlGenerationHelper.DelimitIdentifier(selectExpression.Alias));
                }
            }

            return selectExpression;
        }

        /// <summary>
        ///     Visit a ConditionalExpression.
        /// </summary>
        /// <param name="expression"> The conditional expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            _relationalCommandBuilder.AppendLine("CASE");

            using (_relationalCommandBuilder.Indent())
            {
                _relationalCommandBuilder.Append("WHEN ");

                Visit(expression.Test);

                _relationalCommandBuilder.AppendLine();
                _relationalCommandBuilder.Append("THEN ");

                var constantIfTrue = expression.IfTrue as ConstantExpression;

                if (constantIfTrue != null
                    && constantIfTrue.Type == typeof(bool))
                {
                    _relationalCommandBuilder.Append((bool)constantIfTrue.Value ? TypedTrueLiteral : TypedFalseLiteral);
                }
                else
                {
                    Visit(expression.IfTrue);
                }

                _relationalCommandBuilder.Append(" ELSE ");

                var constantIfFalse = expression.IfFalse as ConstantExpression;

                if (constantIfFalse != null
                    && constantIfFalse.Type == typeof(bool))
                {
                    _relationalCommandBuilder.Append((bool)constantIfFalse.Value ? TypedTrueLiteral : TypedFalseLiteral);
                }
                else
                {
                    Visit(expression.IfFalse);
                }

                _relationalCommandBuilder.AppendLine();
            }

            _relationalCommandBuilder.Append("END");

            return expression;
        }

        /// <summary>
        ///     Extracts the non null expression values from a list of expressions.
        /// </summary>
        /// <param name="inExpressionValues"> The list of expressions. </param>
        /// <returns>
        ///     The extracted non null expression values.
        /// </returns>
        protected override IReadOnlyList<Expression> ExtractNonNullExpressionValues(
            [NotNull] IReadOnlyList<Expression> inExpressionValues)
        {
            var inValuesNotNull = new List<Expression>();

            foreach (var inValue in inExpressionValues)
            {
                var inConstant = inValue as ConstantExpression;

                if (inConstant?.Value != null)
                {
                    inValuesNotNull.Add(inValue);

                    continue;
                }

                var inParameter = inValue as ParameterExpression;

                if (inParameter != null)
                {
                    object parameterValue;

                    if (_parametersValues.TryGetValue(inParameter.Name, out parameterValue))
                    {
                        if (parameterValue != null)
                        {
                            inValuesNotNull.Add(inValue);
                        }
                    }
                }
            }

            return inValuesNotNull;
        }

        /// <summary>
        ///     Visit a CrossJoin expression.
        /// </summary>
        /// <param name="crossJoinExpression"> The cross join expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            Check.NotNull(crossJoinExpression, nameof(crossJoinExpression));

            _relationalCommandBuilder.Append("CROSS JOIN ");

            Visit(crossJoinExpression.TableExpression);

            return crossJoinExpression;
        }

        /// <summary>
        ///     Visit an InnerJoinExpression.
        /// </summary>
        /// <param name="innerJoinExpression"> The inner join expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            Check.NotNull(innerJoinExpression, nameof(innerJoinExpression));

            _relationalCommandBuilder.Append("INNER JOIN ");

            Visit(innerJoinExpression.TableExpression);

            _relationalCommandBuilder.Append(" ON ");

            Visit(innerJoinExpression.Predicate);

            return innerJoinExpression;
        }

        /// <summary>
        ///     Generates the TOP part of the SELECT statement,
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        protected override void GenerateTop([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null
                && selectExpression.Offset == null)
            {
                _relationalCommandBuilder.Append(" LIMIT ");

                Visit(selectExpression.Limit);
            }
        }

        /// <summary>
        ///     Generates the ORDER BY SQL.
        /// </summary>
        /// <param name="orderings"> The orderings. </param>
        protected override void GenerateOrderBy([NotNull] IReadOnlyList<Ordering> orderings)
        {
            _relationalCommandBuilder.Append("ORDER BY ");

            VisitJoin(orderings, t =>
            {
                var aliasExpression = t.Expression as AliasExpression;

                if (aliasExpression != null)
                {
                    if (aliasExpression.Alias != null)
                    {
                        var columnExpression = aliasExpression.TryGetColumnExpression();

                        if (columnExpression != null)
                        {
                            _relationalCommandBuilder
                                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.TableAlias))
                                .Append(".");
                        }

                        _relationalCommandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(aliasExpression.Alias));
                    }
                    else
                    {
                        Visit(aliasExpression.Expression);
                    }
                }
                else
                {
                    Visit(t.Expression);
                }

                if (t.OrderingDirection == OrderingDirection.Desc)
                {
                    _relationalCommandBuilder.Append(" DESC");
                }
            });
        }

        /// <summary>
        ///     Generates the ORDER BY SQL.
        /// </summary>
        /// <param name="orderings"> The orderings. </param>
        protected void GenerateOrderBy([NotNull] IReadOnlyList<Ordering> orderings, IRelationalCommandBuilder commandBuilder)
        {
            VisitJoin(orderings, t =>
            {
                var aliasExpression = t.Expression as AliasExpression;

                if (aliasExpression != null)
                {
                    if (aliasExpression.Alias != null)
                    {
                        var columnExpression = aliasExpression.TryGetColumnExpression();

                        if (columnExpression != null)
                        {
                            commandBuilder
                                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.TableAlias))
                                .Append(".");
                        }

                        commandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(aliasExpression.Alias));
                    }
                    else
                    {
                        Visit(aliasExpression.Expression);
                    }
                }
                else
                {
                    Visit(t.Expression);
                }

                if (t.OrderingDirection == OrderingDirection.Desc)
                {
                    commandBuilder.Append(" DESC");
                }
            });
        }

        private Expression ApplyOptimizations(Expression expression, bool searchCondition)
        {
            var newExpression
                = new NullComparisonTransformingVisitor(_parametersValues)
                    .Visit(expression);

            var relationalNullsOptimizedExpandingVisitor = new RelationalNullsOptimizedExpandingVisitor();
            var optimizedExpression = relationalNullsOptimizedExpandingVisitor.Visit(newExpression);

            newExpression
                = relationalNullsOptimizedExpandingVisitor.IsOptimalExpansion
                    ? optimizedExpression
                    : new RelationalNullsExpandingVisitor().Visit(newExpression);

            newExpression = new PredicateReductionExpressionOptimizer().Visit(newExpression);
            newExpression = new PredicateNegationExpressionOptimizer().Visit(newExpression);
            newExpression = new ReducingExpressionVisitor().Visit(newExpression);
            var searchConditionTranslatingVisitor = new SearchConditionTranslatingVisitor(searchCondition);
            newExpression = searchConditionTranslatingVisitor.Visit(newExpression);

            if (searchCondition && !SearchConditionTranslatingVisitor.IsSearchCondition(newExpression))
            {
                var constantExpression = newExpression as ConstantExpression;
                if ((constantExpression != null)
                    && (bool)constantExpression.Value)
                {
                    return null;
                }
                return Expression.Equal(newExpression, Expression.Constant(true, typeof(bool)));
            }

            return newExpression;
        }

        private void VisitJoin(
            IReadOnlyList<Expression> expressions, Action<IRelationalCommandBuilder> joinAction = null)
            => VisitJoin(expressions, e => Visit(e), joinAction);

        private void VisitJoin(
            IReadOnlyList<Expression> expressions, IRelationalCommandBuilder commandBuilder, Action<IRelationalCommandBuilder> joinAction = null)
            => VisitJoin(expressions, e => Visit(e),commandBuilder, joinAction);

        private void VisitJoin<T>(
            IReadOnlyList<T> items, Action<T> itemAction, Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction = joinAction ?? (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                itemAction(items[i]);
            }
        }

        private void VisitJoin<T>(
            IReadOnlyList<T> items, Action<T> itemAction, IRelationalCommandBuilder commandBuilder, Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction = joinAction ?? (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(commandBuilder);
                }

                itemAction(items[i]);
            }
        }

        public override IRelationalCommand GenerateSql(IReadOnlyDictionary<string, object> parameterValues)
        {
            Check.NotNull(parameterValues, nameof(parameterValues));

            _relationalCommandBuilder = _relationalCommandBuilderFactory.Create();
            _parameterNameGenerator = _parameterNameGeneratorFactory.Create();

            _parametersValues = parameterValues;

            Visit(SelectExpression);

            return _relationalCommandBuilder.Build();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression VisitLateralJoin(LateralJoinExpression lateralJoinExpression)
        {
            Check.NotNull(lateralJoinExpression, nameof(lateralJoinExpression));

            Sql.Append("CROSS JOIN ");

            Visit(lateralJoinExpression.TableExpression);

            return lateralJoinExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression VisitCount(CountExpression countExpression)
        {
            Check.NotNull(countExpression, nameof(countExpression));
            _relationalCommandBuilder.Append("COUNT(*)");

            return countExpression;
        }

        /// <summary>
        ///     Visit a SumExpression.
        /// </summary>
        /// <param name="sumExpression"> The sum expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitSum(SumExpression sumExpression)
        {
            Check.NotNull(sumExpression, nameof(sumExpression));

            _relationalCommandBuilder.Append("SUM(");

            Visit(sumExpression.Expression);

            _relationalCommandBuilder.Append(")");

            return sumExpression;
        }

        /// <summary>
        ///     Visit a MinExpression.
        /// </summary>
        /// <param name="minExpression"> The min expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitMin(MinExpression minExpression)
        {
            Check.NotNull(minExpression, nameof(minExpression));

            _relationalCommandBuilder.Append("MIN(");

            Visit(minExpression.Expression);

            _relationalCommandBuilder.Append(")");

            return minExpression;
        }

        /// <summary>
        ///     Visit a MaxExpression.
        /// </summary>
        /// <param name="maxExpression"> The max expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitMax(MaxExpression maxExpression)
        {
            Check.NotNull(maxExpression, nameof(maxExpression));

            _relationalCommandBuilder.Append("MAX(");

            Visit(maxExpression.Expression);

            _relationalCommandBuilder.Append(")");

            return maxExpression;
        }

        /// <summary>
        ///     Visit a StringCompareExpression.
        /// </summary>
        /// <param name="stringCompareExpression"> The string compare expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitStringCompare(StringCompareExpression stringCompareExpression)
        {
            Visit(stringCompareExpression.Left);

            _relationalCommandBuilder.Append(GenerateBinaryOperator(stringCompareExpression.Operator));

            Visit(stringCompareExpression.Right);

            return stringCompareExpression;
        }

        /// <summary>
        ///     Visit an InExpression.
        /// </summary>
        /// <param name="inExpression"> The in expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitIn(InExpression inExpression)
        {
            if (inExpression.Values != null)
            {
                var inValues = ProcessInExpressionValues(inExpression.Values);
                var inValuesNotNull = ExtractNonNullExpressionValues(inValues);

                if (inValues.Count != inValuesNotNull.Count)
                {
                    var relationalNullsInExpression
                        = Expression.OrElse(
                            new InExpression(inExpression.Operand, inValuesNotNull),
                            new IsNullExpression(inExpression.Operand));

                    return Visit(relationalNullsInExpression);
                }

                if (inValuesNotNull.Count > 0)
                {
                    var parentTypeMapping = _typeMapping;
                    _typeMapping = InferTypeMappingFromColumn(inExpression.Operand) ?? parentTypeMapping;

                    Visit(inExpression.Operand);

                    _relationalCommandBuilder.Append(" IN (");

                    VisitJoin(inValuesNotNull);

                    _relationalCommandBuilder.Append(")");

                    _typeMapping = parentTypeMapping;
                }
                else
                {
                    _relationalCommandBuilder.Append("0 = 1");
                }
            }
            else
            {
                var parentTypeMapping = _typeMapping;
                _typeMapping = InferTypeMappingFromColumn(inExpression.Operand) ?? parentTypeMapping;

                Visit(inExpression.Operand);

                _relationalCommandBuilder.Append(" IN ");

                Visit(inExpression.SubQuery);

                _typeMapping = parentTypeMapping;
            }

            return inExpression;
        }

        /// <summary>
        ///     Visit a negated InExpression.
        /// </summary>
        /// <param name="inExpression"> The in expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitNotIn([NotNull] InExpression inExpression)
        {
            if (inExpression.Values != null)
            {
                var inValues = ProcessInExpressionValues(inExpression.Values);
                var inValuesNotNull = ExtractNonNullExpressionValues(inValues);

                if (inValues.Count != inValuesNotNull.Count)
                {
                    var relationalNullsNotInExpression
                        = Expression.AndAlso(
                            Expression.Not(new InExpression(inExpression.Operand, inValuesNotNull)),
                            Expression.Not(new IsNullExpression(inExpression.Operand)));

                    return Visit(relationalNullsNotInExpression);
                }

                if (inValues.Count > 0)
                {
                    Visit(inExpression.Operand);

                    _relationalCommandBuilder.Append(" NOT IN (");

                    VisitJoin(inValues);

                    _relationalCommandBuilder.Append(")");
                }
                else
                {
                    _relationalCommandBuilder.Append("1 = 1");
                }
            }
            else
            {
                Visit(inExpression.Operand);

                _relationalCommandBuilder.Append(" NOT IN ");

                Visit(inExpression.SubQuery);
            }

            return inExpression;
        }

        /// <summary>
        ///     Process the InExpression values.
        /// </summary>
        /// <param name="inExpressionValues"> The in expression values. </param>
        /// <returns>
        ///     A list of expressions.
        /// </returns>
        protected override IReadOnlyList<Expression> ProcessInExpressionValues(
            [NotNull] IEnumerable<Expression> inExpressionValues)
        {
            Check.NotNull(inExpressionValues, nameof(inExpressionValues));

            var inConstants = new List<Expression>();

            foreach (var inValue in inExpressionValues)
            {
                var inConstant = inValue as ConstantExpression;

                if (inConstant != null)
                {
                    AddInExpressionValues(inConstant.Value, inConstants, inConstant);
                }
                else
                {
                    var inParameter = inValue as ParameterExpression;

                    if (inParameter != null)
                    {
                        object parameterValue;
                        if (_parametersValues.TryGetValue(inParameter.Name, out parameterValue))
                        {
                            AddInExpressionValues(parameterValue, inConstants, inParameter);

                            IsCacheable = false;
                        }
                    }
                    else
                    {
                        var inListInit = inValue as ListInitExpression;

                        if (inListInit != null)
                        {
                            inConstants.AddRange(ProcessInExpressionValues(
                                inListInit.Initializers.SelectMany(i => i.Arguments)));
                        }
                        else
                        {
                            var newArray = inValue as NewArrayExpression;

                            if (newArray != null)
                            {
                                inConstants.AddRange(ProcessInExpressionValues(newArray.Expressions));
                            }
                        }
                    }
                }
            }

            return inConstants;
        }

        private static void AddInExpressionValues(
            object value, List<Expression> inConstants, Expression expression)
        {
            var valuesEnumerable = value as IEnumerable;

            if (valuesEnumerable != null
                && value.GetType() != typeof(string)
                && value.GetType() != typeof(byte[]))
            {
                inConstants.AddRange(valuesEnumerable.Cast<object>().Select(Expression.Constant));
            }
            else
            {
                inConstants.Add(expression);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            if (selectExpression.Projection.OfType<RowNumberExpression>().Any())
            {
                return;
            }

            if (selectExpression.Offset != null
                && !selectExpression.OrderBy.Any())
            {
                //Sql.AppendLine().Append("ORDER BY ROWNUM");
                _relationalCommandBuilder.AppendLine().Append("ORDER BY ROWID");
            }

            if (selectExpression.Offset != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("LIMIT ");

                if (selectExpression.Limit != null)
                {
                    Visit(selectExpression.Limit);
                }
                else
                {
                    _relationalCommandBuilder.Append(_limitCount);
                }

                _relationalCommandBuilder.AppendLine()
                    .Append("OFFSET ");

                Visit(selectExpression.Offset);
            }
        }

        /// <summary>
        ///     Visit an ExistsExpression.
        /// </summary>
        /// <param name="existsExpression"> The exists expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitExists(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, nameof(existsExpression));

            _relationalCommandBuilder.AppendLine("EXISTS (");

            using (_relationalCommandBuilder.Indent())
            {
                Visit(existsExpression.Expression);
            }

            _relationalCommandBuilder.Append(")");

            return existsExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void VisitProjection(IReadOnlyList<Expression> projections)
        {
            var comparisonTransformer = new ProjectionComparisonTransformingVisitor();
            var transformedProjections = projections.Select(comparisonTransformer.Visit).ToList();

            VisitJoin(
            transformedProjections
                .Select(e => ApplyOptimizations(e, searchCondition: false))
                .ToList());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected void VisitProjection(IReadOnlyList<Expression> projections, IRelationalCommandBuilder commandBuilder)
        {
            var comparisonTransformer = new ProjectionComparisonTransformingVisitor();
            var transformedProjections = projections.Select(comparisonTransformer.Visit).ToList();

            VisitJoin(
            transformedProjections
                .Select(e => ApplyOptimizations(e, searchCondition: false))
                .ToList(),
            commandBuilder);
        }

        protected virtual void VisitTempTable(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            var tempTables = selectExpression.Tables.Where(i=>i is SelectExpression).ToList();
            var normalTables = selectExpression.Tables.Except(tempTables).ToList();
            int index = 0;

            foreach (var tempTable in tempTables)
            {
                index++;
                var tempSelectExpression = (SelectExpression)tempTable;
                _relationalCommandBuilder.Append("(SELECT ");
                VisitProjection(tempSelectExpression.Projection);
                _relationalCommandBuilder.Append(" FROM (");
                _relationalCommandBuilder.Append("SELECT ");

                if (tempSelectExpression.Projection.Any())
                {
                    if (tempSelectExpression.IsProjectStar)
                    {
                        _relationalCommandBuilder.Append(", ");
                    }

                    VisitProjection(tempSelectExpression.Projection);
                }
                if (tempSelectExpression.Tables.Any())
                {
                    _relationalCommandBuilder.AppendLine()
                        .Append(" FROM ");

                    VisitJoin(tempSelectExpression.Tables, sql => sql.AppendLine());
                }
                if (tempSelectExpression.OrderBy.Any())
                {
                    _relationalCommandBuilder.AppendLine();

                    GenerateOrderBy(tempSelectExpression.OrderBy);
                }
                _relationalCommandBuilder.Append(") WHERE ROWNUM <=");
                Visit(tempSelectExpression.Limit);
                _relationalCommandBuilder.Append(")");

                if (string.IsNullOrEmpty(tempSelectExpression.Alias))
                {
                    _relationalCommandBuilder.Append(" ")
                    .Append(_sqlGenerationHelper.DelimitIdentifier(tempSelectExpression.Alias));
                }

                if (index < tempTables.Count|| normalTables.Any())
                {
                    _relationalCommandBuilder.Append(", ");
                }
            }

            if (normalTables.Any())
            {
                VisitJoin(normalTables, sql => sql.AppendLine());
            }
            
        }

        /// <summary>
        ///     Visit a BinaryExpression.
        /// </summary>
        /// <param name="expression"> The binary expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitBinary(BinaryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.NodeType == ExpressionType.Coalesce)
            {
                _relationalCommandBuilder.Append("COALESCE(");
                Visit(expression.Left);
                _relationalCommandBuilder.Append(", ");
                Visit(expression.Right);
                _relationalCommandBuilder.Append(")");
            }
            else
            {
                var parentTypeMapping = _typeMapping;

                if (expression.IsComparisonOperation()
                    || (expression.NodeType == ExpressionType.Add))
                {
                    _typeMapping
                        = InferTypeMappingFromColumn(expression.Left)
                          ?? InferTypeMappingFromColumn(expression.Right)
                          ?? parentTypeMapping;
                }

                var needParens = expression.Left is BinaryExpression;

                if (needParens)
                {
                    _relationalCommandBuilder.Append("(");
                }

                Visit(expression.Left);

                if (needParens)
                {
                    _relationalCommandBuilder.Append(")");
                }

                string op;
                if (!TryGenerateBinaryOperator(expression.NodeType, out op))
                {
                    switch (expression.NodeType)
                    {
                        case ExpressionType.Add:
                            op = expression.Type == typeof(string)
                                ? " " + ConcatOperator + " "
                                : " + ";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                _relationalCommandBuilder.Append(op);

                needParens = expression.Right is BinaryExpression;

                if (needParens)
                {
                    _relationalCommandBuilder.Append("(");
                }


                if (expression.Right is SelectExpression selectExpression && selectExpression.Limit!=null)
                {
                    _relationalCommandBuilder.Append("(SELECT ");
                    VisitProjection(selectExpression.Projection);
                    _relationalCommandBuilder.Append(" FROM (");
                    _relationalCommandBuilder.Append("SELECT ");

                    if (selectExpression.Projection.Any())
                    {
                        if (selectExpression.IsProjectStar)
                        {
                            _relationalCommandBuilder.Append(", ");
                        }

                        VisitProjection(selectExpression.Projection);
                    }
                    if (selectExpression.Tables.Any())
                    {
                        _relationalCommandBuilder.AppendLine()
                            .Append(" FROM ");

                        VisitJoin(selectExpression.Tables, sql => sql.AppendLine());
                    }
                    if (selectExpression.OrderBy.Any())
                    {
                        _relationalCommandBuilder.AppendLine();

                        GenerateOrderBy(selectExpression.OrderBy);
                    }
                    _relationalCommandBuilder.Append(") WHERE ROWNUM <=");
                    Visit(selectExpression.Limit);
                    _relationalCommandBuilder.Append(")");
                }
                else
                {
                    Visit(expression.Right);
                }

                if (needParens)
                {
                    _relationalCommandBuilder.Append(")");
                }

                _typeMapping = parentTypeMapping;
            }

            return expression;
        }

        /// <summary>
        ///     Visits a ParameterExpression.
        /// </summary>
        /// <param name="parameterExpression"> The parameter expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            var parameterName = _sqlGenerationHelper.GenerateParameterName(parameterExpression.Name);

            if (_relationalCommandBuilder.ParameterBuilder.Parameters
                .All(p => p.InvariantName != parameterExpression.Name))
            {
                _relationalCommandBuilder.AddParameter(
                    parameterExpression.Name,
                    parameterName,
                    _typeMapping ?? _relationalTypeMapper.GetMapping(parameterExpression.Type),
                    parameterExpression.Type.IsNullableType());
            }

            _relationalCommandBuilder.Append(parameterName);

            return parameterExpression;
        }

        /// <summary>
        ///     Visits a PropertyParameterExpression.
        /// </summary>
        /// <param name="propertyParameterExpression"> The property parameter expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitPropertyParameter(PropertyParameterExpression propertyParameterExpression)
        {
            var parameterName = _sqlGenerationHelper.GenerateParameterName(propertyParameterExpression.Name);

            if (_relationalCommandBuilder.ParameterBuilder.Parameters
                .All(p => p.InvariantName != propertyParameterExpression.PropertyParameterName))
            {
                _relationalCommandBuilder.AddPropertyParameter(
                    propertyParameterExpression.Name,
                    parameterName,
                    propertyParameterExpression.Property);
            }

            _relationalCommandBuilder.Append(parameterName);

            return propertyParameterExpression;
        }

        /// <summary>
        ///     Visit an LeftOuterJoinExpression.
        /// </summary>
        /// <param name="leftOuterJoinExpression"> The left outer join expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitLeftOuterJoin(LeftOuterJoinExpression leftOuterJoinExpression)
        {
            Check.NotNull(leftOuterJoinExpression, nameof(leftOuterJoinExpression));

            _relationalCommandBuilder.Append("LEFT JOIN ");

            Visit(leftOuterJoinExpression.TableExpression);

            _relationalCommandBuilder.Append(" ON ");

            Visit(leftOuterJoinExpression.Predicate);

            return leftOuterJoinExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression VisitRowNumber(RowNumberExpression rowNumberExpression)
        {
            Check.NotNull(rowNumberExpression, nameof(rowNumberExpression));

            Sql.Append("ROW_NUMBER() OVER(");
            GenerateOrderBy(rowNumberExpression.Orderings);
            Sql.Append(") ").Append(SqlGenerator.DelimitIdentifier(rowNumberExpression.ColumnExpression.Name));

            return rowNumberExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression VisitDatePartExpression(DatePartExpression datePartExpression)
        {
            Check.NotNull(datePartExpression, nameof(datePartExpression));

            switch (datePartExpression.DatePart)
            {
                case "year":
                    Sql.Append("YEAR");
                    break;
                case "month":
                    Sql.Append("MONTH");
                    break;
                case "day":
                    Sql.Append("DAY");
                    break;
                case "hour":
                    Sql.Append("HOUR");
                    break;
                case "minute":
                    Sql.Append("MINUTE");
                    break;
                case "second":
                    Sql.Append("SECOND");
                    break;
                case "dayofyear":
                    Sql.Append("DAYOFYEAR");
                    break;
                case "millisecond":
                    Sql.Append("MICROSECOND");
                    break;
            }

            Sql.Append("(");
            Visit(datePartExpression.Argument);
            Sql.Append(")");
            if (datePartExpression.DatePart== "millisecond")
            {
                Sql.Append(" / 1000");
            }
            return datePartExpression;
        }

        /// <summary>
        ///     Visits an IsNullExpression.
        /// </summary>
        /// <param name="isNullExpression"> The is null expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitIsNull(IsNullExpression isNullExpression)
        {
            Check.NotNull(isNullExpression, nameof(isNullExpression));

            Visit(isNullExpression.Operand);

            _relationalCommandBuilder.Append(" IS NULL");

            return isNullExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.FunctionName=="CONVERT")
            {
                _relationalCommandBuilder.Append("CAST");
            }
            else
            {
                _relationalCommandBuilder.Append(sqlFunctionExpression.FunctionName);
            }
            
            _relationalCommandBuilder.Append("(");

            var lastArgument = sqlFunctionExpression.Arguments.Last() as Expression;

            if (sqlFunctionExpression.Arguments.Count==2&&(lastArgument.NodeType==ExpressionType.Convert|| sqlFunctionExpression.FunctionName=="CONVERT"))
            {
                if (lastArgument is SqlFunctionExpression functionExpression)
                {
                    VisitSqlFunction(functionExpression);
                    if (lastArgument.NodeType == ExpressionType.Modulo)
                    {
                         
                    }
                    _relationalCommandBuilder
                        .Append(" AS ")
                        .Append(sqlFunctionExpression.Arguments.First())
                        .Append(")");
                    return sqlFunctionExpression;
                }

                var lastExpression = sqlFunctionExpression.Arguments.Last().RemoveConvert();
                if (lastExpression is AliasExpression aliasExpression)
                {
                    var expression = aliasExpression.Expression as ColumnExpression;

                    _relationalCommandBuilder
                        .Append(_sqlGenerationHelper.DelimitIdentifier(expression.Name, expression.Table.Alias))
                        .Append(" AS ")
                        .Append(sqlFunctionExpression.Arguments.First());
                }
                else if (lastExpression is BinaryExpression binaryExpression)
                {
                    //if (binaryExpression.Left is AliasExpression leftAliasExpression)
                    //{
                    //    var leftColumnExpression= leftAliasExpression.Expression as ColumnExpression;
                    //    _relationalCommandBuilder
                    //    .Append(_sqlGenerationHelper.DelimitIdentifier(leftColumnExpression.Name, leftColumnExpression.Table.Alias));
                        
                    //}
                    var optimizedPredicate = ApplyOptimizations(lastExpression,false);
                    if (optimizedPredicate != null)
                    {

                        Visit(optimizedPredicate);
                        _relationalCommandBuilder
                        .Append(" AS ")
                        .Append(sqlFunctionExpression.Arguments.First());
                    }
                }
                
            }
            else
            {
                VisitJoin(sqlFunctionExpression.Arguments.ToList());
            }
            

            _relationalCommandBuilder.Append(")");

            return sqlFunctionExpression;
        }

        /// <summary>
        ///     Visit a SQL ExplicitCastExpression.
        /// </summary>
        /// <param name="explicitCastExpression"> The explicit cast expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitExplicitCast(ExplicitCastExpression explicitCastExpression)
        {
            _relationalCommandBuilder.Append("CAST(");

            Visit(explicitCastExpression.Operand);

            _relationalCommandBuilder.Append(" AS ");

            var typeMapping = _relationalTypeMapper.FindMapping(explicitCastExpression.Type);

            if (typeMapping == null)
            {
                throw new InvalidOperationException(RelationalStrings.UnsupportedType(explicitCastExpression.Type.Name));
            }

            _relationalCommandBuilder.Append(typeMapping.StoreType);

            _relationalCommandBuilder.Append(")");

            return explicitCastExpression;
        }

        /// <summary>
        ///     Visit a TableExpression.
        /// </summary>
        /// <param name="tableExpression"> The table expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            if (tableExpression.Schema != null)
            {
                _relationalCommandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Schema))
                    .Append(".");
            }

            _relationalCommandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Table))
                .Append(" ")
                .Append(_sqlGenerationHelper.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        /// <summary>
        ///     Visits an AliasExpression.
        /// </summary>
        /// <param name="aliasExpression"> The alias expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitAlias(AliasExpression aliasExpression)
        {
            Check.NotNull(aliasExpression, nameof(aliasExpression));

            if (!aliasExpression.IsProjected)
            {
                Visit(aliasExpression.Expression);

                if (aliasExpression.Alias != null)
                {
                    _relationalCommandBuilder.Append(" ");
                }
            }

            if (aliasExpression.Alias != null)
            {
                _relationalCommandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(aliasExpression.Alias));
            }

            return aliasExpression;
        }

        /// <summary>
        ///     Visits a ColumnExpression.
        /// </summary>
        /// <param name="columnExpression"> The column expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitColumn(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            _relationalCommandBuilder.Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.TableAlias))
                .Append(".")
                .Append(_sqlGenerationHelper.DelimitIdentifier(columnExpression.Name));

            return columnExpression;
        }
        /// <summary>
        ///     Visits a UnaryExpression.
        /// </summary>
        /// <param name="expression"> The unary expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitUnary(UnaryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    {
                        var inExpression = expression.Operand as InExpression;

                        if (inExpression != null)
                        {
                            return VisitNotIn(inExpression);
                        }

                        var isNullExpression = expression.Operand as IsNullExpression;

                        if (isNullExpression != null)
                        {
                            return VisitIsNotNull(isNullExpression);
                        }

                        if (expression.Operand is ExistsExpression)
                        {
                            _relationalCommandBuilder.Append("NOT ");

                            Visit(expression.Operand);

                            return expression;
                        }

                        _relationalCommandBuilder.Append("NOT (");

                        Visit(expression.Operand);

                        _relationalCommandBuilder.Append(")");

                        return expression;
                    }
                case ExpressionType.Convert:
                    {
                        Visit(expression.Operand);

                        return expression;
                    }
            }

            return base.VisitUnary(expression);
        }

        /// <summary>
        ///     Visits a ConstantExpression.
        /// </summary>
        /// <param name="expression"> The constant expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var value = expression.Value;
            _relationalCommandBuilder.Append(value == null
                ? "NULL"
                : _sqlGenerationHelper.GenerateLiteral(value, GetTypeMapping(value)));

            return expression;
        }

        private RelationalTypeMapping GetTypeMapping(object value)
            => _typeMapping ?? _relationalTypeMapper.GetMappingForValue(value);

        /// <summary>
        ///     Visit a FromSqlExpression.
        /// </summary>
        /// <param name="fromSqlExpression"> The FromSql expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            Check.NotNull(fromSqlExpression, nameof(fromSqlExpression));

            _relationalCommandBuilder.AppendLine("(");

            using (_relationalCommandBuilder.Indent())
            {
                GenerateFromSql(fromSqlExpression.Sql, fromSqlExpression.Arguments, _parametersValues);
            }

            _relationalCommandBuilder.Append(") ")
                .Append(_sqlGenerationHelper.DelimitIdentifier(fromSqlExpression.Alias));

            return fromSqlExpression;
        }

        /// <summary>
        ///     Generate SQL corresponding to a FromSql query.
        /// </summary>
        /// <param name="sql"> The FromSql SQL query. </param>
        /// <param name="arguments"> The arguments. </param>
        /// <param name="parameters"> The parameters for this query. </param>
        protected override void GenerateFromSql(
            [NotNull] string sql,
            [NotNull] Expression arguments,
            [NotNull] IReadOnlyDictionary<string, object> parameters)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(parameters, nameof(parameters));

            string[] substitutions = null;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (arguments.NodeType)
            {
                case ExpressionType.Parameter:
                    {
                        var parameterExpression = (ParameterExpression)arguments;

                        object parameterValue;
                        if (parameters.TryGetValue(parameterExpression.Name, out parameterValue))
                        {
                            var argumentValues = (object[])parameterValue;

                            substitutions = new string[argumentValues.Length];

                            _relationalCommandBuilder.AddCompositeParameter(
                                parameterExpression.Name,
                                builder =>
                                {
                                    for (var i = 0; i < argumentValues.Length; i++)
                                    {
                                        var parameterName = _parameterNameGenerator.GenerateNext();

                                        substitutions[i] = SqlGenerator.GenerateParameterName(parameterName);

                                        builder.AddParameter(
                                        parameterName,
                                        substitutions[i]);
                                    }
                                });
                        }

                        break;
                    }
                case ExpressionType.Constant:
                    {
                        var constantExpression = (ConstantExpression)arguments;
                        var argumentValues = (object[])constantExpression.Value;

                        substitutions = new string[argumentValues.Length];

                        for (var i = 0; i < argumentValues.Length; i++)
                        {
                            var value = argumentValues[i];
                            substitutions[i] = SqlGenerator.GenerateLiteral(value, GetTypeMapping(value));
                        }

                        break;
                    }
                case ExpressionType.NewArrayInit:
                    {
                        var newArrayExpression = (NewArrayExpression)arguments;

                        substitutions = new string[newArrayExpression.Expressions.Count];

                        for (var i = 0; i < newArrayExpression.Expressions.Count; i++)
                        {
                            var expression = newArrayExpression.Expressions[i].RemoveConvert();

                            // ReSharper disable once SwitchStatementMissingSomeCases
                            switch (expression.NodeType)
                            {
                                case ExpressionType.Constant:
                                    {
                                        var value = ((ConstantExpression)expression).Value;
                                        substitutions[i]
                                            = SqlGenerator
                                                .GenerateLiteral(value, GetTypeMapping(value));

                                        break;
                                    }
                                case ExpressionType.Parameter:
                                    {
                                        var parameter = (ParameterExpression)expression;

                                        if (_parametersValues.ContainsKey(parameter.Name))
                                        {
                                            substitutions[i] = _sqlGenerationHelper.GenerateParameterName(parameter.Name);

                                            _relationalCommandBuilder.AddParameter(
                                                parameter.Name,
                                                substitutions[i]);
                                        }

                                        break;
                                    }
                            }
                        }

                        break;
                    }
            }

            if (substitutions != null)
            {
                // ReSharper disable once CoVariantArrayConversion
                sql = string.Format(sql, substitutions);
            }

            _relationalCommandBuilder.AppendLines(sql);
        }

        /// <summary>
        ///     Visits an IsNotNullExpression.
        /// </summary>
        /// <param name="isNotNullExpression"> The is not null expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitIsNotNull([NotNull] IsNullExpression isNotNullExpression)
        {
            Check.NotNull(isNotNullExpression, nameof(isNotNullExpression));

            Visit(isNotNullExpression.Operand);

            _relationalCommandBuilder.Append(" IS NOT NULL");

            return isNotNullExpression;
        }

        /// <summary>
        ///     Visit a LikeExpression.
        /// </summary>
        /// <param name="likeExpression"> The like expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public override Expression VisitLike(LikeExpression likeExpression)
        {
            Check.NotNull(likeExpression, nameof(likeExpression));

            var parentTypeMapping = _typeMapping;
            _typeMapping = InferTypeMappingFromColumn(likeExpression.Match) ?? parentTypeMapping;

            Visit(likeExpression.Match);

            _relationalCommandBuilder.Append(" LIKE ");

            Visit(likeExpression.Pattern);

            _typeMapping = parentTypeMapping;

            return likeExpression;
        }

        private class ProjectionComparisonTransformingVisitor : RelinqExpressionVisitor
        {
            private bool _insideConditionalTest;

            protected override Expression VisitUnary(UnaryExpression node)
            {
                if (!_insideConditionalTest
                    && node.NodeType == ExpressionType.Not
                    && node.Operand is AliasExpression)
                {
                    return Expression.Condition(
                        node,
                        Expression.Constant(true, typeof(bool)),
                        Expression.Constant(false, typeof(bool)));
                }

                if (!_insideConditionalTest
                    && node.NodeType == ExpressionType.Not
                    && node.Operand is IsNullExpression)
                {
                    return Expression.Condition(
                        node.Operand,
                        Expression.Constant(false, typeof(bool)),
                        Expression.Constant(true, typeof(bool)));
                }

                if (!_insideConditionalTest
                    && node.Operand is IsNullExpression)
                {
                    return Expression.Condition(
                        node,
                        Expression.Constant(true, typeof(bool)),
                        Expression.Constant(false, typeof(bool)));
                }

                return base.VisitUnary(node);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (!_insideConditionalTest
                    && (node.IsComparisonOperation()
                        || node.IsLogicalOperation()))
                {
                    return Expression.Condition(
                        node,
                        Expression.Constant(true, typeof(bool)),
                        Expression.Constant(false, typeof(bool)));
                }

                return base.VisitBinary(node);
            }

            protected override Expression VisitConditional(ConditionalExpression node)
            {
                _insideConditionalTest = true;
                var test = Visit(node.Test);
                _insideConditionalTest = false;
                if (test is AliasExpression)
                {
                    return Expression.Condition(
                        Expression.Equal(test, Expression.Constant(true, typeof(bool))),
                        Visit(node.IfTrue),
                        Visit(node.IfFalse));
                }

                var condition = test as ConditionalExpression;
                if (condition != null)
                {
                    return Expression.Condition(
                        condition.Test,
                        Visit(node.IfTrue),
                        Visit(node.IfFalse));
                }
                return Expression.Condition(test,
                    Visit(node.IfTrue),
                    Visit(node.IfFalse));
            }
        }
        private class NullComparisonTransformingVisitor : RelinqExpressionVisitor
        {
            private readonly IReadOnlyDictionary<string, object> _parameterValues;

            public NullComparisonTransformingVisitor(IReadOnlyDictionary<string, object> parameterValues)
            {
                _parameterValues = parameterValues;
            }

            protected override Expression VisitBinary(BinaryExpression expression)
            {
                if (expression.NodeType == ExpressionType.Equal
                    || expression.NodeType == ExpressionType.NotEqual)
                {
                    var leftExpression = expression.Left.RemoveConvert();
                    var rightExpression = expression.Right.RemoveConvert();

                    var parameter
                        = rightExpression as ParameterExpression
                          ?? leftExpression as ParameterExpression;

                    object parameterValue;
                    if (parameter != null
                        && _parameterValues.TryGetValue(parameter.Name, out parameterValue))
                    {
                        if (parameterValue == null)
                        {
                            var columnExpression
                                = leftExpression.TryGetColumnExpression()
                                  ?? rightExpression.TryGetColumnExpression();

                            if (columnExpression != null)
                            {
                                return
                                    expression.NodeType == ExpressionType.Equal
                                        ? (Expression)new IsNullExpression(columnExpression)
                                        : Expression.Not(new IsNullExpression(columnExpression));
                            }
                        }

                        var constantExpression
                            = leftExpression as ConstantExpression
                              ?? rightExpression as ConstantExpression;

                        if (constantExpression != null)
                        {
                            if (parameterValue == null
                                && constantExpression.Value == null)
                            {
                                return
                                    expression.NodeType == ExpressionType.Equal
                                        ? Expression.Constant(true)
                                        : Expression.Constant(false);
                            }

                            if ((parameterValue == null && constantExpression.Value != null)
                                || (parameterValue != null && constantExpression.Value == null))
                            {
                                return
                                    expression.NodeType == ExpressionType.Equal
                                        ? Expression.Constant(false)
                                        : Expression.Constant(true);
                            }
                        }
                    }
                }

                return base.VisitBinary(expression);
            }
        }

        private class SearchConditionTranslatingVisitor : RelinqExpressionVisitor
        {
            private bool _isSearchCondition;

            public SearchConditionTranslatingVisitor(bool isSearchCondition)
            {
                _isSearchCondition = isSearchCondition;
            }

            public static bool IsSearchCondition(Expression expression)
            {
                expression = expression.RemoveConvert();

                if (!(expression is BinaryExpression)
                    && (expression.NodeType != ExpressionType.Not)
                    && (expression.NodeType != ExpressionType.Extension))
                {
                    return false;
                }

                if (expression.IsComparisonOperation()
                    || expression.IsLogicalOperation()
                    || expression is LikeExpression
                    || expression is IsNullExpression
                    || expression is InExpression
                    || expression is ExistsExpression
                    || expression is StringCompareExpression)
                {
                    return true;
                }

                return false;
            }

            protected override Expression VisitBinary(BinaryExpression expression)
            {
                if (_isSearchCondition)
                {
                    if (expression.IsComparisonOperation())
                    {
                        var parentIsSearchCondition = _isSearchCondition;
                        _isSearchCondition = false;
                        var left = Visit(expression.Left);
                        var right = Visit(expression.Right);
                        _isSearchCondition = parentIsSearchCondition;

                        return Expression.MakeBinary(expression.NodeType, left, right);
                    }
                }
                else
                {
                    if (expression.IsLogicalOperation())
                    {
                        var parentIsSearchCondition = _isSearchCondition;
                        _isSearchCondition = true;
                        var left = Visit(expression.Left);
                        var right = Visit(expression.Right);
                        _isSearchCondition = parentIsSearchCondition;

                        return Expression.MakeBinary(expression.NodeType, left, right);
                    }

                    if (IsSearchCondition(expression))
                    {
                        return Expression.Condition(
                            expression,
                            Expression.Constant(true, typeof(bool)),
                            Expression.Constant(false, typeof(bool)));
                    }
                }

                return base.VisitBinary(expression);
            }

            protected override Expression VisitConditional(ConditionalExpression expression)
            {
                var parentIsSearchCondition = _isSearchCondition;
                _isSearchCondition = true;
                var test = Visit(expression.Test);
                _isSearchCondition = false;
                var ifTrue = Visit(expression.IfTrue);
                var ifFalse = Visit(expression.IfFalse);
                _isSearchCondition = parentIsSearchCondition;

                var newExpression = Expression.Condition(test, ifTrue, ifFalse);

                if (_isSearchCondition)
                {
                    return Expression.MakeBinary(
                        ExpressionType.Equal,
                        newExpression,
                        Expression.Constant(true, typeof(bool)));
                }
                return newExpression;
            }

            protected override Expression VisitUnary(UnaryExpression expression)
            {
                var operand = Visit(expression.Operand);

                if (_isSearchCondition)
                {
                    if (expression.NodeType == ExpressionType.Not
                        && expression.Operand.IsSimpleExpression())
                    {
                        return Expression.Equal(expression.Operand, Expression.Constant(false, typeof(bool)));
                    }
                }
                else
                {
                    if (IsSearchCondition(expression))
                    {
                        if (expression.NodeType == ExpressionType.Not)
                        {
                            return Expression.Condition(
                                operand,
                                Expression.Constant(false, typeof(bool)),
                                Expression.Constant(true, typeof(bool)));
                        }

                        if (expression.NodeType == ExpressionType.Convert
                            || expression.NodeType == ExpressionType.ConvertChecked)
                        {
                            return Expression.MakeUnary(expression.NodeType, operand, expression.Type);
                        }

                        return Expression.Condition(
                            Expression.MakeUnary(expression.NodeType, operand, expression.Type),
                            Expression.Constant(true, typeof(bool)),
                            Expression.Constant(false, typeof(bool)));
                    }
                }

                return base.VisitUnary(expression);
            }

            protected override Expression VisitExtension(Expression expression)
            {
                if (_isSearchCondition)
                {
                    var parentIsSearchCondition = _isSearchCondition;
                    _isSearchCondition = false;
                    var newExpression = base.VisitExtension(expression);
                    _isSearchCondition = parentIsSearchCondition;
                    return expression is AliasExpression || expression is ColumnExpression || expression is SelectExpression
                        ? Expression.Equal(newExpression, Expression.Constant(true, typeof(bool)))
                        : newExpression;
                }

                return base.VisitExtension(expression);
            }

            protected override Expression VisitParameter(ParameterExpression expression)
            {
                var newExpression = base.VisitParameter(expression);
                return _isSearchCondition && (newExpression.Type == typeof(bool))
                    ? Expression.Equal(newExpression, Expression.Constant(true, typeof(bool)))
                    : newExpression;
            }
        }
    }
}
