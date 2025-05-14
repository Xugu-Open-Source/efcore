// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;

public class XGInlinedParameterExpression : SqlExpression
{
    public XGInlinedParameterExpression(
        SqlParameterExpression parameterExpression,
        SqlConstantExpression valueExpression)
        : base(parameterExpression.Type, parameterExpression.TypeMapping)
    {
        Check.NotNull(parameterExpression, nameof(parameterExpression));

        ParameterExpression = parameterExpression;
        ValueExpression = valueExpression;
    }

    public virtual Expression ParameterExpression { get; }
    public virtual SqlConstantExpression ValueExpression { get; }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var parameterExpression = (SqlParameterExpression)visitor.Visit(ParameterExpression);
        var valueExpression = (SqlConstantExpression)visitor.Visit(ValueExpression);

        return Update(parameterExpression, valueExpression);
    }

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(ValueExpression);
        expressionPrinter.Append(" (<<== ");
        expressionPrinter.Visit(ParameterExpression);
        expressionPrinter.Append(")");
    }

    public virtual XGInlinedParameterExpression Update(SqlParameterExpression parameterExpression, SqlConstantExpression valueExpression)
        => parameterExpression != ParameterExpression || valueExpression != ValueExpression
            ? new XGInlinedParameterExpression(parameterExpression, valueExpression)
            : this;

    public override bool Equals(object obj)
        => obj != null
           && (ReferenceEquals(this, obj)
               || obj is XGInlinedParameterExpression inlinedParameterExpression
               && Equals(inlinedParameterExpression));

    private bool Equals(XGInlinedParameterExpression inlinedParameterExpression)
        => base.Equals(inlinedParameterExpression)
           && ParameterExpression.Equals(inlinedParameterExpression.ParameterExpression)
           && ValueExpression.Equals(inlinedParameterExpression.ValueExpression);

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), ParameterExpression, ValueExpression);
}
