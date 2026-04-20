// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class XGModelAnnotations : RelationalModelAnnotations, IXGModelAnnotations
    {
        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";
        public XGModelAnnotations([NotNull] IModel model)
            : base(model, XGFullAnnotationNames.Instance)
        {
        }

        public XGModelAnnotations([NotNull] RelationalAnnotations annotations)
            : base(annotations, XGFullAnnotationNames.Instance)
        {
        }

        public virtual IXGExtension GetOrAddXGExtension([CanBeNull] string name, [CanBeNull] string schema = null)
            => XGExtension.GetOrAddXGExtension((IMutableModel)Model,
                XGFullAnnotationNames.Instance.XGExtensionPrefix,
                name,
                schema);

        public virtual IReadOnlyList<IXGExtension> XGExtensions
            => XGExtension.GetXGExtensions(Model, XGFullAnnotationNames.Instance.XGExtensionPrefix).ToList();

        public virtual string DatabaseTemplate
        {
            get { return (string)Annotations.GetAnnotation(XGFullAnnotationNames.Instance.DatabaseTemplate, null); }
            [param: CanBeNull]
            set { SetDatabaseTemplate(value); }
        }

        protected virtual bool SetDatabaseTemplate([CanBeNull] string value)
            => Annotations.SetAnnotation(
                XGFullAnnotationNames.Instance.DatabaseTemplate,
                null,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceSchema
        {
            get { return (string)Annotations.GetAnnotation(XGFullAnnotationNames.Instance.HiLoSequenceSchema, null); }
            [param: CanBeNull]
            set { SetHiLoSequenceSchema(value); }
        }

        protected virtual bool SetHiLoSequenceSchema([CanBeNull] string value)
            => Annotations.SetAnnotation(
                XGFullAnnotationNames.Instance.HiLoSequenceSchema,
                null,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual string HiLoSequenceName
        {
            get { return (string)Annotations.GetAnnotation(XGFullAnnotationNames.Instance.HiLoSequenceName, null); }
            [param: CanBeNull]
            set { SetHiLoSequenceName(value); }
        }

        protected virtual bool SetHiLoSequenceName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                XGFullAnnotationNames.Instance.HiLoSequenceName,
                null,
                Check.NullButNotEmpty(value, nameof(value)));

        public virtual XGValueGenerationStrategy? ValueGenerationStrategy
        {
            get
            {
                return (XGValueGenerationStrategy?)Annotations.GetAnnotation(
                    XGFullAnnotationNames.Instance.ValueGenerationStrategy,
                    null);
            }
            set { SetValueGenerationStrategy(value); }
        }

        protected virtual bool SetValueGenerationStrategy(XGValueGenerationStrategy? value)
            => Annotations.SetAnnotation(XGFullAnnotationNames.Instance.ValueGenerationStrategy,
                null,
                value);
    }
}
