using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class XGKeyAnnotations : RelationalKeyAnnotations, IXGKeyAnnotations
    {
        public XGKeyAnnotations([NotNull] IKey key)
            : base(key, XGFullAnnotationNames.Instance)
        {
        }

        protected XGKeyAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, XGFullAnnotationNames.Instance)
        {
        }

        public virtual bool? IsClustered
        {
            get { return (bool?)Annotations.GetAnnotation(XGFullAnnotationNames.Instance.Clustered, null); }
            [param: CanBeNull]
            set { SetIsClustered(value); }
        }

        protected virtual bool SetIsClustered(bool? value)
            => Annotations.SetAnnotation(XGFullAnnotationNames.Instance.Clustered, null, value);
    }
}
