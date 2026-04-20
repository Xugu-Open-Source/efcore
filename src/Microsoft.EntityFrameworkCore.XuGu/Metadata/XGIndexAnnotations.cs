// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class XGIndexAnnotations : RelationalIndexAnnotations, IXGIndexAnnotations
    {
        public XGIndexAnnotations([NotNull] IIndex index)
            : base(index, XGFullAnnotationNames.Instance)
        {
        }

        protected XGIndexAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, XGFullAnnotationNames.Instance)
        {
        }

        public string Method
        {
            get { return (string)Annotations.GetAnnotation(XGFullAnnotationNames.Instance.IndexMethod, null); }
            set { Annotations.SetAnnotation(XGFullAnnotationNames.Instance.IndexMethod, null, value); }
        }
    }
}
