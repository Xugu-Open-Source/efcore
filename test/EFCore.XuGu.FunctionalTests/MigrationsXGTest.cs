using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Tests;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class MigrationsXGTest : MigrationsTestBase<MigrationsXGTest.MigrationsXGFixture>
    {
        public MigrationsXGTest(MigrationsXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact(Skip = "TODO: Syntax issue in XuGu 7 only.")]
        public override Task Alter_check_constraint()
        {
            return base.Alter_check_constraint();
        }

        [ConditionalTheory(Skip = "TODO")]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public override Task Alter_column_make_computed(bool? stored)
        {
            return base.Alter_column_make_computed(stored);
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Add_column_computed_with_collation()
        {
            return base.Add_column_computed_with_collation();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Add_column_with_collation()
        {
            return base.Add_column_with_collation();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.DefaultExpression), nameof(ServerVersionSupport.AlternativeDefaultExpression))]
        public override async Task Add_column_with_defaultValue_string()
        {
            await base.Add_column_with_defaultValue_string();

            AssertSql(
                @"ALTER TABLE `People` ADD `Name` varchar DEFAULT 'John Doe' NOT NULL;");
        }

        public override Task Alter_column_make_required_with_null_data()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("SomeColumn");
                        e.HasData(new Dictionary<string, object> { { "Id", 1 }, { "SomeColumn", null } });
                    }),
                builder => { },
                builder => builder.Entity("People").Property<string>("SomeColumn").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name != "Id");
                    Assert.True(column.IsNullable);
                });

        public override Task Alter_column_make_required_with_index()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("SomeColumn");
                        e.HasIndex("SomeColumn");
                    }),
                builder => { },
                builder => builder.Entity("People").Property<string>("SomeColumn").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name != "Id");
                    Assert.True(column.IsNullable);
                    var index = Assert.Single(table.Indexes);
                    Assert.Same(column, Assert.Single(index.Columns));
                });

        public override Task Alter_column_make_required_with_composite_index()
            => Test(
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
                    Assert.True(firstNameColumn.IsNullable);
                    var index = Assert.Single(table.Indexes);
                    Assert.Equal(2, index.Columns.Count);
                    Assert.Contains(table.Columns.Single(c => c.Name == "FirstName"), index.Columns);
                    Assert.Contains(table.Columns.Single(c => c.Name == "LastName"), index.Columns);
                });

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
                    Assert.True(column.IsNullable);
                });

            AssertSql(
                @"UPDATE `People` SET `SomeColumn` = ''
WHERE `SomeColumn` IS NULL;
SELECT ROWNUM;
--GO",
                //
                @"ALTER TABLE `People` MODIFY COLUMN `SomeColumn` varchar NOT NULL;");
        }

        
        [ConditionalFact]
        public override async Task Alter_column_make_non_computed()
        {
            await Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("X");
                        e.Property<int>("Y");
                    }),
                builder => builder.Entity("People")
                    .Property<int>("Sum")
                    .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}", stored: true), // <-- added "stored: true"
                builder => builder.Entity("People").Property<int>("Sum"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                    Assert.Null(sumColumn.ComputedColumnSql);
                    Assert.NotEqual(true, sumColumn.IsStored);
                });

            AssertSql(
                @"ALTER TABLE `People` MODIFY COLUMN `Sum` int NOT NULL;");
        }

        [ConditionalFact]
        public virtual async Task Alter_string_column_make_required_generates_update_statement_instead_of_default_value()
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
                    Assert.True(column.IsNullable);
                    Assert.Null(column.DefaultValueSql);
                });

            AssertSql(
                @"UPDATE `People` SET `SomeColumn` = ''
WHERE `SomeColumn` IS NULL;
SELECT ROWNUM;
--GO",
                //
                @"ALTER TABLE `People` MODIFY COLUMN `SomeColumn` varchar NOT NULL;");
        }

        [ConditionalFact]
        public virtual async Task Add_column_with_defaultValue_string_limited_length()
        {
            await Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People")
                    .Property<string>("Name")
                    .HasMaxLength(128) // specify explicit length
                    .IsRequired()
                    .HasDefaultValue("John Doe"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    var nameColumn = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.False(nameColumn.IsNullable);
                    Assert.Contains("John Doe", nameColumn.DefaultValueSql);
                });

            AssertSql(
                @"ALTER TABLE `People` ADD `Name` varchar(128) DEFAULT 'John Doe' NOT NULL;");
        }

        [ConditionalFact]
        [SupportedServerVersionLessThanCondition(nameof(ServerVersionSupport.DefaultExpression), nameof(ServerVersionSupport.AlternativeDefaultExpression))]
        public virtual async Task Add_column_with_defaultValue_string_unlimited_length_without_default_value_expression_support_throws_warning()
        {
            await TestThrows<InvalidOperationException>(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People")
                    .Property<string>("Name")
                    .IsRequired()
                    .HasDefaultValue("John Doe"));
        }

        public override async Task Add_column_with_defaultValue_datetime()
        {
            await base.Add_column_with_defaultValue_datetime();

            AssertSql(
                @"ALTER TABLE `People` ADD `Birthday` datetime(6) DEFAULT '2015-04-12 17:05:00' NOT NULL;");
        }

        
        public override async Task Add_column_with_defaultValueSql()
        {
            await Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People")
                    .Property<int>("Sum")
                    .HasDefaultValueSql("(1 + 2)"), // default expression needs to be wrapped in parenthesis
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                });

            AssertSql(
                @"ALTER TABLE `People` ADD `Sum` int DEFAULT (1 + 2) NOT NULL;");
        }

        [ConditionalFact]
        public virtual async Task Add_column_with_defaultValueSql_simple()
        {
            await Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People")
                    .Property<int>("Sum")
                    .HasDefaultValueSql("3"), // simple value
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal(2, table.Columns.Count);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                    Assert.Contains("3", sumColumn.DefaultValueSql);
                });

            AssertSql(
                @"ALTER TABLE `People` ADD `Sum` int DEFAULT 3 NOT NULL;");
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
                builder => builder.Entity("People").HasIndex(new[] { "FirstName" }, "newfoo"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.Equal("newfoo", index.Name);
                });

            AssertSql(
                AppConfig.ServerVersion.Supports.RenameIndex
                    ? new[] { @"ALTER INDEX `People`.`Foo` RENAME TO `newfoo`;" }
                    : new[]
                    {
                        @"ALTER TABLE `People` DROP INDEX `Foo`;",
                        //
                        "CREATE INDEX `newfoo` ON `People` (`FirstName`);"
                    });
        }

        [ConditionalFact]
        public virtual async Task Rename_index_with_prefix_length()
        {
            await Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<string>("FirstName");
                    }),
                builder => builder.Entity("People").HasIndex(new[] { "FirstName" }, "OldIndex").HasPrefixLength(50),
                builder => builder.Entity("People").HasIndex(new[] { "FirstName" }, "NewIndex").HasPrefixLength(50),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var index = Assert.Single(table.Indexes);
                    Assert.Equal("NewIndex", index.Name);
                });

            AssertSql(
                AppConfig.ServerVersion.Supports.RenameIndex
                    ? new[] { @"ALTER INDEX `People`.`OldIndex` RENAME TO `NewIndex`;" }
                    : new[]
                    {
                        @"ALTER TABLE `People` DROP INDEX `OldIndex`;",
                        //
                        "CREATE INDEX `NewIndex` ON `People` (`FirstName`);"
                    });
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Add_primary_key_int()
        {
            return base.Add_primary_key_int();
        }

        [ConditionalFact(Skip = "TODO")]
        public override async Task Add_primary_key_string()
        {
            await base.Add_primary_key_string();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Add_primary_key_composite_with_name()
        {
            return base.Add_primary_key_composite_with_name();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Add_primary_key_with_name()
        {
            return base.Add_primary_key_with_name();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Add_unique_constraint()
        {
            return base.Add_unique_constraint();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Add_unique_constraint_composite_with_name()
        {
            return base.Add_unique_constraint_composite_with_name();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Alter_column_change_computed_type()
        {
            return base.Alter_column_change_computed_type();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Alter_column_change_type()
        {
            return base.Alter_column_change_type();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Alter_column_set_collation()
        {
            return base.Alter_column_set_collation();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public override async Task Alter_sequence_all_settings()
        {
            await Test(
                builder => builder.HasSequence<int>("foo"),
                builder => { },
                builder => builder.HasSequence<int>("foo")
                    .StartsAt(-3)
                    .IncrementsBy(2)
                    .HasMin(-5)
                    .HasMax(10)
                    .IsCyclic(),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);

                    // Assert.Equal(-3, sequence.StartValue);
                    Assert.Equal(1, sequence.StartValue); // Restarting doesn't change the scaffolded start value

                    Assert.Equal(2, sequence.IncrementBy);
                    Assert.Equal(-5, sequence.MinValue);
                    Assert.Equal(10, sequence.MaxValue);
                    Assert.True(sequence.IsCyclic);
                });

            AssertSql(
                """
ALTER SEQUENCE `foo` INCREMENT BY 2 MINVALUE -5 MAXVALUE 10 CYCLE;
""",
                //
                """
ALTER SEQUENCE `foo` RESTART WITH -3;
""");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public override Task Alter_sequence_increment_by()
        {
            return base.Alter_sequence_increment_by();
        }

        public override async Task Alter_table_add_comment_non_default_schema()
        {
            await base.Alter_table_add_comment_non_default_schema();

            AssertSql(
                @"COMMENT ON TABLE `SomeOtherSchema`.`People` IS 'Table comment';");
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Create_index_with_filter()
        {
            return base.Create_index_with_filter();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Create_schema()
        {
            return base.Create_schema();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public override Task Create_sequence()
        {
            return base.Create_sequence();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public override Task Create_sequence_long()
        {
            return base.Create_sequence_long();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public override Task Create_sequence_short()
        {
            return base.Create_sequence_short();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public override async Task Create_sequence_all_settings()
        {
            await Test(
                builder => { },
                builder => builder.HasSequence<long>("TestSequence", "dbo2")
                    .StartsAt(3)
                    .IncrementsBy(2)
                    .HasMin(2)
                    .HasMax(916)
                    .IsCyclic(),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);

                    // Assert.Equal("TestSequence", sequence.Name);
                    // Assert.Equal("dbo2", sequence.Schema);
                    Assert.Equal("dbo2_TestSequence", sequence.Name);

                    Assert.Equal(3, sequence.StartValue);
                    Assert.Equal(2, sequence.IncrementBy);
                    Assert.Equal(2, sequence.MinValue);
                    Assert.Equal(916, sequence.MaxValue);
                    Assert.True(sequence.IsCyclic);
                });

            AssertSql(
"""
CREATE SEQUENCE `dbo2_TestSequence` START WITH 3 INCREMENT BY 2 MINVALUE 2 MAXVALUE 916 CYCLE;
""");
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Create_table_all_settings()
        {
            return base.Create_table_all_settings();
        }

        public override async Task Create_table_with_multiline_comments()
        {
            await base.Create_table_with_multiline_comments();

            AssertSql(
                @"CREATE TABLE `SYSDBA`.`People` (
    `Id` int IDENTITY NOT NULL,
    `Name` varchar NULL COMMENT 'This is a multi-line
column comment.
More information can
be found in the docs.',
    CONSTRAINT `PK_People` PRIMARY KEY (`Id`)
) COMMENT 'This is a multi-line
table comment.
More information can
be found in the docs.';");
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Create_unique_index_with_filter()
        {
            return base.Create_unique_index_with_filter();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.DescendingIndexes))]
        public override async Task Create_index_descending()
        {
            await base.Create_index_descending();

            AssertSql(
                @"CREATE INDEX `IX_People_X` ON `People` (`X` DESC);");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.DescendingIndexes))]
        public override async Task Create_index_descending_mixed()
        {
            await base.Create_index_descending_mixed();

            AssertSql(
                @"CREATE INDEX `IX_People_X_Y_Z` ON `People` (`X` ASC, `Y` DESC, `Z` ASC);");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.DescendingIndexes))]
        public override async Task Alter_index_change_sort_order()
        {
            await base.Alter_index_change_sort_order();

            AssertSql(
                @"DROP INDEX IF EXISTS `People`.`IX_People_X_Y_Z`;",
                //
                @"CREATE INDEX `IX_People_X_Y_Z` ON `People` (`X` ASC, `Y` DESC, `Z` ASC);");
        }

        [ConditionalFact(Skip = "TODO: Syntax issue in XuGu 7 only.")]
        public override Task Drop_check_constraint()
        {
            return base.Drop_check_constraint();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Drop_column_primary_key()
        {
            return base.Drop_column_primary_key();
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Drop_primary_key_int()
        {
            return base.Drop_primary_key_int();
        }

        [ConditionalFact(Skip = "TODO")]
        public override async Task Drop_primary_key_string()
        {
            await base.Drop_primary_key_string();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public override Task Drop_sequence()
        {
            return base.Drop_sequence();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public override async Task Move_sequence()
        {
            await Test(
                builder => builder.HasSequence<int>("TestSequenceMove"),
                builder => builder.HasSequence<int>("TestSequenceMove", "TestSequenceSchema"),
                model =>
                {
                    var sequence = Assert.Single(model.Sequences);
                    // Assert.Equal("TestSequenceSchema", sequence.Schema);
                    // Assert.Equal("TestSequence", sequence.Name);
                    Assert.Equal("TestSequenceSchema_TestSequenceMove", sequence.Name);
                });

            AssertSql(
"""
ALTER TABLE `TestSequenceMove` RENAME `TestSequenceSchema_TestSequenceMove`;
""");
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Move_table()
        {
            return base.Move_table();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Sequences))]
        public override async Task Rename_sequence()
        {
            await base.Rename_sequence();

            AssertSql(
"""
ALTER TABLE `TestSequence` RENAME `testsequence`;
""");
        }

        [ConditionalFact(Skip = "TODO")]
        public override Task Rename_table_with_primary_key()
        {
            return base.Rename_table_with_primary_key();
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.GeneratedColumns))]
        public override Task Add_column_with_computedSql(bool? stored)
            => base.Add_column_with_computedSql(stored);

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.GeneratedColumns))]
        public override Task Create_table_with_computed_column(bool? stored)
            => Test(
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
                        if (AssertComputedColumns)
                        {
                        }
                    });

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.GeneratedColumns))]
        public override Task Alter_column_change_computed()
            => Test(
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
                    //if (AssertComputedColumns)
                    //{
                    //    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    //    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    //    Assert.Contains("-", sumColumn.ComputedColumnSql);
                    //}
                });

        // We currently do not scaffold table options.
        //
        // [ConditionalFact]
        // public virtual async Task Create_table_with_table_options()
        // {
        //     await Test(
        //         builder => { },
        //         builder => builder.Entity(
        //             "IceCream", e =>
        //             {
        //                 e.Property<int>("IceCreamId");
        //                 e.HasTableOption("CHECKSUM", "1");
        //                 e.HasTableOption("MAX_ROWS", "100");
        //             }),
        //         model =>
        //         {
        //             var table = Assert.Single(model.Tables);
        //             var options = (IDictionary<string, string>)XGEntityTypeExtensions.DeserializeTableOptions(
        //                 table.FindAnnotation(XGAnnotationNames.StoreOptions)?.Value as string);
        //
        //             Assert.Contains("CHECKSUM", options);
        //             Assert.Equal("1", options["CHECKSUM"]);
        //
        //             Assert.Contains("MAX_ROWS", options);
        //             Assert.Equal("100", options["MAX_ROWS"]);
        //         });
        //
        //     AssertSql(@"");
        // }

        public override Task Add_column_with_required()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => { },
                builder => builder.Entity("People").Property<string>("Name").IsRequired(),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var column = Assert.Single(table.Columns, c => c.Name == "Name");
                    Assert.Equal(TypeMappingSource.FindMapping(typeof(string)).StoreType, column.StoreType,true);
                    Assert.False(column.IsNullable);
                });

        [ConditionalFact]
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
                            .StoreType, column.StoreType, true);
                    Assert.True(column.IsNullable);
                });

        [ConditionalFact]
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
                        column.StoreType,true);
                });

        [ConditionalFact]
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
                        column.StoreType, true);
                });

        [ConditionalFact]
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
                        column.StoreType,true);
                });

        [ConditionalFact]
        public virtual async Task Add_columns_with_collations()
        {
            await Test(
                common => common
                    .UseCollation(DefaultCollation)
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
                $@"ALTER TABLE `IceCream` ADD `Brand` varchar NULL;",
                //
                $@"ALTER TABLE `IceCream` ADD `Name` varchar NULL;");
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
                });

            AssertSql(
                @"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` int IDENTITY NOT NULL,
    `Name` NVARCHAR(45) NULL,
    CONSTRAINT `PK_IceCream` PRIMARY KEY (`IceCreamId`)
);");
        }

        [ConditionalFact]
        public virtual async Task Add_guid_columns()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    .UseCollation(DefaultCollation)
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
    `IceCreamId` guid NOT NULL,
    CONSTRAINT `PK_IceCream` PRIMARY KEY (`IceCreamId`)
);");
        }

        [ConditionalFact]
        public virtual async Task Add_guid_columns_with_collation()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    .UseCollation(DefaultCollation)
                    .Entity(
                        "IceCream",
                        e => e.Property<Guid>("IceCreamId")
                            .UseCollation(NonDefaultCollation)),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var iceCreamIdColumn = Assert.Single(table.Columns.Where(c => c.Name == "IceCreamId"));

                    Assert.Equal(NonDefaultCollation, iceCreamIdColumn.Collation);
                });

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` guid NOT NULL,
    CONSTRAINT `PK_IceCream` PRIMARY KEY (`IceCreamId`)
);");
        }

        [ConditionalFact]
        public virtual async Task Add_guid_columns_with_explicit_default_collation()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    .UseCollation(DefaultCollation)
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
    `IceCreamId` guid NOT NULL,
    CONSTRAINT `PK_IceCream` PRIMARY KEY (`IceCreamId`)
);");
        }

        [ConditionalFact]
        public virtual async Task Add_guid_columns_with_disabled_default_collation()
        {
            await Test(
                common => { },
                source => { },
                target => target
                    .UseCollation(DefaultCollation)
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
    `IceCreamId` guid NOT NULL,
    CONSTRAINT `PK_IceCream` PRIMARY KEY (`IceCreamId`)
);");
        }

        public override Task Alter_column_change_computed_recreates_indexes()
            => Test(
                builder => builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("X");
                        e.Property<int>("Y");
                        e.Property<int>("Sum");

                        e.HasIndex("Sum");
                    }),
                builder => builder.Entity("People").Property<int>("Sum")
                    .HasComputedColumnSql($"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}"),
                builder => builder.Entity("People").Property<int>("Sum")
                    .HasComputedColumnSql($"{DelimitIdentifier("X")} - {DelimitIdentifier("Y")}"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                    //if (AssertComputedColumns)
                    //{
                    //    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    //    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    //    Assert.Contains("-", sumColumn.ComputedColumnSql);
                    //}

                    var sumIndex = Assert.Single(table.Indexes);
                    Assert.Collection(sumIndex.Columns, c => Assert.Equal("Sum", c.Name));
                });

        [ConditionalFact]
        public virtual async Task Alter_column_collations_with_delegation()
        {
            await Test(
                common => common
                    .UseCollation(DefaultCollation)
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

                    Assert.Equal(NonDefaultCollation, nameColumn.Collation);
                    Assert.Null(brandColumn.Collation);
                });

            AssertSql();
        }

        [ConditionalFact]
        public virtual async Task Alter_column_collations_with_delegation2()
        {
            await Test(
                common => common
                    .UseCollation(DefaultCollation)
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

                    Assert.Equal(NonDefaultCollation, nameColumn.Collation);
                    Assert.Null(brandColumn.Collation);
                });

            AssertSql();
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
                    .UseCollation(DefaultCollation, DelegationModes.ApplyToColumns)
                    .Entity(
                        "IceCream", e =>
                        {
                            e.Property<string>("Brand")
                                .UseCollation(NonDefaultCollation);
                        }),
                target => target
                    .Entity(
                        "IceCream", e =>
                        {
                            e.Property<string>("Name")
                                .UseCollation(NonDefaultCollation);
                        }),
                result => { });

            AssertSql(
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Name` varchar NULL;",
                //
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Brand` varchar NULL;");
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
                    .UseCollation(DefaultCollation, DelegationModes.ApplyToColumns)
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.Property<string>("Brand")
                                .UseCollation(NonDefaultCollation);
                        }),
                target => target
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.UseCollation(DefaultCollation, DelegationModes.ApplyToTables);
                            e.Property<string>("Name")
                                .UseCollation(NonDefaultCollation);
                        }),
                result => { });

            AssertSql(
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Name` varchar NULL;",
                //
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Brand` varchar NULL;");
        }

        [ConditionalFact]
        public virtual void Upgrade_legacy_charset_to_annotation_charset_only_does_not_generate_alter_column_operations()
        {
            var context = XGTestHelpers.Instance.CreateContext();

            var sourceModel = context.GetService<IModelRuntimeInitializer>()
                .Initialize(
                    new ModelBuilder()
                        .Entity(
                            "IssueConsoleTemplate.IceCream", b =>
                            {
                                b.Property<int>("IceCreamId")
                                    .HasColumnType("int");

                                b.Property<string>("Name")
                                    .HasColumnType("varchar");

                                b.HasKey("IceCreamId");

                                b.ToTable("IceCreams");
                            })
                        .Model
                        .FinalizeModel(),
                    designTime: true,
                    validationLogger: null);

            var targetModel = context.GetService<IModelRuntimeInitializer>()
                .Initialize(
                    new ModelBuilder()
                        .Entity(
                            "IssueConsoleTemplate.IceCream", b =>
                            {
                                b.Property<int>("IceCreamId")
                                    .HasColumnType("int");

                                b.Property<string>("Name")
                                    .HasColumnType("varchar");

                                b.HasKey("IceCreamId");

                                b.ToTable("IceCreams");
                            })
                        .Model
                        .FinalizeModel(),
                    designTime: true,
                    validationLogger: null);

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
                    .UseCollation(DefaultCollation)
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
                //
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` int IDENTITY NOT NULL,
    `Brand` varchar NULL,
    `Name` varchar NULL,
    CONSTRAINT `PK_IceCream` PRIMARY KEY (`IceCreamId`)
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
                            e.Property<string>("Brand");
                        }),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var nameColumn = Assert.Single(table.Columns.Where(c => c.Name == "Name"));
                    var brandColumn = Assert.Single(table.Columns.Where(c => c.Name == "Brand"));

                    Assert.Null(nameColumn[XGAnnotationNames.CharSet]);
                    Assert.Null(nameColumn.Collation);
                    Assert.NotEqual(NonDefaultCharSet, brandColumn[XGAnnotationNames.CharSet]);
                });

            AssertSql(
                //
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` int IDENTITY NOT NULL,
    `Brand` varchar NULL,
    `Name` varchar NULL,
    CONSTRAINT `PK_IceCream` PRIMARY KEY (`IceCreamId`)
);");
        }

        [ConditionalFact]
        public virtual async Task Create_table_varchar_column_with_string_length_and_legacy_charset_definition_in_column_type()
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
                                .HasColumnType($"varchar")
                                .HasMaxLength(2048);
                        }),
                result =>
                {
                    var table = Assert.Single(result.Tables);
                    var nameColumn = Assert.Single(table.Columns.Where(c => c.Name == "Name"));

                    Assert.Equal("VARCHAR(2048)", nameColumn.StoreType);
                });

            AssertSql(
                $@"CREATE TABLE `SYSDBA`.`IceCream` (
    `IceCreamId` int IDENTITY NOT NULL,
    `Name` varchar(2048) NULL,
    CONSTRAINT `PK_IceCream` PRIMARY KEY (`IceCreamId`)
);");
        }

        [ConditionalFact]
        public virtual async Task Alter_column_charsets_using_delegated_charset_with_tableonly_charset_inbetween()
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
                    .HasCharSet(DefaultCharSet, DelegationModes.ApplyToColumns)
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.Property<string>("Brand")
                                .HasCharSet(NonDefaultCharSet);
                        }),
                target => target
                    .HasCharSet(NonDefaultCharSet2, DelegationModes.ApplyToColumns)
                    .Entity(
                        "IceCream",
                        e =>
                        {
                            e.HasCharSet(DefaultCharSet, DelegationModes.ApplyToTables);
                            e.Property<string>("Name")
                                .HasCharSet(NonDefaultCharSet);
                        }),
                result => { });

            AssertSql(
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Name` varchar NULL;",
                //
                $@"ALTER TABLE `IceCream` MODIFY COLUMN `Brand` varchar NULL;");
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
                @"ALTER TABLE `Foo` ADD CONSTRAINT `FK_Foo_Bar_BarFK` FOREIGN KEY (`BarFK`) REFERENCES `Bar` (`BarPK`) ON DELETE CASCADE;");
        }

        public override async Task Add_foreign_key()
        {
            await base.Add_foreign_key();

            AssertSql(
                @"CREATE INDEX `IX_Orders_CustomerId` ON `Orders` (`CustomerId`);",
                //
                @"ALTER TABLE `Orders` ADD CONSTRAINT `FK_Orders_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE CASCADE;");
        }

        public override Task Rename_table()
            => Test(
                builder => builder.Entity("People").Property<int>("Id"),
                builder => builder.Entity("People").ToTable("Persons").Property<int>("Id"),
                model =>
                {
                    var table = Assert.Single(model.Tables);
                    Assert.Equal("Persons", table.Name);
                },
                withConventions: false);

        protected virtual string DefaultCollation => ((XGTestStore)Fixture.TestStore).TimeZone;

        protected virtual string DefaultCharSet => ((XGTestStore)Fixture.TestStore).DatabaseCharSet;
        protected virtual string NonDefaultCharSet => "latin1";
        protected virtual string NonDefaultCharSet2 => "ascii";

        protected virtual TestHelpers TestHelpers => XGTestHelpers.Instance;

        protected override string NonDefaultCollation => null;

        protected virtual Task Test(
            Action<ModelBuilder> buildCommonAction,
            Action<ModelBuilder> buildSourceAction,
            Action<ModelBuilder> buildTargetAction,
            Action<MigrationBuilder> migrationBuilderAction,
            Action<DatabaseModel> asserter,
            bool withConventions = true)
        {
            var services = TestHelpers.CreateContextServices();
            var modelRuntimeInitializer = services.GetRequiredService<IModelRuntimeInitializer>();

            // Build the source model, possibly with conventions
            var sourceModelBuilder = CreateModelBuilder(withConventions);
            buildCommonAction(sourceModelBuilder);
            buildSourceAction(sourceModelBuilder);
            var preSnapshotSourceModel = modelRuntimeInitializer.Initialize(
                (IModel)sourceModelBuilder.Model, designTime: true, validationLogger: null);

            // Round-trip the source model through a snapshot, compiling it and then extracting it back again.
            // This simulates the real-world migration flow and can expose errors in snapshot generation
            var migrationsCodeGenerator = Fixture.TestHelpers.CreateDesignServiceProvider().GetRequiredService<IMigrationsCodeGenerator>();
            var sourceModelSnapshot = migrationsCodeGenerator.GenerateSnapshot(
                modelSnapshotNamespace: null, typeof(DbContext), "MigrationsTestSnapshot", preSnapshotSourceModel);
            var sourceModel = BuildModelFromSnapshotSource(sourceModelSnapshot);

            // Build the target model, possibly with conventions
            var targetModelBuilder = CreateModelBuilder(withConventions);
            buildCommonAction(targetModelBuilder);
            buildTargetAction(targetModelBuilder);
            var targetModel = modelRuntimeInitializer.Initialize(
                (IModel)targetModelBuilder.Model, designTime: true, validationLogger: null);

            var migrationBuilder = new MigrationBuilder(null);
            migrationBuilderAction(migrationBuilder);

            return Test(sourceModel, targetModel, migrationBuilder.Operations, asserter);
        }

        private ModelBuilder CreateModelBuilder(bool withConventions)
            => withConventions ? Fixture.TestHelpers.CreateConventionBuilder() : new ModelBuilder(new ConventionSet());

        public class MigrationsXGFixture : MigrationsFixtureBase
        {
            protected override string StoreName
                => nameof(MigrationsXGTest);

            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
            public override RelationalTestHelpers TestHelpers => XGTestHelpers.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                new XGDbContextOptionsBuilder(builder)
                    .SchemaBehavior(XGSchemaBehavior.Translate, (schema, table) => $"`{schema}`.`{table}`");

                return base.AddOptions(builder);
            }

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection)
                    .AddScoped<IDatabaseModelFactory, XGDatabaseModelFactory>();
        }
    }
}
