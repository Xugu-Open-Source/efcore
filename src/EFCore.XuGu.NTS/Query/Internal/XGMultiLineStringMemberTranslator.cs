// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGMultiLineStringMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _isClosed = typeof(MultiLineString).GetRuntimeProperty(nameof(MultiLineString.IsClosed));
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        public XGMultiLineStringMemberTranslator(XGSqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (Equals(member, _isClosed))
            {
                SqlExpression sqlExpression = _sqlExpressionFactory.NullableFunction(
                    "ST_IsClosed",
                    new [] {instance},
                    returnType,
                    false);

                // We return the following instead:
                // CASE
                //     WHEN instance IS NULL THEN NULL
                //     ELSE expression
                // END
                if (returnType == typeof(bool))
                {
                    sqlExpression = _sqlExpressionFactory.Case(
                        new[]
                        {
                            new CaseWhenClause(
                                _sqlExpressionFactory.IsNull(instance),
                                _sqlExpressionFactory.Constant(null, RelationalTypeMapping.NullMapping))
                        },
                        sqlExpression);
                }

                return sqlExpression;
            }

            return null;
        }
    }
}
