// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class XuGuDbModelAnnotations : RelationalModelAnnotations, IXuGuDbModelAnnotations
    {
        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

        public XuGuDbModelAnnotations([NotNull] IModel model)
            : base(model, XuGuDbFullAnnotationNames.Instance)
        {
        }

        protected XuGuDbModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, XuGuDbFullAnnotationNames.Instance)
        {
        }

        public virtual string HiLoSequenceName
        {
            get { return (string)Annotations.GetAnnotation(XuGuDbFullAnnotationNames.Instance.HiLoSequenceName, null); }
            [param: CanBeNull] set { SetHiLoSequenceName(value); }
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                XuGuDbFullAnnotationNames.Instance.HiLoSequenceName,
                null,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceSchema
        {
            get { return (string)Annotations.GetAnnotation(XuGuDbFullAnnotationNames.Instance.HiLoSequenceSchema, null); }
            [param: CanBeNull] set { SetHiLoSequenceSchema(value); }
        }

        protected virtual bool SetHiLoSequenceSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                XuGuDbFullAnnotationNames.Instance.HiLoSequenceSchema,
                null,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual XuGuDbValueGenerationStrategy? ValueGenerationStrategy
        {
            get
            {
                return (XuGuDbValueGenerationStrategy?)Annotations.GetAnnotation(
                    XuGuDbFullAnnotationNames.Instance.ValueGenerationStrategy,
                    null);
            }
            set { SetValueGenerationStrategy(value); }
        }

        protected virtual bool SetValueGenerationStrategy(XuGuDbValueGenerationStrategy? value)
            => Annotations.SetAnnotation(XuGuDbFullAnnotationNames.Instance.ValueGenerationStrategy,
                null,
                value);
    }
}
