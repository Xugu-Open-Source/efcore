// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    public class XGMathAbsTranslator : MultipleOverloadStaticMethodCallTranslator
    {
        public XGMathAbsTranslator()
            : base(typeof(Math), "Abs", "ABS")
        {
        }
    }
}
