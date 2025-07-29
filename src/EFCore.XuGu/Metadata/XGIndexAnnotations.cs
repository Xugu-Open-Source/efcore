// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;
using JetBrains.Annotations;

//ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class XGIndexAnnotations : RelationalIndexAnnotations, IXGIndexAnnotations
    {
        public XGIndexAnnotations([NotNull] IIndex index)
            : base(index)
        {
        }

        protected XGIndexAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations)
        {
        }

        public virtual bool? IsFullText
        {
            get => (bool?)Annotations.Metadata[XGAnnotationNames.FullTextIndex];
            set => SetIsFullText(value);
        }

        protected virtual bool SetIsFullText(bool? value) => Annotations.SetAnnotation(
            XGAnnotationNames.FullTextIndex,
            value);

        public virtual bool? IsSpatial
        {
            get => (bool?)Annotations.Metadata[XGAnnotationNames.SpatialIndex];
            set => SetIsSpatial(value);
        }

        protected virtual bool SetIsSpatial(bool? value) => Annotations.SetAnnotation(
            XGAnnotationNames.SpatialIndex,
            value);
    }
}
