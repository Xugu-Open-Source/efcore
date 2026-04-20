// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore
{
    public static class XGMetadataExtensions
    {
        public static XGPropertyAnnotations XG([NotNull] this IMutableProperty property)
            => (XGPropertyAnnotations)XG((IProperty)property);
        public static IRelationalEntityTypeAnnotations XG([NotNull] this IEntityType entityType)
               => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), XGFullAnnotationNames.Instance);

        public static RelationalEntityTypeAnnotations XG([NotNull] this IMutableEntityType entityType)
            => (RelationalEntityTypeAnnotations)XG((IEntityType)entityType);

        public static IRelationalForeignKeyAnnotations XG([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), XGFullAnnotationNames.Instance);

        public static RelationalForeignKeyAnnotations XG([NotNull] this IMutableForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)XG((IForeignKey)foreignKey);

        public static IXGIndexAnnotations XG([NotNull] this IIndex index)
            => new XGIndexAnnotations(Check.NotNull(index, nameof(index)));

        public static RelationalIndexAnnotations XG([NotNull] this IMutableIndex index)
            => (XGIndexAnnotations)XG((IIndex)index);

        public static IRelationalKeyAnnotations XG([NotNull] this IKey key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)), XGFullAnnotationNames.Instance);

        public static RelationalKeyAnnotations XG([NotNull] this IMutableKey key)
            => (RelationalKeyAnnotations)XG((IKey)key);

        public static IXGModelAnnotations XG([NotNull] this IModel model)
            => new XGModelAnnotations(Check.NotNull(model, nameof(model)));

        public static XGModelAnnotations XG([NotNull] this IMutableModel model)
            => (XGModelAnnotations)XG((IModel)model);

        public static IRelationalPropertyAnnotations XG([NotNull] this IProperty property)
            => new RelationalPropertyAnnotations(Check.NotNull(property, nameof(property)), XGFullAnnotationNames.Instance);

    }
}
