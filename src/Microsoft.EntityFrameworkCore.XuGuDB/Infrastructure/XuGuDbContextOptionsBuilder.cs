// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Allows SQL Server specific configuration to be performed on <see cref="DbContextOptions"/>.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from a call to 
    ///         <see cref="SqlServerDbContextOptionsExtensions.UseSqlServer(DbContextOptionsBuilder, string, System.Action{SqlServerDbContextOptionsBuilder})"/>
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class XuGuDbContextOptionsBuilder
        : RelationalDbContextOptionsBuilder<XuGuDbContextOptionsBuilder, XuGuDbOptionsExtension>
    {
        
        public XuGuDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
            : base(optionsBuilder)
        {
        }

        /// <summary>
        ///     Clones the configuration in this builder.
        /// </summary>
        /// <returns> The cloned configuration. </returns>
        protected override XuGuDbOptionsExtension CloneExtension()
            => new XuGuDbOptionsExtension(OptionsBuilder.Options.GetExtension<XuGuDbOptionsExtension>());

        /// <summary>
        ///     Use a ROW_NUMBER() in queries instead of OFFSET/FETCH. This method is backwards-compatible to SQL Server 2005.
        /// </summary>
        public virtual void UseRowNumberForPaging() => SetOption(e => e.RowNumberPaging = true);
    }
}
