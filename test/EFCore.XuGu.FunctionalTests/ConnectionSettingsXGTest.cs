using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using EntityFrameworkCore.XuGu.Storage;
using EntityFrameworkCore.XuGu.Tests;
using Xunit;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class ConnectionSettingsXGTest
    {
        [ConditionalTheory]
        [InlineData("'850368D893EA4023ACC76FA6E4C3B27F'", null)]
        //[InlineData(XGGuidFormat.Char36, "'850368D8-93EA-4023-ACC7-6FA6E4C3B27F'", null)]
        //[InlineData(XGGuidFormat.Char32, "'850368D893EA4023ACC76FA6E4C3B27F'", null)]
        //[InlineData(XGGuidFormat.Binary16, "UUID_TO_BIN('850368D8-93EA-4023-ACC7-6FA6E4C3B27F', 0)", "8.0.0-XG")]
        //[InlineData(XGGuidFormat.Binary16, "X'850368D893EA4023ACC76FA6E4C3B27F'", null)]
        //[InlineData(XGGuidFormat.TimeSwapBinary16, "UUID_TO_BIN('850368D8-93EA-4023-ACC7-6FA6E4C3B27F', 1)", "8.0.0-XG")]
        //[InlineData(XGGuidFormat.TimeSwapBinary16, "X'402393EA850368D8ACC76FA6E4C3B27F'", null)]
        //[InlineData(XGGuidFormat.LittleEndianBinary16, "X'D8680385EA932340ACC76FA6E4C3B27F'", null)]
        //[InlineData(XGGuidFormat.None, "X'D8680385EA932340ACC76FA6E4C3B27F'", null)]
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
                .FromSqlRaw(@"select * from `SimpleGuidEntities` where `GuidValue` = " + string.Format(sqlEquivalent, new Guid("850368D8-93EA-4023-ACC7-6FA6E4C3B27F")))
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
