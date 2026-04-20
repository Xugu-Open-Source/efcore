// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class XGCompositeMethodCallTranslator : RelationalCompositeMethodCallTranslator
    {
        private static readonly IMethodCallTranslator[] _methodCallTranslators =
        {
            new XGStringSubstringTranslator(),
            new XGMathAbsTranslator(),
            new XGMathCeilingTranslator(),
            new XGMathFloorTranslator(),
            new XGMathPowerTranslator(),
            new XGMathRoundTranslator(),
            new XGMathTruncateTranslator(),
            new XGStringReplaceTranslator(),
            new XGStringToLowerTranslator(),
            new XGStringToUpperTranslator(),
            new XGRegexIsMatchTranslator(),
        };

        public XGCompositeMethodCallTranslator([NotNull] ILogger<XGCompositeMethodCallTranslator> logger)
            : base(logger)
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            AddTranslators(_methodCallTranslators);
        }
    }
}
