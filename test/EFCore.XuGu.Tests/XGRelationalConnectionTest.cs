// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Tests;
using Microsoft.EntityFrameworkCore.XuGu.TestUtilities.FakeProvider;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu;

public class XGRelationalConnectionTest
{
    [Fact]
    public void Creates_XG_Server_connection_string()
    {
        using var connection = CreateConnection();

        Assert.IsType<XGConnection>(connection.DbConnection);
    }

    [Fact]
    public void Accepts_named_connection_string()
    {
        using var connection = CreateConnection(
            new DbContextOptionsBuilder()
                .UseXG(@"name=NamedConnectionString", AppConfig.ServerVersion)
                .UseApplicationServiceProvider(
                    new ServiceCollection()
                        .AddSingleton<IConfiguration, FakeConfiguration>()
                        .BuildServiceProvider())
                .Options);
    }

    //[Fact]
    //public void Uses_DbDataSource_from_DbContextOptions()
    //{
    //    using var dataSource = new XGDataSourceBuilder("Server=FakeHost;AllowUserVariables=True;UseAffectedRows=False").Build();

    //    var serviceCollection = new ServiceCollection();

    //    serviceCollection
    //        .AddXGDataSource("Server=FakeHost")
    //        .AddDbContext<FakeDbContext>(o => o.UseXG(dataSource, AppConfig.ServerVersion));

    //    using var serviceProvider = serviceCollection.BuildServiceProvider();

    //    using var scope1 = serviceProvider.CreateScope();
    //    var context1 = scope1.ServiceProvider.GetRequiredService<FakeDbContext>();
    //    var relationalConnection1 = (XGRelationalConnection)context1.GetService<IRelationalConnection>()!;
    //    Assert.Same(dataSource, relationalConnection1.DbDataSource);

    //    var connection1 = context1.GetService<FakeDbContext>().Database.GetDbConnection();
    //    Assert.Equal("Server=FakeHost;Allow User Variables=True;Use Affected Rows=False", connection1.ConnectionString);

    //    using var scope2 = serviceProvider.CreateScope();
    //    var context2 = scope2.ServiceProvider.GetRequiredService<FakeDbContext>();
    //    var relationalConnection2 = (XGRelationalConnection)context2.GetService<IRelationalConnection>()!;
    //    Assert.Same(dataSource, relationalConnection2.DbDataSource);

    //    var connection2 = context2.GetService<FakeDbContext>().Database.GetDbConnection();
    //    Assert.Equal("Server=FakeHost;Allow User Variables=True;Use Affected Rows=False", connection2.ConnectionString);
    //}

    //[Fact]
    //public void Uses_DbDataSource_from_DbContextOptions_without_mandatory_settings()
    //{
    //    using var dataSource = new XGDataSourceBuilder("Server=FakeHost").Build();

    //    var serviceCollection = new ServiceCollection();

    //    serviceCollection
    //        .AddXGDataSource("Server=FakeHost;Allow User Variables=True;Use Affected Rows=False")
    //        .AddDbContext<FakeDbContext>(o => o.UseXG(dataSource, AppConfig.ServerVersion));

    //    using var serviceProvider = serviceCollection.BuildServiceProvider();

    //    using var scope = serviceProvider.CreateScope();
    //    var context = scope.ServiceProvider.GetRequiredService<FakeDbContext>();

    //    Assert.Equal(
    //        "The connection string of a connection used by Microsoft.EntityFrameworkCore.XuGu must contain \"AllowUserVariables=True;UseAffectedRows=False\".",
    //        Assert.Throws<InvalidOperationException>(
    //                () => (XGRelationalConnection)context.GetService<IRelationalConnection>()!)
    //            .Message);
    //}

    //[Fact]
    //public void Uses_DbDataSource_from_ApplicationServiceProvider()
    //{
    //    var serviceCollection = new ServiceCollection();

    //    serviceCollection
    //        .AddXGDataSource("Server=FakeHost;Allow User Variables=True;Use Affected Rows=False")
    //        .AddDbContext<FakeDbContext>(o => o.UseXG(AppConfig.ServerVersion));

    //    using var serviceProvider = serviceCollection.BuildServiceProvider();

    //    var dataSource = serviceProvider.GetRequiredService<XGDataSource>();

    //    using var scope1 = serviceProvider.CreateScope();
    //    var context1 = scope1.ServiceProvider.GetRequiredService<FakeDbContext>();
    //    var relationalConnection1 = (XGRelationalConnection)context1.GetService<IRelationalConnection>()!;
    //    Assert.Same(dataSource, relationalConnection1.DbDataSource);

    //    var connection1 = context1.GetService<FakeDbContext>().Database.GetDbConnection();
    //    Assert.Equal("Server=FakeHost;Allow User Variables=True;Use Affected Rows=False", connection1.ConnectionString);

    //    using var scope2 = serviceProvider.CreateScope();
    //    var context2 = scope2.ServiceProvider.GetRequiredService<FakeDbContext>();
    //    var relationalConnection2 = (XGRelationalConnection)context2.GetService<IRelationalConnection>()!;
    //    Assert.Same(dataSource, relationalConnection2.DbDataSource);

    //    var connection2 = context2.GetService<FakeDbContext>().Database.GetDbConnection();
    //    Assert.Equal("Server=FakeHost;Allow User Variables=True;Use Affected Rows=False", connection2.ConnectionString);
    //}

    //[Fact]
    //public void Uses_DbDataSource_from_ApplicationServiceProvider_without_mandatory_settings()
    //{
    //    var serviceCollection = new ServiceCollection();

    //    serviceCollection
    //        .AddXGDataSource("Server=FakeHost")
    //        .AddDbContext<FakeDbContext>(o => o.UseXG(AppConfig.ServerVersion));

    //    using var serviceProvider = serviceCollection.BuildServiceProvider();

    //    var dataSource = serviceProvider.GetRequiredService<XGDataSource>();

    //    Assert.Equal("Server=FakeHost", dataSource.ConnectionString);

    //    using var scope = serviceProvider.CreateScope();
    //    var context = scope.ServiceProvider.GetRequiredService<FakeDbContext>();

    //    Assert.Equal(
    //        "The connection string of a connection used by Microsoft.EntityFrameworkCore.XuGu must contain \"AllowUserVariables=True;UseAffectedRows=False\".",
    //        Assert.Throws<InvalidOperationException>(
    //                () => (XGRelationalConnection)context.GetService<IRelationalConnection>()!)
    //            .Message);
    //}

    //[Fact]
    //public void Uses_correct_DbDataSource_from_ApplicationServiceProvider_with_cached_DbContextOptions_extension()
    //{
    //    var serviceCollection1 = new ServiceCollection();

    //    serviceCollection1
    //        .AddXGDataSource("Server=FakeHost1;Allow User Variables=True;Use Affected Rows=False")
    //        .AddDbContext<FakeDbContext>(o => o.UseXG(AppConfig.ServerVersion));

    //    using (var serviceProvider1 = serviceCollection1.BuildServiceProvider())
    //    {
    //        var dataSource1 = serviceProvider1.GetRequiredService<XGDataSource>();
    //        Assert.Equal("Server=FakeHost1;Allow User Variables=True;Use Affected Rows=False", dataSource1.ConnectionString);

    //        var context1 = serviceProvider1.GetRequiredService<FakeDbContext>();

    //        var xgOptions1 = context1.GetService<IXGOptions>();
    //        Assert.Null(xgOptions1.DataSource);

    //        var relationalConnection1 = (XGRelationalConnection)context1.GetService<IRelationalConnection>()!;
    //        Assert.Same(dataSource1, relationalConnection1.DbDataSource);
    //    }

    //    var serviceCollection2 = new ServiceCollection();

    //    serviceCollection2
    //        .AddXGDataSource("Server=FakeHost2;Allow User Variables=True;Use Affected Rows=False")
    //        .AddDbContext<FakeDbContext>(o => o.UseXG(AppConfig.ServerVersion));

    //    using (var serviceProvider2 = serviceCollection2.BuildServiceProvider())
    //    {
    //        var dataSource2 = serviceProvider2.GetRequiredService<XGDataSource>();
    //        Assert.Equal("Server=FakeHost2;Allow User Variables=True;Use Affected Rows=False", dataSource2.ConnectionString);

    //        var context2 = serviceProvider2.GetRequiredService<FakeDbContext>();

    //        var xgOptions2 = context2.GetService<IXGOptions>();
    //        Assert.Null(xgOptions2.DataSource);

    //        var relationalConnection2 = (XGRelationalConnection)context2.GetService<IRelationalConnection>()!;
    //        Assert.Same(dataSource2, relationalConnection2.DbDataSource);
    //    }
    //}

    [Fact]
    public void Can_create_master_connection_with_connection_string()
    {
        using var connection = CreateConnection();
        using var master = connection.CreateMasterConnection();

        Assert.Equal(
            @"Server=localhost;User ID=some_user;Password=some_password;Database=;Pooling=False;Allow User Variables=True;Use Affected Rows=False",
            master.ConnectionString);
    }

    // [Fact]
    // public void Can_create_master_connection_with_connection_string_and_alternate_admin_db()
    // {
    //     var options = new DbContextOptionsBuilder()
    //         .UseXG(
    //             @"Server=localhost;Database=XGConnectionTest;User ID=some_user;Password=some_password",
    //             b => b.UseMasterDatabase("template0"))
    //         .Options;
    //
    //     using var connection = CreateConnection(options);
    //     using var master = connection.CreateMasterConnection();
    //
    //     Assert.Equal(
    //         @"Server=localhost;Database=template0;User ID=some_user;Password=some_password;Pooling=False;Multiplexing=False",
    //         master.ConnectionString);
    // }

    // CHECK: Do we need to fix this test?
    //
    // [Theory]
    // [InlineData("false")]
    // [InlineData("False")]
    // [InlineData("FALSE")]
    // public void CurrentAmbientTransaction_returns_null_with_AutoEnlist_set_to_false(string falseValue)
    // {
    //     var options = new DbContextOptionsBuilder()
    //         .UseXG(
    //             @"Server=localhost;Database=XGConnectionTest;User ID=some_user;Password=some_password;Auto Enlist=" + falseValue,
    //             AppConfig.ServerVersion)
    //         .Options;
    //
    //     Transaction.Current = new CommittableTransaction();
    //
    //     using var connection = CreateConnection(options);
    //     Assert.Null(connection.CurrentAmbientTransaction);
    //
    //     Transaction.Current = null;
    // }

    [Theory]
    [InlineData(";Auto Enlist=true")]
    [InlineData("")] // Auto Enlist is true by default
    public void CurrentAmbientTransaction_returns_transaction_with_AutoEnlist_enabled(string autoEnlist)
    {
        var options = new DbContextOptionsBuilder()
            .UseXG(
                @"Server=localhost;Database=XGConnectionTest;User ID=some_user;Password=some_password" + autoEnlist,
                AppConfig.ServerVersion)
            .Options;

        var transaction = new CommittableTransaction();
        Transaction.Current = transaction;

        using var connection = CreateConnection(options);
        Assert.Equal(transaction, connection.CurrentAmbientTransaction);

        Transaction.Current = null;
    }

    // INFO: We currently don't implement IXGRelationalConnection.CloneWith.
    //
    // [ConditionalFact]
    // public void CloneWith_with_connection_and_connection_string()
    // {
    //     var services = XGTestHelpers.Instance.CreateContextServices(
    //         new DbContextOptionsBuilder()
    //             .UseXG("Server=localhost;Database=DummyDatabase", AppConfig.ServerVersion)
    //             .Options);
    //
    //     var relationalConnection = (IXGRelationalConnection)services.GetRequiredService<IRelationalConnection>();
    //
    //     var clone = relationalConnection.CloneWith("Server=localhost;Database=DummyDatabase;Application Name=foo");
    //
    //     Assert.Equal("Server=localhost;Database=DummyDatabase;Application Name=foo", clone.ConnectionString);
    // }

    private static XGRelationalConnection CreateConnection(DbContextOptions options = null, DbDataSource dataSource = null)
    {
        options ??= new DbContextOptionsBuilder()
            .UseXG(ConnectionString, AppConfig.ServerVersion)
            .Options;

        foreach (var extension in options.Extensions)
        {
            extension.Validate(options);
        }

        var singletonOptions = new XGOptions();
        singletonOptions.Initialize(options);

        return new XGRelationalConnection(
            new RelationalConnectionDependencies(
                options,
                new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                    new LoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener"),
                    new XGLoggingDefinitions(),
                    new NullDbContextLogger()),
                new RelationalConnectionDiagnosticsLogger(
                    new LoggerFactory(),
                    new LoggingOptions(),
                    new DiagnosticListener("FakeDiagnosticListener"),
                    new XGLoggingDefinitions(),
                    new NullDbContextLogger(),
                    options),
                new NamedConnectionStringResolver(options),
                new RelationalTransactionFactory(
                    new RelationalTransactionFactoryDependencies(
                        new RelationalSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies()))),
                new CurrentDbContext(new FakeDbContext()),
                new RelationalCommandBuilderFactory(
                    new RelationalCommandBuilderDependencies(
                        new XGTypeMappingSource(
                            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>(),
                            singletonOptions),
                        new ExceptionDetector()))),
            new XGConnectionStringOptionsValidator(),
            singletonOptions);
    }

    private const string ConnectionString = @"Server=localhost;User ID=some_user;Password=some_password;Database=XGConnectionTest";

    private class FakeDbContext : DbContext
    {
        public FakeDbContext()
        {
        }

        public FakeDbContext(DbContextOptions<FakeDbContext> options)
            : base(options)
        {
        }
    }

    private class FakeConfiguration : IConfiguration
    {
        public IConfigurationSection GetSection(string key)
            => throw new NotImplementedException();

        public IEnumerable<IConfigurationSection> GetChildren()
            => throw new NotImplementedException();

        public IChangeToken GetReloadToken()
            => throw new NotImplementedException();

        public string this[string key]
        {
            get => ConnectionString;
            set => throw new NotImplementedException();
        }
    }
}
