// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGDateDiffFunctionsTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping
            = new Dictionary<MethodInfo, string>
            {
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffYear), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "YEAR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMonth), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "MONTH" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffDay), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "DAY" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffHour), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "HOUR" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMinute), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "MINUTE" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffSecond), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "SECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }), "MICROSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }), "MICROSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }), "MICROSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }), "MICROSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly) }), "MICROSECOND" },
                { typeof(XGDbFunctionsExtensions).GetRuntimeMethod(nameof(XGDbFunctionsExtensions.DateDiffMicrosecond), new[] { typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?) }), "MICROSECOND" },
            };

        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        public XGDateDiffFunctionsTranslator(XGSqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        public virtual SqlExpression Translate(
            SqlExpression instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            if (_methodInfoDateDiffMapping.TryGetValue(method, out var datePart))
            {
                var startDate = arguments[1];
                var endDate = arguments[2];
                var typeMapping = ExpressionExtensions.InferTypeMapping(startDate, endDate);

                startDate = _sqlExpressionFactory.ApplyTypeMapping(startDate, typeMapping);
                endDate = _sqlExpressionFactory.ApplyTypeMapping(endDate, typeMapping);

                return _sqlExpressionFactory.NullableFunction(
                    "TIMESTAMPDIFF",
                    new[]
                    {
                        _sqlExpressionFactory.Fragment(datePart),
                        startDate,
                        endDate
                    },
                    typeof(int),
                    typeMapping: null,
                    onlyNullWhenAnyNullPropagatingArgumentIsNull: true,
                    argumentsPropagateNullability: new []{false, true, true});
            }

            return null;
        }
    }
}
