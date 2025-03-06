// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    public static class XuGuDbMetadataExtensions
    {

        public static XuGuDbPropertyAnnotations XuGuDb([NotNull] this IMutableProperty property)
            => (XuGuDbPropertyAnnotations)XuGuDb((IProperty)property);

        public static IXuGuDbPropertyAnnotations XuGuDb([NotNull] this IProperty property)
            => new XuGuDbPropertyAnnotations(Check.NotNull(property, nameof(property)));

        public static RelationalEntityTypeAnnotations XuGuDb([NotNull] this IMutableEntityType entityType)
            => (RelationalEntityTypeAnnotations)XuGuDb((IEntityType)entityType);

        public static IRelationalEntityTypeAnnotations XuGuDb([NotNull] this IEntityType entityType)
            => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), XuGuDbFullAnnotationNames.Instance);

        public static XuGuDbKeyAnnotations XuGuDb([NotNull] this IMutableKey key)
            => (XuGuDbKeyAnnotations)XuGuDb((IKey)key);

        public static IXuGuDbKeyAnnotations XuGuDb([NotNull] this IKey key)
            => new XuGuDbKeyAnnotations(Check.NotNull(key, nameof(key)));

        public static XuGuDbIndexAnnotations XuGuDb([NotNull] this IMutableIndex index)
            => (XuGuDbIndexAnnotations)XuGuDb((IIndex)index);

        public static IXuGuDbIndexAnnotations XuGuDb([NotNull] this IIndex index)
            => new XuGuDbIndexAnnotations(Check.NotNull(index, nameof(index)));

        public static RelationalForeignKeyAnnotations XuGuDb([NotNull] this IMutableForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)XuGuDb((IForeignKey)foreignKey);

        public static IRelationalForeignKeyAnnotations XuGuDb([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), XuGuDbFullAnnotationNames.Instance);

        public static XuGuDbModelAnnotations XuGuDb([NotNull] this IMutableModel model)
            => (XuGuDbModelAnnotations)XuGuDb((IModel)model);

        public static IXuGuDbModelAnnotations XuGuDb([NotNull] this IModel model)
            => new XuGuDbModelAnnotations(Check.NotNull(model, nameof(model)));
    }
}
