using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using Xunit;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseXG(
                "IP=127.0.0.1;DB=LMSEFCore50;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=off;CHAR_SET=GBK;",
                ServerVersion.AutoDetect("IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=off;CHAR_SET=GBK;")
            ).Options;

            using (var context = new MyDbContext(options))
            {
                // Ensure database is deleted to start fresh
                context.Database.EnsureDeleted();

                // Act
                context.Database.Migrate();

                // Assert
                var migrator = context.GetService<IMigrator>();
                var appliedMigrations = context.Database.GetAppliedMigrations();

                // Check if the latest migration is applied
                Assert.Contains("LatestMigrationName", appliedMigrations);
            }
        }
    }
}
