// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using EntityFrameworkCore.XuGu.Internal;
using EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;
using EntityFrameworkCore.XuGu.Query.Internal;
using EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using EFCore.XuGu.Properties;
using System.Data;

namespace EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    public class XGSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
    {
        private readonly IXGJsonPocoTranslator _jsonPocoTranslator;
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        protected static readonly MethodInfo[] NewArrayExpressionSupportMethodInfos = Array.Empty<MethodInfo>()
            .Concat(typeof(XGDbFunctionsExtensions).GetRuntimeMethods().Where(m => m.Name == nameof(XGDbFunctionsExtensions.Match)))
            .Concat(typeof(string).GetRuntimeMethods().Where(m => m.Name == nameof(string.Concat)))
            .Where(m => m.GetParameters().Any(p => p.ParameterType.IsArray))
            .ToArray();

        public XGSqlTranslatingExpressionVisitor(
            RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            QueryCompilationContext queryCompilationContext,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            [CanBeNull] IXGJsonPocoTranslator jsonPocoTranslator)
            : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
        {
            _jsonPocoTranslator = jsonPocoTranslator;
            _sqlExpressionFactory = (XGSqlExpressionFactory)Dependencies.SqlExpressionFactory;
        }

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.ArrayLength)
            {
                if (TranslationFailed(unaryExpression.Operand, Visit(unaryExpression.Operand), out var sqlOperand))
                {
                    return null;
                }

                if (sqlOperand.Type == typeof(byte[]) &&
                    (sqlOperand.TypeMapping == null || sqlOperand.TypeMapping is XGByteArrayTypeMapping))
                {
                    return _sqlExpressionFactory.NullableFunction(
                        "LENGTH",
                        new[] {sqlOperand},
                        typeof(int));
                }

                return _jsonPocoTranslator?.TranslateArrayLength(sqlOperand);
            }


            // Make explicit casts implicit if they are applied to a JSON traversal object.
            // It is pretty common for Newtonsoft.Json objects to be cast to other types (e.g. casting from
            // JToken to JArray to check an arrays length via the JContainer.Count property).
            if (unaryExpression.NodeType == ExpressionType.Convert ||
                unaryExpression.NodeType == ExpressionType.ConvertChecked)
            {
                var visitedOperand = Visit(unaryExpression.Operand);
                if (visitedOperand is XGJsonTraversalExpression traversal)
                {
                    return unaryExpression.Type == typeof(object)
                        ? traversal
                        : traversal.Clone(
                            traversal.ReturnsText,
                            unaryExpression.Type,
                            Dependencies.TypeMappingSource.FindMapping(unaryExpression.Type));
                }

                ResetTranslationErrorDetails();
            }

            var visitedExpression = base.VisitUnary(unaryExpression);
            if (visitedExpression == null)
            {
                return null;
            }

            // XG implicitly casts numbers used in BITWISE NOT operations (~ operator) to BIGINT UNSIGNED.
            // We need to cast them back, to get the expected result.
            if (visitedExpression is SqlUnaryExpression sqlUnaryExpression &&
                sqlUnaryExpression.OperatorType == ExpressionType.Not &&
                sqlUnaryExpression.Type != typeof(bool))
            {
                return _sqlExpressionFactory.Convert(
                    sqlUnaryExpression,
                    sqlUnaryExpression.Type,
                    sqlUnaryExpression.TypeMapping);
            }

            return visitedExpression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
            {
                if (TranslationFailed(binaryExpression.Left, Visit(TryRemoveImplicitConvert(binaryExpression.Left)), out var sqlLeft)
                    || TranslationFailed(binaryExpression.Right, Visit(TryRemoveImplicitConvert(binaryExpression.Right)), out var sqlRight))
                {
                    return null;
                }

                // Try translating ArrayIndex inside json column
                var expression = _jsonPocoTranslator?.TranslateMemberAccess(
                    sqlLeft,
                    _sqlExpressionFactory.JsonArrayIndex(sqlRight),
                    binaryExpression.Type);

                if (expression != null)
                {
                    return expression;
                }
            }

            var visitedExpression = (SqlExpression)base.VisitBinary(binaryExpression);
            if (visitedExpression == null)
            {
                return null;
            }

            if (visitedExpression is SqlBinaryExpression visitedBinaryExpression)
            {
                // Returning null forces client projection.
                // CHECK: Is this still true in .NET Core 3.0?
                switch (visitedBinaryExpression.OperatorType)
                {
                    case ExpressionType.Add:
                        if (IsDateTimeBasedOperation(visitedBinaryExpression))
                        {
                            return null;
                        }
                        else if (visitedBinaryExpression.Type == typeof(string))
                        {
                            return new SqlFunctionExpression("CONCAT", new[] { visitedBinaryExpression.Left, visitedBinaryExpression.Right }, false, new[] { false, false }, visitedBinaryExpression.Type, visitedBinaryExpression.TypeMapping);
                        }
                        else if (visitedBinaryExpression.Type == typeof(int))
                        {
                            return new SqlBinaryExpression(visitedBinaryExpression.OperatorType, visitedBinaryExpression.Left, visitedBinaryExpression.Right, typeof(long), new LongTypeMapping("BIGINT", DbType.Int64));
                        }
                        else
                        {
                            return visitedBinaryExpression;
                        }
                        //return IsDateTimeBasedOperation(visitedBinaryExpression)
                        //    ? null
                        //    : visitedBinaryExpression.Type!=typeof(string) ?visitedBinaryExpression:new SqlBinaryExpression(ExpressionType.OrElse,visitedBinaryExpression.Left,visitedBinaryExpression.Right,visitedBinaryExpression.Type,visitedBinaryExpression.TypeMapping);
                    case ExpressionType.Subtract:
                    case ExpressionType.Multiply:
                    case ExpressionType.Divide:
                        if (IsDateTimeBasedOperation(visitedBinaryExpression))
                        {
                            return null;
                        }
                        else if (visitedBinaryExpression.Type == typeof(int))
                        {
                            return new SqlBinaryExpression(visitedBinaryExpression.OperatorType, visitedBinaryExpression.Left, visitedBinaryExpression.Right, typeof(long), new LongTypeMapping("BIGINT", DbType.Int64));
                        }
                        else
                        {
                            return visitedBinaryExpression;
                        }
                    case ExpressionType.Modulo:
                        return IsDateTimeBasedOperation(visitedBinaryExpression)
                            ? null
                            : new SqlFunctionExpression("MOD", new[] { visitedBinaryExpression.Left, visitedBinaryExpression.Right }, false, new[] { false, false }, visitedBinaryExpression.Type, visitedBinaryExpression.TypeMapping);
                }
            }

            return visitedExpression;
        }

        protected virtual Expression VisitMethodCallNewArray(NewArrayExpression newArrayExpression)
        {
            // Needed for XGDbFunctionsExtensions.Match() and String.Concat() translation.
            if (newArrayExpression.Type == typeof(string[]))
            {
                return _sqlExpressionFactory.ComplexFunctionArgument(
                    newArrayExpression.Expressions.Select(e => (SqlExpression)Visit(e))
                        .ToArray(),
                    ", ",
                    typeof(string[]));
            }

            // Needed for String.Concat() translation.
            if (newArrayExpression.Type == typeof(object[]))
            {
                var typeMapping = ((XGStringTypeMapping)Dependencies.TypeMappingSource.GetMapping(typeof(string))).Clone(forceToString: true);
                return _sqlExpressionFactory.ComplexFunctionArgument(
                    newArrayExpression.Expressions.Select(e => Dependencies.SqlExpressionFactory.ApplyTypeMapping((SqlExpression)Visit(e), typeMapping))
                        .ToArray(),
                    ", ",
                    typeof(object[]),
                    typeMapping);
            }

            return base.VisitNewArray(newArrayExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (NewArrayExpressionSupportMethodInfos.Contains(methodCallExpression.Method))
            {
                var arguments = new Expression[methodCallExpression.Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                {
                    var argument = methodCallExpression.Arguments[i];

                    if (argument is NewArrayExpression newArrayExpression)
                    {
                        if (TranslationFailed(argument, VisitMethodCallNewArray(newArrayExpression), out var sqlExpression))
                        {
                            return null;
                        }

                        arguments[i] = sqlExpression;
                    }
                    else
                    {
                        arguments[i] = argument;
                    }
                }

                methodCallExpression = methodCallExpression.Update(methodCallExpression.Object, arguments);
            }

            var result = base.VisitMethodCall(methodCallExpression);
            if (result == null &&
                XGStringComparisonMethodTranslator.StringComparisonMethodInfos.Any(m => m == methodCallExpression.Method))
            {
                var message = XGStrings.QueryUnableToTranslateMethodWithStringComparison(
                    methodCallExpression.Method.DeclaringType.Name,
                    methodCallExpression.Method.Name,
                    nameof(XGDbContextOptionsBuilder.EnableStringComparisonTranslations));

                // EF Core returns an error message on its own, when the string.Equals() methods (static and non-static) are being used with
                // a `StringComparison` parameter.
                // Since we also support other translations, but all of them only when opted in, we will replace the EF Core error message
                // with our own, that is more appropriate for our case.
                if (TranslationErrorDetails.Contains(CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison))
                {
                    var translationErrorDetails = TranslationErrorDetails;
                    ResetTranslationErrorDetails();
                    message = translationErrorDetails.Replace(CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison, message);
                }

                AddTranslationErrorDetails(message);
            }

            return result;
        }

        private static bool IsDateTimeBasedOperation(SqlBinaryExpression binaryExpression)
        {
            if (binaryExpression.TypeMapping != null
                && (binaryExpression.TypeMapping.StoreType.StartsWith("date") || binaryExpression.TypeMapping.StoreType.StartsWith("time")))
            {
                return true;
            }

            return false;
        }

        protected virtual void ResetTranslationErrorDetails()
        {
            // When we try translating an expression and later decide that we want to discard the result, we need to remove any translation
            // error details, or those details might end up more than once in generated exceptions down the stack.
            //
            // We use a workaround here, that will result in the TranslationErrorDetails being set to `null` again.
            // Otherwise, we would need to override  TranslationErrorDetails, AddTranslationErrorDetails and Translate, reimplement the
            // TranslationErrorDetails functionality and maintain everything just to support resetting the TranslationErrorDetails property.
            base.Translate(Expression.Constant(0));
        }

        #region Copied from RelationalSqlTranslatingExpressionVisitor

        private static Expression TryRemoveImplicitConvert(Expression expression)
        {
            if (expression is UnaryExpression unaryExpression)
            {
                if (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                {
                    var innerType = unaryExpression.Operand.Type.UnwrapNullableType();
                    if (innerType.IsEnum)
                    {
                        innerType = Enum.GetUnderlyingType(innerType);
                    }
                    var convertedType = unaryExpression.Type.UnwrapNullableType();

                    if (innerType == convertedType
                        || (convertedType == typeof(int)
                            && (innerType == typeof(byte)
                                || innerType == typeof(sbyte)
                                || innerType == typeof(char)
                                || innerType == typeof(short)
                                || innerType == typeof(ushort))))
                    {
                        return TryRemoveImplicitConvert(unaryExpression.Operand);
                    }
                }
            }

            return expression;
        }

        [DebuggerStepThrough]
        private bool TranslationFailed(Expression original, Expression translation, out SqlExpression castTranslation)
        {
            if (original != null && !(translation is SqlExpression))
            {
                castTranslation = null;
                return true;
            }

            castTranslation = translation as SqlExpression;
            return false;
        }
        public override SqlExpression TranslateLongCount(SqlExpression sqlExpression)
        {
            Check.NotNull(sqlExpression, nameof(sqlExpression));

            return Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(
                Dependencies.SqlExpressionFactory.Function(
                    "COUNT",
                    new[] { sqlExpression },
                    nullable: false,
                    argumentsPropagateNullability: new[] { false },
                    typeof(long)));
        }
        public override SqlExpression TranslateCount(SqlExpression sqlExpression)
        {
            Check.NotNull(sqlExpression, nameof(sqlExpression));

            return Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(
                Dependencies.SqlExpressionFactory.Function(
                    "COUNT",
                    new[] { sqlExpression },
                    nullable: false,
                    argumentsPropagateNullability: new[] { false },
                    typeof(long)));
        }

        #endregion Copied from RelationalSqlTranslatingExpressionVisitor
    }
}
