// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;
using EntityFrameworkCore.XuGu.Storage.Internal;

namespace EntityFrameworkCore.XuGu.Query.Internal
{
    public class XGMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public XGMethodCallTranslatorProvider(
            [NotNull] RelationalMethodCallTranslatorProviderDependencies dependencies,
            [NotNull] IXGOptions options)
            : base(dependencies)
        {
            var sqlExpressionFactory = (XGSqlExpressionFactory)dependencies.SqlExpressionFactory;
            var relationalTypeMappingSource = (XGTypeMappingSource)dependencies.RelationalTypeMappingSource;

            AddTranslators(new IMethodCallTranslator[]
            {
                new XGByteArrayMethodTranslator(sqlExpressionFactory),
                new XGConvertTranslator(sqlExpressionFactory),
                new XGDateTimeMethodTranslator(sqlExpressionFactory),
                new XGDateDiffFunctionsTranslator(sqlExpressionFactory),
                new XGDbFunctionsExtensionsMethodTranslator(sqlExpressionFactory),
                new XGJsonDbFunctionsTranslator(sqlExpressionFactory),
                new XGMathMethodTranslator(sqlExpressionFactory),
                new XGNewGuidTranslator(sqlExpressionFactory),
                new XGObjectToStringTranslator(sqlExpressionFactory),
                new XGRegexIsMatchTranslator(sqlExpressionFactory),
                new XGStringComparisonMethodTranslator(sqlExpressionFactory, options),
                new XGStringMethodTranslator(sqlExpressionFactory, relationalTypeMappingSource, options),
            });
        }
    }
}
