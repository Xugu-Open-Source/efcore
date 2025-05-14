// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

/// <summary>
/// Inject parameter inlining expressions where parameters are not supported for some reason.
/// </summary>
public class XGParametersInliningExpressionVisitor : ExpressionVisitor
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IXGOptions _options;

    private IReadOnlyDictionary<string, object> _parametersValues;
    private bool _canCache;

    private bool _inJsonTableSourceParameterCall;

    public XGParametersInliningExpressionVisitor(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlExpressionFactory sqlExpressionFactory,
        IXGOptions options)
    {
        _typeMappingSource = typeMappingSource;
        _sqlExpressionFactory = sqlExpressionFactory;
        _options = options;
    }

    public virtual Expression Process(Expression expression, IReadOnlyDictionary<string, object> parametersValues, out bool canCache)
    {
        Check.NotNull(expression, nameof(expression));

        _parametersValues = parametersValues;
        _canCache = true;

        var result = Visit(expression);

        canCache = _canCache;

        return result;
    }

    protected override Expression VisitExtension(Expression extensionExpression)
        => extensionExpression switch
        {
            XGJsonTableExpression jsonTableExpression => VisitJsonTable(jsonTableExpression),
            SqlParameterExpression sqlParameterExpression => VisitSqlParameter(sqlParameterExpression),
            ShapedQueryExpression shapedQueryExpression => shapedQueryExpression.Update(
                Visit(shapedQueryExpression.QueryExpression),
                Visit(shapedQueryExpression.ShaperExpression)),
            _ => base.VisitExtension(extensionExpression)
        };

    protected virtual Expression VisitJsonTable(XGJsonTableExpression jsonTableExpression)
    {
        var parentInJsonTableSourceParameterCall = _inJsonTableSourceParameterCall;
        _inJsonTableSourceParameterCall = true;
        var jsonExpression = (SqlExpression)Visit(jsonTableExpression.JsonExpression);
        _inJsonTableSourceParameterCall = parentInJsonTableSourceParameterCall;

        return jsonTableExpression.Update(
            jsonExpression,
            jsonTableExpression.Path,
            jsonTableExpression.ColumnInfos);
    }

    protected virtual Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
    {
        // For test simplicity, we currently inline parameters even for non XG database engines.
        // TODO: Use inlined parameters only if JsonTableImplementationUsingParameterAsSourceWithoutEngineCrash is true.
        if (_inJsonTableSourceParameterCall /*&&
            !_options.ServerVersion.Supports.JsonTableImplementationUsingParameterAsSourceWithoutEngineCrash*/)
        {
            _canCache = false;

            return new XGInlinedParameterExpression(
                sqlParameterExpression,
                _sqlExpressionFactory.Constant(
                    _parametersValues[sqlParameterExpression.Name],
                    sqlParameterExpression.TypeMapping));
        }

        return sqlParameterExpression;
    }
}
