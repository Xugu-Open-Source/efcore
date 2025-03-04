// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Query.Internal;
using EntityFrameworkCore.XuGu.Storage.Internal;

namespace EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    public class XGJsonParameterExpressionVisitor : ExpressionVisitor
    {
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;
        private readonly IXGOptions _options;

        public XGJsonParameterExpressionVisitor(XGSqlExpressionFactory sqlExpressionFactory, IXGOptions options)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _options = options;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
            {
                SqlParameterExpression sqlParameterExpression => VisitParameter(sqlParameterExpression),
                ShapedQueryExpression shapedQueryExpression => shapedQueryExpression.Update(Visit(shapedQueryExpression.QueryExpression), Visit(shapedQueryExpression.ShaperExpression)),
                _ => base.VisitExtension(extensionExpression)
            };

        protected virtual SqlExpression VisitParameter(SqlParameterExpression sqlParameterExpression)
        {
            if (sqlParameterExpression.TypeMapping is XGJsonTypeMapping)
            {
                var typeMapping = _sqlExpressionFactory.FindMapping(sqlParameterExpression.Type, "json");

                // XG has a real JSON datatype, and string parameters need to be converted to it.
                // MariaDB defines the JSON datatype just as a synonym for LONGTEXT.
                if (!_options.ServerVersion.Supports.JsonDataTypeEmulation)
                {
                    return _sqlExpressionFactory.Convert(
                        sqlParameterExpression,
                        typeMapping.ClrType, // will be typeof(string) when `sqlParameterExpression.Type`
                        typeMapping);        // is typeof(XGJsonString)
                }
            }

            return sqlParameterExpression;
        }
    }
}
