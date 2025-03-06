// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XuGuDbCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            new XuGuDbMathAbsTranslator(),
            new XuGuDbMathCeilingTranslator(),
            new XuGuDbMathFloorTranslator(),
            new XuGuDbMathPowerTranslator(),
            new XuGuDbMathRoundTranslator(),
            new XuGuDbMathTruncateTranslator(),
            new XuGuDbNewGuidTranslator(),
            new XuGuDbStringIsNullOrWhiteSpaceTranslator(),
            new XuGuDbStringReplaceTranslator(),
            new XuGuDbStringSubstringTranslator(),
            new XuGuDbStringToLowerTranslator(),
            new XuGuDbStringToUpperTranslator(),
            new XuGuDbStringTrimEndTranslator(),
            new XuGuDbStringTrimStartTranslator(),
            new XuGuDbStringTrimTranslator(),
            new XuGuDbConvertTranslator()
        };

        // ReSharper disable once SuggestBaseTypeForParameter
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XuGuDbCompositeMethodCallTranslator([NotNull] ILogger<XuGuDbCompositeMethodCallTranslator> logger)
            : base(logger)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);
        }
    }
}
