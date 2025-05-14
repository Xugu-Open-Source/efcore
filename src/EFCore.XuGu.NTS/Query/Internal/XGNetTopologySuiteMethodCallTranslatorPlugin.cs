// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGNetTopologySuiteMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XGNetTopologySuiteMethodCallTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IXGOptions options)
        {
            var xgSqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;

            Translators = new IMethodCallTranslator[]
            {
                new XGGeometryMethodTranslator(typeMappingSource, xgSqlExpressionFactory, options),
                new XGGeometryCollectionMethodTranslator(typeMappingSource, xgSqlExpressionFactory),
                new XGLineStringMethodTranslator(typeMappingSource, xgSqlExpressionFactory),
                new XGPolygonMethodTranslator(typeMappingSource, xgSqlExpressionFactory),
                new XGSpatialDbFunctionsExtensionsMethodTranslator(typeMappingSource, xgSqlExpressionFactory, options)
            };
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<IMethodCallTranslator> Translators { get; }
    }
}
