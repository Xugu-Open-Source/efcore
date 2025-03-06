// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests
{
    public class XuGuDbValueGeneratorSelectorTest
    {
        [Fact]
        public void Returns_built_in_generators_for_types_setup_for_value_generation()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = XuGuDbTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            var test = entityType.FindProperty("Id");
            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("Id"), entityType));
            Assert.IsType<TemporaryLongValueGenerator>(selector.Select(entityType.FindProperty("Long"), entityType));
            Assert.IsType<TemporaryShortValueGenerator>(selector.Select(entityType.FindProperty("Short"), entityType));
            Assert.IsType<TemporarySByteValueGenerator>(selector.Select(entityType.FindProperty("SByte"), entityType));
            Assert.IsType<TemporaryCharValueGenerator>(selector.Select(entityType.FindProperty("Char"), entityType));
            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("NullableInt"), entityType));
            Assert.IsType<TemporaryLongValueGenerator>(selector.Select(entityType.FindProperty("NullableLong"), entityType));
            Assert.IsType<TemporaryShortValueGenerator>(selector.Select(entityType.FindProperty("NullableShort"), entityType));
            Assert.IsType<TemporarySByteValueGenerator>(selector.Select(entityType.FindProperty("NullableByte"), entityType));
            Assert.IsType<TemporaryCharValueGenerator>(selector.Select(entityType.FindProperty("NullableChar"), entityType));
            Assert.IsType<StringValueGenerator>(selector.Select(entityType.FindProperty("String"), entityType));
            Assert.IsType<SequentialGuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
            Assert.IsType<BinaryValueGenerator>(selector.Select(entityType.FindProperty("Binary"), entityType));
            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("AlwaysIdentity"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("AlwaysSequence"), entityType));
        }

        [Fact]
        public void Returns_temp_guid_generator_when_default_sql_set()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            entityType.FindProperty("Guid").XuGuDb().DefaultValueSql = "SYS_GUID";

            var selector = XuGuDbTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<TemporaryGuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
        }

        [Fact]
        public void Returns_sequence_value_generators_when_configured_for_model()
        {
            var model = BuildModel();
            model.XuGuDb().ValueGenerationStrategy = XuGuDbValueGenerationStrategy.SequenceHiLo;
            model.XuGuDb().GetOrAddSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName);
            var entityType = model.FindEntityType(typeof(AnEntity));

            foreach (var property in entityType.GetProperties())
            {
                property.ValueGenerated = ValueGenerated.OnAdd;
            }

            var selector = XuGuDbTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("Id"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<long>>(selector.Select(entityType.FindProperty("Long"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<short>>(selector.Select(entityType.FindProperty("Short"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<sbyte>>(selector.Select(entityType.FindProperty("SByte"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<char>>(selector.Select(entityType.FindProperty("Char"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("NullableInt"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<long>>(selector.Select(entityType.FindProperty("NullableLong"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<short>>(selector.Select(entityType.FindProperty("NullableShort"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<sbyte>>(selector.Select(entityType.FindProperty("NullableByte"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<char>>(selector.Select(entityType.FindProperty("NullableChar"), entityType));
            Assert.IsType<StringValueGenerator>(selector.Select(entityType.FindProperty("String"), entityType));
            Assert.IsType<SequentialGuidValueGenerator>(selector.Select(entityType.FindProperty("Guid"), entityType));
            Assert.IsType<BinaryValueGenerator>(selector.Select(entityType.FindProperty("Binary"), entityType));
            Assert.IsType<TemporaryIntValueGenerator>(selector.Select(entityType.FindProperty("AlwaysIdentity"), entityType));
            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("AlwaysSequence"), entityType));
        }

        [Fact]
        public void Throws_for_unsupported_combinations()
        {
            var model = BuildModel();
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = XuGuDbTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.Equal(
                CoreStrings.NoValueGenerator("Random", "AnEntity", typeof(Random).Name),
                Assert.Throws<NotSupportedException>(() => selector.Select(entityType.FindProperty("Random"), entityType)).Message);
        }

        [Fact]
        public void Returns_generator_configured_on_model_when_property_is_Identity()
        {
            var model = XuGuDbTestHelpers.Instance.BuildModelFor<AnEntity>();
            model.XuGuDb().ValueGenerationStrategy = XuGuDbValueGenerationStrategy.SequenceHiLo;
            model.XuGuDb().GetOrAddSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName);
            var entityType = model.FindEntityType(typeof(AnEntity));

            var selector = XuGuDbTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IValueGeneratorSelector>();

            Assert.IsType<XuGuDbSequenceHiLoValueGenerator<int>>(selector.Select(entityType.FindProperty("Id"), entityType));
        }

        private static IMutableModel BuildModel(bool generateValues = true)
        {
            var builder = XuGuDbTestHelpers.Instance.CreateConventionBuilder();
            builder.Ignore<Random>();
            builder.Entity<AnEntity>();
            var model = builder.Model;
            model.XuGuDb().GetOrAddSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName);
            var entityType = model.FindEntityType(typeof(AnEntity));
            entityType.AddProperty("Random", typeof(Random), shadow: false);

            foreach (var property in entityType.GetProperties())
            {
                property.RequiresValueGenerator = generateValues;
                property.ValueGenerated = ValueGenerated.OnAdd;
            }

            entityType.FindProperty("AlwaysIdentity").ValueGenerated = ValueGenerated.OnAdd;
            entityType.FindProperty("AlwaysIdentity").XuGuDb().ValueGenerationStrategy = XuGuDbValueGenerationStrategy.IdentityColumn;

            entityType.FindProperty("AlwaysSequence").ValueGenerated = ValueGenerated.OnAdd;
            entityType.FindProperty("AlwaysSequence").XuGuDb().ValueGenerationStrategy = XuGuDbValueGenerationStrategy.SequenceHiLo;

            return model;
        }

        private class AnEntity
        {
            public int Id { get; set; }
            public long Long { get; set; }
            public short Short { get; set; }
            public sbyte SByte { get; set; }
            public char Char { get; set; }
            public int? NullableInt { get; set; }
            public long? NullableLong { get; set; }
            public short? NullableShort { get; set; }
            public sbyte? NullableByte { get; set; }
            public char? NullableChar { get; set; }
            public string String { get; set; }
            public Guid Guid { get; set; }
            public byte[] Binary { get; set; }
            public float Float { get; set; }
            public int AlwaysIdentity { get; set; }
            public int AlwaysSequence { get; set; }
            public Random Random { get; set; }
        }
    }
}
