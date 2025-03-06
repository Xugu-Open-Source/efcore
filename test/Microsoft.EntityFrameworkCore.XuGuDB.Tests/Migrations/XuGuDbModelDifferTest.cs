// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Relational.Tests.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests.Migrations
{
    public class XuGuDbModelDifferTest : MigrationsModelDifferTestBase
    {
        [Fact]
        public void Create_table_overridden()
        {
            Execute(
                _ => { },
                modelBuilder => modelBuilder.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").ForXuGuDbHasName("PK_People");
                            x.ForXuGuDbToTable("People", "SYSDBA");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createSchemaOperation = Assert.IsType<EnsureSchemaOperation>(operations[0]);
                        Assert.Equal("SYSDBA", createSchemaOperation.Name);

                        var addTableOperation = Assert.IsType<CreateTableOperation>(operations[1]);
                        Assert.Equal("SYSDBA", addTableOperation.Schema);
                        Assert.Equal("People", addTableOperation.Name);

                        Assert.Equal("PK_People", addTableOperation.PrimaryKey.Name);
                    
                    });
        }

        [Fact]
        public void Rename_table_overridden()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id").HasName("PK_Person");
                            x.ForXuGuDbToTable("People", "SYSDBA");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createSchemaOperation = Assert.IsType<EnsureSchemaOperation>(operations[0]);
                        Assert.Equal("SYSDBA", createSchemaOperation.Name);

                        var addTableOperation = Assert.IsType<RenameTableOperation>(operations[1]);
                        Assert.Equal("Person", addTableOperation.Name);
                        Assert.Equal("SYSDBA", addTableOperation.NewSchema);
                        Assert.Equal("People", addTableOperation.NewName);
                    });
        }

        [Fact]
        public void Drop_table_overridden()
        {
            Execute(
                modelBuilder => modelBuilder.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.ForXuGuDbToTable("People", "SYSDBA");
                        }),
                _ => { },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var addTableOperation = Assert.IsType<DropTableOperation>(operations[0]);
                        Assert.Equal("SYSDBA", addTableOperation.Schema);
                        Assert.Equal("People", addTableOperation.Name);
                    });
        }

        [Fact]
        public void Rename_column_overridden()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value").ForXuGuDbHasColumnName("PersonValue");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var addTableOperation = Assert.IsType<RenameColumnOperation>(operations[0]);
                        Assert.Equal("Person", addTableOperation.Table);
                        Assert.Equal("Value", addTableOperation.Name);
                        Assert.Equal("PersonValue", addTableOperation.NewName);
                    });
        }

        [Fact]
        public void Alter_column_overridden()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value")
                                .ForXuGuDbHasColumnType("varchar(8000)")
                                .HasDefaultValueSql("1 + 1");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var addTableOperation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("Person", addTableOperation.Table);
                        Assert.Equal("Value", addTableOperation.Name);
                        Assert.Equal("varchar(8000)", addTableOperation.ColumnType);
                        Assert.Equal("1 + 1", addTableOperation.DefaultValueSql);
                    });
        }

        [Fact]
        public void Alter_column_identity()
        {
            Execute(
                source => source.Entity(
                    "Lamb",
                    x =>
                        {
                            x.ToTable("Lamb", "bah");
                            x.Property<int>("Id").ValueGeneratedNever();
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Lamb",
                    x =>
                        {
                            x.ToTable("Lamb", "bah");
                            x.Property<int>("Id").ValueGeneratedOnAdd();
                            x.HasKey("Id");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                        Assert.Equal("bah", operation.Schema);
                        Assert.Equal("Lamb", operation.Table);
                        Assert.Equal("Id", operation.Name);
                        Assert.Equal(XuGuDbValueGenerationStrategy.IdentityColumn, operation["XuGuDb:ValueGenerationStrategy"]);
                    });
        }

        [Fact]
        public void Add_column_overridden()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value").ForXuGuDbHasColumnName("PersonValue");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var addTableOperation = Assert.IsType<AddColumnOperation>(operations[0]);
                        Assert.Equal("Person", addTableOperation.Table);
                        Assert.Equal("PersonValue", addTableOperation.Name);
                    });
        }

        [Fact]
        public void Drop_column_overridden()
        {
            Execute(
                source => source.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value").ForXuGuDbHasColumnName("PersonValue");
                        }),
                target => target.Entity(
                    "Person",
                    x =>
                        {
                            x.Property<int>("Id");
                            x.HasKey("Id");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var addTableOperation = Assert.IsType<DropColumnOperation>(operations[0]);
                        Assert.Equal("Person", addTableOperation.Table);
                        Assert.Equal("PersonValue", addTableOperation.Name);
                    });
        }

        [Fact]
        public void Alter_primary_key_clustering()
        {
            Execute(
                source => source.Entity(
                    "Ram",
                    x =>
                        {
                            x.ToTable("Ram", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id").ForXuGuDbIsClustered(false);
                        }),
                target => target.Entity(
                    "Ram",
                    x =>
                        {
                            x.ToTable("Ram", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id").ForXuGuDbIsClustered(true);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
                        Assert.Equal("bah", dropOperation.Schema);
                        Assert.Equal("Ram", dropOperation.Table);
                        Assert.Equal("PK_Ram", dropOperation.Name);

                        var addOperation = Assert.IsType<AddPrimaryKeyOperation>(operations[1]);
                        Assert.Equal("bah", addOperation.Schema);
                        Assert.Equal("Ram", addOperation.Table);
                        Assert.Equal("PK_Ram", addOperation.Name);
                        Assert.True((bool)addOperation["XuGuDb:Clustered"]);
                    });
        }

        [Fact]
        public void Alter_unique_constraint_clustering()
        {
            Execute(
                source => source.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").ForXuGuDbIsClustered(false);
                        }),
                target => target.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").ForXuGuDbIsClustered(true);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("bah", dropOperation.Schema);
                        Assert.Equal("Ewe", dropOperation.Table);
                        Assert.Equal("AK_Ewe_AlternateId", dropOperation.Name);

                        var addOperation = Assert.IsType<AddUniqueConstraintOperation>(operations[1]);
                        Assert.Equal("bah", addOperation.Schema);
                        Assert.Equal("Ewe", addOperation.Table);
                        Assert.Equal("AK_Ewe_AlternateId", addOperation.Name);
                        Assert.True((bool)addOperation["XuGuDb:Clustered"]);
                    });
        }

        [Fact]
        public void Add_unique_constraint_overridden()
        {
            Execute(
                source => source.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                        }),
                target => target.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").ForXuGuDbHasName("AK_Ewe");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AddUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("bah", operation.Schema);
                        Assert.Equal("Ewe", operation.Table);
                        Assert.Equal("AK_Ewe", operation.Name);
                    });
        }

        [Fact]
        public void Drop_unique_constraint_overridden()
        {
            Execute(
                source => source.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                            x.HasAlternateKey("AlternateId").ForXuGuDbHasName("AK_Ewe");
                        }),
                target => target.Entity(
                    "Ewe",
                    x =>
                        {
                            x.ToTable("Ewe", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("AlternateId");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
                        Assert.Equal("bah", operation.Schema);
                        Assert.Equal("Ewe", operation.Table);
                        Assert.Equal("AK_Ewe", operation.Name);
                    });
        }

        [Fact]
        public void Add_foreign_key_overridden()
        {
            Execute(
                source => source.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "SYSDBA");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                        }),
                target => target.Entity(
                    "Amoeba",
                    x =>
                        {
                            x.ToTable("Amoeba", "SYSDBA");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Amoeba").WithMany().HasForeignKey("ParentId").ForXuGuDbHasConstraintName("FK_Amoeba_Parent");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var createIndexOperation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("SYSDBA", createIndexOperation.Schema);
                        Assert.Equal("Amoeba", createIndexOperation.Table);
                        Assert.Equal("IX_Amoeba_ParentId", createIndexOperation.Name);

                        var addFkOperation = Assert.IsType<AddForeignKeyOperation>(operations[1]);
                        Assert.Equal("SYSDBA", addFkOperation.Schema);
                        Assert.Equal("Amoeba", addFkOperation.Table);
                        Assert.Equal("FK_Amoeba_Parent", addFkOperation.Name);
                    });
        }

        [Fact]
        public void Drop_foreign_key_overridden()
        {
            Execute(
                source => source.Entity(
                    "Anemone",
                    x =>
                        {
                            x.ToTable("Anemone", "SYSDBA");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                            x.HasOne("Anemone").WithMany().HasForeignKey("ParentId").ForXuGuDbHasConstraintName("FK_Anemone_Parent");
                        }),
                target => target.Entity(
                    "Anemone",
                    x =>
                        {
                            x.ToTable("Anemone", "SYSDBA");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("ParentId");
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropFkOperation = Assert.IsType<DropForeignKeyOperation>(operations[0]);
                        Assert.Equal("SYSDBA", dropFkOperation.Schema);
                        Assert.Equal("Anemone", dropFkOperation.Table);
                        Assert.Equal("FK_Anemone_Parent", dropFkOperation.Name);

                        var dropIndexOperation = Assert.IsType<DropIndexOperation>(operations[1]);
                        Assert.Equal("SYSDBA", dropIndexOperation.Schema);
                        Assert.Equal("Anemone", dropIndexOperation.Table);
                        Assert.Equal("IX_Anemone_ParentId", dropIndexOperation.Name);
                    });
        }

        [Fact]
        public void Alter_index_clustering()
        {
            Execute(
                source => source.Entity(
                    "Mutton",
                    x =>
                        {
                            x.ToTable("Mutton", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").ForXuGuDbIsClustered(false);
                        }),
                target => target.Entity(
                    "Mutton",
                    x =>
                        {
                            x.ToTable("Mutton", "bah");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").ForXuGuDbIsClustered(true);
                        }),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        var dropOperation = Assert.IsType<DropIndexOperation>(operations[0]);
                        Assert.Equal("bah", dropOperation.Schema);
                        Assert.Equal("Mutton", dropOperation.Table);
                        Assert.Equal("IX_Mutton_Value", dropOperation.Name);

                        var createOperation = Assert.IsType<CreateIndexOperation>(operations[1]);
                        Assert.Equal("bah", createOperation.Schema);
                        Assert.Equal("Mutton", createOperation.Table);
                        Assert.Equal("IX_Mutton_Value", createOperation.Name);
                        Assert.True((bool)createOperation["XuGuDb:Clustered"]);
                    });
        }

        [Fact]
        public void Rename_index_overridden()
        {
            Execute(
                source => source.Entity(
                    "Donkey",
                    x =>
                        {
                            x.ToTable("Donkey", "SYSDBA");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value");
                        }),
                target => target.Entity(
                    "Donkey",
                    x =>
                        {
                            x.ToTable("Donkey", "SYSDBA");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").ForXuGuDbHasName("IX_SYSDBA.Donkey_Value");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<RenameIndexOperation>(operations[0]);
                        Assert.Equal("SYSDBA", operation.Schema);
                        Assert.Equal("Donkey", operation.Table);
                        Assert.Equal("IX_Donkey_Value", operation.Name);
                        Assert.Equal("IX_SYSDBA.Donkey_Value", operation.NewName);
                    });
        }

        [Fact]
        public void Create_index_overridden()
        {
            Execute(
                source => source.Entity(
                    "Hippo",
                    x =>
                        {
                            x.ToTable("Hippo", "SYSDBA");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                target => target.Entity(
                    "Hippo",
                    x =>
                        {
                            x.ToTable("Hippo", "SYSDBA");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").ForXuGuDbHasName("IX_HipVal");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<CreateIndexOperation>(operations[0]);
                        Assert.Equal("SYSDBA", operation.Schema);
                        Assert.Equal("Hippo", operation.Table);
                        Assert.Equal("IX_HipVal", operation.Name);
                    });
        }

        [Fact]
        public void Drop_index_overridden()
        {
            Execute(
                source => source.Entity(
                    "Horse",
                    x =>
                        {
                            x.ToTable("Horse", "SYSDBA");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                            x.HasIndex("Value").ForXuGuDbHasName("IX_HorseVal");
                        }),
                target => target.Entity(
                    "Horse",
                    x =>
                        {
                            x.ToTable("Horse", "SYSDBA");
                            x.Property<int>("Id");
                            x.HasKey("Id");
                            x.Property<int>("Value");
                        }),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropIndexOperation>(operations[0]);
                        Assert.Equal("SYSDBA", operation.Schema);
                        Assert.Equal("Horse", operation.Table);
                        Assert.Equal("IX_HorseVal", operation.Name);
                    });
        }

        [Fact]
        public void Create_sequence_overridden()
        {
            Execute(
                _ => { },
                modelBuilder => modelBuilder.ForXuGuDbHasSequence("Tango", "SYSDBA"),
                operations =>
                    {
                        Assert.Equal(2, operations.Count);

                        Assert.IsType<EnsureSchemaOperation>(operations[0]);

                        var operation = Assert.IsType<CreateSequenceOperation>(operations[1]);
                        Assert.Equal("Tango", operation.Name);
                        Assert.Equal("SYSDBA", operation.Schema);
                    });
        }

        [Fact]
        public void Drop_sequence_overridden()
        {
            Execute(
                modelBuilder => modelBuilder.ForXuGuDbHasSequence("Bravo", "SYSDBA"),
                _ => { },
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<DropSequenceOperation>(operations[0]);
                        Assert.Equal("Bravo", operation.Name);
                        Assert.Equal("SYSDBA", operation.Schema);
                    });
        }

        [Fact]
        public void Alter_sequence_overridden()
        {
            Execute(
                source => source.HasSequence("Bravo"),
                target => target.ForXuGuDbHasSequence("Bravo").IncrementsBy(2),
                operations =>
                    {
                        Assert.Equal(1, operations.Count);

                        var operation = Assert.IsType<AlterSequenceOperation>(operations[0]);
                        Assert.Equal("Bravo", operation.Name);
                        Assert.Equal(2, operation.IncrementBy);
                    });
        }

        [Fact]
        public void Alter_column_rowversion()
        {
            Execute(
                source => source.Entity(
                    "Toad",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<byte[]>("Version");
                    }),
                target => target.Entity(
                    "Toad",
                    x =>
                    {
                        x.Property<int>("Id");
                        x.Property<byte[]>("Version")
                            .ValueGeneratedOnAddOrUpdate()
                            .IsConcurrencyToken();
                    }),
                operations =>
                {
                    Assert.Equal(1, operations.Count);

                    var operation = Assert.IsType<AlterColumnOperation>(operations[0]);
                    Assert.Equal("Toad", operation.Table);
                    Assert.Equal("Version", operation.Name);
                    Assert.True(operation.IsRowVersion);
                    Assert.True(operation.IsDestructiveChange);
                });
        }

        protected override ModelBuilder CreateModelBuilder() => XuGuDbTestHelpers.Instance.CreateConventionBuilder();

        protected override MigrationsModelDiffer CreateModelDiffer()
            => new MigrationsModelDiffer(
                new XuGuDbTypeMapper(),
                new XuGuDbAnnotationProvider(),
                new XuGuDbMigrationsAnnotationProvider());
    }
}
