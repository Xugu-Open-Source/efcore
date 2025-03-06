// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     XuGuDb specific extension methods for <see cref="ReferenceCollectionBuilder"/>.
    /// </summary>
    public static class XuGuDbReferenceCollectionBuilderExtensions
    {
        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting XuGuDb.
        /// </summary>
        /// <param name="referenceCollectionBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceCollectionBuilder ForXuGuDbHasConstraintName(
            [NotNull] this ReferenceCollectionBuilder referenceCollectionBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceCollectionBuilder, nameof(referenceCollectionBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceCollectionBuilder.Metadata.XuGuDb().Name = name;

            return referenceCollectionBuilder;
        }

        /// <summary>
        ///     Configures the foreign key constraint name for this relationship when targeting XuGuDb.
        /// </summary>
        /// <param name="referenceCollectionBuilder"> The builder being used to configure the relationship. </param>
        /// <param name="name"> The name of the foreign key constraint. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        /// <typeparam name="TEntity"> The principal entity type in this relationship. </typeparam>
        /// <typeparam name="TRelatedEntity"> The dependent entity type in this relationship. </typeparam>
        public static ReferenceCollectionBuilder<TEntity, TRelatedEntity> ForXuGuDbHasConstraintName<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceCollectionBuilder<TEntity, TRelatedEntity>)ForXuGuDbHasConstraintName(
                (ReferenceCollectionBuilder)referenceCollectionBuilder, name);
    }
}
