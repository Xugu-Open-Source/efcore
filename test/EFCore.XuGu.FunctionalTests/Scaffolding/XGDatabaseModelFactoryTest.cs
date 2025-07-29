using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Scaffolding
{
    public class XGDatabaseModelFactoryTest : IClassFixture<XGDatabaseModelFactoryTest.XGDatabaseModelFixture>
    {
        protected XGDatabaseModelFixture Fixture { get; }

        public XGDatabaseModelFactoryTest(XGDatabaseModelFixture fixture) => Fixture = fixture;

        protected readonly List<(LogLevel Level, EventId Id, string Message)> Log = new List<(LogLevel Level, EventId Id, string Message)>();

        private void Test(string createSql, IEnumerable<string> tables, IEnumerable<string> schemas, Action<DatabaseModel> asserter, string cleanupSql)
        {
            Fixture.TestStore.ExecuteNonQuery(createSql);

            try
            {
                var databaseModelFactory = new XGDatabaseModelFactory(new LoggerFactory());

                var databaseModel = databaseModelFactory.Create(Fixture.TestStore.ConnectionString, tables, schemas);
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

        [Fact/*(Skip = "Issue #582")*/]
        public void Filter_tables()
        {
            Test(
                @"
CREATE TABLE Everest ( id int );
CREATE TABLE Denali ( id int );",
                new[] { "Everest".ToUpper() },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("Everest".ToUpper(), table.Name);
                    },
                @"
DROP TABLE Everest;
DROP TABLE Denali;");
        }

        [Fact/*(Skip = "Issue #582")*/]
        public void Filter_tables_is_case_insensitive()
        {
            Test(
                @"
CREATE TABLE Everest ( id int );
CREATE TABLE Denali ( id int );",
                new[] { "eVeReSt".ToUpper() },
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = Assert.Single(dbModel.Tables);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("Everest".ToUpper(), table.Name);
                    },
                @"
DROP TABLE Everest;
DROP TABLE Denali;");
        }

        #endregion

        #region Table

        [Fact/*(Skip = "Issue #582")*/]
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
DROP TABLE Everest;
DROP TABLE Denali;");
        }

        [Fact/*(Skip = "Issue #582")*/]
        public void Create_columns()
        {
            Test(
                @"
CREATE TABLE MountainsColumns (
    Id integer primary key,
    Name varchar NOT NULL
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var table = dbModel.Tables.Single();

                        Assert.Equal(2, table.Columns.Count);
                        Assert.All(
                            table.Columns, c => { Assert.Equal("MountainsColumns".ToUpper(), c.Table.Name); });

                        Assert.Single(table.Columns.Where(c => c.Name == "Id".ToUpper()));
                        Assert.Single(table.Columns.Where(c => c.Name == "Name".ToUpper()));
                    },
                @"DROP TABLE MountainsColumns;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal(new List<string> { "Id".ToUpper() }, pk.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE Place;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                @"DROP TABLE Place;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                @"DROP TABLE IndexTable;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal(new List<string> { "Id".ToUpper() }, firstFk.PrincipalColumns.Select(ic => ic.Name).ToList());
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
DROP TABLE SecondDependent;
DROP TABLE FirstDependent;
DROP TABLE PrincipalTable;");
        }

        #endregion

        #region ColumnFacets

        [Fact/*(Skip = "Issue #582")*/]
        public void Column_storetype_is_set()
        {
            Test(
                @"
CREATE TABLE StoreType (
    IntegerProperty integer,
    RealProperty real,
    TextProperty CLOB,
    BlobProperty blob,
    RandomProperty integer default rand()
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.Equal("integer".ToUpper(), columns.Single(c => c.Name == "IntegerProperty".ToUpper()).StoreType);
                        Assert.Equal("float".ToUpper(), columns.Single(c => c.Name == "RealProperty".ToUpper()).StoreType);
                        Assert.Equal("clob".ToUpper(), columns.Single(c => c.Name == "TextProperty".ToUpper()).StoreType);
                        Assert.Equal("blob".ToUpper(), columns.Single(c => c.Name == "BlobProperty".ToUpper()).StoreType);
                        Assert.Equal("integer".ToUpper(), columns.Single(c => c.Name == "RandomProperty".ToUpper()).StoreType);
                    },
                @"DROP TABLE StoreType;");
        }

        [Fact/*(Skip = "Issue #582")*/]
        public void Column_nullability_is_set()
        {
            Test(
                @"
CREATE TABLE Nullable (
    Id int,
    NullableInt int NULL,
    NonNullString varchar NOT NULL
);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var columns = dbModel.Tables.Single().Columns;

                        Assert.True(columns.Single(c => c.Name == "NullableInt".ToUpper()).IsNullable);
                        Assert.False(columns.Single(c => c.Name == "NonNullString".ToUpper()).IsNullable);
                    },
                @"DROP TABLE Nullable;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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

                        Assert.Equal("'\\'Something\\''", columns.Single(c => c.Name == "SomeText".ToUpper()).DefaultValueSql);
                        Assert.Equal("'3.14'", columns.Single(c => c.Name == "RealColumn".ToUpper()).DefaultValueSql);
                        Assert.Equal("'\\'2015-10-20 11:00:00\\''", columns.Single(c => c.Name == "Created".ToUpper()).DefaultValueSql);
                    },
                @"DROP TABLE DefaultValue;");
        }

        [Theory/*(Skip = "Issue #582")*/]
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
                    Assert.Equal("'0'", column.DefaultValueSql);
                },
                "DROP TABLE DefaultValueClr");
        }

        #endregion

        #region PrimaryKeyFacets

        [Fact/*(Skip = "Issue #582")*/]
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

                        Assert.Equal("CompositePrimaryKey".ToUpper(), pk.Table.Name);
                        Assert.Equal(new List<string> { "Id2".ToUpper(), "Id1".ToUpper() }, pk.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE CompositePrimaryKey;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal(new List<string> { "Id".ToUpper() }, pk.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE RowidPrimaryKey;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal(new List<string> { "Id".ToUpper() }, pk.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE PrimaryKeyName;");
        }

        #endregion

        #region UniqueConstraintFacets

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal("CompositeUniqueConstraint".ToUpper(), constraint.Table.Name);
                        Assert.Equal(new List<string> { "Id2".ToUpper(), "Id1".ToUpper() }, constraint.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE CompositeUniqueConstraint;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal("UK", constraint.Name);
                        Assert.Equal(new List<string> { "Id".ToUpper() }, constraint.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE UniqueConstraintName;");
        }

        #endregion

        #region IndexFacets

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal("CompositeIndex".ToUpper(), index.Table.Name);
                        Assert.Equal("IX_COMPOSITE", index.Name);
                        Assert.Equal(new List<string> { "Id2".ToUpper(), "Id1".ToUpper() }, index.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE CompositeIndex;");
        }

        [Fact/*(Skip = "Issue #582")*/]
        public void Set_unique_for_unique_index()
        {
            Test(
                @"
CREATE TABLE UniqueIndex (
    Id1 int,
    Id2 text
);

CREATE UNIQUE INDEX IX_UNIQUE on UniqueIndex (Id2);",
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>(),
                dbModel =>
                    {
                        var index = Assert.Single(dbModel.Tables.Single().Indexes);

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("UniqueIndex".ToUpper(), index.Table.Name);
                        Assert.Equal("IX_UNIQUE".ToUpper(), index.Name);
                        Assert.True(index.IsUnique);
                        Assert.Equal(new List<string> { "Id2".ToUpper() }, index.Columns.Select(ic => ic.Name).ToList());
                    },
                @"DROP TABLE UniqueIndex;");
        }

        #endregion

        #region ForeignKeyFacets

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal(new List<string> { "Id1".ToUpper(), "Id2".ToUpper() }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                    },
                @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal(new List<string> { "Id".ToUpper() }, principalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, principalFk.OnDelete);

                        var anotherPrincipalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "AnotherPrincipalTable".ToUpper()));

                        // ReSharper disable once PossibleNullReferenceException
                        Assert.Equal("DependentTable".ToUpper(), anotherPrincipalFk.Table.Name);
                        Assert.Equal("AnotherPrincipalTable".ToUpper(), anotherPrincipalFk.PrincipalTable.Name);
                        Assert.Equal(new List<string> { "ForeignKeyId2".ToUpper() }, anotherPrincipalFk.Columns.Select(ic => ic.Name).ToList());
                        Assert.Equal(new List<string> { "Id".ToUpper() }, anotherPrincipalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, anotherPrincipalFk.OnDelete);
                    },
                @"
DROP TABLE DependentTable;
DROP TABLE AnotherPrincipalTable;
DROP TABLE PrincipalTable;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal(new List<string> { "Id2".ToUpper() }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                    },
                @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal(new List<string> { "Id".ToUpper() }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
                        Assert.Equal("MYFK", fk.Name);
                    },
                @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                        Assert.Equal(new List<string> { "Id".ToUpper() }, fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                        Assert.Equal(ReferentialAction.SetNull, fk.OnDelete);
                    },
                @"
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        #endregion

        #region Warnings

        [Fact/*(Skip = "Issue #582")*/]
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
                @"DROP TABLE Everest;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
                @"DROP TABLE Blank;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        [Fact/*(Skip = "Issue #582")*/]
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
DROP TABLE DependentTable;
DROP TABLE PrincipalTable;");
        }

        #endregion

        public class XGDatabaseModelFixture : SharedStoreFixtureBase<DbContext>
        {
            protected override string StoreName { get; } = nameof(XGDatabaseModelFactoryTest);
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
            public new XGTestStore TestStore => (XGTestStore)base.TestStore;

            protected override bool UsePooling => false;
        }
    }
}
