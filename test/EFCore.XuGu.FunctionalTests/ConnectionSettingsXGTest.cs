// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class ConnectionSettingsXGTest
    {
        [ConditionalTheory]
        [InlineData("'850368d8-93ea-4023-acc7-6fa6e4c3b27f'", null)]
        public virtual void Insert_and_read_Guid_value(string sqlEquivalent, string supportedServerVersion)
        {
            if (supportedServerVersion != null &&
                !AppConfig.ServerVersion.Supports.Version(ServerVersion.Parse(supportedServerVersion)))
            {
                return;
            }

            using var context = CreateContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.SimpleGuidEntities.Add(new SimpleGuidEntity { GuidValue = new Guid("850368D8-93EA-4023-ACC7-6FA6E4C3B27F") });
            context.SaveChanges();

            var result = context.SimpleGuidEntities
                .Where(e => e.GuidValue == new Guid("850368D8-93EA-4023-ACC7-6FA6E4C3B27F"))
                .ToList();

            var sqlResult = context.SimpleGuidEntities
                .FromSqlRaw("select * from `SimpleGuidEntities` where `GuidValue` = " + sqlEquivalent)
                .ToList();

            Assert.Single(result);
            Assert.Equal(new Guid("850368D8-93EA-4023-ACC7-6FA6E4C3B27F"), result[0].GuidValue);
            Assert.Single(sqlResult);
        }

        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddEntityFrameworkXG()
            .BuildServiceProvider();

        protected ConnectionSettingsContext CreateContext()
            => new ConnectionSettingsContext(_serviceProvider, "ConnectionSettings");
    }

    public class ConnectionSettingsContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _databaseName;

        public ConnectionSettingsContext(IServiceProvider serviceProvider, string databaseName)
        {
            _serviceProvider = serviceProvider;
            _databaseName = databaseName;
        }

        public DbSet<SimpleGuidEntity> SimpleGuidEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseXG(XGTestStore.CreateConnectionString(_databaseName, false), AppConfig.ServerVersion)
                .UseInternalServiceProvider(_serviceProvider);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<SimpleGuidEntity>();
    }

    public class SimpleGuidEntity
    {
        public int SimpleGuidEntityId { get; set; }
        public Guid GuidValue { get; set; }
    }
}
