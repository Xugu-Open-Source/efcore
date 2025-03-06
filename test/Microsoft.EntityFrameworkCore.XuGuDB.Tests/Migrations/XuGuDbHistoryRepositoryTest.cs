// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests.Migrations
{
    public class XuGuDbHistoryRepositoryTest
    {
        private static string EOL => Environment.NewLine;

        [Fact]
        public void GetCreateScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateScript();

            Assert.Equal(
                "CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (" + EOL +
                "    `MigrationId` VARCHAR(150) NOT NULL," + EOL +
                "    `ProductVersion` VARCHAR(32) NOT NULL," + EOL +
                "    CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (`MigrationId`)" + EOL +
                ");" + EOL,
                sql);
        }

        [Fact]
        public void GetCreateScript_works_with_schema()
        {
            var sql = CreateHistoryRepository("my").GetCreateScript();

            Assert.Equal(
                "BEGIN" + EOL +
                "IF NOT EXISTS (SELECT 1 FROM `ALL_SCHEMAS` WHERE `SCHEMA_NAME` = 'my') THEN" + EOL +
                "CREATE SCHEMA `my`;" + EOL +
                "END IF;" + EOL +
                "CREATE TABLE IF NOT EXISTS `my`.`__EFMigrationsHistory` (" + EOL +
                "    `MigrationId` VARCHAR(150) NOT NULL," + EOL +
                "    `ProductVersion` VARCHAR(32) NOT NULL," + EOL +
                "    CONSTRAINT PK___EFMigrationsHistory PRIMARY KEY (`MigrationId`)" + EOL +
                ");" + EOL +
                "END;",
                sql);
        }

        [Fact]
        public void GetDeleteScript_works()
        {
            var sql = CreateHistoryRepository().GetDeleteScript("Migration1");

            Assert.Equal(
                "DELETE FROM `__EFMigrationsHistory`" + EOL +
                "WHERE `MigrationId` = 'Migration1';" + EOL,
                sql);
        }

        [Fact]
        public void GetInsertScript_works()
        {
            var sql = CreateHistoryRepository().GetInsertScript(
                new HistoryRow("Migration1", "`7`.`0`.0"));

            Assert.Equal(
                "INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)" + EOL +
                "VALUES ('Migration1', '`7`.`0`.0');" + EOL,
                sql);
        }

        [Fact]
        public void GetBeginIfNotExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetBeginIfNotExistsScript("Migration1");

            Assert.Equal(
                "BEGIN" + EOL +
                "IF NOT EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = 'Migration1') THEN" + EOL,
                sql);
        }

        [Fact]
        public void GetBeginIfExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetBeginIfExistsScript("Migration1");

            Assert.Equal(
                "BEGIN" + EOL +
                "IF EXISTS(SELECT * FROM `__EFMigrationsHistory` WHERE `MigrationId` = 'Migration1') THEN" + EOL,
                sql);
        }

        [Fact]
        public void GetEndIfScript_works()
        {
            var sql = CreateHistoryRepository().GetEndIfScript();

            Assert.Equal("END;" + EOL, sql);
        }

        private static IHistoryRepository CreateHistoryRepository(string schema = null)
        {
            var annotationsProvider = new XuGuDbAnnotationProvider();
            var sqlGenerator = new XuGuDbSqlGenerationHelper();
            var typeMapper = new XuGuDbTypeMapper();

            var commandBuilderFactory = new RelationalCommandBuilderFactory(
                new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                new DiagnosticListener("Fake"),
                typeMapper);

            return new XuGuDbHistoryRepository(
                Mock.Of<IRelationalDatabaseCreator>(),
                Mock.Of<IRawSqlCommandBuilder>(),
                Mock.Of<IXuGuDbConnection>(),
                new DbContextOptions<DbContext>(
                    new Dictionary<Type, IDbContextOptionsExtension>
                    {
                        {
                            typeof(XuGuDbOptionsExtension),
                            new XuGuDbOptionsExtension { MigrationsHistoryTableSchema = schema }
                        }
                    }),
                new MigrationsModelDiffer(
                    new XuGuDbTypeMapper(),
                    annotationsProvider,
                    new XuGuDbMigrationsAnnotationProvider()),
                new XuGuDbMigrationsSqlGenerator(
                    commandBuilderFactory,
                    new XuGuDbSqlGenerationHelper(),
                    typeMapper,
                    annotationsProvider),
                annotationsProvider,
                sqlGenerator);
        }

        private class Context : DbContext
        {
        }
    }
}
