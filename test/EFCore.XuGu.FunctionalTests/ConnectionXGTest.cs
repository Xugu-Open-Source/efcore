using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class ConnectionXGTest
    {
        [ConditionalFact]
        public virtual void SetConnectionString()
        {
            var correctConnectionString = XGTestStore.CreateConnectionString("ConnectionTest");

            var csb = new XGConnectionStringBuilder(correctConnectionString);
            //var correctPort = csb.Port;

            //// Set an incorrect port, where no database server is listening.
            //csb.Port = 65123;

            var incorrectConnectionString = csb.ConnectionString;
            using var context = CreateContext(incorrectConnectionString);

            context.Database.SetConnectionString(correctConnectionString);

            var connection = (XGConnection)context.Database.GetDbConnection();
            csb = new XGConnectionStringBuilder(connection.ConnectionString);

            Assert.Equal(csb.ConnectionString, correctConnectionString);
        }

        [ConditionalFact]
        public virtual void SetConnectionString_affects_master_connection()
        {
            var correctConnectionString = XGTestStore.CreateConnectionString("ConnectionTest");

            // Set an incorrect port, where no database server is listening.
            var csb = new XGConnectionStringBuilder(correctConnectionString);

            var incorrectConnectionString = csb.ConnectionString;
            using var context = CreateContext(incorrectConnectionString);

            context.Database.SetConnectionString(correctConnectionString);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddEntityFrameworkXG()
            .BuildServiceProvider();

        protected ConnectionXGContext CreateContext(string connectionString)
            => new ConnectionXGContext(_serviceProvider, connectionString);
    }

    public class ConnectionXGContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _connectionString;

        public ConnectionXGContext(IServiceProvider serviceProvider, string connectionString)
        {
            _serviceProvider = serviceProvider;
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseXG(_connectionString, AppConfig.ServerVersion)
                .UseInternalServiceProvider(_serviceProvider);
    }
}
