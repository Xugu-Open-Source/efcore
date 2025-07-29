// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            new XGContainsOptimizedTranslator(),
            new XGConvertTranslator(),
            new XGDateAddTranslator(),
            new XGEndsWithOptimizedTranslator(),
            new XGMathTranslator(),
            new XGNewGuidTranslator(),
            new XGObjectToStringTranslator(),
            new XGRegexIsMatchTranslator(),
            new XGStartsWithOptimizedTranslator(),
            new XGStringIsNullOrWhiteSpaceTranslator(),
            new XGStringReplaceTranslator(),
            new XGStringSubstringTranslator(),
            new XGStringToLowerTranslator(),
            new XGStringToUpperTranslator(),
            new XGStringTrimEndTranslator(),
            new XGStringTrimStartTranslator(),
            new XGStringTrimTranslator(),
            new XGStringIndexOfTranslator(),
            new XGStringPadLeftRightTranslator()
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XGCompositeMethodCallTranslator(
            [NotNull] RelationalCompositeMethodCallTranslatorDependencies dependencies)
            : base(dependencies)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);
        }
    }
}
