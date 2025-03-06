// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class CommandConfigurationTest : IClassFixture<CommandConfigurationTest.CommandConfigurationTestFixture>
    {
        private const string DatabaseName = "NotKettleChips";

        private readonly CommandConfigurationTestFixture _fixture;

        public CommandConfigurationTest(CommandConfigurationTestFixture fixture)
        {
            _fixture = fixture;
            _fixture.CreateDatabase();
        }

        public class CommandConfigurationTestFixture : IDisposable
        {
            public IServiceProvider ServiceProvider { get; } = new ServiceCollection()
                .AddEntityFrameworkXuGuDb()
                .BuildServiceProvider();

            public virtual void CreateDatabase()
            {
                XuGuDbTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        using (var context = new ChipsContext(ServiceProvider))
                        {
                            context.Database.EnsureDeleted();
                            context.Database.EnsureCreated();
                        }
                    });
            }

            public void Dispose()
            {
                using (var context = new ChipsContext(ServiceProvider))
                {
                    context.Database.EnsureDeleted();
                }
            }
        }

        [Fact]
        public void Constructed_select_query_CommandBuilder_throws_when_negative_CommandTimeout_is_used()
        {
            using (var context = new ConfiguredChipsContext(_fixture.ServiceProvider))
            {
                Assert.Throws<ArgumentException>(() => context.Database.SetCommandTimeout(-5));
            }
        }

        [ConditionalTheory]
        [XuGuDbCondition(XuGuDbCondition.SupportsSequences)]
        [InlineData(51, 6)]
        [InlineData(50, 5)]
        [InlineData(20, 2)]
        [InlineData(2, 1)]
        public void Keys_generated_in_batches(int count, int expected)
        {
            var loggerFactory = new TestSqlLoggerFactory();
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXuGuDb()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

            using (var context = new ConfiguredChipsContext(serviceProvider))
            {
                context.Database.EnsureCreated();

                for (var i = 0; i < count; i++)
                {
                    context.Chips.Add(new KettleChips { BestBuyDate = DateTime.Now, Name = "Doritos Locos Tacos " + i });
                }
                context.SaveChanges();
            }

            Assert.Equal(expected, CountSqlLinesContaining("SELECT NEXT VALUE FOR"));
        }

        public int CountSqlLinesContaining(string searchTerm) 
            => CountLinesContaining(Sql, searchTerm);

        public int CountLinesContaining(string source, string searchTerm)
        {
            var text = source.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            var matchQuery = from word in text
                             where word.Contains(searchTerm)
                             select word;

            return matchQuery.Count();
        }

        private class ChipsContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public ChipsContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<KettleChips> Chips { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseXuGuDb(XuGuDbTestStore.CreateConnectionString(DatabaseName))
                    .UseInternalServiceProvider(_serviceProvider);

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                if (TestEnvironment.GetFlag(nameof(XuGuDbCondition.SupportsSequences)) ?? true)
                {
                    modelBuilder.ForXuGuDbUseSequenceHiLo();
                }
            }
        }

        private class KettleChips
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime BestBuyDate { get; set; }
        }

        private class ConfiguredChipsContext : ChipsContext
        {
            public ConfiguredChipsContext(IServiceProvider serviceProvider)
                : base(serviceProvider)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => base.OnConfiguring(
                    optionsBuilder.UseXuGuDb("Database=" + DatabaseName, b => b.CommandTimeout(77)));
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
