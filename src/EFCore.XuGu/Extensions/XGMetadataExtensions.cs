// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     XuGu specific extension methods for metadata.
    /// </summary>
    public static class XGMetadataExtensions
    {
        /// <summary>
        ///     Gets the XuGu specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The XuGu specific metadata for the property. </returns>
        public static XGPropertyAnnotations XG([NotNull] this IMutableProperty property)
            => (XGPropertyAnnotations)XG((IProperty)property);

        /// <summary>
        ///     Gets the XuGu specific metadata for a property.
        /// </summary>
        /// <param name="property"> The property to get metadata for. </param>
        /// <returns> The XuGu specific metadata for the property. </returns>
        public static IXGPropertyAnnotations XG([NotNull] this IProperty property)
            => new XGPropertyAnnotations(Check.NotNull(property, nameof(property)));

        /// <summary>
        ///     Gets the XuGu specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The XuGu specific metadata for the index. </returns>
        public static XGIndexAnnotations XG([NotNull] this IMutableIndex index)
            => (XGIndexAnnotations)XG((IIndex)index);

        /// <summary>
        ///     Gets the XuGu specific metadata for an index.
        /// </summary>
        /// <param name="index"> The index to get metadata for. </param>
        /// <returns> The XuGu specific metadata for the index. </returns>
        public static IXGIndexAnnotations XG([NotNull] this IIndex index)
            => new XGIndexAnnotations(Check.NotNull(index, nameof(index)));
    }
}
