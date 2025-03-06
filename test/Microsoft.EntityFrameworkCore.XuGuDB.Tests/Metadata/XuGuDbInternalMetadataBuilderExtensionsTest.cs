// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests.Metadata
{
    public class XuGuDbInternalMetadataBuilderExtensionsTest
    {
        private InternalModelBuilder CreateBuilder()
            => new InternalModelBuilder(new Model());

        [Fact]
        public void Can_access_model()
        {
            var builder = CreateBuilder();

            Assert.True(builder.XuGuDb(ConfigurationSource.Convention).ValueGenerationStrategy(XuGuDbValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, builder.Metadata.XuGuDb().ValueGenerationStrategy);

            Assert.True(builder.XuGuDb(ConfigurationSource.DataAnnotation).ValueGenerationStrategy(XuGuDbValueGenerationStrategy.IdentityColumn));
            Assert.Equal(XuGuDbValueGenerationStrategy.IdentityColumn, builder.Metadata.XuGuDb().ValueGenerationStrategy);

            Assert.False(builder.XuGuDb(ConfigurationSource.Convention).ValueGenerationStrategy(XuGuDbValueGenerationStrategy.SequenceHiLo));
            Assert.Equal(XuGuDbValueGenerationStrategy.IdentityColumn, builder.Metadata.XuGuDb().ValueGenerationStrategy);

            Assert.Equal(1, builder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(XuGuDbAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_entity_type()
        {
            var typeBuilder = CreateBuilder().Entity(typeof(Splot), ConfigurationSource.Convention);

            Assert.True(typeBuilder.XuGuDb(ConfigurationSource.Convention).ToTable("Splew"));
            Assert.Equal("Splew", typeBuilder.Metadata.XuGuDb().TableName);

            Assert.True(typeBuilder.XuGuDb(ConfigurationSource.DataAnnotation).ToTable("Splow"));
            Assert.Equal("Splow", typeBuilder.Metadata.XuGuDb().TableName);

            Assert.False(typeBuilder.XuGuDb(ConfigurationSource.Convention).ToTable("Splod"));
            Assert.Equal("Splow", typeBuilder.Metadata.XuGuDb().TableName);

            Assert.Equal(1, typeBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(XuGuDbAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_property()
        {
            var propertyBuilder = CreateBuilder()
                .Entity(typeof(Splot), ConfigurationSource.Convention)
                .Property("Id", typeof(int), ConfigurationSource.Convention);

            Assert.True(propertyBuilder.XuGuDb(ConfigurationSource.Convention).HiLoSequenceName("Splew"));
            Assert.Equal("Splew", propertyBuilder.Metadata.XuGuDb().HiLoSequenceName);

            Assert.True(propertyBuilder.XuGuDb(ConfigurationSource.DataAnnotation).HiLoSequenceName("Splow"));
            Assert.Equal("Splow", propertyBuilder.Metadata.XuGuDb().HiLoSequenceName);

            Assert.False(propertyBuilder.XuGuDb(ConfigurationSource.Convention).HiLoSequenceName("Splod"));
            Assert.Equal("Splow", propertyBuilder.Metadata.XuGuDb().HiLoSequenceName);

            Assert.Equal(1, propertyBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(XuGuDbAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_key()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            var idProperty = entityTypeBuilder.Property("Id", typeof(string), ConfigurationSource.Convention).Metadata;
            var keyBuilder = entityTypeBuilder.HasKey(new[] { idProperty.Name }, ConfigurationSource.Convention);

            Assert.True(keyBuilder.XuGuDb(ConfigurationSource.Convention).IsClustered(true));
            Assert.True(keyBuilder.Metadata.XuGuDb().IsClustered);

            Assert.True(keyBuilder.XuGuDb(ConfigurationSource.DataAnnotation).IsClustered(false));
            Assert.False(keyBuilder.Metadata.XuGuDb().IsClustered);

            Assert.False(keyBuilder.XuGuDb(ConfigurationSource.Convention).IsClustered(true));
            Assert.False(keyBuilder.Metadata.XuGuDb().IsClustered);

            Assert.Equal(1, keyBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(XuGuDbAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_index()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var indexBuilder = entityTypeBuilder.HasIndex(new[] { "Id" }, ConfigurationSource.Convention);

            Assert.True(indexBuilder.XuGuDb(ConfigurationSource.Convention).IsClustered(true));
            Assert.True(indexBuilder.Metadata.XuGuDb().IsClustered);

            Assert.True(indexBuilder.XuGuDb(ConfigurationSource.DataAnnotation).IsClustered(false));
            Assert.False(indexBuilder.Metadata.XuGuDb().IsClustered);

            Assert.False(indexBuilder.XuGuDb(ConfigurationSource.Convention).IsClustered(true));
            Assert.False(indexBuilder.Metadata.XuGuDb().IsClustered);

            Assert.Equal(1, indexBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(XuGuDbAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        [Fact]
        public void Can_access_relationship()
        {
            var modelBuilder = CreateBuilder();
            var entityTypeBuilder = modelBuilder.Entity(typeof(Splot), ConfigurationSource.Convention);
            entityTypeBuilder.Property("Id", typeof(int), ConfigurationSource.Convention);
            var relationshipBuilder = entityTypeBuilder.HasForeignKey("Splot", new[] { "Id" }, ConfigurationSource.Convention);

            Assert.True(relationshipBuilder.XuGuDb(ConfigurationSource.Convention).HasConstraintName("Splew"));
            Assert.Equal("Splew", relationshipBuilder.Metadata.XuGuDb().Name);

            Assert.True(relationshipBuilder.XuGuDb(ConfigurationSource.DataAnnotation).HasConstraintName("Splow"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.XuGuDb().Name);

            Assert.False(relationshipBuilder.XuGuDb(ConfigurationSource.Convention).HasConstraintName("Splod"));
            Assert.Equal("Splow", relationshipBuilder.Metadata.XuGuDb().Name);

            Assert.Equal(1, relationshipBuilder.Metadata.GetAnnotations().Count(
                a => a.Name.StartsWith(XuGuDbAnnotationNames.Prefix, StringComparison.Ordinal)));
        }

        private class Splot
        {
        }
    }
}
