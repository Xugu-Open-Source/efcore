// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class XuGuDbKeyBuilderExtensions
    {
        /// <summary>
        ///     Configures the name of the key constraint in the database when targeting XuGuDb.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="name"> The name of the key. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static KeyBuilder ForXuGuDbHasName([NotNull] this KeyBuilder keyBuilder, [CanBeNull] string name)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            keyBuilder.Metadata.XuGuDb().Name = name;

            return keyBuilder;
        }

        /// <summary>
        ///     Configures whether the key is clustered when targeting XuGuDb.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key being configured. </param>
        /// <param name="clustered"> A value indicating whether the key is clustered. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static KeyBuilder ForXuGuDbIsClustered([NotNull] this KeyBuilder keyBuilder, bool clustered = true)
        {
            Check.NotNull(keyBuilder, nameof(keyBuilder));

            keyBuilder.Metadata.XuGuDb().IsClustered = clustered;

            return keyBuilder;
        }
    }
}
