// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGNetTopologySuiteMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XGNetTopologySuiteMemberTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IXGOptions options)
        {
            var xgSqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;

            Translators = new IMemberTranslator[]
            {
                new XGGeometryMemberTranslator(typeMappingSource, xgSqlExpressionFactory),
                new XGGeometryCollectionMemberTranslator(xgSqlExpressionFactory),
                new XGLineStringMemberTranslator(typeMappingSource, xgSqlExpressionFactory, options),
                new XGMultiLineStringMemberTranslator(xgSqlExpressionFactory),
                new XGPointMemberTranslator(xgSqlExpressionFactory),
                new XGPolygonMemberTranslator(typeMappingSource, xgSqlExpressionFactory)
            };
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEnumerable<IMemberTranslator> Translators { get; }
    }
}
