// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class MigrationsXuGuDbTest : MigrationsTestBase<MigrationsXuGuDbFixture>
    {
        private readonly MigrationsXuGuDbFixture _fixture;
        public MigrationsXuGuDbTest(MigrationsXuGuDbFixture fixture)
            : base(fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public new void Can_apply_all_migrations()
        {
            using (var db = _fixture.CreateContext())
            {
                db.Database.EnsureDeleted();

                db.Database.Migrate();

                var history = db.GetInfrastructure().GetRequiredService<IHistoryRepository>();
                Assert.Collection(
                    history.GetAppliedMigrations(),
                    x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
                    x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
                    x => Assert.Equal("00000000000003_Migration3", x.MigrationId));
            }
        }

        [Fact]
        public override async Task Can_execute_operations()
        {
            using (var db = _fixture.CreateContext())
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var services = db.GetInfrastructure();
                var connection = db.Database.GetDbConnection();

                await db.Database.OpenConnectionAsync();

                await ExecuteAsync(services, BuildFirstMigration);
                await AssertFirstMigrationAsync(connection);
                await ExecuteAsync(services, BuildSecondMigration);
                await AssertSecondMigrationAsync(connection);
            }
        }

        [Fact]
		public override void Can_generate_migration_from_initial_database_to_initial()
        {
            base.Can_generate_migration_from_initial_database_to_initial();

            Assert.Equal(
                @"    CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
        `MigrationId` VARCHAR(150) NOT NULL,
        `ProductVersion` VARCHAR(32) NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (`MigrationId`)
    );

",
                Sql);
        }

        [Fact]
		public override void Can_generate_no_migration_script()
        {
            base.Can_generate_no_migration_script();

            Assert.Equal(
                @"    CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
        `MigrationId` VARCHAR(150) NOT NULL,
        `ProductVersion` VARCHAR(32) NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (`MigrationId`)
    );

",
                Sql);
        }

        [Fact]
		public override void Can_generate_up_scripts()
        {
            base.Can_generate_up_scripts();

            Assert.Equal(
                @"    CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
        `MigrationId` VARCHAR(150) NOT NULL,
        `ProductVersion` VARCHAR(32) NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (`MigrationId`)
    );

CREATE TABLE IF NOT EXISTS `Table1` (
    `Id` INTEGER NOT NULL,
    CONSTRAINT PK_Table1 PRIMARY KEY (`Id`)
);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('00000000000001_Migration1', '7.0.0-test');

ALTER TABLE `Table1` RENAME TO `Table2`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('00000000000002_Migration2', '7.0.0-test');

CREATE DATABASE IF NOT EXISTS `TransactionSuppressed`;

DROP DATABASE IF EXISTS `TransactionSuppressed`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('00000000000003_Migration3', '7.0.0-test');

",
                Sql);
        }

        [Fact]
		public override void Can_generate_idempotent_up_scripts()
        {
            base.Can_generate_idempotent_up_scripts();

            Assert.Equal(
                @"    CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
        `MigrationId` VARCHAR(150) NOT NULL,
        `ProductVersion` VARCHAR(32) NOT NULL,
        CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (`MigrationId`)
    );

BEGIN
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000001_Migration1') THEN

    CREATE TABLE IF NOT EXISTS `Table1` (
        `Id` INTEGER NOT NULL,
        CONSTRAINT PK_Table1 PRIMARY KEY (`Id`)
    );
END IF;
END;

BEGIN
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000001_Migration1') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('00000000000001_Migration1', '7.0.0-test');
END IF;
END;

BEGIN
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000002_Migration2') THEN

    ALTER TABLE `Table1` RENAME TO `Table2`;
END IF;
END;

BEGIN
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000002_Migration2') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('00000000000002_Migration2', '7.0.0-test');
END IF;
END;

BEGIN
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000003_Migration3') THEN

    CREATE DATABASE IF NOT EXISTS `TransactionSuppressed`;
END IF;
END;

BEGIN
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000003_Migration3') THEN

    DROP DATABASE IF EXISTS `TransactionSuppressed`;
END IF;
END;

BEGIN
IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000003_Migration3') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('00000000000003_Migration3', '7.0.0-test');
END IF;
END;

",
                Sql);
        }

        [Fact]
		public override void Can_generate_down_scripts()
        {
            base.Can_generate_down_scripts();

            Assert.Equal(
                @"DELETE FROM `__EFMigrationsHistory`
WHERE `MigrationId` = '00000000000003_Migration3';

ALTER TABLE `Table2` RENAME TO `Table1`;

DELETE FROM `__EFMigrationsHistory`
WHERE `MigrationId` = '00000000000002_Migration2';

DROP TABLE `Table1`;

DELETE FROM `__EFMigrationsHistory`
WHERE `MigrationId` = '00000000000001_Migration1';

",
                Sql);
        }

        [Fact]
		public override void Can_generate_idempotent_down_scripts()
        {
            base.Can_generate_idempotent_down_scripts();

            Assert.Equal(
                @"BEGIN
IF EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000003_Migration3') THEN

    DELETE FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '00000000000003_Migration3';
END IF;
END;

BEGIN
IF EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000002_Migration2') THEN

    ALTER TABLE `Table2` RENAME TO `Table1`;
END IF;
END;

BEGIN
IF EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000002_Migration2') THEN

    DELETE FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '00000000000002_Migration2';
END IF;
END;

BEGIN
IF EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000001_Migration1') THEN

    DROP TABLE `Table1`;
END IF;
END;

BEGIN
IF EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000001_Migration1') THEN

    DELETE FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = '00000000000001_Migration1';
END IF;
END;

",
                Sql);
        }

        [Fact]
		public override void Can_get_active_provider()
        {
            base.Can_get_active_provider();

            Assert.Equal("Microsoft.EntityFrameworkCore.XuGuDB", ActiveProvider);
        }

        protected override async Task AssertFirstMigrationAsync(DbConnection connection)
        {
            var sql = await GetDatabaseSchemaAsync(connection);
            Assert.Equal(
                @"
CreatedTable
    Id INTEGER NOT NULL
    ColumnWithDefaultToDrop INTEGER NULL DEFAULT 0
    ColumnWithDefaultToAlter INTEGER NULL DEFAULT 1
",
                sql);
        }

        protected override async Task AssertSecondMigrationAsync(DbConnection connection)
        {
            var sql = await GetDatabaseSchemaAsync(connection);
            Assert.Equal(
                @"
CreatedTable
    Id INTEGER NOT NULL
    ColumnWithDefaultToAlter INTEGER NULL
",
                sql);
        }

        private async Task<string> GetDatabaseSchemaAsync(DbConnection connection)
        {
            var builder = new IndentedStringBuilder();

            var command = connection.CreateCommand();
            command.CommandText = @"
                 SELECT
    `t`.`TABLE_NAME`,
    `c`.`COL_NAME`,
    `c`.`TYPE_NAME`,
    `c`.`NOT_NULL`,
    `c`.`DEF_VAL`
FROM `ALL_TABLES` `t`
LEFT JOIN `ALL_COLUMNS` `c` ON `c`.`TABLE_ID` = `t`.`TABLE_ID`
LEFT JOIN `ALL_CONSTRAINTS` `d` ON '""' || `c`.`COL_NAME` || '""' LIKE `d`.`DEFINE`  AND `d`.`TABLE_ID` = `t`.`TABLE_ID`
WHERE `t`.`TABLE_TYPE` = 0
ORDER BY `t`.`TABLE_NAME`, `c`.`COL_NO`; ";

            using (var reader = await command.ExecuteReaderAsync())
            {
                var first = true;
                string lastTable = null;
                while (await reader.ReadAsync())
                {
                    var currentTable = reader.GetString(0);
                    if (currentTable != lastTable)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            builder.DecrementIndent();
                        }

                        builder
                            .AppendLine()
                            .AppendLine(currentTable)
                            .IncrementIndent();

                        lastTable = currentTable;
                    }

                    builder
                        .Append(reader[1]) // Name
                        .Append(" ")
                        .Append(reader[2]) // Type
                        .Append(" ")
                        .Append(!reader.GetBoolean(3) ? "NULL" : "NOT NULL");

                    var test1= reader[4];
                    var test2 = reader.GetString(4);
                    var test3 = await reader.IsDBNullAsync(4);

                    if (!await reader.IsDBNullAsync(4))
                    {
                        builder
                            .Append(" DEFAULT ")
                            .Append(reader[4]);
                    }

                    builder.AppendLine();
                }
            }

            return builder.ToString();
        }
    }
}
