// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public static class XGAnnotationNames
    {
        public const string Prefix = "XG:";
        public const string Clustered = "Clustered";
        public const string Serial = "Serial";
        public const string ValueGeneratedOnAdd = "ValueGeneratedOnAdd";
        public const string ValueGeneratedOnAddOrUpdate = "ValueGeneratedOnAddOrUpdate";
        public const string DefaultSequenceName = "DefaultSequenceName";
        public const string DefaultSequenceSchema = "DefaultSequenceSchema";
        public const string SequenceName = "SequenceName";
        public const string SequenceSchema = "SequenceSchema";
        public const string IndexMethod = "IndexMethod";
        public const string ValueGenerationStrategy = "ValueGenerationStrategy";
        public const string XGExtensionPrefix = "XGExtension:";
        public const string DatabaseTemplate = "DatabaseTemplate";
        public const string HiLoSequenceSchema = "HiLoSequenceSchema";
        public const string HiLoSequenceName = "HiLoSequenceName";
    }
}
