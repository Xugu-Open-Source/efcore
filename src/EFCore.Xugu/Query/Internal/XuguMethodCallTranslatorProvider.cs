// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Xugu.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class XuguMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguMethodCallTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies)
            : base(dependencies)
        {
            var sqlExpressionFactory = dependencies.SqlExpressionFactory;
            var typeMappingSource = dependencies.RelationalTypeMappingSource;
            AddTranslators(
                new IMethodCallTranslator[]
                {
                    new XuguConvertTranslator(sqlExpressionFactory),
                    new XuguDateDiffFunctionsTranslator(sqlExpressionFactory),
                    new XuguFullTextSearchFunctionsTranslator(sqlExpressionFactory),
                    new XuguIsDateFunctionTranslator(sqlExpressionFactory),
                    new XuguMathTranslator(sqlExpressionFactory),
                    new XuguBinaryExpressionTranslator(sqlExpressionFactory),
                    new XuguNewGuidTranslator(sqlExpressionFactory),
                    new XuguObjectToStringTranslator(sqlExpressionFactory),
                    new XuguStringMethodTranslator(sqlExpressionFactory)
                });
        }
    }
}
