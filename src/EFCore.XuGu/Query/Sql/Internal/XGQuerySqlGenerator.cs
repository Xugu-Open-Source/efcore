// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Sql.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGQuerySqlGenerator : DefaultQuerySqlGenerator, IXGExpressionVisitor
    {
        private const ulong LimitUpperBound = 999999999;

        protected override string TypedTrueLiteral => "TRUE";
        protected override string TypedFalseLiteral => "FALSE";

        private readonly bool _noBackslashEscapes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XGQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] SelectExpression selectExpression,
                IXGOptions options)
            : base(dependencies, selectExpression)
        {
            _noBackslashEscapes = options?.NoBackslashEscapes ?? false;
        }

        protected override void GenerateTop(SelectExpression selectExpression)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void GenerateLimitOffset(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null)
            {
                Sql.AppendLine().Append("LIMIT ");
                Visit(selectExpression.Limit);
            }

            if (selectExpression.Offset != null)
            {
                if (selectExpression.Limit == null)
                {
                    // if we want to use Skip() without Take() we have to define the upper limit of LIMIT
                    Sql.AppendLine().Append("LIMIT ").Append(LimitUpperBound);
                }

                Sql.Append(" OFFSET ");
                Visit(selectExpression.Offset);
            }
        }

        private static readonly HashSet<string> _builtInFunctions
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "MAX",
                "MIN",
                "SUM",
                "SUBSTR",
                "INSTR",
                "LENGTH",
                "COUNT"
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            switch (sqlFunctionExpression.FunctionName)
            {
                case "EXTRACT":
                    Sql.Append(sqlFunctionExpression.FunctionName);
                    Sql.Append("(");

                    Visit(sqlFunctionExpression.Arguments[0]);

                    Sql.Append(" FROM ");

                    Visit(sqlFunctionExpression.Arguments[1]);

                    Sql.Append(")");

                    return sqlFunctionExpression;

                case "CAST":
                    Sql.Append(sqlFunctionExpression.FunctionName);
                    Sql.Append("(");

                    Visit(sqlFunctionExpression.Arguments[0]);

                    Sql.Append(" AS ");

                    Visit(sqlFunctionExpression.Arguments[1]);

                    Sql.Append(")");

                    return sqlFunctionExpression;

                case "AVG" when sqlFunctionExpression.Type == typeof(decimal):
                case "SUM" when sqlFunctionExpression.Type == typeof(decimal):
                    Sql.Append("CAST(");

                    base.VisitSqlFunction(sqlFunctionExpression);

                    Sql.Append(" AS NUMBER(29,4))");

                    return sqlFunctionExpression;

                case "INSTR":
                    if (sqlFunctionExpression.Arguments[1] is ParameterExpression parameterExpression
                        && ParameterValues.TryGetValue(parameterExpression.Name, out var value)
                        && ((string)value)?.Length == 0)
                    {
                        return Visit(Expression.Constant(1));
                    }

                    break;

                case "ADD_MONTHS":
                    Sql.Append("CAST(");

                    base.VisitSqlFunction(sqlFunctionExpression);

                    Sql.Append(" AS TIMESTAMP)");

                    return sqlFunctionExpression;

                case "COUNT":
                    if (sqlFunctionExpression.Type == typeof(int))
                    {
                        Sql.Append("CAST(");
                        base.VisitSqlFunction(sqlFunctionExpression);
                        Sql.Append(" AS INTEGER)");
                    }
                    
                    return sqlFunctionExpression;

            }

            return base.VisitSqlFunction(
                // non-instance & non-built-in functions without schema needs to be delimited
                (!_builtInFunctions.Contains(sqlFunctionExpression.FunctionName)
                && sqlFunctionExpression.Instance == null)
                    ? sqlFunctionExpression.IsNiladic
                        ? new SqlFunctionExpression(
                            SqlGenerator.DelimitIdentifier(sqlFunctionExpression.FunctionName),
                            sqlFunctionExpression.Type,
                            sqlFunctionExpression.IsNiladic)
                        : new SqlFunctionExpression(
                            SqlGenerator.DelimitIdentifier(sqlFunctionExpression.FunctionName),
                            sqlFunctionExpression.Type,
                            /* schema:*/ null,
                            sqlFunctionExpression.Arguments)
                    : sqlFunctionExpression);
        }

        protected override void GenerateProjection(Expression projection)
        {
            var aliasedProjection = projection as AliasExpression;
            var expressionToProcess = aliasedProjection?.Expression ?? projection;
            var updatedExperssion = ExplicitCastToBool(expressionToProcess);

            expressionToProcess = aliasedProjection != null
                ? new AliasExpression(aliasedProjection.Alias, updatedExperssion)
                : updatedExperssion;

            base.GenerateProjection(expressionToProcess);
        }

        private Expression ExplicitCastToBool(Expression expression)
        {
            return (expression as BinaryExpression)?.NodeType == ExpressionType.Coalesce
                   && expression.Type.UnwrapNullableType() == typeof(bool)
                ? new ExplicitCastExpression(expression, expression.Type)
                : expression;
        }

        protected override Expression ApplyExplicitCastToBoolInProjectionOptimization(Expression expression)
        {
            //if (expression is AliasExpression aliasExpression && aliasExpression.Expression is BinaryExpression binaryExpression && binaryExpression.NodeType==ExpressionType.And)
            //{
            //    return new AliasExpression(aliasExpression.Alias, ExplicitCastToBool(aliasExpression.Expression));
            //}

            return expression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.Add &&
                binaryExpression.Left.Type == typeof(string) &&
                binaryExpression.Right.Type == typeof(string))
            {
                Sql.Append("CONCAT(");
                Visit(binaryExpression.Left);
                Sql.Append(", ");
                var exp = Visit(binaryExpression.Right);
                Sql.Append(")");

                return binaryExpression;
            }
            else if ((binaryExpression.NodeType == ExpressionType.And || binaryExpression.NodeType == ExpressionType.Or) &&
                    binaryExpression.Left.Type == typeof(bool) && 
                    binaryExpression.Right.Type == typeof(bool))
            {
                Visit(binaryExpression.Left);
                Sql.Append(binaryExpression.NodeType == ExpressionType.And ? " AND " : " OR ");
                Visit(binaryExpression.Right);
                return binaryExpression;
            }

            return base.VisitBinary(binaryExpression);
        }

        private bool _isParameterReplaced = false;
        public override bool IsCacheable => !_isParameterReplaced && base.IsCacheable;

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            if (_noBackslashEscapes)
            {
                //instead of having XGConnector replace parameter placeholders with escaped values
                //(causing "parameterized" queries to fail with NO_BACKSLASH_ESCAPES),
                //directly insert the value with only replacing ' with ''
                Check.NotNull(parameterExpression, nameof(parameterExpression));
                object value;
                var isRegistered = ParameterValues.TryGetValue(parameterExpression.Name, out value);
                if (isRegistered && value is string)
                {
                    _isParameterReplaced = true;
                    return VisitConstant(Expression.Constant(value));
                }
            }

            return base.VisitParameter(parameterExpression);
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            Check.NotNull(constantExpression, nameof(constantExpression));
            if (constantExpression.Type == typeof(bool))
            {
                Sql.Append(" "+constantExpression.Value.ToString().ToUpper()+" ");
            }
            else
            {
                base.VisitConstant(constantExpression);
            }

            return constantExpression;
        }

        public virtual Expression VisitRegexp(RegexpExpression regexpExpression)
        {
            Check.NotNull(regexpExpression, nameof(regexpExpression));

            Visit(regexpExpression.Match);
            Sql.Append(" REGEXP ");
            Visit(regexpExpression.Pattern);

            return regexpExpression;
        }

        private static readonly Dictionary<string, string[]> CastMappings = new Dictionary<string, string[]>
        {
            { "signed", new []{ "tinyint", "smallint", "mediumint", "int", "bigint" }},
            { "decimal", new []{ "decimal", "double", "float" } },
            { "binary", new []{ "binary", "varbinary", "tinyblob", "blob", "mediumblob", "longblob" } },
            { "datetime", new []{ "datetime", "timestamp" } },
            { "time", new []{ "time" } },
            { "json", new []{ "json" } },
        };

        public override Expression VisitExplicitCast(ExplicitCastExpression explicitCastExpression)
        {
            Sql.Append("CAST(");
            Visit(explicitCastExpression.Operand);
            Sql.Append(" AS ");
            var typeMapping = Dependencies.TypeMappingSource.FindMapping(explicitCastExpression.Type);
            if (typeMapping == null)
            {
                throw new InvalidOperationException($"Cannot cast to type '{explicitCastExpression.Type.Name}'");
            }

            var storeTypeLower = typeMapping.StoreType.ToLower();
            string castMapping = null;
            foreach (var kvp in CastMappings)
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
            if (castMapping == "signed" && storeTypeLower.Contains("unsigned"))
            {
                castMapping = "unsigned";
            }
            else if (castMapping == null)
            {
                castMapping = "char";
            }

            Sql.Append(castMapping);
            Sql.Append(")");

            return explicitCastExpression;
        }

        public Expression VisitXGComplexFunctionArgumentExpression(
            [NotNull] XGComplexFunctionArgumentExpression xgComplexFunctionArgumentExpression)
        {
            Check.NotNull(xgComplexFunctionArgumentExpression, nameof(xgComplexFunctionArgumentExpression));

            var first = true;
            foreach (var argument in xgComplexFunctionArgumentExpression.ArgumentParts)
            {
                if (first)
                {
                    first = false;
                }
                else
                { 
                    Sql.Append(" ");
                }

                Visit(argument);
            }

            return xgComplexFunctionArgumentExpression;
        }
    }
}
