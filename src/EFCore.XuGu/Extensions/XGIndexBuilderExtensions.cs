// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{

    public static class XGIndexBuilderExtensions
    {
        public static IndexBuilder ForXGIsFullText([NotNull] this IndexBuilder indexBuilder, bool fullText = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.XG().IsFullText = fullText;

            return indexBuilder;
        }

        public static IndexBuilder ForXGIsSpatial([NotNull] this IndexBuilder indexBuilder, bool Spatial = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.XG().IsSpatial = Spatial;

            return indexBuilder;
        }
    }
}
