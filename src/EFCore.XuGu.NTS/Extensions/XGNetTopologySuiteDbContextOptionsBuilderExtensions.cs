// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     NetTopologySuite specific extension methods for <see cref="XGDbContextOptionsBuilder" />.
    /// </summary>
    public static class XGNetTopologySuiteDbContextOptionsBuilderExtensions
    {
        /// <summary>
        ///     Use NetTopologySuite to access XG spatial data.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure XG. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static XGDbContextOptionsBuilder UseNetTopologySuite(
            [NotNull] this XGDbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;

            var extension = coreOptionsBuilder.Options.FindExtension<XGNetTopologySuiteOptionsExtension>()
                ?? new XGNetTopologySuiteOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)coreOptionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}
