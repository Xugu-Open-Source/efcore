// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class MigrationsXGFixture : MigrationsFixtureBase
    {
        private readonly DbContextOptions _options;

        public MigrationsXGFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXG()
                .BuildServiceProvider();

            var connectionStringBuilder = new XGConnectionStringBuilder()
            {
                ConnectionString = @"IP=127.0.0.1;DB=StateManagerBug;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=UTF8"
            };

            _options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseXG(connectionStringBuilder.ConnectionString).Options;
        }

        public override MigrationsContext CreateContext() => new MigrationsContext(_options);

        public override EmptyMigrationsContext CreateEmptyContext() => new EmptyMigrationsContext(_options);

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000001_Migration1")]
        private class Migration1 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                MigrationsFixtureBase.ActiveProvider = migrationBuilder.ActiveProvider;

                migrationBuilder
                    .CreateTable(
                        name: "Table1",
                        columns: x => new
                        {
                            Id = x.Column<int>()
                        })
                    .PrimaryKey(
                        name: "PK_Table1",
                        columns: x => x.Id);
            }

            protected override void Down(MigrationBuilder migrationBuilder)
                => migrationBuilder.DropTable("Table1");
        }

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000002_Migration2")]
        private class Migration2 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
                => migrationBuilder.RenameTable(
                    name: "Table1",
                    newName: "Table2");

            protected override void Down(MigrationBuilder migrationBuilder)
                => migrationBuilder.RenameTable(
                    name: "Table2",
                    newName: "Table1");
        }

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000003_Migration3")]
        private class Migration3 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                if (ActiveProvider == "Microsoft.EntityFrameworkCore.XG")
                {
                    migrationBuilder.Sql("CREATE DATABASE IF NOT EXISTS TransactionSuppressed", suppressTransaction: true);
                    migrationBuilder.Sql("DROP DATABASE IF EXISTS TransactionSuppressed", suppressTransaction: true);
                }
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
            }
        }
    }
}
