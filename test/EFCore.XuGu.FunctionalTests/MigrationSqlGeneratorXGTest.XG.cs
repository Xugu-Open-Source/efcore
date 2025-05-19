// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public partial class XGMigrationsSqlGeneratorTest : MigrationsSqlGeneratorTestBase
    {
        [ConditionalFact]
        public virtual void DropUniqueConstraintOperation()
        {
            Generate(
                SetupModel,
                new DropUniqueConstraintOperation
                {
                    Table = "Cars",
                    Name = "AK_Cars_LicensePlateNumber",
                });

            AssertSql(@"ALTER TABLE `Cars` DROP CONSTRAINT `AK_Cars_LicensePlateNumber`;");
        }

        [ConditionalFact]
        public virtual void XGDropUniqueConstraintAndRecreateForeignKeysOperation_temporarily_drops_foreign_keys()
        {
            // A foreign key might reuse the alternate key for its own purposes and prohibit its deletion,
            // if the foreign key columns are listed as the first columns and in the same order as in the foreign key (#678).
            // We therefore drop and later recreate all foreign keys to ensure, that no other dependencies on the
            // alternate key exist.
            Generate(
                SetupModel,
                new XGDropUniqueConstraintAndRecreateForeignKeysOperation
                {
                    Table = "Cars",
                    Name = "AK_Cars_LicensePlateNumber",
                    RecreateForeignKeys = true,
                });

            AssertSql(
                @"ALTER TABLE `Cars` DROP CONSTRAINT `FK_Cars_LicensePlates_LicensePlateNumber`;

ALTER TABLE `Cars` DROP CONSTRAINT `AK_Cars_LicensePlateNumber`;

ALTER TABLE `Cars` ADD CONSTRAINT `FK_Cars_LicensePlates_LicensePlateNumber` FOREIGN KEY (`LicensePlateNumber`) REFERENCES `LicensePlates` (`LicensePlateNumber`) ON DELETE CASCADE;");
        }

        [ConditionalFact]
        public virtual void DropPrimaryKeyOperation()
        {
            Generate(
                SetupModel,
                new DropPrimaryKeyOperation
                {
                    Table = "Cars",
                    Name = "PK_Cars_CarId",
                });

            AssertSql(
                @"DECLARE
pktotal int;
BEGIN
pktotal:=(SELECT COUNT(CONS_NAME) FROM ALL_CONSTRAINTS WHERE CONS_TYPE='P' AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Cars' AND SCHEMA_ID=(SELECT SCHEMA_ID FROM ALL_SCHEMAS WHERE SCHEMA_NAME='' LIMIT 1) LIMIT 1) LIMIT 1);
IF pktotal>0 THEN
EXECUTE IMMEDIATE 'ALTER TABLE TESTDELETEP DROP CONSTRAINT '||(SELECT CONS_NAME FROM ALL_CONSTRAINTS WHERE CONS_TYPE='P' AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Cars' AND SCHEMA_ID=(SELECT SCHEMA_ID FROM ALL_SCHEMAS WHERE SCHEMA_NAME='' LIMIT 1) LIMIT 1) LIMIT 1);
END IF;
END;
;");
        }

        [ConditionalFact]
        public virtual void XGDropPrimaryKeyAndRecreateForeignKeysOperation_temporarily_drops_foreign_keys()
        {
            // A foreign key might reuse the primary key for its own purposes and prohibit its deletion,
            // if the foreign key columns are listed as the first columns and in the same order as in the foreign key (#678).
            // We therefore drop and later recreate all foreign keys to ensure, that no other dependencies on the
            // primary key exist.
            Generate(
                SetupModel,
                new XGDropPrimaryKeyAndRecreateForeignKeysOperation
                {
                    Table = "Cars",
                    Name = "PK_Cars_CarId",
                    RecreateForeignKeys = true,
                });

            AssertSql(
                @"ALTER TABLE `Cars` DROP CONSTRAINT `FK_Cars_LicensePlates_LicensePlateNumber`;

DECLARE
pktotal int;
BEGIN
pktotal:=(SELECT COUNT(CONS_NAME) FROM ALL_CONSTRAINTS WHERE CONS_TYPE='P' AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Cars' AND SCHEMA_ID=(SELECT SCHEMA_ID FROM ALL_SCHEMAS WHERE SCHEMA_NAME='' LIMIT 1) LIMIT 1) LIMIT 1);
IF pktotal>0 THEN
EXECUTE IMMEDIATE 'ALTER TABLE TESTDELETEP DROP CONSTRAINT '||(SELECT CONS_NAME FROM ALL_CONSTRAINTS WHERE CONS_TYPE='P' AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='Cars' AND SCHEMA_ID=(SELECT SCHEMA_ID FROM ALL_SCHEMAS WHERE SCHEMA_NAME='' LIMIT 1) LIMIT 1) LIMIT 1);
END IF;
END;

ALTER TABLE `Cars` ADD CONSTRAINT `FK_Cars_LicensePlates_LicensePlateNumber` FOREIGN KEY (`LicensePlateNumber`) REFERENCES `LicensePlates` (`LicensePlateNumber`) ON DELETE CASCADE;");
        }

        [ConditionalFact(Skip = "Not supported yet")]
        public virtual void CreateTable_uses_srid()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "IceCreamShops",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Location",
                            ClrType = typeof(Point),
                            ColumnType = "GEOMETRY",
                            [XGAnnotationNames.SpatialReferenceSystemId] = 0,
                        }
                    }
                });

            Assert.Equal(
                @"CREATE TABLE `SYSDBA`.`IceCreamShops` (
    `Location` GEOMETRY NOT NULL /*!80003 SRID 0 */
);
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact(Skip = "Not supported yet")]
        public virtual void CreateTable_uses_srid_geometry_derived()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "IceCreamShops",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "Location",
                            ClrType = typeof(Point),
                            ColumnType = "POINT",
                            [XGAnnotationNames.SpatialReferenceSystemId] = 4326,
                        }
                    }
                });

            Assert.Equal(
                @"CREATE TABLE `SYSDBA`.`IceCreamShops` (
    `Location` POINT NOT NULL /*!80003 SRID 4326 */
);
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void CreateTable_with_SchemaNameTranslator()
        {
            Generate(
                optionsBuilder => optionsBuilder.SchemaBehavior(
                    XGSchemaBehavior.Translate,
                    (schemaName, objectName) => $"{schemaName}_{objectName}"),
                null,
                new MigrationOperation[]
                {
                    new CreateTableOperation
                    {
                        Name = "IceCreams",
                        Schema = "IceCreamParlor",
                        Columns =
                        {
                            new AddColumnOperation
                            {
                                Name = "Name",
                                ClrType = typeof(string),
                                ColumnType = "varchar(255)",
                            }
                        }
                    }
                },
                MigrationsSqlGenerationOptions.Default);

            Assert.Equal(
                @"CREATE TABLE `IceCreamParlor`.`IceCreams` (
    `Name` varchar(255) NOT NULL
);
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void CreateTable_with_ValueGenerationStrategy_int_value()
        {
            Generate(
                new CreateTableOperation
                {
                    Name = "IceCreamShops",
                    Columns =
                    {
                        new AddColumnOperation
                        {
                            Name = "IceCreamShopId",
                            ClrType = typeof(int),
                            ColumnType = "int",
                            [XGAnnotationNames.ValueGenerationStrategy] = (int)XGValueGenerationStrategy.IdentityColumn,
                        }
                    }
                });

            Assert.Equal(
                @"CREATE TABLE `SYSDBA`.`IceCreamShops` (
    `IceCreamShopId` int IDENTITY NOT NULL
);
",
                Sql,
                ignoreLineEndingDifferences: true);
        }

        private static void SetupModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>(entity =>
            {
                entity.ToTable("Cars");

                entity.HasKey(e => e.CarId);
                entity.HasAlternateKey(e => e.LicensePlateNumber);

                entity.Property(e => e.LicensePlateNumber)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.HasOne(e => e.LicensePlate)
                    .WithMany()
                    .HasForeignKey(e => e.LicensePlateNumber);
            });

            modelBuilder.Entity<LicensePlate>(entity =>
            {
                entity.ToTable("LicensePlates");

                entity.HasKey(e => e.LicensePlateNumber);

                entity.Property(e => e.LicensePlateNumber)
                    .HasMaxLength(255)
                    .IsRequired();
            });
        }

        protected class Car
        {
            public int CarId { get; set; }
            public string LicensePlateNumber { get; set; }

            public LicensePlate LicensePlate { get; set; }
        }

        protected class LicensePlate
        {
            public string LicensePlateNumber { get; set; }
        }
    }
}
