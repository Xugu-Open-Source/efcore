// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class XuGuDbInternalMetadataBuilderExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static XuGuDbModelBuilderAnnotations XuGuDb(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new XuGuDbModelBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static XuGuDbPropertyBuilderAnnotations XuGuDb(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new XuGuDbPropertyBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static RelationalEntityTypeBuilderAnnotations XuGuDb(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeBuilderAnnotations(builder, configurationSource, XuGuDbFullAnnotationNames.Instance);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static XuGuDbKeyBuilderAnnotations XuGuDb(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new XuGuDbKeyBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static XuGuDbIndexBuilderAnnotations XuGuDb(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new XuGuDbIndexBuilderAnnotations(builder, configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static RelationalForeignKeyBuilderAnnotations XuGuDb(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource, XuGuDbFullAnnotationNames.Instance);
    }
}
