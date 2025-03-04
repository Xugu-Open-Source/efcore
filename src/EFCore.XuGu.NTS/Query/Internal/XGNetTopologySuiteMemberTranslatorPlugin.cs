// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace EntityFrameworkCore.XuGu.Query.Internal
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
            var XGSqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;

            Translators = new IMemberTranslator[]
            {
                new XGGeometryMemberTranslator(typeMappingSource, XGSqlExpressionFactory),
                new XGGeometryCollectionMemberTranslator(XGSqlExpressionFactory),
                new XGLineStringMemberTranslator(typeMappingSource, XGSqlExpressionFactory, options),
                new XGMultiLineStringMemberTranslator(XGSqlExpressionFactory),
                new XGPointMemberTranslator(XGSqlExpressionFactory),
                new XGPolygonMemberTranslator(typeMappingSource, XGSqlExpressionFactory)
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
