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
    public class XuguDateTimeMemberTranslator : IMemberTranslator
    {
        private static readonly Dictionary<string, string> _datePartMapping
            = new()
            {
                { nameof(DateTime.Year), "year" },
                { nameof(DateTime.Month), "month" },
                { nameof(DateTime.DayOfYear), "dayofyear" },
                { nameof(DateTime.Day), "day" },
                { nameof(DateTime.Hour), "hour" },
                { nameof(DateTime.Minute), "minute" },
                { nameof(DateTime.Second), "second" },
                { nameof(DateTime.Millisecond), "millisecond" }
            };

        private readonly ISqlExpressionFactory _sqlExpressionFactory;


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguDateTimeMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
            MemberInfo member,
            Type returnType,
            IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        {
            Check.NotNull(member, nameof(member));
            Check.NotNull(returnType, nameof(returnType));
            Check.NotNull(logger, nameof(logger));

            var declaringType = member.DeclaringType;

            if (declaringType == typeof(DateTime)
                || declaringType == typeof(DateTimeOffset))
            {
                var memberName = member.Name;

                if (_datePartMapping.TryGetValue(memberName, out var datePart))
                {
                    return _sqlExpressionFactory.Function(
                        "EXTRACT",
                       new[] { _sqlExpressionFactory.Fragment(datePart + " FROM `" + ((ColumnExpression)instance!).Name + "`") },
                        nullable: true,
                        argumentsPropagateNullability: new[] { false, true },
                        returnType);
                }

                switch (memberName)
                {
                    case nameof(DateTime.TimeOfDay):
                        return _sqlExpressionFactory.Convert(instance!, returnType);

#pragma warning disable CS0618 // 类型或成员已过时
                    case nameof(DateTime.DayOfYear):
                        return _sqlExpressionFactory.Function(
                          "DAYOFYEAR",
                          new[] { instance! },
                          returnType,
                          instance!.TypeMapping);

                    case nameof(DateTime.Date):
                        if (AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue19052", out var isEnabled) && isEnabled)
                        {
                            return _sqlExpressionFactory.Function(
                                "DATE",
                                new[] { instance! },
                                returnType,
                                instance!.TypeMapping);
                        }

                        return _sqlExpressionFactory.Function(
                            "DATE",
                            new[] { instance! },
                            returnType,
                            declaringType == typeof(DateTime)
                                ? instance!.TypeMapping
                                : _sqlExpressionFactory.FindMapping(typeof(DateTime)));
                    case nameof(DateTime.Now):
                        return _sqlExpressionFactory.Function(
                            declaringType == typeof(DateTimeOffset) ? "UTC_TIMESTAMP" : "CURRENT_TIMESTAMP",
                            Array.Empty<SqlExpression>(),
                            returnType);
                    case nameof(DateTime.UtcNow):
                        var serverTranslation = _sqlExpressionFactory.Function(
                            declaringType == typeof(DateTime) ? "UTC_TIMESTAMP" : "UTC_TIMESTAMP",
                            Array.Empty<SqlExpression>(),
                            returnType);

                        return declaringType == typeof(DateTime)
                            ? (SqlExpression)serverTranslation
                            : _sqlExpressionFactory.Convert(serverTranslation, returnType);

                    case nameof(DateTime.Today):
                        return _sqlExpressionFactory.Function(
                            declaringType == typeof(DateTimeOffset)
                            ? "UTC_DATE"
                            : "CURDATE",
                            Array.Empty<SqlExpression>(),
                            returnType);
#pragma warning restore CS0618 // 类型或成员已过时
                }
            }

            return null;
        }
    }
}
