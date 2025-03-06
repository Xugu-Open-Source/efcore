// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class NorthwindQueryXuGuDbFixture : NorthwindQueryRelationalFixture, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;

        private readonly XuGuDbTestStore _testStore = XuGuDbNorthwindContext.GetSharedStore();
        private readonly TestSqlLoggerFactory _testSqlLoggerFactory = new TestSqlLoggerFactory();

        public NorthwindQueryXuGuDbFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkXuGuDb()
                    .AddSingleton(TestXuGuDbModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(_testSqlLoggerFactory)
                    .BuildServiceProvider();

            _options = BuildOptions();
        }

        protected DbContextOptions BuildOptions()
            => ConfigureOptions(
                new DbContextOptionsBuilder()
                    .EnableSensitiveDataLogging()
                    .UseInternalServiceProvider(_serviceProvider))
                .UseXuGuDb(
                    _testStore.ConnectionString,
                    b =>
                        {
                            ConfigureOptions(b);
                            b.ApplyConfiguration();
                        }).Options;

        protected virtual DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder dbContextOptionsBuilder)
            => dbContextOptionsBuilder;

        protected virtual void ConfigureOptions(XuGuDbContextOptionsBuilder sqlServerDbContextOptionsBuilder)
        {
        }

        public override NorthwindContext CreateContext()
            => new XuGuDbNorthwindContext(_options);

        public void Dispose() => _testStore.Dispose();

        public override CancellationToken CancelQuery() => _testSqlLoggerFactory.CancelQuery();
    }
}
