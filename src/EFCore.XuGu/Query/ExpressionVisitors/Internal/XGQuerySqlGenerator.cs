// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGQuerySqlGenerator : QuerySqlGenerator
    {
        // The order in which the types are specified matters, because types get matched by using StartsWith.
        private static readonly Dictionary<string, string[]> _castMappings = new Dictionary<string, string[]>
        {
            { "signed", new []{ "tinyint", "smallint", "mediumint", "int", "bigint", "bit" }},
            { "decimal(65,30)", new []{ "decimal" } },
            { "double", new []{ "double" } },
            { "float", new []{ "float" } },
            { "binary", new []{ "binary", "varbinary", "tinyblob", "blob", "mediumblob", "longblob" } },
            { "datetime(6)", new []{ "datetime(6)" } },
            { "datetime", new []{ "datetime" } },
            { "date", new []{ "date" } },
            { "timestamp(6)", new []{ "timestamp(6)" } },
            { "timestamp", new []{ "timestamp" } },
            { "time(6)", new []{ "time(6)" } },
            { "time", new []{ "time" } },
            { "json", new []{ "json" } },
            { "char", new []{ "char", "varchar", "text", "tinytext", "mediumtext", "longtext" } },
            { "nchar", new []{ "nchar", "nvarchar" } },
            { "boolean", new []{ "boolean" } },
        };

        private const ulong LimitUpperBound = 999999999;

        private readonly IXGOptions _options;
        private string _removeTableAliasOld;
        private string _removeTableAliasNew;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XGQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [CanBeNull] IXGOptions options)
            : base(dependencies)
        {
            _options = options;
        }

        protected override void GenerateExists(ExistsExpression existsExpression, bool negated)
        {
            Sql.AppendLine("(SELECT CASE ")
                .Append("WHEN ");
            if (negated)
            {
                Sql.Append("NOT ");
            }

            Sql.AppendLine("EXISTS (");
            using (Sql.Indent())
            {
                Visit(existsExpression.Subquery);
            }

            Sql.Append(") ");
            Sql.AppendLine("THEN TRUE ELSE FALSE END");
            Sql.AppendLine("FROM DUAL)");
        }

        protected override Expression VisitCollate(CollateExpression collateExpression)
        {
            Visit(collateExpression.Operand);

            return collateExpression;
        }

        //protected override Expression VisitCase(CaseExpression caseExpression)
        //{
        //    Sql.Append("CASE");

        //    if (caseExpression.Operand != null)
        //    {
        //        Sql.Append(" ");
        //        Visit(caseExpression.Operand);
        //    }

        //    using (Sql.Indent())
        //    {
        //        foreach (var whenClause in caseExpression.WhenClauses)
        //        {
        //            Sql
        //                .AppendLine()
        //                .Append("WHEN ");
        //            Visit(whenClause.Test);
        //            Sql.Append(" THEN ");
        //            if (whenClause.Result.Print()=="1")
        //            {
        //                Sql.Append("TRUE ");
        //            }
        //            else if (whenClause.Result.Print() == "0")
        //            {
        //                Sql.Append("FALSE ");
        //            }
        //            else
        //            {
        //                Visit(whenClause.Result);
        //            }
        //        }

        //        if (caseExpression.ElseResult != null)
        //        {
        //            Sql
        //                .AppendLine()
        //                .Append("ELSE ");
        //            if (caseExpression.ElseResult.Print() == "1")
        //            {
        //                Sql.Append("TRUE ");
        //            }
        //            else if (caseExpression.ElseResult.Print() == "0")
        //            {
        //                Sql.Append("FALSE ");
        //            }
        //            else
        //            {
        //                Visit(caseExpression.ElseResult);
        //            }
        //        }
        //    }

        //    Sql
        //        .AppendLine()
        //        .Append("END");

        //    return caseExpression;
        //}

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
            {
                XGJsonTraversalExpression jsonTraversalExpression => VisitJsonPathTraversal(jsonTraversalExpression),
                XGColumnAliasReferenceExpression columnAliasReferenceExpression => VisitColumnAliasReference(columnAliasReferenceExpression),
                _ => base.VisitExtension(extensionExpression)
            };

        private Expression VisitColumnAliasReference(XGColumnAliasReferenceExpression columnAliasReferenceExpression)
        {
            Sql.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnAliasReferenceExpression.Alias));

            return columnAliasReferenceExpression;
        }

        protected virtual Expression VisitJsonPathTraversal(XGJsonTraversalExpression expression)
        {
            // If the path contains parameters, then the -> and ->> aliases are not supported by XuGu, because
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

        protected override Expression VisitColumn(ColumnExpression columnExpression)
        {
            if (_removeTableAliasOld is not null &&
                columnExpression.TableAlias == _removeTableAliasOld)
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
            if (_removeTableAliasOld is not null &&
                tableExpression.Alias == _removeTableAliasOld)
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
                    Sql.AppendLine().Append($"LIMIT {LimitUpperBound}");
                }

                Sql.Append(" OFFSET ");
                Visit(selectExpression.Offset);
            }
        }

        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (sqlFunctionExpression.Name.StartsWith("@@", StringComparison.Ordinal))
            {
                Sql.Append(sqlFunctionExpression.Name);

                return sqlFunctionExpression;
            }

            return base.VisitSqlFunction(sqlFunctionExpression);
        }

        protected override Expression VisitCrossApply(CrossApplyExpression crossApplyExpression)
        {
            Sql.Append("JOIN ");

            if (crossApplyExpression.Table is not TableExpression)
            {
                Sql.Append("LATERAL ");
            }

            Visit(crossApplyExpression.Table);

            Sql.Append(" ON TRUE");

            return crossApplyExpression;
        }

        protected override Expression VisitOuterApply(OuterApplyExpression outerApplyExpression)
        {
            Sql.Append("LEFT JOIN ");

            if (outerApplyExpression.Table is not TableExpression)
            {
                Sql.Append("LATERAL ");
            }

            Visit(outerApplyExpression.Table);

            Sql.Append(" ON TRUE");

            return outerApplyExpression;
        }

        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            Check.NotNull(sqlBinaryExpression, nameof(sqlBinaryExpression));

            if (sqlBinaryExpression.OperatorType == ExpressionType.Add &&
                sqlBinaryExpression.Type == typeof(string) &&
                sqlBinaryExpression.Left.TypeMapping?.ClrType == typeof(string) &&
                sqlBinaryExpression.Right.TypeMapping?.ClrType == typeof(string))
            {
                Sql.Append("CONCAT(");
                Visit(sqlBinaryExpression.Left);
                Sql.Append(", ");
                Visit(sqlBinaryExpression.Right);
                Sql.Append(")");

                return sqlBinaryExpression;
            }
            if (sqlBinaryExpression.OperatorType == ExpressionType.Modulo &&
                sqlBinaryExpression.Type == typeof(int) &&
                sqlBinaryExpression.Left.TypeMapping?.ClrType == typeof(int) &&
                sqlBinaryExpression.Right.TypeMapping?.ClrType == typeof(int))
            {
                Sql.Append("MOD(");
                Visit(sqlBinaryExpression.Left);
                Sql.Append(", ");
                Visit(sqlBinaryExpression.Right);
                Sql.Append(")");

                return sqlBinaryExpression;
            }

            var requiresBrackets = RequiresBrackets(sqlBinaryExpression.Left);

            if (requiresBrackets)
            {
                Sql.Append("(");
            }

            Visit(sqlBinaryExpression.Left);

            if (requiresBrackets)
            {
                Sql.Append(")");
            }

            Sql.Append(GetOperator(sqlBinaryExpression));

            // EF uses unary Equal and NotEqual to represent is-null checking.
            // These need to be surrounded with parenthesis in various cases (e.g. where TRUE = x IS NOT NULL).
            // See https://github.com/PomeloFoundation/Microsoft.EntityFrameworkCore.XuGu/issues/1309
            requiresBrackets = RequiresBrackets(sqlBinaryExpression.Right) ||
                               !requiresBrackets &&
                               sqlBinaryExpression.Right is SqlUnaryExpression sqlUnaryExpression &&
                               (sqlUnaryExpression.OperatorType == ExpressionType.Equal || sqlUnaryExpression.OperatorType == ExpressionType.NotEqual);

            if (requiresBrackets)
            {
                Sql.Append("(");
            }

            Visit(sqlBinaryExpression.Right);

            if (requiresBrackets)
            {
                Sql.Append(")");
            }

            return sqlBinaryExpression;
        }

        protected override Expression VisitDelete(DeleteExpression deleteExpression)
        {
            var selectExpression = deleteExpression.SelectExpression;

            if (selectExpression.Offset == null
                && selectExpression.Having == null
                && selectExpression.GroupBy.Count == 0
                && selectExpression.Projection.Count == 0
                && (selectExpression.Tables.Count == 1 || selectExpression.Orderings.Count == 0 && selectExpression.Limit is null))
            {
                var removeSingleTableAlias = selectExpression.Tables.Count == 1 &&
                                             selectExpression.Orderings.Count > 0 || selectExpression.Limit is not null;

                Sql.Append($"DELETE");

                if (!removeSingleTableAlias)
                {
                    Sql.Append($" {Dependencies.SqlGenerationHelper.DelimitIdentifier(deleteExpression.Table.Name)}");
                }

                Sql.AppendLine().Append("FROM ");

                if (removeSingleTableAlias)
                {
                    _removeTableAliasOld = selectExpression.Tables[0].Alias;
                    _removeTableAliasNew = null;
                }

                GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());

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
                RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(RelationalQueryableExtensions.ExecuteDelete)));
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
                GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());

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

                GenerateLimitOffset(selectExpression);

                return updateExpression;
            }

            throw new InvalidOperationException(
                RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(RelationalQueryableExtensions.ExecuteUpdate)));
        }

        protected virtual void GenerateList<T>(
            IReadOnlyList<T> items,
            Action<T> generationAction,
            Action<IRelationalCommandBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(Sql);
                }

                generationAction(items[i]);
            }
        }

        private static bool RequiresBrackets(SqlExpression expression)
            => expression is SqlBinaryExpression
               || expression is LikeExpression
               || (expression is SqlUnaryExpression unary
                   && unary.Operand.Type == typeof(bool)
                   && (unary.OperatorType == ExpressionType.Equal
                       || unary.OperatorType == ExpressionType.NotEqual));

        public virtual Expression VisitXGRegexp(XGRegexpExpression xgRegexpExpression)
        {
            Check.NotNull(xgRegexpExpression, nameof(xgRegexpExpression));

            Visit(xgRegexpExpression.Match);
            Sql.Append(" REGEXP ");
            Visit(xgRegexpExpression.Pattern);

            return xgRegexpExpression;
        }

        public virtual Expression VisitXGMatch(XGMatchExpression xgMatchExpression)
        {
            Check.NotNull(xgMatchExpression, nameof(xgMatchExpression));
            string against = xgMatchExpression.Against.Print().Replace(" == 'True'", "").Replace("'True' == ", "");

            Sql.Append("CONTAINS (");
            if (xgMatchExpression.Match.Type == typeof(String[]) && xgMatchExpression.Match is XGComplexFunctionArgumentExpression complexFunctionArgumentExpression)
            {
                for (int i = 0; i < complexFunctionArgumentExpression.ArgumentParts.Count; i++)
                {
                    if (i > 0)
                    {
                        Sql.Append($", {complexFunctionArgumentExpression.ArgumentParts[i].Print().Replace(" == 'True'", "").Replace("'True' == ", "")}");
                    }
                    else
                    {
                        Sql.Append($"{complexFunctionArgumentExpression.ArgumentParts[i].Print().Replace(" == 'True'", "").Replace("'True' == ", "")}");
                    }
                }
            }
            else
            {
                Sql.Append($"{xgMatchExpression.Match.Print().Replace(" == 'True'", "").Replace("'True' == ", "")}");
            }

            Sql.Append(",");
            Sql.Append($"{against})");

            //switch (xgMatchExpression.SearchMode)
            //{
            //    case XGMatchSearchMode.NaturalLanguage:
            //        break;
            //    case XGMatchSearchMode.NaturalLanguageWithQueryExpansion:
            //        Sql.Append(" WITH QUERY EXPANSION");
            //        break;
            //    case XGMatchSearchMode.Boolean:
            //        Sql.Append(", 1)");
            //        return xgMatchExpression;
            //}

            //Sql.Append(")");


            return xgMatchExpression;
        }

        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
            => sqlUnaryExpression.OperatorType == ExpressionType.Convert
                ? VisitConvert(sqlUnaryExpression)
                : base.VisitSqlUnary(sqlUnaryExpression);

        private SqlUnaryExpression VisitConvert(SqlUnaryExpression sqlUnaryExpression)
        {
            List<string> numberTypes = new List<string> { "tinyint", "smallint", "mediumint", "int", "bigint", "decimal", "float", "double" };
            var castMapping = GetCastStoreType(sqlUnaryExpression.TypeMapping);

            if (castMapping == "binary")
            {
                Sql.Append("UNHEX(HEX(");
                Visit(sqlUnaryExpression.Operand);
                Sql.Append("))");
                return sqlUnaryExpression;
            }

            // There needs to be no CAST() applied between the exact same store type. This could happen, e.g. if
            // `System.DateTime` and `System.DateTimeOffset` are used in conjunction, because both use different type
            // mappings, but map to the same store type (e.g. `datetime(6)`).
            //
            // There also is no need for a double CAST() to the same type. Due to only rudimentary CAST() support in
            // XuGu, the final store type of a CAST() operation might be different than the store type of the type
            // mapping of the expression (e.g. "float" will be cast to "double"). So we optimize these cases too.
            //
            // An exception is the JSON data type, when used in conjunction with a parameter (like `JsonDocument`).
            // JSON parameters like that will be serialized to string and supplied as a string parameter to XuGu
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

                if (castMapping.StartsWith("double", StringComparison.OrdinalIgnoreCase) &&
                    !_options.ServerVersion.Supports.DoubleCast)
                {
                    useDecimalToDoubleWorkaround = true;
                    castMapping = "decimal(65,30)";
                }

                if (useDecimalToDoubleWorkaround)
                {
                    Sql.Append("(");
                }

                Sql.Append("CAST(");
                Visit(sqlUnaryExpression.Operand);
                Sql.Append(" AS ");
                Sql.Append(castMapping);
                Sql.Append(")");

                if (useDecimalToDoubleWorkaround)
                {
                    Sql.Append(" + 0e0)");
                }
            }
            else if (castMapping == "boolean" && sqlUnaryExpression.Operand is SqlUnaryExpression sqlExpression && numberTypes.Contains(sqlExpression.Operand.TypeMapping.StoreType))
            {
                Sql.Append("CAST(");
                Sql.Append("(CASE WHEN ");
                Visit(sqlExpression.Operand);
                Sql.Append(" != 0 THEN 1 ELSE 0 END");
                Sql.Append(")");
                Sql.Append(" AS ");
                Sql.Append(castMapping);
                Sql.Append(")");
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
                    if (storeTypeLower.StartsWith(storeType, StringComparison.OrdinalIgnoreCase))
                    {
                        //castMapping = kvp.Key;
                        castMapping = storeType;
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

            // As of XuGu 8.0.18, a FLOAT cast might unnecessarily drop decimal places and round,
            // so we just keep casting to double instead. XuguClient ensures, that a System.Single
            // will be returned if expected, even if we return a DOUBLE.
            if (castMapping.StartsWith("float", StringComparison.OrdinalIgnoreCase) &&
                !_options.ServerVersion.Supports.FloatCast)
            {
                castMapping = "double";
            }

            return castMapping;
        }

        public virtual Expression VisitXGComplexFunctionArgumentExpression(XGComplexFunctionArgumentExpression xgComplexFunctionArgumentExpression)
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
                    Sql.Append(xgComplexFunctionArgumentExpression.Delimiter);
                }

                Visit(argument);
            }

            return xgComplexFunctionArgumentExpression;
        }

        public virtual Expression VisitXGCollateExpression(XGCollateExpression xgCollateExpression)
        {
            Check.NotNull(xgCollateExpression, nameof(xgCollateExpression));

            Visit(xgCollateExpression.ValueExpression);

            return xgCollateExpression;
        }

        public virtual Expression VisitXGBinaryExpression(XGBinaryExpression xgBinaryExpression)
        {
            if (xgBinaryExpression.OperatorType == XGBinaryExpressionOperatorType.NonOptimizedEqual)
            {
                var equalExpression = new SqlBinaryExpression(
                    ExpressionType.Equal,
                    xgBinaryExpression.Left,
                    xgBinaryExpression.Right,
                    xgBinaryExpression.Type,
                    xgBinaryExpression.TypeMapping);

                Visit(equalExpression);
            }
            else
            {
                Sql.Append("(");
                Visit(xgBinaryExpression.Left);
                Sql.Append(")");

                switch (xgBinaryExpression.OperatorType)
                {
                    case XGBinaryExpressionOperatorType.IntegerDivision:
                        Sql.Append(" DIV ");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                Sql.Append("(");
                Visit(xgBinaryExpression.Right);
                Sql.Append(")");
            }

            return xgBinaryExpression;
        }

        /// <inheritdoc />
        protected override void CheckComposableSql(string sql)
        {
            // XuGu supports CTE (WITH) expressions within subqueries, as well as others,
            // so we allow any raw SQL to be composed over.
        }
    }
}
