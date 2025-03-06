// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Relational.Tests.Migrations;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests.Migrations
{
    public class XuGuDbMigrationSqlGeneratorTest : MigrationSqlGeneratorTestBase
    {
        protected override IMigrationsSqlGenerator SqlGenerator
        {
            get
            {
                var typeMapper = new XuGuDbTypeMapper();

                return new XuGuDbMigrationsSqlGenerator(
                    new RelationalCommandBuilderFactory(
                        new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                        new DiagnosticListener("Fake"),
                        typeMapper),
                    new XuGuDbSqlGenerationHelper(),
                    typeMapper,
                    new XuGuDbAnnotationProvider());
            }
        }

        [Fact]
        public virtual void AddColumnOperation_with_computedSql()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "ColName",
                    ClrType = typeof(string)
                });

            Assert.Equal("ALTER TABLE `People` ADD `ColName` VARCHAR NOT NULL;" + EOL,Sql);
        }


        [Fact]
        public virtual void AddColumnOperation_identity()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false,
                    [XuGuDbFullAnnotationNames.Instance.ValueGenerationStrategy] =
                        XuGuDbValueGenerationStrategy.IdentityColumn
                });

            Assert.Equal(
                "ALTER TABLE `People` ADD `Id` INT IDENTITY NOT NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_without_column_type()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "People",
                    Name = "Alias",
                    ClrType = typeof(string)
                });

            Assert.Equal(
                "ALTER TABLE `People` ADD `Alias` VARCHAR NOT NULL;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_unicode_no_model()
        {
            Generate(
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    IsUnicode = false,
                    IsNullable = true
                });

            Assert.Equal(
                "ALTER TABLE `Person` ADD `Name` VARCHAR;" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<string>("Name").HasMaxLength(30),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true
                });

            Assert.Equal(
                "ALTER TABLE `Person` ADD `Name` VARCHAR(30);" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength_overridden()
        {
            Generate(
                modelBuilder => modelBuilder.Entity("Person").Property<string>("Name").HasMaxLength(30),
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 32,
                    IsNullable = true
                });

            Assert.Equal(
                "ALTER TABLE `Person` ADD `Name` VARCHAR(32);" + EOL,
                Sql);
        }

        public override void AddColumnOperation_with_maxLength_on_derived()
        {
            Generate(
                modelBuilder =>
                {
                    modelBuilder.Entity("Person");
                    modelBuilder.Entity("SpecialPerson", b =>
                    {
                        b.HasBaseType("Person");
                        b.Property<string>("Name").HasMaxLength(30);
                    });

                    modelBuilder.Entity("MoreSpecialPerson").HasBaseType("SpecialPerson");
                },
                new AddColumnOperation
                {
                    Table = "Person",
                    Name = "Name",
                    ClrType = typeof(string),
                    MaxLength = 30,
                    IsNullable = true
                });

            Assert.Equal(
                "ALTER TABLE `Person` ADD `Name` VARCHAR(30);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AddPrimaryKeyOperation_nonclustered()
        {
            Generate(
                new AddPrimaryKeyOperation
                {
                    Table = "People",
                    Columns = new[] { "Id" }
                });

            Assert.Equal(
                "ALTER TABLE `People` ADD PRIMARY KEY (`Id`);" + EOL,
                Sql);
        }

        public override void AlterColumnOperation()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Schema = "Schema",
                    Name = "LuckyNumber",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false,
                    DefaultValue = 7
                });

            Assert.Equal(
                "DECLARE" + EOL +
                "CURSOR fk_cursor IS" + EOL +
                "(SELECT `c`.`CONS_NAME` FROM `sys_constraints` `c` INNER JOIN `sys_tables` `t` ON `c`.`TABLE_ID`=`t`.`TABLE_ID` WHERE `t`.`TABLE_ID`=(SELECT `TABLE_ID` FROM `sys_tables` WHERE `TABLE_NAME`= 'People' AND `SCHEMA_ID` IN (SELECT `SCHEMA_ID` FROM `sys_schemas` WHERE `SCHEMA_NAME`='Schema')) AND `c`.`DEFINE` LIKE '%\"LuckyNumber\"%');" + EOL +
                "sqlstr VARCHAR;" + EOL +
                "BEGIN" + EOL +
                "FOR fk IN fk_cursor LOOP" + EOL +
                "sqlstr:='ALTER TABLE `Schema`.`People` DROP CONSTRAINT ' || `fk`.`CONS_NAME`;" + EOL +
                "EXECUTE IMMEDIATE sqlstr;" + EOL +
                "END LOOP;" + EOL +
                "END;" + EOL +
                "ALTER TABLE `Schema`.`People` ALTER COLUMN `LuckyNumber` INT NOT NULL;" + EOL +
                "ALTER TABLE `Schema`.`People` ALTER COLUMN `LuckyNumber` SET DEFAULT 7;" + EOL,
                Sql);
        }

        public override void AlterColumnOperation_without_column_type()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "LuckyNumber",
                    ColumnType = "int",
                    ClrType = typeof(int)
                });

            Assert.Equal(
                "DECLARE" + EOL +
                "CURSOR fk_cursor IS" + EOL +
                "(SELECT `c`.`CONS_NAME` FROM `sys_constraints` `c` INNER JOIN `sys_tables` `t` ON `c`.`TABLE_ID`=`t`.`TABLE_ID` WHERE `t`.`TABLE_ID`=(SELECT `TABLE_ID` FROM `sys_tables` WHERE `TABLE_NAME`= 'People') AND `c`.`DEFINE` LIKE '%\"LuckyNumber\"%');" + EOL +
                "sqlstr VARCHAR;" + EOL +
                "BEGIN" + EOL +
                "FOR fk IN fk_cursor LOOP" + EOL +
                "sqlstr:='ALTER TABLE `People` DROP CONSTRAINT ' || `fk`.`CONS_NAME`;" + EOL +
                "EXECUTE IMMEDIATE sqlstr;" + EOL +
                "END LOOP;" + EOL +
                "END;" + EOL +
                "ALTER TABLE `People` ALTER COLUMN `LuckyNumber` INT NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void AlterColumnOperation_with_identity()
        {
            Generate(
                new AlterColumnOperation
                {
                    Table = "People",
                    Name = "Id",
                    ColumnType = "int",
                    ClrType = typeof(int),
                    [XuGuDbFullAnnotationNames.Instance.ValueGenerationStrategy] =
                        XuGuDbValueGenerationStrategy.IdentityColumn
                });

            Assert.Equal(
                "DECLARE" + EOL +
                "CURSOR fk_cursor IS" + EOL +
                "(SELECT `c`.`CONS_NAME` FROM `sys_constraints` `c` INNER JOIN `sys_tables` `t` ON `c`.`TABLE_ID`=`t`.`TABLE_ID` WHERE `t`.`TABLE_ID`=(SELECT `TABLE_ID` FROM `sys_tables` WHERE `TABLE_NAME`= 'People') AND `c`.`DEFINE` LIKE '%\"Id\"%');" + EOL +
                "sqlstr VARCHAR;" + EOL +
                "BEGIN" + EOL +
                "FOR fk IN fk_cursor LOOP" + EOL +
                "sqlstr:='ALTER TABLE `People` DROP CONSTRAINT ' || `fk`.`CONS_NAME`;" + EOL +
                "EXECUTE IMMEDIATE sqlstr;" + EOL +
                "END LOOP;" + EOL +
                "END;" + EOL +
                "ALTER TABLE `People` ALTER COLUMN `Id` INT NOT NULL;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateDatabaseOperation()
        {
            Generate(new XuGuDbCreateDatabaseOperation { Name = "Northwind" });

            Assert.Equal("CREATE DATABASE IF NOT EXISTS `Northwind`;" + EOL,Sql);
        }

        public override void CreateIndexOperation_nonunique()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Columns = new[] { "Name" },
                    IsUnique = false
                });

            Assert.Equal(
                "CREATE INDEX IF NOT EXISTS `IX_People_Name` ON `People` (`Name`);" + EOL,
                Sql);
        }

        public override void CreateIndexOperation_unique()
        {
            Generate(
                new CreateIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Schema = "Schema",
                    Columns = new[] { "FirstName", "LastName" },
                    IsUnique = true
                });

            Assert.Equal(
                "CREATE UNIQUE INDEX IF NOT EXISTS `IX_People_Name` ON `Schema`.`People` (`FirstName`, `LastName`);" + EOL,
                Sql);
        }

        [Fact]
        public virtual void CreateSchemaOperation()
        {
            Generate(new XuGuDbCreateSchemaOperation { Name = "my" });
            //var xuGuMigrationsSqlGenerator = SqlGenerator as XuGuDbMigrationsSqlGenerator;
            //xuGuMigrationsSqlGenerator.Generate(new EnsureSchemaOperation { Name = "my" },true);

            Assert.Equal(
                "BEGIN" + EOL +
                "IF NOT EXISTS (SELECT 1 FROM `ALL_SCHEMAS` WHERE `SCHEMA_NAME` = 'my') THEN" + EOL +
                "CREATE SCHEMA `my`;" + EOL +
                "END IF;" + EOL +
                "END;",
                Sql);
        }

        public override void DropColumnOperation()
        {
            Generate(
                new DropColumnOperation
                {
                    Table = "People",
                    Schema = "Schema",
                    Name = "LuckyNumber"
                });

            Assert.Equal(
                "DECLARE" + EOL +
                "CURSOR fk_cursor IS" + EOL +
                "(SELECT `c`.`CONS_NAME` FROM `sys_constraints` `c` INNER JOIN `sys_tables` `t` ON `c`.`TABLE_ID`=`t`.`TABLE_ID` WHERE `t`.`TABLE_ID`=(SELECT `TABLE_ID` FROM `sys_tables` WHERE `TABLE_NAME`= 'People' AND `SCHEMA_ID` IN (SELECT `SCHEMA_ID` FROM `sys_schemas` WHERE `SCHEMA_NAME`='Schema')) AND `c`.`DEFINE` LIKE '%\"LuckyNumber\"%');" + EOL +
                "sqlstr VARCHAR;" + EOL +
                "BEGIN" + EOL +
                "FOR fk IN fk_cursor LOOP" + EOL +
                "sqlstr:='ALTER TABLE `Schema`.`People` DROP CONSTRAINT ' || `fk`.`CONS_NAME`;" + EOL +
                "EXECUTE IMMEDIATE sqlstr;" + EOL +
                "END LOOP;" + EOL +
                "END;" + EOL +
                "ALTER TABLE `Schema`.`People` DROP COLUMN `LuckyNumber`;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void DropDatabaseOperation()
        {
            Generate(new XuGuDbDropDatabaseOperation { Name = "Northwind" });

            Assert.Equal("DROP DATABASE `Northwind`;" + EOL,Sql);
        }

        public override void DropIndexOperation()
        {
            Generate(
                new DropIndexOperation
                {
                    Name = "IX_People_Name",
                    Table = "People",
                    Schema = "Schema"
                });

            Assert.Equal(
                "DROP INDEX `Schema`.`People`.`IX_People_Name`;" + EOL,
                Sql);
        }

        //[Fact]
        //public virtual void MoveSequenceOperation()
        //{
        //    Generate(
        //        new RenameSequenceOperation
        //        {
        //            Name = "EntityFrameworkHiLoSequence",
        //            Schema = "dbo",
        //            NewSchema = "my"
        //        });

        //    Assert.Equal(
        //        "ALTER SCHEMA [my] TRANSFER [dbo].[EntityFrameworkHiLoSequence];" + EOL,
        //        Sql);
        //}

        //[Fact]
        //public virtual void MoveTableOperation()
        //{
        //    Generate(
        //        new RenameTableOperation
        //        {
        //            Name = "People",
        //            Schema = "dbo",
        //            NewSchema = "hr"
        //        });

        //    Assert.Equal(
        //        "ALTER SCHEMA [hr] TRANSFER [dbo].[People];" + EOL,
        //        Sql);
        //}

        [Fact]
        public virtual void RenameColumnOperation()
        {
            Generate(
                new RenameColumnOperation
                {
                    Table = "People",
                    Schema = "Schema",
                    Name = "Name",
                    NewName = "FullName"
                });

            Assert.Equal(
                "ALTER TABLE `Schema`.`People` RENAME COLUMN `Name` TO `FullName`;" + EOL,
                Sql);
        }

        [Fact]
        public virtual void RenameIndexOperation()
        {
            Generate(
                new RenameIndexOperation
                {
                    Table = "People",
                    Schema = "Schema",
                    Name = "IX_People_Name",
                    NewName = "IX_People_FullName"
                });

            Assert.Equal(
                "ALTER INDEX `Schema`.`People`.`IX_People_Name` RENAME TO `IX_People_FullName`;" + EOL,
                Sql);
        }

        //[Fact]
        //public virtual void RenameSequenceOperation()
        //{
        //    Generate(
        //        new RenameSequenceOperation
        //        {
        //            Name = "EntityFrameworkHiLoSequence",
        //            Schema = "dbo",
        //            NewName = "MySequence"
        //        });

        //    Assert.Equal(
        //        "EXEC sp_rename N'dbo.EntityFrameworkHiLoSequence', N'MySequence';" + EOL,
        //        Sql);
        //}

        [Fact]
        public virtual void RenameTableOperation()
        {
            Generate(
                new RenameTableOperation
                {
                    Name = "People",
                    Schema = "Schema",
                    NewName = "People_New"
                });

            Assert.Equal(
                "ALTER TABLE `Schema`.`People` RENAME TO `People_New`;" + EOL,
                Sql);
        }
    }
}
