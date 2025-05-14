// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            .UseXG(@"Server=localhost;User ID=some_user;Password=some_password;Database=XGConnectionTest", AppConfig.ServerVersion)
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
                        new ExceptionDetector())))/*,
            new XGConnectionStringOptionsValidator(),
            singletonOptions*/);
    }

    private const string ConnectionString = "Fake Connection String";

    private static IDbContextOptions CreateOptions(
        RelationalOptionsExtension optionsExtension = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(
                optionsExtension
                ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

        return optionsBuilder.Options;
    }

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
}
