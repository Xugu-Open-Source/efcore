// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;
using EntityFrameworkCore.XuGu.Query.Internal;

namespace EntityFrameworkCore.XuGu.Json.Newtonsoft.Query.Internal
{
    public class XGJsonNewtonsoftMemberTranslatorPlugin : IMemberTranslatorPlugin
    {
        public XGJsonNewtonsoftMemberTranslatorPlugin(
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IXGJsonPocoTranslator jsonPocoTranslator)
        {
            var XGSqlExpressionFactory = (XGSqlExpressionFactory)sqlExpressionFactory;
            var XGJsonPocoTranslator = (XGJsonPocoTranslator)jsonPocoTranslator;

            Translators = new IMemberTranslator[]
            {
                new XGJsonNewtonsoftDomTranslator(
                    XGSqlExpressionFactory,
                    typeMappingSource,
                    XGJsonPocoTranslator),
                jsonPocoTranslator,
            };
        }

        public virtual IEnumerable<IMemberTranslator> Translators { get; }
    }
}
