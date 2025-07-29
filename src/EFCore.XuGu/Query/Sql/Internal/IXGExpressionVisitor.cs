// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Sql.Internal
{
    public interface IXGExpressionVisitor
    {
        Expression VisitRegexp([NotNull] RegexpExpression regexpExpression);
        Expression VisitXGComplexFunctionArgumentExpression([NotNull] XGComplexFunctionArgumentExpression xgComplexFunctionArgumentExpression);
    }
}
