// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests.Metadata
{
    public class XuGuDbBuilderExtensionsTest
    {
        [Fact]
        public void Can_set_column_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnName("Eman");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForXuGuDbHasColumnName("MyNameIs");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.Relational().ColumnName);
            Assert.Equal("MyNameIs", property.XuGuDb().ColumnName);
        }

        [Fact]
        public void Can_set_column_type()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnType("varchar(42)");

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForXuGuDbHasColumnType("varchar(24)");

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Name");

            Assert.Equal("varchar(42)", property.Relational().ColumnType);
            Assert.Equal("varchar(24)", property.XuGuDb().ColumnType);
        }

        [Fact]
        public void Can_set_column_default_value()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Offset)
                .HasDefaultValue(new DateTime(1973, 9, 3, 0, 10, 0));

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Offset)
                .ForXuGuDbHasDefaultValue(new DateTime(2006, 9, 19, 19, 0, 0));

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Offset");

            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), property.Relational().DefaultValue);
            Assert.Equal(new DateTime(2006, 9, 19, 19, 0, 0), property.XuGuDb().DefaultValue);
        }

        [Fact]
        public void Setting_column_default_value_does_not_modify_explicitly_set_value_generated()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Offset)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValue(new DateTime(1973, 9, 3, 0, 10, 0));

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Offset)
                .ForXuGuDbHasDefaultValue(new DateTime(2006, 9, 19, 19, 0, 0));

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty("Offset");

            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), property.Relational().DefaultValue);
            Assert.Equal(new DateTime(2006, 9, 19, 19, 0, 0), property.XuGuDb().DefaultValue);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, property.ValueGenerated);
        }

        [Fact]
        public void Setting_XuGuDb_default_value_is_higher_priority_than_relational_default_values()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForXuGuDbHasDefaultValue(2);

            var property = modelBuilder.Model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Null(property.Relational().DefaultValue);
            Assert.Equal(2, property.XuGuDb().DefaultValue);
        }

        [Fact]
        public void Setting_column_default_value_does_not_set_identity_column()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValue(1);

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Null(property.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }


        [Fact]
        public void Setting_XuGuDb_identity_column_is_higher_priority_than_relational_default_values()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseXuGuDbIdentityColumn();

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Equal(XuGuDbValueGenerationStrategy.IdentityColumn, property.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Can_set_key_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .HasName("KeyLimePie")
                .ForXuGuDbHasName("LemonSupreme");

            var key = modelBuilder.Model.FindEntityType(typeof(Customer)).FindPrimaryKey();

            Assert.Equal("KeyLimePie", key.Relational().Name);
            Assert.Equal("LemonSupreme", key.XuGuDb().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasConstraintName("LemonSupreme")
                .ForXuGuDbHasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.XuGuDb().Name);

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForXuGuDbHasConstraintName(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.XuGuDb().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_with_FK_specified()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("LemonSupreme")
                .ForXuGuDbHasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.XuGuDb().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasConstraintName("LemonSupreme")
                .ForXuGuDbHasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.XuGuDb().Name);

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForXuGuDbHasConstraintName(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.XuGuDb().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_with_FK_specified()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("LemonSupreme")
                .ForXuGuDbHasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(Order)).GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(Customer));

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.XuGuDb().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasPrincipalKey<Order>(e => e.OrderId)
                .HasConstraintName("LemonSupreme")
                .ForXuGuDbHasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(OrderDetails)).GetForeignKeys().Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.XuGuDb().Name);

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ForXuGuDbHasConstraintName(null);

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("LemonSupreme", foreignKey.XuGuDb().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_with_FK_specified()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasForeignKey<OrderDetails>(e => e.Id)
                .HasConstraintName("LemonSupreme")
                .ForXuGuDbHasConstraintName("ChocolateLimes");

            var foreignKey = modelBuilder.Model.FindEntityType(typeof(OrderDetails)).GetForeignKeys().Single();

            Assert.Equal("LemonSupreme", foreignKey.Relational().Name);
            Assert.Equal("ChocolateLimes", foreignKey.XuGuDb().Name);
        }

        [Fact]
        public void Can_set_index_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .HasName("Eeeendeeex")
                .ForXuGuDbHasName("Dexter");

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.Equal("Eeeendeeex", index.Relational().Name);
            Assert.Equal("Dexter", index.XuGuDb().Name);
        }

        [Fact]
        public void Can_set_table_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ToTable("Customizer")
                .ForXuGuDbToTable("Custardizer");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.XuGuDb().TableName);
        }

        [Fact]
        public void Can_set_table_name_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToTable("Customizer")
                .ForXuGuDbToTable("Custardizer");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.XuGuDb().TableName);
        }

        [Fact]
        public void Can_set_table_and_schema_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ToTable("Customizer", "Schema")
                .ForXuGuDbToTable("Custardizer", "Schema1");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.XuGuDb().TableName);
            Assert.Equal("Schema", entityType.Relational().Schema);
            Assert.Equal("Schema1", entityType.XuGuDb().Schema);
        }

        [Fact]
        public void Can_set_table_and_schema_name_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ToTable("Customizer", "Schema")
                .ForXuGuDbToTable("Custardizer", "Schema1");

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.XuGuDb().TableName);
            Assert.Equal("Schema", entityType.Relational().Schema);
            Assert.Equal("Schema1", entityType.XuGuDb().Schema);
        }

        [Fact]
        public void Can_set_sequences_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForXuGuDbUseSequenceHiLo();

            var relationalExtensions = modelBuilder.Model.Relational();
            var XuGuDbExtensions = modelBuilder.Model.XuGuDb();

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, XuGuDbExtensions.ValueGenerationStrategy);
            Assert.Equal(XuGuDbModelAnnotations.DefaultHiLoSequenceName, XuGuDbExtensions.HiLoSequenceName);
            Assert.Null(XuGuDbExtensions.HiLoSequenceSchema);

            Assert.Null(relationalExtensions.FindSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName));
            Assert.NotNull(XuGuDbExtensions.FindSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForXuGuDbUseSequenceHiLo("Snook");

            var relationalExtensions = modelBuilder.Model.Relational();
            var XuGuDbExtensions = modelBuilder.Model.XuGuDb();

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, XuGuDbExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", XuGuDbExtensions.HiLoSequenceName);
            Assert.Null(XuGuDbExtensions.HiLoSequenceSchema);

            Assert.Null(relationalExtensions.FindSequence("Snook"));

            var sequence = XuGuDbExtensions.FindSequence("Snook");

            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForXuGuDbUseSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var XuGuDbExtensions = modelBuilder.Model.XuGuDb();

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, XuGuDbExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", XuGuDbExtensions.HiLoSequenceName);
            Assert.Equal("Tasty", XuGuDbExtensions.HiLoSequenceSchema);

            Assert.Null(relationalExtensions.FindSequence("Snook", "Tasty"));

            var sequence = XuGuDbExtensions.FindSequence("Snook", "Tasty");
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder.ForXuGuDbUseSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var XuGuDbExtensions = modelBuilder.Model.XuGuDb();

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, XuGuDbExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", XuGuDbExtensions.HiLoSequenceName);
            Assert.Equal("Tasty", XuGuDbExtensions.HiLoSequenceSchema);

            ValidateSchemaNamedSpecificSequence(relationalExtensions.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(XuGuDbExtensions.FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder.ForXuGuDbUseSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model.Relational();
            var XuGuDbExtensions = modelBuilder.Model.XuGuDb();

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, XuGuDbExtensions.ValueGenerationStrategy);
            Assert.Equal("Snook", XuGuDbExtensions.HiLoSequenceName);
            Assert.Equal("Tasty", XuGuDbExtensions.HiLoSequenceSchema);

            Assert.Null(relationalExtensions.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(XuGuDbExtensions.FindSequence("Snook", "Tasty"));
        }

        private static void ValidateSchemaNamedSpecificSequence(ISequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(111, sequence.MinValue);
            Assert.Equal(2222, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);
        }

        [Fact]
        public void Can_set_identities_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForXuGuDbUseIdentityColumns();

            var relationalExtensions = modelBuilder.Model.Relational();
            var XuGuDbExtensions = modelBuilder.Model.XuGuDb();

            Assert.Equal(XuGuDbValueGenerationStrategy.IdentityColumn, XuGuDbExtensions.ValueGenerationStrategy);
            Assert.Null(XuGuDbExtensions.HiLoSequenceName);
            Assert.Null(XuGuDbExtensions.HiLoSequenceSchema);

            Assert.Null(relationalExtensions.FindSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName));
            Assert.Null(XuGuDbExtensions.FindSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Setting_XuGuDb_identities_for_model_is_lower_priority_than_relational_default_values()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>(eb =>
                    {
                        eb.Property(e => e.Id).HasDefaultValue(1);
                    });

            modelBuilder.ForXuGuDbUseIdentityColumns();

            var model = modelBuilder.Model;
            var idProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));
            Assert.Null(idProperty.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
            Assert.Equal(1, idProperty.Relational().DefaultValue);
            Assert.Equal(1, idProperty.XuGuDb().DefaultValue);

            var nameProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Name));
            Assert.Null(nameProperty.XuGuDb().ValueGenerationStrategy);

            var offsetProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Offset));
            Assert.Null(offsetProperty.XuGuDb().ValueGenerationStrategy);
        }

        [Fact]
        public void Can_set_sequence_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForXuGuDbUseSequenceHiLo();

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, property.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal(XuGuDbModelAnnotations.DefaultHiLoSequenceName, property.XuGuDb().HiLoSequenceName);

            Assert.Null(model.Relational().FindSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName));
            Assert.NotNull(model.XuGuDb().FindSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForXuGuDbUseSequenceHiLo("Snook");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, property.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.XuGuDb().HiLoSequenceName);
            Assert.Null(property.XuGuDb().HiLoSequenceSchema);

            Assert.Null(model.Relational().FindSequence("Snook"));

            var sequence = model.XuGuDb().FindSequence("Snook");

            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForXuGuDbUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, property.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.XuGuDb().HiLoSequenceName);
            Assert.Equal("Tasty", property.XuGuDb().HiLoSequenceSchema);

            Assert.Null(model.Relational().FindSequence("Snook", "Tasty"));

            var sequence = model.XuGuDb().FindSequence("Snook", "Tasty");
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForXuGuDbUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, property.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.XuGuDb().HiLoSequenceName);
            Assert.Equal("Tasty", property.XuGuDb().HiLoSequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.XuGuDb().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty", b => b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222))
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForXuGuDbUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, property.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.XuGuDb().HiLoSequenceName);
            Assert.Equal("Tasty", property.XuGuDb().HiLoSequenceSchema);

            ValidateSchemaNamedSpecificSequence(model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.XuGuDb().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForXuGuDbUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, property.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.XuGuDb().HiLoSequenceName);
            Assert.Equal("Tasty", property.XuGuDb().HiLoSequenceSchema);

            Assert.Null(model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.XuGuDb().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence<int>("Snook", "Tasty", b =>
                    {
                        b.IncrementsBy(11)
                            .StartsAt(1729)
                            .HasMin(111)
                            .HasMax(2222);
                    })
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForXuGuDbUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(XuGuDbValueGenerationStrategy.SequenceHiLo, property.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.XuGuDb().HiLoSequenceName);
            Assert.Equal("Tasty", property.XuGuDb().HiLoSequenceSchema);

            Assert.Null(model.Relational().FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.XuGuDb().FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_identities_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .UseXuGuDbIdentityColumn();

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(XuGuDbValueGenerationStrategy.IdentityColumn, property.XuGuDb().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Null(property.XuGuDb().HiLoSequenceName);

            Assert.Null(model.Relational().FindSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName));
            Assert.Null(model.XuGuDb().FindSequence(XuGuDbModelAnnotations.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Can_create_named_sequence()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForXuGuDbHasSequence("Snook");

            Assert.Null(modelBuilder.Model.Relational().FindSequence("Snook"));
            var sequence = modelBuilder.Model.XuGuDb().FindSequence("Snook");

            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_create_schema_named_sequence()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForXuGuDbHasSequence("Snook", "Tasty");

            Assert.Null(modelBuilder.Model.Relational().FindSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.XuGuDb().FindSequence("Snook", "Tasty");

            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence<int>("Snook")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            Assert.Null(modelBuilder.Model.Relational().FindSequence("Snook"));
            var sequence = modelBuilder.Model.XuGuDb().FindSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence(typeof(int), "Snook")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            Assert.Null(modelBuilder.Model.Relational().FindSequence("Snook"));
            var sequence = modelBuilder.Model.XuGuDb().FindSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence<int>("Snook", b =>
                    {
                        b.IncrementsBy(11)
                            .StartsAt(1729)
                            .HasMin(111)
                            .HasMax(2222);
                    });

            Assert.Null(modelBuilder.Model.Relational().FindSequence("Snook"));
            var sequence = modelBuilder.Model.XuGuDb().FindSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_named_sequence_with_specific_facets_using_nested_closure_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence(typeof(int), "Snook", b =>
                    {
                        b.IncrementsBy(11)
                            .StartsAt(1729)
                            .HasMin(111)
                            .HasMax(2222);
                    });

            Assert.Null(modelBuilder.Model.Relational().FindSequence("Snook"));
            var sequence = modelBuilder.Model.XuGuDb().FindSequence("Snook");

            ValidateNamedSpecificSequence(sequence);
        }

        private static void ValidateNamedSpecificSequence(ISequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(111, sequence.MinValue);
            Assert.Equal(2222, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            Assert.Null(modelBuilder.Model.Relational().FindSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.XuGuDb().FindSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence(typeof(int), "Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            Assert.Null(modelBuilder.Model.Relational().FindSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.XuGuDb().FindSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence<int>("Snook", "Tasty", b => { b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222); });

            Assert.Null(modelBuilder.Model.Relational().FindSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.XuGuDb().FindSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [Fact]
        public void Can_create_schema_named_sequence_with_specific_facets_using_nested_closure_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .ForXuGuDbHasSequence(typeof(int), "Snook", "Tasty", b => { b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222); });

            Assert.Null(modelBuilder.Model.Relational().FindSequence("Snook", "Tasty"));
            var sequence = modelBuilder.Model.XuGuDb().FindSequence("Snook", "Tasty");

            ValidateSchemaNamedSpecificSequence(sequence);
        }

        [Fact]
        public void XuGuDb_entity_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .ForXuGuDbToTable("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .ForXuGuDbToTable("Jay", "Simon"));
        }

        [Fact]
        public void XuGuDb_entity_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForXuGuDbToTable("Will");

            modelBuilder
                .Entity<Customer>()
                .ForXuGuDbToTable("Jay", "Simon");
        }

        [Fact]
        public void XuGuDb_property_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .ForXuGuDbHasColumnName("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .ForXuGuDbHasColumnType("Jay"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Name)
                    .ForXuGuDbHasDefaultValue("Neil"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForXuGuDbUseSequenceHiLo());

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseXuGuDbIdentityColumn());
        }

        [Fact]
        public void XuGuDb_property_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .ForXuGuDbHasColumnName("Will");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .ForXuGuDbHasColumnName("Jay");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .ForXuGuDbHasColumnType("Simon");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .ForXuGuDbHasColumnType("Neil");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(string), "Name")
                .ForXuGuDbHasDefaultValue("Simon");

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(string), "Name")
                .ForXuGuDbHasDefaultValue("Neil");

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(int), "Id")
                .ForXuGuDbUseSequenceHiLo();

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(int), "Id")
                .ForXuGuDbUseSequenceHiLo();

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(int), "Id")
                .UseXuGuDbIdentityColumn();

            modelBuilder
                .Entity<Customer>()
                .Property(typeof(int), "Id")
                .UseXuGuDbIdentityColumn();
        }

        [Fact]
        public void XuGuDb_relationship_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>().HasMany(e => e.Orders)
                    .WithOne(e => e.Customer)
                    .ForXuGuDbHasConstraintName("Will"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Order>()
                    .HasOne(e => e.Customer)
                    .WithMany(e => e.Orders)
                    .ForXuGuDbHasConstraintName("Jay"));

            AssertIsGeneric(
                modelBuilder
                    .Entity<Order>()
                    .HasOne(e => e.Details)
                    .WithOne(e => e.Order)
                    .ForXuGuDbHasConstraintName("Simon"));
        }

        [Fact]
        public void XuGuDb_relationship_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>().HasMany(typeof(Order), "Orders")
                .WithOne("Customer")
                .ForXuGuDbHasConstraintName("Will");

            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Customer)
                .WithMany(e => e.Orders)
                .ForXuGuDbHasConstraintName("Jay");

            modelBuilder
                .Entity<Order>()
                .HasOne(e => e.Details)
                .WithOne(e => e.Order)
                .ForXuGuDbHasConstraintName("Simon");
        }

        private void AssertIsGeneric(EntityTypeBuilder<Customer> _)
        {
        }

        private void AssertIsGeneric(PropertyBuilder<string> _)
        {
        }

        private void AssertIsGeneric(PropertyBuilder<int> _)
        {
        }

        private void AssertIsGeneric(ReferenceCollectionBuilder<Customer, Order> _)
        {
        }

        private void AssertIsGeneric(ReferenceReferenceBuilder<Order, OrderDetails> _)
        {
        }

        protected virtual ModelBuilder CreateConventionModelBuilder()
        {
            return XuGuDbTestHelpers.Instance.CreateConventionBuilder();
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Offset { get; set; }

            public IEnumerable<Order> Orders { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }
    }
}
