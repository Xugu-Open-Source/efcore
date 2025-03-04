using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using EntityFrameworkCore.XuGu.Infrastructure;
using EntityFrameworkCore.XuGu.Metadata.Internal;
using EntityFrameworkCore.XuGu.Scaffolding.Internal;
using EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class MigrationsXGTest : MigrationsTestBase<MigrationsXGTest.MigrationsXGFixture>
    {
        public MigrationsXGTest(MigrationsXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }


        [ConditionalTheory(Skip = "TODO: Syntax issue in XG 7 only.")]
        public override Task Alter_check_constraint()
        {
            return base.Alter_check_constraint();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Alter_column_make_computed(bool? stored)
        {
            return base.Alter_column_make_computed(stored);
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Add_column_computed_with_collation()
        {
            return base.Add_column_computed_with_collation();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Add_column_with_collation()
        {
            return base.Add_column_with_collation();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Add_column_with_defaultValue_string()
        {
            return base.Add_column_with_defaultValue_string();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Add_column_with_defaultValueSql()
        {
            return base.Add_column_with_defaultValueSql();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Add_primary_key()
        {
            return base.Add_primary_key();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Add_primary_key_composite_with_name()
        {
            return base.Add_primary_key_composite_with_name();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Add_primary_key_with_name()
        {
            return base.Add_primary_key_with_name();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Add_unique_constraint()
        {
            return base.Add_unique_constraint();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Add_unique_constraint_composite_with_name()
        {
            return base.Add_unique_constraint_composite_with_name();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Alter_column_change_computed_type()
        {
            return base.Alter_column_change_computed_type();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Alter_column_change_type()
        {
            return base.Alter_column_change_type();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Alter_column_set_collation()
        {
            return base.Alter_column_set_collation();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Alter_sequence_all_settings()
        {
            return base.Alter_sequence_all_settings();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Alter_sequence_increment_by()
        {
            return base.Alter_sequence_increment_by();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Alter_table_add_comment_non_default_schema()
        {
            return base.Alter_table_add_comment_non_default_schema();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Create_index_with_filter()
        {
            return base.Create_index_with_filter();
        }

        public override async Task Alter_column_make_required()
        {
            await Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("SomeColumn");
                    }),
                builder => { },
                builder => builder.Entity("People").Property<string>("SomeColumn").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name != "Id");
                    Assert.False(column.IsNullable);
                }); ;

            AssertSql(
                @"ALTER TABLE `People` MODIFY COLUMN `SomeColumn` clob DEFAULT '' NOT NULL;");
        }

        [Fact]
        public override Task Create_schema()
        {
            //return base.Create_schema();
            var result=Test(
                builder => { },
                builder => builder.Entity("People")
                    .ToTable("People", "SomeOtherSchema")
                    .Property<int>("Id"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    if (AssertSchemaNames)
                        Assert.Equal("SomeOtherSchema", table.Schema);
                });
            AssertSql(
    @"BEGIN IF (SELECT COUNT(*) FROM `ALL_SCHEMAS` WHERE `SCHEMA_NAME` = 'SomeOtherSchema') < 1 THEN CREATE SCHEMA `SomeOtherSchema`;END IF;END;",
    //
    @"CREATE TABLE `SomeOtherSchema`.`People` (
    `Id` int NOT NULL
);");
            return result;
        }

        public override async Task Add_column_with_comment()
        {
            await Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("FullName").HasComment("My comment"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "FullName");
                    if (AssertComments)
                        Assert.Equal("My comment", column.Comment);
                });

            AssertSql(
                @"ALTER TABLE `People` ADD `FullName` clob NULL;
COMMENT ON COLUMN `People`.`FullName` IS 'My comment';");
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Create_sequence()
        {
            return base.Create_sequence();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Create_sequence_all_settings()
        {
            return base.Create_sequence_all_settings();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Create_table_all_settings()
        {
            return base.Create_table_all_settings();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Create_table_with_multiline_comments()
        {
            return base.Create_table_with_multiline_comments();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Create_unique_index_with_filter()
        {
            return base.Create_unique_index_with_filter();
        }

        [ConditionalTheory(Skip = "TODO: Syntax issue in XG 7 only.")]
        public override Task Drop_check_constraint()
        {
            return base.Drop_check_constraint();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Drop_column_primary_key()
        {
            return base.Drop_column_primary_key();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Drop_primary_key()
        {
            return base.Drop_primary_key();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Drop_sequence()
        {
            return base.Drop_sequence();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Move_sequence()
        {
            return base.Move_sequence();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Move_table()
        {
            return base.Move_table();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Rename_sequence()
        {
            return base.Rename_sequence();
        }

        [ConditionalTheory(Skip = "TODO")]
        public override Task Rename_table_with_primary_key()
        {
            return base.Rename_table_with_primary_key();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.GeneratedColumns))]
        public override async Task Add_column_with_computedSql(bool? stored)
        {
            await Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("X");
                        e.Property<int>("Y");
                    }),
                builder => { },
                builder => builder.Entity("People").Property<string>("Sum")
                    .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}", stored),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                });

            AssertSql(
                @$"ALTER TABLE `People` ADD `Sum` clob NULL;");
        }

        public override async Task SqlOperation()
        {
            await Test(
                builder => { },
                new SqlOperation { Sql = "show auto_commit" },
                model =>
                {
                    Assert.Empty(model.Tables);
                    Assert.Empty(model.Sequences);
                });

            AssertSql(
                @"show auto_commit");
        }

        public override async Task Rename_index()
        {
            await Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("FirstName");
                    }),
                builder => builder.Entity("People").HasIndex(new[] { "FirstName" }, "Foo"),
                builder => builder.Entity("People").HasIndex(new[] { "FirstName" }, "NEWfoo"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.Equal("NEWfoo", index.Name);
                });

            AssertSql(
                @"ALTER INDEX `People`.`Foo` RENAME TO `NEWfoo`;");
        }
        public override Task Add_column_with_ansi()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name").IsUnicode(false),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(
                        TypeMappingSource
                            .FindMapping(typeof(string), storeTypeName: null, unicode: false)
                            .StoreType, column.StoreType,ignoreCase: true);
                    Assert.True(column.IsNullable);
                });

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.GeneratedColumns))]
        public override async Task Create_table_with_computed_column(bool? stored)
        {
            await Test(
                builder => { },
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("X");
                        e.Property<int>("Y");
                        e.Property<string>("Sum").HasComputedColumnSql(
                            $"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}",
                            stored);
                    }),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                    //if (AssertComputedColumns)
                    //{
                    //    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    //    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    //    if (stored != null)
                    //        Assert.Equal(stored, sumColumn.IsStored);
                    //}
                });

            var storedSql = stored == true ? " PERSISTED" : "";

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`People` (
    `Id` int NOT NULL,
    `Sum` clob NULL,
    `X` int NOT NULL,
    `Y` int NOT NULL
);");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.GeneratedColumns))]
        public override async Task Alter_column_change_computed()
        {
            await Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("X");
                        e.Property<int>("Y");
                        e.Property<int>("Sum");
                    }),
                builder => builder.Entity("People").Property<int>("Sum")
                    .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}"),
                builder => builder.Entity("People").Property<int>("Sum")
                    .HasComputedColumnSql($"{DelimitIdentifier("X")} - {DelimitIdentifier("Y")}"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                });

            AssertSql(
                @"ALTER TABLE `People` MODIFY COLUMN `Sum` int NOT NULL;");
        }

        [ConditionalFact]
        public virtual async Task Add_columns_with_collations()
        {
            await Test(
                common => common
                    //.UseCollation(DefaultCollation)
                    .Entity(
                        "IceCream",
                        e => e.Property<int>("IceCreamId")),
                source => { },
                target => target.Entity(
                    "IceCream", e =>
                    {
                        e.Property<string>("Name");
                        e.Property<string>("Brand")
                            .UseCollation(NonDefaultCollation);
                    }),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var nameColumn = Assert.Single(table.Columns.Where(c => c.Name == "Name"));
                    var brandColumn = Assert.Single(table.Columns.Where(c => c.Name == "Brand"));

                    Assert.Null(nameColumn.Collation);
                    Assert.Equal(NonDefaultCollation, brandColumn.Collation);
                });

            AssertSql(
                $@"ALTER TABLE `IceCream` ADD `Brand` clob NULL;",
                //
                $@"ALTER TABLE `IceCream` ADD `Name` clob NULL;");
        }

        public override async Task Alter_column_add_comment()
        {
            await Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").Property<int>("Id").HasComment("Some comment"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns);
                    if (AssertComments)
                        Assert.Equal("Some comment", column.Comment);
                });

            AssertSql(
                @"ALTER TABLE `People` MODIFY COLUMN `Id` int NOT NULL;
COMMENT ON COLUMN `People`.`Id` IS 'Some comment';");
        }

        [ConditionalFact]
        public virtual async Task Create_table_NVARCHAR_UPPERCASE_column()
        {
            await Test(
                common => { },
                source => { },
                target => target.Entity(
                    "IceCream",
                    e =>
                    {
                        e.Property<int>("IceCreamId");
                        e.Property<string>("Name")
                            .HasColumnType("NVARCHAR") // UPPERCASE
                            .HasMaxLength(45);
                    }),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var nameColumn = Assert.Single(table.Columns.Where(c => c.Name == "Name"));

                    //Assert.True(nameColumn[XGAnnotationNames.CharSet] is "utf8mb3"
                    //    or "utf8");
                });

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` int NOT NULL,
    `Name` NVARCHAR(45) NULL
);");
        }

        public override Task Add_column_with_fixed_length()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name")
                    .IsFixedLength()
                    .HasMaxLength(100),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(
                        TypeMappingSource
                            .FindMapping(typeof(string), storeTypeName: null, fixedLength: true, size: 100)
                            .StoreType,
                        column.StoreType,ignoreCase: true);
                });

        public override Task Add_column_with_max_length()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name").HasMaxLength(30),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(
                        TypeMappingSource
                            .FindMapping(typeof(string), storeTypeName: null, size: 30)
                            .StoreType,
                        column.StoreType,ignoreCase: true);
                });

        public override Task Add_column_with_max_length_on_derived()
            => Test(
                builder =>
                {
                    builder.Entity("Person");
                    builder.Entity(
                        "SpecialPerson", e =>
                        {
                            e.HasBaseType("Person");
                            e.Property<string>("Name").HasMaxLength(30);
                        });

                    builder.Entity("MoreSpecialPerson").HasBaseType("SpecialPerson");
                },
                builder => { },
                builder => builder.Entity("Person").Property<string>("Name").HasMaxLength(30),
                model =>
                {
                    var table = Assert.Single(model.Tables, t => t.Name == "Person");
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(
                        TypeMappingSource
                            .FindMapping(typeof(string), storeTypeName: null, size: 30)
                            .StoreType,
                        column.StoreType,ignoreCase: true);
                });

        public override Task Add_column_with_required()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(TypeMappingSource.FindMapping(typeof(string)).StoreType, column.StoreType,ignoreCase: true);
                    Assert.False(column.IsNullable);
                });

        public override async Task Create_table_with_comments()
        {
            await Test(
                builder => { },
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("Name").HasComment("Column comment");
                        e.HasComment("Table comment");
                    }),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    if (AssertComments)
                    {
                        Assert.Equal("Table comment", table.Comment);
                        Assert.Equal("Column comment", column.Comment);
                    }
                });

            AssertSql(
                @"CREATE TABLE `SYSDBA`.`People` (
    `Id` int NOT NULL,
    `Name` clob NULL COMMENT 'Column comment'
) COMMENT 'Table comment';");
        }

        [ConditionalFact]
        public virtual async Task Add_guid_columns()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    //.UseCollation(DefaultCollation)
                    .Entity(
                        "IceCream",
                        e => e.Property<Guid>("IceCreamId")),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var iceCreamIdColumn = Assert.Single(table.Columns.Where(c => c.Name == "IceCreamId"));
                });

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` guid NOT NULL
);");
        }

        [ConditionalFact]
        public virtual async Task Add_guid_columns_with_collation()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    //.UseCollation(DefaultCollation)
                    .Entity(
                        "IceCream",
                        e => e.Property<Guid>("IceCreamId")
                            .UseCollation(NonDefaultCollation)),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var iceCreamIdColumn = Assert.Single(table.Columns.Where(c => c.Name == "IceCreamId"));
                });

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` guid NOT NULL
);");
        }

        [ConditionalFact]
        public virtual async Task Add_guid_columns_with_explicit_default_collation()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    //.UseCollation(DefaultCollation)
                    .UseGuidCollation(NonDefaultCollation)
                    .Entity(
                        "IceCream",
                        e => e.Property<Guid>("IceCreamId")),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var iceCreamIdColumn = Assert.Single(table.Columns.Where(c => c.Name == "IceCreamId"));

                    Assert.Equal(NonDefaultCollation, iceCreamIdColumn.Collation);
                });

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` guid NOT NULL
);");
        }

        [ConditionalFact]
        public virtual async Task Add_guid_columns_with_disabled_default_collation()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    //.UseCollation(DefaultCollation)
                    .UseGuidCollation(string.Empty)
                    .Entity(
                        "IceCream",
                        e => e.Property<Guid>("IceCreamId")),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var iceCreamIdColumn = Assert.Single(table.Columns.Where(c => c.Name == "IceCreamId"));

                    Assert.Null(iceCreamIdColumn.Collation);
                });

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` guid NOT NULL
);");
        }

        public override async Task Alter_column_make_required_with_composite_index()
        {
            await Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("FirstName");
                        e.Property<string>("LastName");
                        e.HasIndex("FirstName", "LastName");
                    }),
                builder => { },
                builder => builder.Entity("People").Property<string>("FirstName").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var firstNameColumn = Assert.Single(table.Columns, c => c.Name == "FirstName");
                    Assert.False(firstNameColumn.IsNullable);
                    var index = Assert.Single(table.Indexes);
                    Assert.Equal(2, index.Columns.Count);
                    Assert.Contains(table.Columns.Single(c => c.Name == "FirstName"), index.Columns);
                    Assert.Contains(table.Columns.Single(c => c.Name == "LastName"), index.Columns);
                });

            AssertSql(
                @"ALTER TABLE `People` MODIFY COLUMN `FirstName` varchar(255) DEFAULT '' NOT NULL;");
        }

        [ConditionalFact]
        public virtual async Task Alter_column_collations_with_delegation()
        {
            await Test(
                common => common
                    //.UseCollation(DefaultCollation)
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.Property<int>("IceCreamId");
                            e.Property<string>("Name");
                            e.Property<string>("Brand");
                        }),
                source => source.Entity(
                    "IceCream", e =>
                    {
                        e.Property<string>("Brand")
                            .UseCollation(NonDefaultCollation);
                    }),
                target => target.Entity(
                    "IceCream", e =>
                    {
                        e.Property<string>("Name")
                            .UseCollation(NonDefaultCollation);
                    }),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var nameColumn = Assert.Single(table.Columns.Where(c => c.Name == "Name"));
                    var brandColumn = Assert.Single(table.Columns.Where(c => c.Name == "Brand"));
                    Assert.Null(brandColumn.Collation);
                });

            AssertSql(
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Name` clob NULL;",
                //
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Brand` clob NULL;");
        }

        [ConditionalFact]
        public virtual async Task Alter_column_collations_with_delegation2()
        {
            await Test(
                common => common
                    //.UseCollation(DefaultCollation)
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.Property<int>("IceCreamId");
                            e.Property<string>("Name");
                            e.Property<string>("Brand");
                        }),
                source => source.Entity(
                    "IceCream", e =>
                    {
                        e.Property<string>("Brand")
                            .UseCollation(NonDefaultCollation);
                    }),
                target => target.Entity(
                    "IceCream", e =>
                    {
                        e.UseCollation(NonDefaultCollation2);
                        e.Property<string>("Name")
                            .UseCollation(NonDefaultCollation);
                    }),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var nameColumn = Assert.Single(table.Columns.Where(c => c.Name == "Name"));
                    var brandColumn = Assert.Single(table.Columns.Where(c => c.Name == "Brand"));

                    Assert.Equal(NonDefaultCollation, nameColumn.Collation);
                    Assert.Null(brandColumn.Collation);
                });

            AssertSql(
                $"ALTER TABLE `IceCream` COLLATE {NonDefaultCollation2};",
                //
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Name` longtext COLLATE {NonDefaultCollation} NULL;",
                //
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Brand` longtext COLLATE {NonDefaultCollation2} NULL;");
        }

        [ConditionalFact]
        public virtual async Task Alter_column_collations_with_delegation_columns_only()
        {
            await Test(
                common => common
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.Property<int>("IceCreamId");
                            e.Property<string>("Name");
                            e.Property<string>("Brand");
                        }),
                source => source
                    //.UseCollation(DefaultCollation, DelegationModes.ApplyToColumns)
                    .Entity(
                        "IceCream", e =>
                        {
                            e.Property<string>("Brand")
                                .UseCollation(NonDefaultCollation);
                        }),
                target => target
                    .UseCollation(NonDefaultCollation2, DelegationModes.ApplyToColumns)
                    .Entity(
                        "IceCream", e =>
                        {
                            e.Property<string>("Name")
                                .UseCollation(NonDefaultCollation);
                        }),
                result => { });

            AssertSql(
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Name` clob NULL;",
                //
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Brand` clob NULL;");
        }

        [ConditionalFact]
        public virtual async Task Alter_column_collations_with_delegation_columns_only_with_inbetween_tableonly_collation()
        {
            await Test(
                common => common
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.Property<int>("IceCreamId");
                            e.Property<string>("Name");
                            e.Property<string>("Brand");
                        }),
                source => source
                    //.UseCollation(DefaultCollation, DelegationModes.ApplyToColumns)
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.Property<string>("Brand")
                                .UseCollation(NonDefaultCollation);
                        }),
                target => target
                    //.UseCollation(NonDefaultCollation2, DelegationModes.ApplyToColumns)
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            //e.UseCollation(DefaultCollation, DelegationModes.ApplyToTables);
                            e.Property<string>("Name")
                                .UseCollation(NonDefaultCollation);
                        }),
                result => { });

            AssertSql(
                $@"ALTER TABLE `IceCream`;",
                //
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Name` longtext NULL;",
                //
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Brand` longtext NULL;");
        }

        [ConditionalFact]
        public virtual void Upgrade_legacy_charset_to_annotation_charset_only_does_not_generate_alter_column_operations()
        {
            var context = XGTestHelpers.Instance.CreateContext();

            var sourceModel = CreateConventionlessModelBuilder()
                .Entity(
                    "IssueConsoleTemplate.IceCream", b =>
                    {
                        b.Property<int>("IceCreamId")
                            .HasColumnType("int");

                        b.Property<string>("Name")
                            .HasColumnType("longtext CHARACTER SET utf8mb4");

                        b.HasKey("IceCreamId");

                        b.ToTable("IceCreams");
                    })
                .Model
                .FinalizeModel();

            var targetModel = CreateConventionlessModelBuilder()
                .Entity(
                    "IssueConsoleTemplate.IceCream", b =>
                    {
                        b.Property<int>("IceCreamId")
                            .HasColumnType("int");

                        b.Property<string>("Name")
                            .HasColumnType("longtext");

                        b.HasKey("IceCreamId");

                        b.ToTable("IceCreams");
                    })
                .Model
                .FinalizeModel();

            var modelDiffer = context.GetService<IMigrationsModelDiffer>();

            var operations = modelDiffer.GetDifferences(
                sourceModel.GetRelationalModel(),
                targetModel.GetRelationalModel());

            Assert.Empty(operations);
        }

        [ConditionalFact]
        public virtual async Task Create_table_explicit_column_charset_takes_precedence_over_inherited_collation()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    //.UseCollation(DefaultCollation)
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.Property<int>("IceCreamId");
                            e.Property<string>("Name");
                            e.Property<string>("Brand")
                                .HasCharSet(NonDefaultCharSet);
                        }),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var nameColumn = Assert.Single(table.Columns.Where(c => c.Name == "Name"));
                    var brandColumn = Assert.Single(table.Columns.Where(c => c.Name == "Brand"));

                    Assert.Null(nameColumn[XGAnnotationNames.CharSet]);
                    Assert.Null(nameColumn.Collation);
                    //Assert.Equal(NonDefaultCharSet, brandColumn[XGAnnotationNames.CharSet]);
                    //Assert.NotEqual(DefaultCollation, brandColumn.Collation);
                });

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `Brand` clob NULL,
    `IceCreamId` int NOT NULL,
    `Name` clob NULL
);");
        }

        [ConditionalFact]
        public virtual async Task Create_table_explicit_column_collation_takes_precedence_over_inherited_charset()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    .HasCharSet(NonDefaultCharSet)
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.Property<int>("IceCreamId");
                            e.Property<string>("Name");
                            e.Property<string>("Brand")
                                .UseCollation(NonDefaultCollation2);
                        }),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var nameColumn = Assert.Single(table.Columns.Where(c => c.Name == "Name"));
                    var brandColumn = Assert.Single(table.Columns.Where(c => c.Name == "Brand"));

                    Assert.Null(nameColumn[XGAnnotationNames.CharSet]);
                    Assert.Null(nameColumn.Collation);
                });

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `Brand` clob NULL,
    `IceCreamId` int NOT NULL,
    `Name` clob NULL
);");
        }

        [ConditionalFact]
        public virtual async Task Create_table_clob_column_with_string_length_and_legacy_charset_definition_in_column_type()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.Property<int>("IceCreamId");
                            e.Property<string>("Name")
                                .HasColumnType($"clob")
                                .HasMaxLength(2048);
                        }),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var nameColumn = Assert.Single(table.Columns.Where(c => c.Name == "Name"));

                    //Assert.Equal(NonDefaultCharSet, nameColumn[XGAnnotationNames.CharSet]);
                    Assert.Equal("CLOB", nameColumn.StoreType);
                });

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` int NOT NULL,
    `Name` clob NULL
);");
        }

        [ConditionalFact]
        public virtual async Task Drop_unique_constraint_without_recreating_foreign_keys()
        {
            await Test(
                builder => builder
                    .Entity(
                        "Foo", e =>
                        {
                            e.Property<int>("FooPK");
                            e.Property<int>("FooAK");
                            e.Property<int>("BarFK");
                            e.HasKey("FooPK");
                            e.HasOne("Bar", "Bars")
                                .WithMany()
                                .HasForeignKey("BarFK");
                        })
                    .Entity(
                        "Bar", e =>
                        {
                            e.Property<int>("BarPK");
                            e.HasKey("BarPK");
                        }),
                builder => builder
                    .Entity(
                        "Foo", e => e.HasAlternateKey("FooAK")),
                builder => { },
                model => Assert.Empty(Assert.Single(model.Tables.Where(t => t.Name == "Foo"))?.UniqueConstraints));

            AssertSql(
                @"ALTER TABLE `Foo` DROP CONSTRAINT `AK_Foo_FooAK`;");
        }

        [ConditionalFact]
        public virtual async Task Drop_unique_constraint_without_recreating_foreign_keys_MigrationBuilder()
        {
            await Test(
                builder => builder
                    .Entity(
                        "Foo", e =>
                        {
                            e.Property<int>("FooPK");
                            e.Property<int>("FooAK");
                            e.Property<int>("BarFK");
                            e.HasKey("FooPK");
                            e.HasOne("Bar", "Bars")
                                .WithMany()
                                .HasForeignKey("BarFK");
                        })
                    .Entity(
                        "Bar", e =>
                        {
                            e.Property<int>("BarPK");
                            e.HasKey("BarPK");
                        }),
                builder => builder
                    .Entity(
                        "Foo", e => e.HasAlternateKey("FooAK")),
                builder => { },
                migrationBuilder => migrationBuilder.DropUniqueConstraint("AK_Foo_FooAK", "Foo"),
                model => Assert.Empty(Assert.Single(model.Tables.Where(t => t.Name == "Foo"))?.UniqueConstraints));

            AssertSql(
                @"ALTER TABLE `Foo` DROP CONSTRAINT `AK_Foo_FooAK`;");
        }

        [ConditionalFact]
        public virtual async Task Drop_unique_constraint_with_recreating_foreign_keys_MigrationBuilder()
        {
            await Test(
                builder => builder
                    .Entity(
                        "Foo", e =>
                        {
                            e.Property<int>("FooPK");
                            e.Property<int>("FooAK");
                            e.Property<int>("BarFK");
                            e.HasKey("FooPK");
                            e.HasOne("Bar", "Bars")
                                .WithMany()
                                .HasForeignKey("BarFK");
                        })
                    .Entity(
                        "Bar", e =>
                        {
                            e.Property<int>("BarPK");
                            e.HasKey("BarPK");
                        }),
                builder => builder
                    .Entity(
                        "Foo", e => e.HasAlternateKey("FooAK")),
                builder => { },
                migrationBuilder => migrationBuilder.DropUniqueConstraint("AK_Foo_FooAK", "Foo", recreateForeignKeys: true),
                model => Assert.Empty(Assert.Single(model.Tables.Where(t => t.Name == "Foo"))?.UniqueConstraints));

            AssertSql(
                @"ALTER TABLE `Foo` DROP CONSTRAINT `FK_Foo_Bar_BarFK`;",
                //
                @"ALTER TABLE `Foo` DROP CONSTRAINT `AK_Foo_FooAK`;",
                //
                @"ALTER TABLE `Foo` ADD CONSTRAINT `FK_Foo_Bar_BarFK` FOREIGN KEY (`BarFK`) REFERENCES `Bar` (`BarPK`) ON DELETE RESTRICT;");
        }

        //protected virtual string DefaultCollation => ((XGTestStore)Fixture.TestStore).DatabaseCollation;

        //protected override string NonDefaultCollation
        //    => DefaultCollation == ((XGTestStore)Fixture.TestStore).GetCaseSensitiveUtf8Mb4Collation()
        //        ? ((XGTestStore)Fixture.TestStore).GetCaseInsensitiveUtf8Mb4Collation()
        //        : ((XGTestStore)Fixture.TestStore).GetCaseSensitiveUtf8Mb4Collation();

        protected virtual string NonDefaultCollation2
            => null;

        protected virtual string DefaultCharSet => ((XGTestStore)Fixture.TestStore).DatabaseCharSet;
        protected virtual string NonDefaultCharSet => "latin1";

        protected override string NonDefaultCollation => null;

        protected virtual Task Test(
            Action<ModelBuilder> buildCommonAction,
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<MigrationBuilder> migrationBuilderAction,
            Action<DatabaseModel> asserter)
        {
            // Build the source and target models. Add current/latest product version if one wasn't set.
            var sourceModelBuilder = CreateConventionlessModelBuilder();
            buildCommonAction(sourceModelBuilder);
            buildSourceAction(sourceModelBuilder);
            var sourceModel = sourceModelBuilder.FinalizeModel();

            var targetModelBuilder = CreateConventionlessModelBuilder();
            buildCommonAction(targetModelBuilder);
            buildTargetAction(targetModelBuilder);
            var targetModel = targetModelBuilder.FinalizeModel();

            var migrationBuilder = new MigrationBuilder(null);
            migrationBuilderAction(migrationBuilder);

            return Test(sourceModel, targetModel, migrationBuilder.Operations, asserter);
        }

        public class MigrationsXGFixture : MigrationsFixtureBase
        {
            protected override string StoreName { get; } = nameof(MigrationsXGTest);
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
            public override TestHelpers TestHelpers => XGTestHelpers.Instance;

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddScoped<IDatabaseModelFactory, XGDatabaseModelFactory>();
        }
    }
}
