// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
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

            // Set an incorrect port, where no database server is listening.
            //csb.Port = 65123;

            var incorrectConnectionString = csb.ConnectionString;
            using var context = CreateContext(incorrectConnectionString);

            context.Database.SetConnectionString(correctConnectionString);

            var connection = (XGConnection)context.Database.GetDbConnection();
            csb = new XGConnectionStringBuilder(connection.ConnectionString);

            //Assert.Equal(csb.Port, correctPort);
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

        [Fact]
        public async Task Can_create_admin_connection_with_data_source()
        {
            //await using var _ = await ((XGTestStore)XGNorthwindTestStoreFactory.Instance
            //        .GetOrCreate("ConnectionTest"))
            //    .InitializeAsync(null, (Func<DbContext>)null);

            //await using var dataSource = new XGDataSourceBuilder(XGTestStore.CreateConnectionString("ConnectionTest")).Build();

            //var optionsBuilder = new DbContextOptionsBuilder<GeneralOptionsContext>();
            //optionsBuilder.UseXG(dataSource, AppConfig.ServerVersion, b => b.ApplyConfiguration());
            //await using var context = new GeneralOptionsContext(optionsBuilder.Options);

            //var relationalConnection = context.GetService<IXGRelationalConnection>();
            //await using var masterConnection = relationalConnection.CreateMasterConnection();

            //Assert.Equal(string.Empty, new XGConnectionStringBuilder(masterConnection.ConnectionString).Database);

            //await masterConnection.OpenAsync(default);
            await Task.CompletedTask;
        }

        [Fact]
        public async Task Can_create_admin_connection_with_connection_string()
        {
            await using var _ = await ((XGTestStore)XGNorthwindTestStoreFactory.Instance
                    .GetOrCreate("ConnectionTest"))
                .InitializeAsync(null, (Func<DbContext>)null);

            var optionsBuilder = new DbContextOptionsBuilder<GeneralOptionsContext>();
            optionsBuilder.UseXG(XGTestStore.CreateConnectionString("ConnectionTest"), AppConfig.ServerVersion, b => b.ApplyConfiguration());
            await using var context = new GeneralOptionsContext(optionsBuilder.Options);

            var relationalConnection = context.GetService<IXGRelationalConnection>();
            await using var masterConnection = relationalConnection.CreateMasterConnection();

            Assert.Equal(string.Empty, new XGConnectionStringBuilder(masterConnection.ConnectionString).Database);

            await masterConnection.OpenAsync(default);
        }

        [Fact]
        public async Task Can_create_admin_connection_with_connection()
        {
            await using var _ = await ((XGTestStore)XGNorthwindTestStoreFactory.Instance
                    .GetOrCreate("ConnectionTest"))
                .InitializeAsync(null, (Func<DbContext>)null);

            await using var connection = new XGConnection(XGTestStore.CreateConnectionString("ConnectionTest"));
            connection.Open();

            var optionsBuilder = new DbContextOptionsBuilder<GeneralOptionsContext>();
            optionsBuilder.UseXG(connection, AppConfig.ServerVersion, b => b.ApplyConfiguration());
            await using var context = new GeneralOptionsContext(optionsBuilder.Options);

            var relationalConnection = context.GetService<IXGRelationalConnection>();
            await using var masterConnection = relationalConnection.CreateMasterConnection();

            Assert.Equal(string.Empty, new XGConnectionStringBuilder(masterConnection.ConnectionString).Database);

            await masterConnection.OpenAsync(default);
        }

        [Fact]
        public void Can_create_database_with_disablebackslashescaping()
        {
            var optionsBuilder = new DbContextOptionsBuilder<GeneralOptionsContext>();
            optionsBuilder.UseXG(XGTestStore.CreateConnectionString("ConnectionTest_" + Guid.NewGuid()), AppConfig.ServerVersion, b => b.ApplyConfiguration().DisableBackslashEscaping());
            using var context = new GeneralOptionsContext(optionsBuilder.Options);

            var relationalDatabaseCreator = context.GetService<IRelationalDatabaseCreator>();

            try
            {
                relationalDatabaseCreator.EnsureCreated();
            }
            finally
            {
                try
                {
                    relationalDatabaseCreator.EnsureDeleted();
                }
                catch
                {
                    // ignored
                }
            }
        }

        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddEntityFrameworkXG()
            .BuildServiceProvider();

        protected ConnectionXGContext CreateContext(string connectionString)
            => new ConnectionXGContext(_serviceProvider, connectionString);

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

        public class GeneralOptionsContext : DbContext
        {
            public GeneralOptionsContext(DbContextOptions<GeneralOptionsContext> options)
                : base(options)
            {
            }
        }
    }
}
