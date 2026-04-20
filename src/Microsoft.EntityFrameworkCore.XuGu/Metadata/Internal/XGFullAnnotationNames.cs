// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class XGFullAnnotationNames : RelationalFullAnnotationNames
    {
        protected XGFullAnnotationNames(string prefix)
            : base(prefix)
        {
            Clustered = prefix + XGAnnotationNames.Clustered;
            Serial = prefix + XGAnnotationNames.Serial;
            DefaultSequenceName = prefix + XGAnnotationNames.DefaultSequenceName;
            DefaultSequenceSchema = prefix + XGAnnotationNames.DefaultSequenceSchema;
            SequenceName = prefix + XGAnnotationNames.SequenceName;
            SequenceSchema = prefix + XGAnnotationNames.SequenceSchema;
            IndexMethod = prefix + XGAnnotationNames.IndexMethod;
            XGExtensionPrefix = prefix + XGAnnotationNames.XGExtensionPrefix;
            DatabaseTemplate = prefix + XGAnnotationNames.DatabaseTemplate;
            ValueGeneratedOnAdd = prefix + XGAnnotationNames.ValueGeneratedOnAdd;
            ValueGeneratedOnAddOrUpdate = prefix + XGAnnotationNames.ValueGeneratedOnAddOrUpdate;
            ValueGenerationStrategy = prefix + XGAnnotationNames.ValueGenerationStrategy;
            HiLoSequenceSchema = prefix + XGAnnotationNames.HiLoSequenceSchema;
            HiLoSequenceName = prefix + XGAnnotationNames.HiLoSequenceName;
        }

        public new static XGFullAnnotationNames Instance { get; } = new XGFullAnnotationNames(XGAnnotationNames.Prefix);

        public readonly string Clustered;
        public readonly string Serial;
        public readonly string DefaultSequenceName;
        public readonly string DefaultSequenceSchema;
        public readonly string SequenceName;
        public readonly string SequenceSchema;
        public readonly string IndexMethod;
        public readonly string XGExtensionPrefix;
        public readonly string DatabaseTemplate;
        public readonly string ValueGeneratedOnAdd;
        public readonly string ValueGeneratedOnAddOrUpdate;
        public readonly string ValueGenerationStrategy;
        public readonly string HiLoSequenceSchema;
        public readonly string HiLoSequenceName;
    }
}