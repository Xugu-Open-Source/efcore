// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
    {
        private readonly IEnumerable<IXGEvaluatableExpressionFilter> _XGEvaluatableExpressionFilters;

        public XGEvaluatableExpressionFilter(
            [NotNull] EvaluatableExpressionFilterDependencies dependencies,
            [NotNull] RelationalEvaluatableExpressionFilterDependencies relationalDependencies,
            [NotNull] IEnumerable<IXGEvaluatableExpressionFilter> XGEvaluatableExpressionFilters)
            : base(dependencies, relationalDependencies)
        {
            _XGEvaluatableExpressionFilters = XGEvaluatableExpressionFilters;
        }

        public override bool IsEvaluatableExpression(Expression expression, IModel model)
        {
            foreach (var evaluatableExpressionFilter in _XGEvaluatableExpressionFilters)
            {
                var evaluatable = evaluatableExpressionFilter.IsEvaluatableExpression(expression, model);
                if (evaluatable.HasValue)
                {
                    return evaluatable.Value;
                }
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                var declaringType = methodCallExpression.Method.DeclaringType;

                if (declaringType == typeof(XGDbFunctionsExtensions) ||
                    declaringType == typeof(XGJsonDbFunctionsExtensions))
                {
                    return false;
                }
            }

            return base.IsEvaluatableExpression(expression, model);
        }
    }
}
