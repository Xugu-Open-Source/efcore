// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class XGCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        public XGCompositeMemberTranslator()
        {
            var XGTranslators = new List<IMemberTranslator>
            {
                new XGStringLengthTranslator(),
                new XGDateTimeNowTranslator()
            };

            AddTranslators(XGTranslators);
        }
    }
}
