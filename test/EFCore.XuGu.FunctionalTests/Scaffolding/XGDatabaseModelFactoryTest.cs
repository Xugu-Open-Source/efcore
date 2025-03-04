using System;
using System.Collections.Generic;
using System.Linq;
using EntityFrameworkCore.XuGu.Scaffolding.Internal;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.EntityFrameworkCore.Internal;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using EntityFrameworkCore.XuGu.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using EntityFrameworkCore.XuGu.Infrastructure;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Metadata.Internal;
using EntityFrameworkCore.XuGu.Tests;
using EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;

// ReSharper disable InconsistentNaming
namespace EntityFrameworkCore.XuGu.FunctionalTests.Scaffolding
{
    public class XGDatabaseModelFactoryTest : IClassFixture<XGDatabaseModelFactoryTest.XGDatabaseModelFixture>
    {
        protected XGDatabaseModelFixture Fixture { get; }

        public XGDatabaseModelFactoryTest(XGDatabaseModelFixture fixture)
        {
            Fixture = fixture;
            Fixture.ListLoggerFactory.Clear();
        }

        protected readonly List<(LogLevel Level, EventId Id, string Message)> Log = new List<(LogLevel Level, EventId Id, string Message)>();

        private void Test(string createSql, IEnumerable<string> tables, IEnumerable<string> schemas, Action<DatabaseModel> asserter, string cleanupSql)
        {
            try
            {
                Fixture.TestStore.ExecuteNonQuery(createSql);

                var databaseModelFactory = new XGDatabaseModelFactory(
                    new DiagnosticsLogger<DbLoggerCategory.Scaffolding>(
                        Fixture.ListLoggerFactory,
                        new LoggingOptions(),
                        new DiagnosticListener("Fake"),
                        new XGLoggingDefinitions(),
                        new NullDbContextLogger()),
                    Fixture.ServiceProvider.GetService<IRelationalTypeMappingSource>(),
                    Fixture.ServiceProvider.GetService<IXGOptions>());

                var databaseModel = databaseModelFactory.Create(Fixture.TestStore.ConnectionString,
                    new DatabaseModelFactoryOptions(tables, schemas));
                Assert.NotNull(databaseModel);
                asserter(databaseModel);
            }
            finally
            {
                if (!string.IsNullOrEmpty(cleanupSql))
                {
                    Fixture.TestStore.ExecuteNonQuery(cleanupSql);
                }
            }
        }

        #region FilteringSchemaTable

        [Fact]
        public void Filter_tables()
        {
            Test(
                @"
CREATE TABLE Everest ( id int );
CREATE TABLE Denali ( id int );",
                new[] { "Everest" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("Everest".ToUpper(), table.Name);
                    },
                @"
DROP TABLE IF EXISTS Everest;
DROP TABLE IF EXISTS Denali;");
        }

        [Fact]
        public void Filter_tables_is_case_insensitive()
        {
            Test(
                @"
CREATE TABLE Everest ( id int );
CREATE TABLE Denali ( id int );",
                new[] { "eVeReSt" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("Everest".ToUpper(), table.Name);
                    },
                @"
DROP TABLE IF EXISTS Everest;
DROP TABLE IF EXISTS Denali;");
        }

        #endregion

        #region Table

        [Fact]
        public void Create_tables()
        {
            Test(
                @"
CREATE TABLE Everest ( id int );
CREATE TABLE Denali ( id int );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        Assert.Collection(
                            dbModel.Tables.OrderBy(t => t.Name),
                            d => Assert.Equal("Denali".ToUpper(), d.Name),
                            e => Assert.Equal("Everest".ToUpper(), e.Name));
                    },
                @"
DROP TABLE IF EXISTS Everest;
DROP TABLE IF EXISTS Denali;");
        }

        [Fact]
        public void Create_columns()
        {
            Test(
                @"
CREATE TABLE MountainsColumns (
    Id integer primary key,
    Name VARCHAR NOT NULL
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = dbModel.Tables.Single();

                        Assert.Equal(2, table.Columns.Count);
                        Assert.All(
                            table.Columns, c => { Assert.Equal("MountainsColumns".ToUpper(), c.Table.Name); });

                        Assert.Single(table.Columns.Where(c => c.Name == "ID"));
                        Assert.Single(table.Columns.Where(c => c.Name == "NAME"));
                    },
                @"DROP TABLE IF EXISTS MountainsColumns;");
        }

        [Fact]
        public void Create_primary_key()
        {
            Test(
                @"CREATE TABLE Place ( Id int PRIMARY KEY );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var pk = dbModel.Tables.Single().PrimaryKey;

                        Assert.Equal("Place".ToUpper(), pk.Table.Name);
                        Assert.Equal(new List<string> { "ID" }, pk.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE IF EXISTS Place;");
        }

        [Fact]
        public void Create_unique_constraints()
        {
            Test(
                @"
CREATE TABLE Place (
    Id int PRIMARY KEY,
    Name int UNIQUE,
    Location int
);

CREATE INDEX IX_Location_Name ON Place (Location, Name);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("Place".ToUpper(), uniqueConstraint.Table.Name);
                        Assert.Equal(new List<string> { "Name".ToUpper() }, uniqueConstraint.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE IF EXISTS Place;");
        }

        [Fact]
        public void Create_indexes()
        {
            Test(
                @"
CREATE TABLE IndexTable (
    Id int,
    Name int,
    IndexProperty int
);

CREATE INDEX IX_NAME on IndexTable ( Name );
CREATE INDEX IX_INDEX on IndexTable ( IndexProperty );",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = dbModel.Tables.Single();

                        Assert.Equal(2, table.Indexes.Count);
                        Assert.All(
                            table.Indexes, c => { Assert.Equal("IndexTable".ToUpper(), c.Table.Name); });

                        Assert.Single(table.Indexes.Where(c => c.Name == "IX_NAME"));
                        Assert.Single(table.Indexes.Where(c => c.Name == "IX_INDEX"));
                    },
                @"DROP TABLE IF EXISTS IndexTable;");
        }

        [Fact]
        public void Create_foreign_keys()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE FirstDependent (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE
);

CREATE TABLE SecondDependent (
    Id int PRIMARY KEY,
    FOREIGN KEY (Id) REFERENCES PrincipalTable(Id) ON DELETE NO ACTION
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var firstFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "FirstDependent".ToUpper()).ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("FirstDependent".ToUpper(), firstFk.Table.Name);
                        Assert.Equal("PrincipalTable".ToUpper(), firstFk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId".ToUpper() }, firstFk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "ID" }, firstFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, firstFk.OnDelete);

                        var secondFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "SecondDependent".ToUpper()).ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("SecondDependent".ToUpper(), secondFk.Table.Name);
                        Assert.Equal("PrincipalTable".ToUpper(), secondFk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "Id".ToUpper() }, secondFk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "Id".ToUpper() }, secondFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.NoAction, secondFk.OnDelete);
                    },
                @"
DROP TABLE IF EXISTS SecondDependent;
DROP TABLE IF EXISTS FirstDependent;
DROP TABLE IF EXISTS PrincipalTable;");
        }

        [Fact]
        public void Create_dependent_table_with_missing_principal_table_creates_model_without_it()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id)
);",
                new[] { "DEPENDENTTABLE" },
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables);
                    Assert.Equal("DEPENDENTTABLE", table.Name);
                },
                @"
DROP TABLE IF EXISTS DependentTable;
DROP TABLE IF EXISTS PrincipalTable;");
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.DefaultExpression), nameof(ServerVersionSupport.AlternativeDefaultExpression))]
        public void Create_guid_columns()
        {
            Test(
                @"
CREATE TABLE `GuidTable`  (
  `GuidTableId` guid NOT NULL DEFAULT UUID() PRIMARY KEY,
  `DefaultUuid` guid NOT NULL DEFAULT UUID()
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables.Where(t => t.Name == "GuidTable"));
                        var guidTableIdColumn = Assert.Single(table.Columns.Where(c => c.Name == "GuidTableId"));
                        var defaultUuidColumn = Assert.Single(table.Columns.Where(c => c.Name == "DefaultUuid"));

                        Assert.Equal(ValueGenerated.OnAdd, guidTableIdColumn.ValueGenerated);
                        Assert.Equal("\"UUID\"()", guidTableIdColumn.DefaultValueSql);

                        //Assert.Null(defaultUuidColumn.ValueGenerated);
                        Assert.Equal(ValueGenerated.OnAdd, defaultUuidColumn.ValueGenerated);
                        Assert.Equal("\"UUID\"()", defaultUuidColumn.DefaultValueSql);
                    },
                @"
DROP TABLE IF EXISTS `GuidTable`;");
        }

        [ConditionalFact]
        public void Create_default_value_column()
        {
            Test(
                $@"
CREATE TABLE `DefaultValueTable` (
    `DefaultValueInt` int not null default '42',
    `DefaultValueString` varchar(255) not null default 'Answer to everything',
    `DefaultValueFunction` datetime not null default CURRENT_TIMESTAMP()
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables.Where(t => t.Name == "DefaultValueTable"));
                        var defaultValueIntColumn = Assert.Single(table.Columns.Where(c => c.Name == "DefaultValueInt"));
                        var defaultValueStringColumn = Assert.Single(table.Columns.Where(c => c.Name == "DefaultValueString"));
                        var defaultValueFunctionColumn = Assert.Single(table.Columns.Where(c => c.Name == "DefaultValueFunction"));

                        Assert.Equal("'42'", defaultValueIntColumn.DefaultValueSql);
                        Assert.Equal("'Answer to everything'", defaultValueStringColumn.DefaultValueSql);
                        Assert.Contains("current_timestamp", defaultValueFunctionColumn.DefaultValueSql, StringComparison.OrdinalIgnoreCase);
                    },
                @"
DROP TABLE IF EXISTS `DefaultValueTable`;");
        }

        [ConditionalFact]
        //[SupportedServerVersionCondition(nameof(ServerVersionSupport.AlternativeDefaultExpression))]
        public void Create_default_value_column_simple_function_expression()
        {
            Test(
                $@"
CREATE TABLE `DefaultValueSimpleExpressionTable` (
    `DefaultValueSimpleFunctionExpression` double not null default rand()
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables.Where(t => t.Name == "DefaultValueSimpleExpressionTable"));
                    var defaultValueSimpleFunctionExpressionColumn = table.Columns.SingleOrDefault(c => c.Name == "DefaultValueSimpleFunctionExpression");

                    Assert.Equal("\"RAND\"()", defaultValueSimpleFunctionExpressionColumn.DefaultValueSql);
                },
                @"
DROP TABLE IF EXISTS `DefaultValueSimpleExpressionTable`;");
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.DefaultExpression), nameof(ServerVersionSupport.AlternativeDefaultExpression))]
        public void Create_default_value_expression_column()
        {
            Test(
                $@"
CREATE TABLE `DefaultValueExpressionTable` (
    `DefaultValueExpression` varchar(255) not null default (CONCAT(CAST(42 as char), ' is the answer to everything')),
    `DefaultValueExpressionInt` int not null default (42)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables.Where(t => t.Name == "DefaultValueExpressionTable"));
                        var defaultValueExpressionColumn = Assert.Single(table.Columns.Where(c => c.Name == "DefaultValueExpression"));
                        var defaultValueExpressionIntColumn = Assert.Single(table.Columns.Where(c => c.Name == "DefaultValueExpressionInt"));

                        Assert.Contains("\"CONCAT\"(CAST(42 as char", defaultValueExpressionColumn.DefaultValueSql, StringComparison.OrdinalIgnoreCase);
                        Assert.Contains(" is the answer to everything", defaultValueExpressionColumn.DefaultValueSql, StringComparison.OrdinalIgnoreCase);

                        Assert.Equal("42", defaultValueExpressionIntColumn.DefaultValueSql);
                    },
                @"
DROP TABLE IF EXISTS `DefaultValueExpressionTable`;");
        }

        [ConditionalFact]
        public void Create_view_with_column_type_containing_comment_after_cast()
        {
            Test(
                @"
CREATE TABLE `item_data` (
    `id` INT NOT NULL,
    `text_datetime` VARCHAR NOT NULL,
    `real_datetime_1` DATETIME NOT NULL,
    `real_datetime_2` DATETIME NOT NULL,
    PRIMARY KEY (`id`)
);

CREATE VIEW `item_data_view` AS
SELECT `item`.`id`,
       CAST(`item`.`text_datetime` AS DATETIME) AS `text_datetime_converted`,
	   CAST(`item`.`real_datetime_1` AS DATETIME) AS `real_datetime_1_converted`,
       `item`.`real_datetime_2` AS `real_datetime_2_original`
FROM `item_data` `item`;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = Assert.Single(dbModel.Tables.Where(t => t.Name == "item_data_view"));
                    var textDateTimeConvertedColumn = Assert.Single(table.Columns.Where(c => c.Name == "text_datetime_converted"));
                    var realDateTime1ConvertedColumn = Assert.Single(table.Columns.Where(c => c.Name == "real_datetime_1_converted"));
                    var realDateTime2OriginalColumn = Assert.Single(table.Columns.Where(c => c.Name == "real_datetime_2_original"));

                    Assert.Equal("datetime".ToUpper(), textDateTimeConvertedColumn.StoreType);
                    Assert.Equal("datetime".ToUpper(), realDateTime1ConvertedColumn.StoreType);
                    Assert.Equal("datetime".ToUpper(), realDateTime2OriginalColumn.StoreType);
                },
                @"
DROP VIEW IF EXISTS `item_data_view`;
DROP TABLE IF EXISTS `item_data`;");
        }

        #endregion

        #region ColumnFacets

        [Fact]
        public void Column_storetype_is_set()
        {
            Test(
                @"
CREATE TABLE StoreType (
    IntegerProperty int,
    RealProperty real,
    TextProperty clob,
    BlobProperty blob,
    GeometryProperty varchar,
    PointProperty point,
    RandomProperty int
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.Equal("INTEGER", columns.Single(c => c.Name == "INTEGERPROPERTY").StoreType);
                        Assert.Equal("FLOAT", columns.Single(c => c.Name == "REALPROPERTY").StoreType);
                        Assert.Equal("CLOB", columns.Single(c => c.Name == "TEXTPROPERTY").StoreType);
                        Assert.Equal("BLOB", columns.Single(c => c.Name == "BLOBPROPERTY").StoreType);
                        //Assert.Equal("varchar", columns.Single(c => c.Name == "GEOMETRYPROPERTY").StoreType);
                        Assert.Equal("POINT", columns.Single(c => c.Name == "POINTPROPERTY").StoreType);
                        Assert.Equal("INTEGER", columns.Single(c => c.Name == "RANDOMPROPERTY").StoreType);
                    },
                @"DROP TABLE IF EXISTS StoreType;");
        }

        [Fact]
        public void Column_nullability_is_set()
        {
            Test(
                @"
CREATE TABLE Nullable (
    Id int,
    NullableInt int NULL,
    NonNullString VARCHAR NOT NULL
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.True(columns.Single(c => c.Name == "NullableInt".ToUpper()).IsNullable);
                        Assert.False(columns.Single(c => c.Name == "NonNullString".ToUpper()).IsNullable);
                    },
                @"DROP TABLE IF EXISTS Nullable;");
        }

        [Fact]
        public void Column_default_value_is_set()
        {
            Test(
                @"
CREATE TABLE DefaultValue (
    Id int,
    SomeText varchar DEFAULT 'Something',
    RealColumn real DEFAULT 3.14,
    Created datetime DEFAULT '2015-10-20 11:00:00'
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.Equal("'Something'", columns.Single(c => c.Name == "SOMETEXT").DefaultValueSql);
                        Assert.Equal("3.14", columns.Single(c => c.Name == "REALCOLUMN").DefaultValueSql);
                        Assert.Equal("'2015-10-20 11:00:00'", columns.Single(c => c.Name == "CREATED").DefaultValueSql);
                    },
                @"DROP TABLE IF EXISTS DefaultValue;");
        }

        [Theory]
        [InlineData("DOUBLE NOT NULL DEFAULT 0")]
        [InlineData("FLOAT NOT NULL DEFAULT 0")]
        [InlineData("INT NOT NULL DEFAULT 0")]
        [InlineData("INTEGER NOT NULL DEFAULT 0")]
        [InlineData("REAL NOT NULL DEFAULT 0")]
        //[InlineData("NULL DEFAULT NULL")]
        //[InlineData("NOT NULL DEFAULT NULL")]
        public void Column_default_value_is_ignored_when_clr_default(string columnSql)
        {
            Test(
                $"CREATE TABLE DefaultValueClr (IgnoredDefault {columnSql})",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var column = Assert.Single(Assert.Single(dbModel.Tables).Columns);
                    Assert.Equal("0",column.DefaultValueSql);
                },
                "DROP TABLE IF EXISTS DefaultValueClr");
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.GeneratedColumns))]
        public void Computed_value_virtual()
            => Test(@"
CREATE TABLE `ComputedValues` (
    `Id` int,
    `A` int NOT NULL,
    `B` int NOT NULL,
    `SumOfAAndB` int
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    var column = columns.Single(c => c.Name == "SumOfAAndB");
                    Assert.Null(column.DefaultValueSql);
                    Assert.Null(column.ComputedColumnSql);
                    Assert.Null(column.IsStored);
                },
                @"DROP TABLE IF EXISTS `ComputedValues`");

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.GeneratedColumns))]
        public void Computed_value_stored()
            => Test(@"
CREATE TABLE `ComputedValues` (
    `Id` int,
    `A` int NOT NULL,
    `B` int NOT NULL,
    `SumOfAAndB` int
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    var column = columns.Single(c => c.Name == "SumOfAAndB");
                    Assert.Null(column.DefaultValueSql);
                    Assert.Null(column.IsStored);
                },
                @"DROP TABLE IF EXISTS `ComputedValues`");

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.GeneratedColumns))]
        public void Computed_value_virtual_using_constant_string()
            => Test(@"
CREATE TABLE `Users` (
  `id` int identity NOT NULL,
  `FirstName` varchar(150) NOT NULL,
  `LastName` varchar(150) NOT NULL,
  `FullName` varchar(301),
  PRIMARY KEY (`id`)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    var column = columns.Single(c => c.Name == "FullName");
                    Assert.Null(column.ComputedColumnSql);
                    Assert.Null(column.IsStored);
                },
                @"DROP TABLE IF EXISTS `Users`");

        [ConditionalFact]
        public void Default_value_curdate()
        {
            Test(
                @"
CREATE TABLE `IceCreams` (
    `IceCreamId` int,
    `Name` varchar(255) NOT NULL,
    `BestServedBefore` datetime DEFAULT CURRENT_DATETIME()
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var table = dbModel.Tables.FirstOrDefault();
                    var columns = table.Columns;

                    var column = columns.Single(c => c.Name == "BestServedBefore");
                    Assert.Equal("\"CURRENT_DATETIME\"()", column.DefaultValueSql);
                },
                @"DROP TABLE IF EXISTS `IceCreams`");
        }

        #endregion

        #region PrimaryKeyFacets

        [Fact]
        public void Create_composite_primary_key()
        {
            Test(
                @"
CREATE TABLE CompositePrimaryKey (
    Id1 int,
    Id2 text,
    PRIMARY KEY ( Id2, Id1 )
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var pk = dbModel.Tables.Single().PrimaryKey;

                        Assert.Equal("COMPOSITEPRIMARYKEY", pk.Table.Name);
                        Assert.Equal(new List<string> { "ID2", "ID1" }, pk.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE IF EXISTS CompositePrimaryKey;");
        }

        [Fact]
        public void Create_primary_key_when_integer_primary_key_alised_to_rowid()
        {
            Test(
                @"
CREATE TABLE RowidPrimaryKey (
    Id integer PRIMARY KEY
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var pk = dbModel.Tables.Single().PrimaryKey;

                        Assert.Equal("RowidPrimaryKey".ToUpper(), pk.Table.Name);
                        Assert.Equal(new List<string> { "ID" }, pk.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE IF EXISTS RowidPrimaryKey;");
        }

        [Fact]
        public void Set_name_for_primary_key()
        {
            Test(
                @"
CREATE TABLE PrimaryKeyName (
    Id int,
    CONSTRAINT PK PRIMARY KEY (Id)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var pk = dbModel.Tables.Single().PrimaryKey;

                    Assert.Equal("PrimaryKeyName".ToUpper(), pk.Table.Name);
                    Assert.StartsWith("PK", pk.Name);
                    Assert.Equal(new List<string> { "ID" }, pk.Columns.Select(ic => ic.Name).ToList());
                },
                @"DROP TABLE IF EXISTS PrimaryKeyName;");
        }

        [Fact]
        public void Prefix_lengths_for_primary_key()
        {
            Test(
                @"
CREATE TABLE `IceCreams` (
    `Brand` varchar NOT NULL,
    `Name` varchar(128) NOT NULL,
    PRIMARY KEY (`Name`, `Brand`)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var pk = dbModel.Tables.Single().PrimaryKey;

                    Assert.Equal("IceCreams", pk.Table.Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(2, pk.Columns.Count);
                    Assert.Equal("Name", pk.Columns[0].Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal("Brand", pk.Columns[1].Name, StringComparer.OrdinalIgnoreCase);
                },
                @"DROP TABLE IF EXISTS `IceCreams`;");
        }

        [Fact]
        public void Column_srid_value_is_set()
        {
            Test(
                @"
CREATE TABLE `IceCreamShop` (
    `IceCreamShopId` int NOT NULL,
    `Location` VARCHAR NOT NULL,
    PRIMARY KEY (`IceCreamShopId`)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var columns = dbModel.Tables.Single().Columns;

                    if (AppConfig.ServerVersion.Supports.SpatialReferenceSystemRestrictedColumns)
                    {
                        Assert.Equal(
                            0, columns.Single(c => c.Name == "Location")
                                .FindAnnotation(XGAnnotationNames.SpatialReferenceSystemId)
                                ?.Value);
                    }
                    else
                    {
                        Assert.Null(
                            columns.Single(c => c.Name == "Location")
                                .FindAnnotation(XGAnnotationNames.SpatialReferenceSystemId)
                                ?.Value);
                    }
                },
                @"DROP TABLE IF EXISTS `IceCreamShop`;");
        }

        #endregion

        #region UniqueConstraintFacets

        [Fact]
        public void Create_composite_unique_constraint()
        {
            Test(
                @"
CREATE TABLE CompositeUniqueConstraint (
    Id1 int,
    Id2 text,
    UNIQUE ( Id2, Id1 )
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var constraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("CompositeUniqueConstraint", constraint.Table.Name, StringComparer.OrdinalIgnoreCase);
                        Assert.Equal(new List<string> { "ID2", "ID1" }, constraint.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE IF EXISTS CompositeUniqueConstraint;");
        }

        [Fact]
        public void Set_name_for_unique_constraint()
        {
            Test(
                @"
CREATE TABLE UniqueConstraintName (
    Id int,
    CONSTRAINT UK UNIQUE (Id)
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var constraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("UniqueConstraintName".ToUpper(), constraint.Table.Name);
                        Assert.StartsWith("UK", constraint.Name);
                        Assert.Equal(new List<string> { "ID" }, constraint.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE IF EXISTS UniqueConstraintName;");
        }

        #endregion

        #region IndexFacets

        [Fact]
        public void Create_composite_index()
        {
            Test(
                @"
CREATE TABLE CompositeIndex (
    Id1 int,
    Id2 text
);

CREATE INDEX IX_COMPOSITE on CompositeIndex (Id2, Id1);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var index = Assert.Single(dbModel.Tables.Single().Indexes);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("COMPOSITEINDEX", index.Table.Name);
                        Assert.Equal("IX_COMPOSITE", index.Name);
                        Assert.Equal(new List<string> { "ID2", "ID1" }, index.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE IF EXISTS CompositeIndex;");
        }

        [Fact]
        public void Set_unique_for_unique_index()
        {
            Test(
                @"
CREATE TABLE UniqueIndex (
    Id1 int,
    Id2 VARCHAR
);

CREATE UNIQUE INDEX IX_UNIQUE on UniqueIndex (Id2);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var index = Assert.Single(dbModel.Tables.Single().Indexes);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("UniqueIndex".ToUpper(), index.Table.Name);
                        Assert.Equal("IX_UNIQUE", index.Name);
                        Assert.True(index.IsUnique);
                        Assert.Equal(new List<string> { "ID2" }, index.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE IF EXISTS UniqueIndex;");
        }

        [Fact]
        public void Prefix_lengths_for_index()
        {
            Test(
                @"
CREATE TABLE `IceCreams` (
    `IceCreamId` int NOT NULL,
    `Brand` CHAR NOT NULL,
    `Name` varchar(128) NOT NULL,
    PRIMARY KEY (`IceCreamId`)
);

CREATE INDEX `IX_IceCreams_Brand_Name` ON `IceCreams` (`Name`, `Brand`);
",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var index = Assert.Single(dbModel.Tables.Single().Indexes);

                    Assert.Equal("IceCreams", index.Table.Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal("IX_IceCreams_Brand_Name", index.Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(2, index.Columns.Count);
                    Assert.Equal("Name", index.Columns[0].Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal("Brand", index.Columns[1].Name, StringComparer.OrdinalIgnoreCase);
                    //Assert.Equal(new [] { 0, 20 }, index.FindAnnotation(XGAnnotationNames.IndexPrefixLength)?.Value);
                },
                @"DROP TABLE IF EXISTS `IceCreams`;");
        }

        [Fact]
        public void Prefix_lengths_for_multiple_indexes_same_colums()
        {
            Test(
                @"
CREATE TABLE `IceCreams` (
    `IceCreamId` int NOT NULL,
    `Brand` varchar(128) NOT NULL,
    `Name` varchar(128) NOT NULL,
    PRIMARY KEY (`IceCreamId`)
);

CREATE INDEX `IX_IceCreams_Brand_Name_1` ON `IceCreams` (`Name`, `Brand`);
CREATE UNIQUE INDEX `IX_IceCreams_Brand_Name_2` ON `IceCreams` (`Brand`, `Name`);
",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var index = Assert.Single(dbModel.Tables.Single().Indexes);

                    Assert.Equal("IceCreams", index.Table.Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal("IX_IceCreams_Brand_Name_1", index.Name, StringComparer.OrdinalIgnoreCase);
                    Assert.True(index.IsUnique);
                    Assert.Equal(2, index.Columns.Count);
                    Assert.Equal("Name", index.Columns[0].Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal("Brand", index.Columns[1].Name, StringComparer.OrdinalIgnoreCase);
                },
                @"DROP TABLE IF EXISTS `IceCreams`;");
        }

        [Fact]
        public void Prefix_lengths_for_multiple_indexes_same_columns_without_prefix_lengths()
        {
            Test(
                @"
CREATE TABLE `IceCreams` (
    `IceCreamId` int NOT NULL,
    `Brand` varchar(128) NOT NULL,
    `Name` varchar(128) NOT NULL,
    PRIMARY KEY (`IceCreamId`)
);

CREATE INDEX `IX_IceCreams_Brand_Name_1` ON `IceCreams` (`Name`, `Brand`);
CREATE UNIQUE INDEX `IX_IceCreams_Brand_Name_2` ON `IceCreams` (`Brand`, `Name`);
",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var index = Assert.Single(dbModel.Tables.Single().Indexes);

                    Assert.Equal("IceCreams", index.Table.Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal("IX_IceCreams_Brand_Name_1", index.Name, StringComparer.OrdinalIgnoreCase);
                    Assert.True(index.IsUnique);
                    Assert.Equal(2, index.Columns.Count);
                    Assert.Equal("Name", index.Columns[0].Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal("Brand", index.Columns[1].Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Null(index[XGAnnotationNames.IndexPrefixLength]);
                },
                @"DROP TABLE IF EXISTS `IceCreams`;");
        }

        [Fact]
        public void Set_fulltext_for_fulltext_index()
        {
            Test(
                @"
CREATE TABLE `IceCreams` (
    `IceCreamId` int NOT NULL,
    `Name` varchar(255) NOT NULL,
    PRIMARY KEY (`IceCreamId`)
);

CREATE INDEX `IX_IceCreams_Name` ON `IceCreams` (`Name`);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var index = Assert.Single(dbModel.Tables.Single().Indexes);

                    Assert.Equal("IceCreams", index.Table.Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(1, index.Columns.Count);
                    Assert.Equal("Name", index.Columns[0].Name, StringComparer.OrdinalIgnoreCase);
                },
                @"DROP TABLE IF EXISTS `IceCreams`;");
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.FullTextParser))]
        public void Set_fulltextparser_for_fulltext_index_with_parser()
        {
            Test(
                @"
CREATE TABLE `IceCreams` (
    `IceCreamId` int NOT NULL,
    `Name` varchar(255) NOT NULL,
    PRIMARY KEY (`IceCreamId`)
);

CREATE INDEX `IX_IceCreams_Name` ON `IceCreams` (`Name`) /*!50703 WITH PARSER `ngram` */;",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var index = Assert.Single(dbModel.Tables.Single().Indexes);

                    Assert.Equal("IceCreams", index.Table.Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(1, index.Columns.Count);
                    Assert.Equal("Name", index.Columns[0].Name, StringComparer.OrdinalIgnoreCase);
                },
                @"DROP TABLE IF EXISTS `IceCreams`;");
        }

        [ConditionalFact]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.SpatialIndexes))]
        public void Set_spatial_for_spatial_index()
        {
            Test(
                @"
CREATE TABLE `IceCreamShop` (
    `IceCreamShopId` int NOT NULL,
    `Location` varchar NOT NULL,
    PRIMARY KEY (`IceCreamShopId`)
);

CREATE INDEX `IX_IceCreams_Location` ON `IceCreamShop` (`Location`);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var index = Assert.Single(dbModel.Tables.Single().Indexes);

                    Assert.Equal("IceCreamShop", index.Table.Name, StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(1, index.Columns.Count);
                    Assert.Equal("Location", index.Columns[0].Name, StringComparer.OrdinalIgnoreCase);
                },
                @"DROP TABLE IF EXISTS `IceCreamShop`;");
        }

        #endregion

        #region ForeignKeyFacets

        [Fact]
        public void Create_composite_foreign_key()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id1 int,
    Id2 int,
    PRIMARY KEY (Id1, Id2)
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId1 int,
    ForeignKeyId2 int,
    FOREIGN KEY (ForeignKeyId1, ForeignKeyId2) REFERENCES PrincipalTable(Id1, Id2) ON DELETE CASCADE
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable".ToUpper()).ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("DependentTable".ToUpper(), fk.Table.Name);
                        Assert.Equal("PrincipalTable".ToUpper(), fk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId1".ToUpper(), "ForeignKeyId2".ToUpper() }, fk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "ID1", "ID2" }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                    },
                @"
DROP TABLE IF EXISTS DependentTable;
DROP TABLE IF EXISTS PrincipalTable;");
        }

        [Fact]
        public void Create_multiple_foreign_key_in_same_table()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE AnotherPrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId1 int,
    ForeignKeyId2 int,
    FOREIGN KEY (ForeignKeyId1) REFERENCES PrincipalTable(Id) ON DELETE CASCADE,
    FOREIGN KEY (ForeignKeyId2) REFERENCES AnotherPrincipalTable(Id) ON DELETE CASCADE
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var foreignKeys = dbModel.Tables.Single(t => t.Name == "DependentTable".ToUpper()).ForeignKeys;

                        Assert.Equal(2, foreignKeys.Count);

                        var principalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "PrincipalTable".ToUpper()));

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("DependentTable".ToUpper(), principalFk.Table.Name);
                        Assert.Equal("PrincipalTable".ToUpper(), principalFk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId1".ToUpper() }, principalFk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "ID" }, principalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, principalFk.OnDelete);

                        var anotherPrincipalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "AnotherPrincipalTable".ToUpper()));

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("DependentTable".ToUpper(), anotherPrincipalFk.Table.Name);
                        Assert.Equal("AnotherPrincipalTable".ToUpper(), anotherPrincipalFk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId2".ToUpper() }, anotherPrincipalFk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "ID" }, anotherPrincipalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, anotherPrincipalFk.OnDelete);
                    },
                @"
DROP TABLE IF EXISTS DependentTable;
DROP TABLE IF EXISTS AnotherPrincipalTable;
DROP TABLE IF EXISTS PrincipalTable;");
        }

        [Fact]
        public void Create_foreign_key_referencing_unique_constraint()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id1 int,
    Id2 int UNIQUE
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id2) ON DELETE CASCADE
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable".ToUpper()).ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("DependentTable".ToUpper(), fk.Table.Name);
                        Assert.Equal("PrincipalTable".ToUpper(), fk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId".ToUpper() }, fk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "ID2" }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                    },
                @"
DROP TABLE IF EXISTS DependentTable;
DROP TABLE IF EXISTS PrincipalTable;");
        }

        [Fact]
        public void Set_name_for_foreign_key()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    CONSTRAINT MYFK FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable".ToUpper()).ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("DependentTable".ToUpper(), fk.Table.Name);
                        Assert.Equal("PrincipalTable".ToUpper(), fk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId".ToUpper() }, fk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "ID" }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                        Assert.Equal("MYFK", fk.Name);
                    },
                @"
DROP TABLE IF EXISTS DependentTable;
DROP TABLE IF EXISTS PrincipalTable;");
        }

        [Fact]
        public void Set_referential_action_for_foreign_key()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE SET NULL
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DependentTable".ToUpper()).ForeignKeys);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("DependentTable".ToUpper(), fk.Table.Name);
                        Assert.Equal("PrincipalTable".ToUpper(), fk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId".ToUpper() }, fk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "ID" }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.SetNull, fk.OnDelete);
                    },
                @"
DROP TABLE IF EXISTS DependentTable;
DROP TABLE IF EXISTS PrincipalTable;");
        }

        [Fact]
        public void Ensure_constraints_scaffold_with_case_mismatch()
        {
            // The lower case table reference to a mixed cased table will only be accepted under certain conditions
            // (lower_case_table_names <> 0).
            Test(
                @"
CREATE TABLE `PrincipalTable` (
  `Id` INT NOT NULL,
  PRIMARY KEY (`Id`));

CREATE TABLE `DependentTable` (
  `Id` INT NOT NULL,
  `ForeignKeyId` INT NOT NULL,
  PRIMARY KEY (`Id`),
  CONSTRAINT `ForeignKey_Id`
    FOREIGN KEY (`ForeignKeyId`)
    REFERENCES `PrincipalTable` (`Id`));",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                {
                    var principal = dbModel.Tables.FirstOrDefault(t => string.Equals(t.Name, "PrincipalTable", StringComparison.OrdinalIgnoreCase));
                    var dependent = dbModel.Tables.FirstOrDefault(t => string.Equals(t.Name, "DependentTable", StringComparison.OrdinalIgnoreCase));

                    Assert.NotNull(principal);
                    Assert.NotNull(dependent);

                    Assert.Contains(dependent.ForeignKeys, t => t.PrincipalTable.Name == principal.Name);
                },
                @"
DROP TABLE IF EXISTS DependentTable;
DROP TABLE IF EXISTS PrincipalTable;"
            );
        }

        #endregion

        #region Warnings

        [Fact(Skip = "Issue #582")]
        public void Warn_for_schema_filtering()
        {
            Test(
                @"CREATE TABLE Everest ( id int );",
                Enumerable.Empty<string>(),
                new[] { "dbo" },
                dbModel =>
                    {
                        var (Level, Id, Message) = Assert.Single(Log.Where(t => t.Level == LogLevel.Warning));
                    },
                @"DROP TABLE IF EXISTS Everest;");
        }

        [Fact(Skip = "Issue #582")]
        public void Warn_missing_table()
        {
            Test(
                @"CREATE TABLE Blank ( Id int );",
                new[] { "MyTable" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        Assert.Empty(dbModel.Tables);

                        var (Level, Id, Message) = Assert.Single(Log.Where(t => t.Level == LogLevel.Warning));
                    },
                @"DROP TABLE IF EXISTS Blank;");
        }

        [Fact(Skip = "Issue #582")]
        public void Warn_missing_principal_table_for_foreign_key()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    CONSTRAINT MYFK FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(Id) ON DELETE CASCADE
);",
                new[] { "DependentTable" },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var (Level, Id, Message) = Assert.Single(Log.Where(t => t.Level == LogLevel.Warning));
                    },
                @"
DROP TABLE IF EXISTS DependentTable;
DROP TABLE IF EXISTS PrincipalTable;");
        }

        [Fact(Skip = "Issue #582")]
        public void Warn_missing_principal_column_for_foreign_key()
        {
            Test(
                @"
CREATE TABLE PrincipalTable (
    Id int PRIMARY KEY
);

CREATE TABLE DependentTable (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    CONSTRAINT MYFK FOREIGN KEY (ForeignKeyId) REFERENCES PrincipalTable(ImaginaryId) ON DELETE CASCADE
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var (Level, Id, Message) = Assert.Single(Log.Where(t => t.Level == LogLevel.Warning));
                    },
                @"
DROP TABLE IF EXISTS DependentTable;
DROP TABLE IF EXISTS PrincipalTable;");
        }

        #endregion

        public class XGDatabaseModelFixture : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = nameof(XGDatabaseModelFactoryTest);
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
            public new XGTestStore TestStore => (XGTestStore)base.TestStore;

            protected override bool ShouldLogCategory(string logCategory)
                => logCategory == DbLoggerCategory.Scaffolding.Name;
        }
    }
}
