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
    public class XuguDateTimeMethodTranslator : IMethodCallTranslator
    {
        private readonly Dictionary<MethodInfo, string> _methodInfoDatePartMapping = new()
        {
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddYears), typeof(int)), "year" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMonths), typeof(int)), "month" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddDays), typeof(double)), "day" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddHours), typeof(double)), "hour" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMinutes), typeof(double)), "minute" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddSeconds), typeof(double)), "second" },
            { typeof(DateTime).GetRequiredRuntimeMethod(nameof(DateTime.AddMilliseconds), typeof(double)), "millisecond" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddYears), typeof(int)), "year" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddMonths), typeof(int)), "month" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddDays), typeof(double)), "day" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddHours), typeof(double)), "hour" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddMinutes), typeof(double)), "minute" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddSeconds), typeof(double)), "second" },
            { typeof(DateTimeOffset).GetRequiredRuntimeMethod(nameof(DateTimeOffset.AddMilliseconds), typeof(double)), "microsecond" }
        };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguDateTimeMethodTranslator(
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

            if (_methodInfoDatePartMapping.TryGetValue(method, out var datePart))
            {
                // DateAdd does not accept number argument outside of int range
                // AddYears/AddMonths take int argument so no need to check for range
#pragma warning disable CS0618 // 类型或成员已过时
                return !datePart.Equals("year")
                    && !datePart.Equals("month")
                    && arguments[0] is SqlConstantExpression sqlConstant
                    && ((double)sqlConstant.Value! >= int.MaxValue
                        || (double)sqlConstant.Value <= int.MinValue)
                        ? null
                        : _sqlExpressionFactory.Function(
                            "DATE_ADD",
                            new[]
                            {instance!,
                                _sqlExpressionFactory.Fragment("INTERVAL " + ((SqlConstantExpression)arguments[0]).Value!.ToString() + " " + datePart)
                            },
                            instance!.Type,
                            instance.TypeMapping);
#pragma warning restore CS0618 // 类型或成员已过时
            }

            return null;
        }
    }
}
