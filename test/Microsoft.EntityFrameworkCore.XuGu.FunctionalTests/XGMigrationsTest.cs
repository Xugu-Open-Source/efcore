// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class XGMigrationsTest
    {
        [Fact]
        public async Task Empty_Migration_Creates_Database()
        {
            using (var testDatabase = await XGTestStore.CreateScratchAsync(createDatabase: true))
            {
                using (var context = CreateContext(testDatabase))
                {
                    context.Database.Migrate();

                    Assert.True(context.GetService<IRelationalDatabaseCreator>().Exists());
                }
            }
        }

        private static BloggingContext CreateContext(XGTestStore testStore)
        {
            var serviceProvider =
                new ServiceCollection()
                    .AddEntityFrameworkXG()
                    .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseXG(testStore.ConnectionString)
                .UseInternalServiceProvider(serviceProvider);

            return new BloggingContext(optionsBuilder.Options);
        }

        private class BloggingContext : DbContext
        {
            public BloggingContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Blog> Blogs { get; set; }

            public class Blog
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }

        [DbContext(typeof(BloggingContext))]
        [Migration("00000000000000_Empty")]
        public class EmptyMigration : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
            }
        }
    }
}
