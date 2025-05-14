// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGGeometryCollectionMemberTranslator : IMemberTranslator
    {
        private static readonly MemberInfo _count = typeof(GeometryCollection).GetRuntimeProperty(nameof(GeometryCollection.Count));
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        public XGGeometryCollectionMemberTranslator(XGSqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(SqlExpression instance, MemberInfo member, Type returnType, IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (Equals(member, _count))
            {
                // Returns NULL for an empty geometry argument.
                return _sqlExpressionFactory.NullableFunction(
                    "ST_NumGeometries",
                    new [] {instance},
                    returnType,
                    false);
            }

            return null;
        }
    }
}
