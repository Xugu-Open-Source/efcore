// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Design.FunctionalTests
{
    public class XuGuDbDatabaseModelFactoryTest : IClassFixture<XuGuDbDatabaseModelFixture>
    {
        [Fact]
        public void It_reads_tables()
        {
            var sql = @"
CREATE TABLE `SYSDBA`.`Everest` ( id int );
CREATE TABLE `SYSDBA`.`Denali` ( id int );";
            var dbModel = CreateModel(sql, new TableSelectionSet(new List<string> { "Everest", "Denali" }));

            Assert.Collection(dbModel.Tables.OrderBy(t => t.Name),
                d =>
                    {
                        Assert.Equal("SYSDBA", d.SchemaName);
                        Assert.Equal("Denali", d.Name);
                    },
                e =>
                    {
                        Assert.Equal("SYSDBA", e.SchemaName);
                        Assert.Equal("Everest", e.Name);
                    });
        }

        [Fact]
        public void It_filters_views()
        {
            _fixture.ExecuteNonQuery(@"CREATE TABLE `SYSDBA`.`Hills` (Id int PRIMARY KEY, Name varchar(100), Height int);");
            _fixture.ExecuteNonQuery(@"CREATE VIEW `SYSDBA`.`ShortHills` AS SELECT Name FROM `SYSDBA`.`Hills` WHERE Height < 100;");
            var sql = "CREATE UNIQUE INDEX IX_Hills_Names ON Hills (Name);";

            var selectionSet = new TableSelectionSet(new List<string> { "Hills", "ShortHills" });
            var logger = new XuGuDbDatabaseModelFixture.TestLogger();

            var dbModel = CreateModel(sql, selectionSet, logger);
            Assert.Single(dbModel.Tables);
            //Assert.DoesNotContain(logger.Items, i => i.logLevel == LogLevel.Warning);
        }

        [Fact]
        public void It_reads_foreign_keys()
        {
            _fixture.ExecuteNonQuery("CREATE SCHEMA db2");
            var sql = "CREATE TABLE SYSDBA.Ranges ( Id INT IDENTITY (1,1) PRIMARY KEY);" +
                      "CREATE TABLE db2.Mountains ( RangeId INT NOT NULL, FOREIGN KEY (RangeId) REFERENCES SYSDBA.Ranges(Id) ON DELETE CASCADE)";
            var dbModel = CreateModel(sql, new TableSelectionSet(new List<string> { "Ranges", "Mountains" }));

            var fk = Assert.Single(dbModel.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            Assert.Equal("DB2", fk.Table.SchemaName);
            Assert.Equal("MOUNTAINS", fk.Table.Name);
            Assert.Equal("SYSDBA", fk.PrincipalTable.SchemaName);
            Assert.Equal("RANGES", fk.PrincipalTable.Name);
            Assert.Equal("RANGEID", fk.Columns.Single().Column.Name);
            Assert.Equal("ID", fk.Columns.Single().PrincipalColumn.Name);
            Assert.Equal(ReferentialAction.Cascade, fk.OnDelete);
        }

        [Fact]
        public void It_reads_composite_foreign_keys()
        {
            _fixture.ExecuteNonQuery("CREATE SCHEMA db3");
            var sql = "CREATE TABLE SYSDBA.Ranges1 ( Id INT IDENTITY (1,1), AltId INT, PRIMARY KEY(Id, AltId));" +
                      "CREATE TABLE db3.Mountains1 ( RangeId INT NOT NULL, RangeAltId INT NOT NULL, FOREIGN KEY (RangeId, RangeAltId) REFERENCES SYSDBA.Ranges1(Id, AltId) ON DELETE NO ACTION)";
            var dbModel = CreateModel(sql, new TableSelectionSet(new List<string> { "Ranges1", "Mountains1" }));

            var fk = Assert.Single(dbModel.Tables.Single(t => t.ForeignKeys.Count > 0).ForeignKeys);

            Assert.Equal("DB3", fk.Table.SchemaName);
            Assert.Equal("MOUNTAINS1", fk.Table.Name);
            Assert.Equal("SYSDBA", fk.PrincipalTable.SchemaName);
            Assert.Equal("RANGES1", fk.PrincipalTable.Name);
            Assert.Equal(new[] { "RANGEID", "RANGEALTID" }, fk.Columns.OrderBy(c=>c.Ordinal).Select(c => c.Column.Name).ToArray());
            Assert.Equal(new[] { "ID", "ALTID" }, fk.Columns.OrderBy(c=>c.Ordinal).Select(c => c.PrincipalColumn.Name).ToArray());
            Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
        }

        [Fact]
        public void It_reads_indexes()
        {
            var sql = "CREATE TABLE Place ( Id int PRIMARY KEY, Name int UNIQUE, Location int);" +
                      "CREATE INDEX IX_Location_Name ON Place (Location, Name);" +
                      "CREATE INDEX IX_Location ON Place (Location);";
            var dbModel = CreateModel(sql, new TableSelectionSet(new List<string> { "Place" }));

            var indexes = dbModel.Tables.Single().Indexes;

            Assert.All(indexes, c =>
                {
                    Assert.Equal("SYSDBA", c.Table.SchemaName);
                    Assert.Equal("PLACE", c.Table.Name);
                });

            Assert.Collection(indexes.OrderBy(i => i.Name),
                nonClustered =>
                    {
                        Assert.Equal("IX_LOCATION", nonClustered.Name);
                        Assert.False(nonClustered.XuGuDb().IsClustered);
                        Assert.Equal("LOCATION", nonClustered.IndexColumns.Select(ic => ic.Column.Name).Single());
                    },
                clusteredIndex =>
                    {
                        Assert.Equal("IX_LOCATION_NAME", clusteredIndex.Name);
                        Assert.False(clusteredIndex.IsUnique);
                        Assert.Equal(new List<string> { "LOCATION", "NAME" }, clusteredIndex.IndexColumns.Select(ic => ic.Column.Name).ToList());
                        Assert.Equal(new List<int> { 1, 2 }, clusteredIndex.IndexColumns.Select(ic => ic.Ordinal).ToList());
                    },
                pkIndex =>
                    {
                        Assert.StartsWith("PK_IDX", pkIndex.Name);
                        Assert.True(pkIndex.IsUnique);
                        Assert.Equal(new List<string> { "ID" }, pkIndex.IndexColumns.Select(ic => ic.Column.Name).ToList());
                    },
                unique =>
                    {
                        Assert.True(unique.IsUnique);
                        Assert.Equal("NAME", unique.IndexColumns.Single().Column.Name);
                    });
        }

        [Fact]
        public void It_reads_columns()
        {
            var sql = @"
CREATE TABLE `SYSDBA`.`MountainsColumns` (
    Id int,
    Name varchar(100) NOT NULL,
    Latitude decimal( 5, 2 ) DEFAULT 0.0,
    Created datetime(6) DEFAULT '2015-10-20 11:00:00',
    DiscoveredDate datetime,
    CurrentDate timestamp,
    Sum decimal( 5, 2 ),
    Modified rowversion,
    Primary Key (Name, Id)
);";
            var dbModel = CreateModel(sql, new TableSelectionSet(new List<string> { "MountainsColumns" }));

            var columns = dbModel.Tables.Single().Columns.OrderBy(c => c.Ordinal);

            Assert.All(columns, c =>
                {
                    Assert.Equal("SYSDBA", c.Table.SchemaName);
                    Assert.Equal("MountainsColumns", c.Table.Name);
                });

            Assert.Collection(columns,
                id =>
                    {
                        Assert.Equal("ID", id.Name);
                        Assert.Equal("INTEGER", id.DataType);
                        Assert.Equal(2, id.PrimaryKeyOrdinal);
                        Assert.False(id.IsNullable);
                        Assert.Equal(0, id.Ordinal);
                        Assert.Null(id.DefaultValue);
                    },
                name =>
                    {
                        Assert.Equal("NAME", name.Name);
                        Assert.Equal("CHAR", name.DataType);
                        Assert.Equal(1, name.PrimaryKeyOrdinal);
                        Assert.False(name.IsNullable);
                        Assert.Equal(1, name.Ordinal);
                        Assert.Null(name.DefaultValue);
                        Assert.Equal(100, name.MaxLength);
                    },
                lat =>
                    {
                        Assert.Equal("LATITUDE", lat.Name);
                        Assert.Equal("NUMERIC", lat.DataType);
                        Assert.Null(lat.PrimaryKeyOrdinal);
                        Assert.True(lat.IsNullable);
                        Assert.Equal(2, lat.Ordinal);
                        Assert.Equal("0.0", lat.DefaultValue);
                        Assert.Equal(5, lat.Precision);
                        Assert.Equal(2, lat.Scale);
                        //Assert.Null(lat.MaxLength);
                        Assert.Null(lat.XuGuDb().DateTimePrecision);
                    },
                created =>
                    {
                        Assert.Equal("CREATED", created.Name);
                        //Assert.Null(created.Scale);
                        Assert.Equal(6, created.XuGuDb().DateTimePrecision);
                        Assert.Equal("'2015-10-20 11:00:00'", created.DefaultValue);
                    },
                discovered =>
                    {
                        Assert.Equal("DISCOVEREDDATE", discovered.Name);
                        Assert.Equal(6, discovered.XuGuDb().DateTimePrecision);
                    },
                current =>
                    {
                        Assert.Equal("CURRENTDATE", current.Name);
                        Assert.Equal("DATETIME", current.DataType);
                    },
                sum =>
                    {
                        Assert.Equal("SUM", sum.Name);
                    },
                modified =>
                    {
                        Assert.Equal("MODIFIED", modified.Name);
                        Assert.Equal("ROWVERSION", modified.DataType); // intentional - testing the alias
                    });
        }

        [Theory]
        [InlineData("nvarchar(55)", 55)]
        [InlineData("varchar(341)", 341)]
        [InlineData("nchar(14)", 14)]
        [InlineData("char(89)", 89)]
        [InlineData("varchar", null)]
        public void It_reads_max_length(string type, int? length)
        {
            var sql = @"DROP TABLE IF EXISTS `SYSDBA`.`Strings`;" +
                      "CREATE TABLE `SYSDBA`.`Strings` ( CharColumn " + type + ");";
            var db = CreateModel(sql, new TableSelectionSet(new List<string> { "Strings" }));

            Assert.Equal(length, db.Tables.Single().Columns.Single().MaxLength);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void It_reads_identity(bool isIdentity)
        {
            var dbModel = CreateModel(
                @"DROP TABLE IF EXISTS `SYSDBA`.`Identities`;
                  CREATE TABLE `SYSDBA`.`Identities` ( Id INT " + (isIdentity ? "IDENTITY(1,1)" : "") + ")",
                new TableSelectionSet(new List<string> { "Identities" }));

            var column = Assert.Single(dbModel.Tables.Single().Columns);
            Assert.Equal(isIdentity, column.XuGuDb().IsIdentity);
        }

        [Fact]
        public void It_filters_tables()
        {
            var sql = @"CREATE TABLE `SYSDBA`.`K2` ( Id int, A varchar, UNIQUE (A ) );
CREATE TABLE `SYSDBA`.`Kilimanjaro` ( Id int, B varchar, UNIQUE (B), FOREIGN KEY (B) REFERENCES K2 (A) );";

            var selectionSet = new TableSelectionSet(new List<string> { "K2" });

            var dbModel = CreateModel(sql, selectionSet);
            var table = Assert.Single(dbModel.Tables);
            Assert.Equal("K2", table.Name);
            Assert.Equal(2, table.Columns.Count);
            Assert.Equal(1, table.Indexes.Count);
            Assert.Empty(table.ForeignKeys);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsSequences)]
        public void It_reads_sequences()
        {
            var sql = @"CREATE SEQUENCE DefaultValues_read;
 
CREATE SEQUENCE CustomSequence_read
    START WITH 1 
    INCREMENT BY 2 
    MAXVALUE 8 
    MINVALUE -3 
    CYCLE;";

            var dbModel = CreateModel(sql);
            Assert.Collection(dbModel.Sequences.Where(s => s.Name.EndsWith("_READ")).OrderBy(s => s.Name),
                c =>
                    {
                        Assert.Equal(c.Name, "CUSTOMSEQUENCE_READ");
                        Assert.Equal(c.SchemaName, "SYSDBA");
                        Assert.Equal(c.DataType, "int64");
                        Assert.Equal(1, c.Start);
                        Assert.Equal(2, c.IncrementBy);
                        Assert.Equal(8, c.Max);
                        Assert.Equal(-3, c.Min);
                        Assert.True(c.IsCyclic);
                    },
                d =>
                    {
                        Assert.Equal(d.Name, "DEFAULTVALUES_READ");
                        Assert.Equal(d.SchemaName, "SYSDBA");
                        Assert.Equal(d.DataType, "int64");
                        Assert.Equal(1, d.IncrementBy);
                        Assert.False(d.IsCyclic);
                        Assert.Null(d.Max);
                        Assert.Equal(1,d.Min);
                        Assert.Equal(1,d.Start);
                    });
        }

        [Fact]
        public async Task It_reads_default_schema()
        {
            var defaultSchema = await _fixture.TestStore.ExecuteScalarAsync<string>("SELECT current_schema", default(CancellationToken));

            var model = _fixture.CreateModel("SELECT 1");
            Assert.Equal(defaultSchema, model.DefaultSchemaName);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsSequences)]
        public void SequenceModel_values_null_for_default_min_max_start()
        {
            var sql = @"CREATE SEQUENCE `TinyIntSequence_defaults`;
CREATE SEQUENCE `SmallIntSequence_defaults`;
CREATE SEQUENCE `IntSequence_defaults`;
CREATE SEQUENCE `DecimalSequence_defaults`;
CREATE SEQUENCE `NumericSequence_defaults`;";
            var dbModel = CreateModel(sql);
            Assert.All(dbModel.Sequences.Where(s => s.Name.EndsWith("_DEFAULTS")), s =>
                {
                    Assert.Null(s.Start);
                    Assert.Null(s.Min);
                    Assert.Null(s.Max);
                });
        }

        private readonly XuGuDbDatabaseModelFixture _fixture;

        public DatabaseModel CreateModel(string createSql, TableSelectionSet selection = null, ILogger logger = null)
            => _fixture.CreateModel(createSql, selection, logger);

        public XuGuDbDatabaseModelFactoryTest(XuGuDbDatabaseModelFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
