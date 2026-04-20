// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class NorthwindSprocQueryXGFixture : NorthwindSprocQueryRelationalFixture, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly XGTestStore _testStore;

        public NorthwindSprocQueryXGFixture()
        {
            _testStore = XGNorthwindContext.GetSharedStore();

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXG()
                .AddSingleton(TestXGModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(serviceProvider)
                .UseXG(_testStore.ConnectionString).Options;

            serviceProvider.GetRequiredService<ILoggerFactory>();
        }

        public override NorthwindContext CreateContext()
        {
            var context = new XGNorthwindContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }

        public void Dispose() => _testStore.Dispose();
    }
}
