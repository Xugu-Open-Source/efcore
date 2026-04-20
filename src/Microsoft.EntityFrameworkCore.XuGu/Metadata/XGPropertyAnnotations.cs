// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Properties;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class XGPropertyAnnotations : RelationalPropertyAnnotations, IXGPropertyAnnotations
    {
        public XGPropertyAnnotations([NotNull] IProperty property) : base(property, XGFullAnnotationNames.Instance)
        {
        }
        public XGPropertyAnnotations([NotNull] IProperty property, [CanBeNull] string providerPrefix) : base(property, XGFullAnnotationNames.Instance)
        {
        }

        public XGPropertyAnnotations([NotNull] RelationalAnnotations annotations) : base(annotations, XGFullAnnotationNames.Instance)
        {
        }

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

        public virtual ISequence FindHiLoSequence()
        {
            var modelExtensions = Property.DeclaringEntityType.Model.XG();

            if (ValueGenerationStrategy != XGValueGenerationStrategy.SequenceHiLo)
            {
                return null;
            }

            var sequenceName = HiLoSequenceName
                               ?? modelExtensions.HiLoSequenceName
                               ?? XGModelAnnotations.DefaultHiLoSequenceName;

            var sequenceSchema = HiLoSequenceSchema
                                 ?? modelExtensions.HiLoSequenceSchema;

            return modelExtensions.FindSequence(sequenceName, sequenceSchema);
        }

        public virtual XGValueGenerationStrategy? ValueGenerationStrategy
        {
            get { return GetXGValueGenerationStrategy(fallbackToModel: true); }
            [param: CanBeNull]
            set { SetValueGenerationStrategy(value); }
        }

        private XGValueGenerationStrategy? GetXGValueGenerationStrategy(bool fallbackToModel)
        {
            if (GetDefaultValue(false) != null
                || GetDefaultValueSql(false) != null
                || GetComputedColumnSql(false) != null)
            {
                return null;
            }

            var value = (XGValueGenerationStrategy?)Annotations.GetAnnotation(
                XGFullAnnotationNames.Instance.ValueGenerationStrategy,
                null);

            var relationalProperty = Property.Relational();
            return value ??
                   (fallbackToModel
                    && Property.ValueGenerated == ValueGenerated.OnAdd
                    && (Property.ClrType.UnwrapNullableType().IsInteger()
                        || Property.ClrType.UnwrapNullableType() == typeof(decimal))
                    && relationalProperty.DefaultValue == null
                    && relationalProperty.DefaultValueSql == null
                    && relationalProperty.ComputedColumnSql == null
                       ? Property.DeclaringEntityType.Model.XG().ValueGenerationStrategy
                       : null);
        }

        protected virtual bool SetValueGenerationStrategy(XGValueGenerationStrategy? value)
        {
            if (value != null)
            {
                var propertyType = Property.ClrType;

                if (value == XGValueGenerationStrategy.IdentityColumn
                    && propertyType != typeof(decimal)
                    && (!propertyType.IsInteger()
                        || propertyType == typeof(byte)
                        || propertyType == typeof(byte?)))
                {
                    throw new ArgumentException(XGStrings.IdentityBadType(
                        Property.Name, Property.DeclaringEntityType.Name, propertyType.Name));
                }

                if ((value == XGValueGenerationStrategy.SequenceHiLo)
                    && !propertyType.IsInteger())
                {
                    throw new ArgumentException(XGStrings.SequenceBadType(
                        Property.Name, Property.DeclaringEntityType.Name, propertyType.Name));
                }
            }

            if (!CanSetValueGenerationStrategy(value))
            {
                return false;
            }

            if (!ShouldThrowOnConflict
                && ValueGenerationStrategy != value
                && value != null)
            {
                ClearAllServerGeneratedValues();
            }

            return Annotations.SetAnnotation(XGFullAnnotationNames.Instance.ValueGenerationStrategy, null, value);
        }

        protected virtual bool CanSetValueGenerationStrategy(XGValueGenerationStrategy? value)
        {
            if (GetXGValueGenerationStrategy(fallbackToModel: false) == value)
            {
                return true;
            }

            if (!Annotations.CanSetAnnotation(XGFullAnnotationNames.Instance.ValueGenerationStrategy, null, value))
            {
                return false;
            }

            if (ShouldThrowOnConflict)
            {
                if (GetDefaultValue(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(DefaultValue)));
                }
                if (GetDefaultValueSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(DefaultValueSql)));
                }
                if (GetComputedColumnSql(false) != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ValueGenerationStrategy), Property.Name, nameof(ComputedColumnSql)));
                }
            }
            else if (value != null
                     && (!CanSetDefaultValue(null)
                         || !CanSetDefaultValueSql(null)
                         || !CanSetComputedColumnSql(null)))
            {
                return false;
            }

            return true;
        }

        protected override object GetDefaultValue(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetDefaultValue(fallback);
        }

        protected override bool CanSetDefaultValue(object value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValue), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetDefaultValue(value);
        }

        protected override string GetDefaultValueSql(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetDefaultValueSql(fallback);
        }

        protected override bool CanSetDefaultValueSql(string value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(DefaultValueSql), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetDefaultValueSql(value);
        }

        protected override string GetComputedColumnSql(bool fallback)
        {
            if (fallback
                && ValueGenerationStrategy != null)
            {
                return null;
            }

            return base.GetComputedColumnSql(fallback);
        }

        protected override bool CanSetComputedColumnSql(string value)
        {
            if (ShouldThrowOnConflict)
            {
                if (ValueGenerationStrategy != null)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingColumnServerGeneration(nameof(ComputedColumnSql), Property.Name, nameof(ValueGenerationStrategy)));
                }
            }
            else if (value != null
                     && !CanSetValueGenerationStrategy(null))
            {
                return false;
            }

            return base.CanSetComputedColumnSql(value);
        }

        protected override void ClearAllServerGeneratedValues()
        {
            SetValueGenerationStrategy(null);

            base.ClearAllServerGeneratedValues();
        }
    }
}
