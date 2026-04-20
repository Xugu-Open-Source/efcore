// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class XGInternalMetadataBuilderExtensions
    {
        public static RelationalModelBuilderAnnotations XG(
            [NotNull] this InternalModelBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalModelBuilderAnnotations(builder, configurationSource, XGFullAnnotationNames.Instance);

        public static XGPropertyBuilderAnnotations XG(
            [NotNull] this InternalPropertyBuilder builder,
            ConfigurationSource configurationSource)
            => new XGPropertyBuilderAnnotations(builder, configurationSource);

        public static RelationalEntityTypeBuilderAnnotations XG(
            [NotNull] this InternalEntityTypeBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalEntityTypeBuilderAnnotations(builder, configurationSource, XGFullAnnotationNames.Instance);

        public static RelationalKeyBuilderAnnotations XG(
            [NotNull] this InternalKeyBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalKeyBuilderAnnotations(builder, configurationSource, XGFullAnnotationNames.Instance);

        public static RelationalIndexBuilderAnnotations XG(
            [NotNull] this InternalIndexBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalIndexBuilderAnnotations(builder, configurationSource, XGFullAnnotationNames.Instance);

        public static RelationalForeignKeyBuilderAnnotations XG(
            [NotNull] this InternalRelationshipBuilder builder,
            ConfigurationSource configurationSource)
            => new RelationalForeignKeyBuilderAnnotations(builder, configurationSource, XGFullAnnotationNames.Instance);

        
    }
}