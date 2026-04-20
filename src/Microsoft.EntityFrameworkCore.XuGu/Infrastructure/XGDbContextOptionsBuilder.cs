// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class XGDbContextOptionsBuilder : RelationalDbContextOptionsBuilder<XGDbContextOptionsBuilder, XGDbOptionsExtension>
    {
        public XGDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }

        protected override XGDbOptionsExtension CloneExtension()
            => new XGDbOptionsExtension(OptionsBuilder.Options.GetExtension<XGDbOptionsExtension>());
        public virtual void UseRowNumberForPaging() => SetOption(e => e.RowNumberPaging = true);
    }
}
