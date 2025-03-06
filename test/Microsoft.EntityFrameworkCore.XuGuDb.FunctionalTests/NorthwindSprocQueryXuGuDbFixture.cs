// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class NorthwindSprocQueryXuGuDbFixture : NorthwindSprocQueryRelationalFixture, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly XuGuDbTestStore _testStore;

        public NorthwindSprocQueryXuGuDbFixture()
        {
            _testStore = XuGuDbNorthwindContext.GetSharedStore();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXuGuDb()
                .AddSingleton(TestXuGuDbModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(serviceProvider)
                .UseXuGuDb(_testStore.ConnectionString).Options;

            serviceProvider.GetRequiredService<ILoggerFactory>();
        }

        public override NorthwindContext CreateContext()
        {
            var context = new XuGuDbNorthwindContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }

        public void Dispose() => _testStore.Dispose();
    }
}
