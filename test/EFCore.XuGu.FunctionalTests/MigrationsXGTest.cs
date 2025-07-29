using System; 
using System.Data.Common; 
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading.Tasks;


namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests 
{ 
    public class MigrationsXGTest : MigrationsTestBase<MigrationsXGFixture> 
    { 
        public MigrationsXGTest(MigrationsXGFixture fixture) 
            : base(fixture) 
        { 
        }

        public override void Can_apply_all_migrations()
        {
            using (var db = Fixture.CreateContext())
            {
                db.Database.EnsureDeleted();

                GiveMeSomeTime(db);

                db.Database.EnsureCreated();

                db.Database.Migrate();

                var history = db.GetService<IHistoryRepository>();
                Assert.Collection(
                    history.GetAppliedMigrations(),
                    x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
                    x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
                    x => Assert.Equal("00000000000003_Migration3", x.MigrationId));
            }
        }

        public override async Task Can_apply_all_migrations_async()
        {
            using (var db = Fixture.CreateContext())
            {
                await db.Database.EnsureDeletedAsync();

                await GiveMeSomeTimeAsync(db);

                await db.Database.EnsureCreatedAsync();

                await db.Database.MigrateAsync();

                var history = db.GetService<IHistoryRepository>();
                Assert.Collection(
                    await history.GetAppliedMigrationsAsync(),
                    x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
                    x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
                    x => Assert.Equal("00000000000003_Migration3", x.MigrationId));
            }
        }

        public override void Can_generate_migration_from_initial_database_to_initial() 
        { 
            base.Can_generate_migration_from_initial_database_to_initial(); 
 
            Assert.Equal( 
                @"CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

", 
                Sql, 
                ignoreLineEndingDifferences: true); 
        } 
 
        public override void Can_generate_no_migration_script() 
        { 
            base.Can_generate_no_migration_script(); 
 
            Assert.Equal( 
                @"CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

", 
                Sql, 
                ignoreLineEndingDifferences: true); 
        } 
 
        public override void Can_generate_up_scripts() 
        { 
            base.Can_generate_up_scripts(); 
 
            Assert.Equal( 
                @"CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);

CREATE TABLE `Table1` (
    `Id` INTEGER NOT NULL,
    CONSTRAINT `PK_Table1` PRIMARY KEY (`Id`)
);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES (N'00000000000001_Migration1', N'7.0.0-test');

ALTER TABLE `Table1` RENAME TO `Table2`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES (N'00000000000003_Migration3', N'7.0.0-test');

", 
                Sql, 
                ignoreLineEndingDifferences: true); 
        } 
 
        public override void Can_generate_one_up_script() 
        { 
            base.Can_generate_one_up_script(); 
 
            Assert.Equal( 
                @"ALTER TABLE `Table1` RENAME TO `Table2`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

", 
                Sql, 
                ignoreLineEndingDifferences: true); 
        } 
 
        public override void Can_generate_up_script_using_names() 
        { 
            base.Can_generate_up_script_using_names(); 
 
            Assert.Equal( 
                @"ALTER TABLE `Table1` RENAME TO `Table2`;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES (N'00000000000002_Migration2', N'7.0.0-test');

", 
                Sql, 
                ignoreLineEndingDifferences: true); 
        } 
 
        public override void Can_generate_idempotent_up_scripts() 
        { 
            base.Can_generate_idempotent_up_scripts(); 
 
            Assert.Equal(@"CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(95) NOT NULL,
    `ProductVersion` varchar(32) NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
);


DROP PROCEDURE IF EXISTS MigrationsScript;
CREATE PROCEDURE MigrationsScript() IS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000001_Migration1') THEN

    CREATE TABLE `Table1` (
        `Id` INTEGER NOT NULL,
        CONSTRAINT `PK_Table1` PRIMARY KEY (`Id`)
    );

    END IF;
END;
EXEC MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
CREATE PROCEDURE MigrationsScript() IS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000001_Migration1') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES (N'00000000000001_Migration1', N'7.0.0-test');

    END IF;
END;
EXEC MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
CREATE PROCEDURE MigrationsScript() IS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000002_Migration2') THEN

    ALTER TABLE `Table1` RENAME TO `Table2`;

    END IF;
END;
EXEC MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
CREATE PROCEDURE MigrationsScript() IS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000002_Migration2') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES (N'00000000000002_Migration2', N'7.0.0-test');

    END IF;
END;
EXEC MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
CREATE PROCEDURE MigrationsScript() IS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000003_Migration3') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES (N'00000000000003_Migration3', N'7.0.0-test');

    END IF;
END;
EXEC MigrationsScript();
DROP PROCEDURE MigrationsScript;

", 
                Sql, 
                ignoreLineEndingDifferences: true); 
        } 
 
        public override void Can_generate_down_scripts() 
        { 
            base.Can_generate_down_scripts(); 
 
            Assert.Equal( 
                @"ALTER TABLE `Table2` RENAME TO `Table1`;

DELETE FROM `__EFMigrationsHistory`
WHERE `MigrationId` = N'00000000000002_Migration2';

DROP TABLE `Table1`;

DELETE FROM `__EFMigrationsHistory`
WHERE `MigrationId` = N'00000000000001_Migration1';

", 
                Sql, 
                ignoreLineEndingDifferences: true); 
        } 
 
        public override void Can_generate_one_down_script() 
        { 
            base.Can_generate_one_down_script(); 
 
            Assert.Equal( 
                @"ALTER TABLE `Table2` RENAME TO `Table1`;

DELETE FROM `__EFMigrationsHistory`
WHERE `MigrationId` = N'00000000000002_Migration2';

", 
                Sql, 
                ignoreLineEndingDifferences: true); 
        } 
 
        public override void Can_generate_down_script_using_names() 
        { 
            base.Can_generate_down_script_using_names(); 
 
            Assert.Equal( 
                @"ALTER TABLE `Table2` RENAME TO `Table1`;

DELETE FROM `__EFMigrationsHistory`
WHERE `MigrationId` = N'00000000000002_Migration2';

", 
                Sql, 
                ignoreLineEndingDifferences: true); 
        } 
 
        public override void Can_generate_idempotent_down_scripts() 
        { 
            base.Can_generate_idempotent_down_scripts();
 
            Assert.Equal(
                @"
DROP PROCEDURE IF EXISTS MigrationsScript;
CREATE PROCEDURE MigrationsScript() IS
BEGIN
    IF EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000002_Migration2') THEN

    ALTER TABLE `Table2` RENAME TO `Table1`;

    END IF;
END;
EXEC MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
CREATE PROCEDURE MigrationsScript() IS
BEGIN
    IF EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000002_Migration2') THEN

    DELETE FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = N'00000000000002_Migration2';

    END IF;
END;
EXEC MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
CREATE PROCEDURE MigrationsScript() IS
BEGIN
    IF EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000001_Migration1') THEN

    DROP TABLE `Table1`;

    END IF;
END;
EXEC MigrationsScript();
DROP PROCEDURE MigrationsScript;


DROP PROCEDURE IF EXISTS MigrationsScript;
CREATE PROCEDURE MigrationsScript() IS
BEGIN
    IF EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '00000000000001_Migration1') THEN

    DELETE FROM `__EFMigrationsHistory`
    WHERE `MigrationId` = N'00000000000001_Migration1';

    END IF;
END;
EXEC MigrationsScript();
DROP PROCEDURE MigrationsScript;

", 
                Sql, 
                ignoreLineEndingDifferences: true); 
        } 
 
        public override void Can_get_active_provider() 
        { 
            base.Can_get_active_provider(); 
 
            Assert.Equal("EFCore.XuGu", ActiveProvider); 
        } 
 
        protected override void AssertFirstMigration(DbConnection connection) 
        { 
            // TODO: Add assert 
        } 
 
        protected override void AssertSecondMigration(DbConnection connection) 
        { 
            // TODO: Add assert 
        } 
    } 
} 
