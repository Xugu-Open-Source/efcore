// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class XuGuDbKeyAnnotations : RelationalKeyAnnotations, IXuGuDbKeyAnnotations
    {
        public XuGuDbKeyAnnotations([NotNull] IKey key)
            : base(key, XuGuDbFullAnnotationNames.Instance)
        {
        }

        protected XuGuDbKeyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, XuGuDbFullAnnotationNames.Instance)
        {
        }

        public virtual bool? IsClustered
        {
            get { return (bool?)Annotations.GetAnnotation(XuGuDbFullAnnotationNames.Instance.Clustered, null); }
            [param: CanBeNull] set { SetIsClustered(value); }
        }

        protected virtual bool SetIsClustered(bool? value)
            => Annotations.SetAnnotation(XuGuDbFullAnnotationNames.Instance.Clustered, null, value);
    }
}
