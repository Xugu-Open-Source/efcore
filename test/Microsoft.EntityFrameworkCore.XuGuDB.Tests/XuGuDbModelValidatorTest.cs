// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests
{
    public class XuGuDbModelValidatorTest : RelationalModelValidatorTest
    {
        public override void Detects_duplicate_column_names()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            modelBuilder.Entity<Product>();
            modelBuilder.Entity<Product>().Property(b => b.Name).ForXuGuDbHasColumnName("Id");

            VerifyError(RelationalStrings.DuplicateColumnName(typeof(Product).Name, "Id", typeof(Product).Name, "Name", "Id", ".Product", "integer", "varchar"), modelBuilder.Model);
        }

        public override void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>();
            modelBuilder.Entity<Dog>();

            VerifyError(RelationalStrings.DuplicateColumnName(typeof(Cat).Name, "Type", typeof(Dog).Name, "Type", "Type", ".Animal", "varchar", "integer"), modelBuilder.Model);
        }
        
        public override void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
        {
            var modelBuilder = new ModelBuilder(TestConventionalSetBuilder.Build());
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().Ignore(e => e.Type).Property(c => c.Breed).HasMaxLength(30);
            modelBuilder.Entity<Dog>().Ignore(e => e.Type).Property(d => d.Breed).HasMaxLength(15);

            VerifyError(RelationalStrings.DuplicateColumnName(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), ".Animal", "varchar(30)", "varchar(15)"), modelBuilder.Model);
        }

        public virtual void Throws_for_unsupported_data_types()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            modelBuilder.Entity<Cheese>();

            Assert.Equal(
                XuGuDbStrings.UnqualifiedDataType("varchar"),
                Assert.Throws<NotSupportedException>(() => Validate(modelBuilder.Model)).Message);
        }

        private class Cheese
        {
            public int Id { get; set; }

            [Column(TypeName = "varchar")]
            public string Name { get; set; }
        }

        protected override ModelValidator CreateModelValidator()
            => new RelationalModelValidator(
                new Logger<RelationalModelValidator>(
                    new ListLoggerFactory(Log, l => l == typeof(RelationalModelValidator).FullName)),
                new TestXuGuDbAnnotationProvider(),
                new XuGuDbTypeMapper());
    }

    public class TestXuGuDbAnnotationProvider : TestAnnotationProvider
    {
        public override IRelationalPropertyAnnotations For(IProperty property) => new XuGuDbPropertyAnnotations(property);
    }
}
