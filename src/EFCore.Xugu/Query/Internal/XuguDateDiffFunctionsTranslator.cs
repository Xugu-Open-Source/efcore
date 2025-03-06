// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class XuguDateDiffFunctionsTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping
            = new()
            {
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "YEAR"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "YEAR"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "YEAR"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffYear),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "YEAR"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MONTH"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MONTH"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MONTH"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMonth),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MONTH"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "DAY"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "DAY"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "DAY"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffDay),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "DAY"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "HOUR"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "HOUR"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "HOUR"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "HOUR"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "HOUR"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffHour),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "HOUR"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MINUTE"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MINUTE"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MINUTE"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MINUTE"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "MINUTE"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMinute),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "MINUTE"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "SECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "SECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "SECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "SECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "SECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffSecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "SECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MILLISECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MILLISECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MILLISECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MILLISECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "MILLISECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMillisecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "MILLISECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTime), typeof(DateTime) }),
                    "MICROSECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?) }),
                    "MICROSECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset) }),
                    "MICROSECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?) }),
                    "MICROSECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan) }),
                    "MICROSECOND"
                },
                {
                    typeof(XuguDbFunctionsExtensions).GetRequiredRuntimeMethod(
                        nameof(XuguDbFunctionsExtensions.DateDiffMicrosecond),
                        new[] { typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?) }),
                    "MICROSECOND"
                },



            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguDateDiffFunctionsTranslator(
            ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression? Translate(
            SqlExpression? instance,
            MethodInfo method,
            IReadOnlyList<SqlExpression> arguments,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(method, nameof(method));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(logger, nameof(logger));

            if (_methodInfoDateDiffMapping.TryGetValue(method, out var datePart))
            {
                var startDate = arguments[1];
                var endDate = arguments[2];
                var typeMapping = ExpressionExtensions.InferTypeMapping(startDate, endDate);

                startDate = _sqlExpressionFactory.ApplyTypeMapping(startDate, typeMapping);
                endDate = _sqlExpressionFactory.ApplyTypeMapping(endDate, typeMapping);

                return _sqlExpressionFactory.Function(
                    "TIMESTAMPDIFF",
                    new[] { _sqlExpressionFactory.Fragment(datePart), startDate, endDate },
                    nullable: true,
                    argumentsPropagateNullability: new[] { false, true, true },
                    typeof(int));
            }

            return null;
        }
    }
}
