// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Migrations.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.Migrations;

public class XGHistoryRepositoryTest
{
    [ConditionalFact]
    public void ExistsSql_respects_SchemaBehavior_explicit()
    {
        var sql = CreateHistoryRepository("IgnoreThisDefaultSchema")
            .ExistsSql;

        Assert.Equal(
            @"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='DummyDatabase' AND TABLE_NAME='__EFMigrationsHistory';",
            sql);
    }

    [ConditionalFact]
    public void ExistsSql_respects_SchemaBehavior()
    {
        var sql = CreateHistoryRepository("IgnoreThisExplicitSchema")
            .ExistsSql;

        Assert.Equal(
            @"SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='DummyDatabase' AND TABLE_NAME='__EFMigrationsHistory';",
            sql);
    }

    private static TestXGHistoryRepository CreateHistoryRepository(string schema = null)
        => (TestXGHistoryRepository)new TestDbContext(
                new DbContextOptionsBuilder(
                        XGTestHelpers.Instance.CreateOptions(builder => builder
                            .SchemaBehavior(XGSchemaBehavior.Ignore)
                            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema)))
                    .UseInternalServiceProvider(
                        XGTestHelpers.Instance.CreateServiceProvider(
                            new ServiceCollection()
                                .AddScoped<IHistoryRepository, TestXGHistoryRepository>()))
                    .Options)
            .GetService<IHistoryRepository>();

    private class TestXGHistoryRepository : XGHistoryRepository
    {
        public TestXGHistoryRepository([NotNull] HistoryRepositoryDependencies dependencies)
            : base(dependencies)
        {
        }

        public new string ExistsSql => base.ExistsSql;
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("IgnoreThisDefaultSchema");
        }
    }

    private class Blog
    {
        public int Id { get; set; }
    }
}
