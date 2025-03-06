// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XuGuDbMigrationsAnnotationProvider : MigrationsAnnotationProvider
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IKey key)
        {
            var isClustered = key.XuGuDb().IsClustered;
            if (isClustered.HasValue)
            {
                yield return new Annotation(
                    XuGuDbFullAnnotationNames.Instance.Clustered,
                    isClustered.Value);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IIndex index)
        {
            var isClustered = index.XuGuDb().IsClustered;
            if (isClustered.HasValue)
            {
                yield return new Annotation(
                    XuGuDbFullAnnotationNames.Instance.Clustered,
                    isClustered.Value);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IProperty property)
        {
            if (property.XuGuDb().ValueGenerationStrategy == XuGuDbValueGenerationStrategy.IdentityColumn)
            {
                yield return new Annotation(
                    XuGuDbFullAnnotationNames.Instance.ValueGenerationStrategy,
                    XuGuDbValueGenerationStrategy.IdentityColumn);
            }
        }
    }
}
