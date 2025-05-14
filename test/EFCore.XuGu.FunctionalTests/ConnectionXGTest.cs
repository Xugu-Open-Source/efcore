// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
            var correctPort = csb.ConnectionString;

            // Set an incorrect port, where no database server is listening.
            //csb.Port = 65123;

            var incorrectConnectionString = csb.ConnectionString;
            using var context = CreateContext(incorrectConnectionString);

            context.Database.SetConnectionString(correctConnectionString);

            var connection = (XGConnection)context.Database.GetDbConnection();
            csb = new XGConnectionStringBuilder(connection.ConnectionString);

            //Assert.Equal(csb.ConnectionString, correctPort);
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
        public void Can_create_admin_connection_with_data_source()
        {
            using var _ = ((XGTestStore)XGNorthwindTestStoreFactory.Instance
                    .GetOrCreate("ConnectionTest"))
                .Initialize(null, (Func<DbContext>)null);

            //using var dataSource = new XGDataSourceBuilder(XGTestStore.CreateConnectionString("ConnectionTest")).Build();

            var dataSource = new XGConnectionStringBuilder(XGTestStore.CreateConnectionString("ConnectionTest"));

            var optionsBuilder = new DbContextOptionsBuilder<GeneralOptionsContext>();
            //optionsBuilder.UseXG(dataSource, AppConfig.ServerVersion, b => b.ApplyConfiguration());
            optionsBuilder.UseXG(dataSource.ConnectionString, AppConfig.ServerVersion, b => b.ApplyConfiguration());
            using var context = new GeneralOptionsContext(optionsBuilder.Options);

            var relationalConnection = context.GetService<IXGRelationalConnection>();
            using var masterConnection = relationalConnection.CreateMasterConnection();

            Assert.Null(new XGConnectionStringBuilder(masterConnection.ConnectionString).Database);

            masterConnection.Open();
        }

        [Fact]
        public void Can_create_admin_connection_with_connection_string()
        {
            using var _ = ((XGTestStore)XGNorthwindTestStoreFactory.Instance
                    .GetOrCreate("ConnectionTest"))
                .Initialize(null, (Func<DbContext>)null);

            var optionsBuilder = new DbContextOptionsBuilder<GeneralOptionsContext>();
            optionsBuilder.UseXG(XGTestStore.CreateConnectionString("ConnectionTest"), AppConfig.ServerVersion, b => b.ApplyConfiguration());
            using var context = new GeneralOptionsContext(optionsBuilder.Options);

            var relationalConnection = context.GetService<IXGRelationalConnection>();
            using var masterConnection = relationalConnection.CreateMasterConnection();

            Assert.Null(new XGConnectionStringBuilder(masterConnection.ConnectionString).Database);

            masterConnection.Open();
        }

        [Fact]
        public void Can_create_admin_connection_with_connection()
        {
            using var _ = ((XGTestStore)XGNorthwindTestStoreFactory.Instance
                    .GetOrCreate("ConnectionTest"))
                .Initialize(null, (Func<DbContext>)null);

            using var connection = new XGConnection(XGTestStore.CreateConnectionString("ConnectionTest"));
            connection.Open();

            var optionsBuilder = new DbContextOptionsBuilder<GeneralOptionsContext>();
            optionsBuilder.UseXG(connection, AppConfig.ServerVersion, b => b.ApplyConfiguration());
            using var context = new GeneralOptionsContext(optionsBuilder.Options);

            var relationalConnection = context.GetService<IXGRelationalConnection>();
            using var masterConnection = relationalConnection.CreateMasterConnection();

            Assert.Null(new XGConnectionStringBuilder(masterConnection.ConnectionString).Database);

            masterConnection.Open();
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
