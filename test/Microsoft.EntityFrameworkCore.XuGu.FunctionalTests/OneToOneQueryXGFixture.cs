// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class OneToOneQueryXGFixture : OneToOneQueryFixtureBase, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly XGTestStore _testStore;

        public OneToOneQueryXGFixture()
        {
            _testStore = XGTestStore.CreateScratch();

            _options = new DbContextOptionsBuilder()
                .UseXG(_testStore.ConnectionString)
                .UseInternalServiceProvider(new ServiceCollection()
                    .AddEntityFrameworkXG()
                    .AddSingleton(TestXGModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                    .BuildServiceProvider())
                .Options;

            using (var context = new DbContext(_options))
            {
                context.Database.EnsureCreated();

                AddTestData(context);
            }
        }

        public DbContext CreateContext() => new DbContext(_options);

        public void Dispose() => _testStore.Dispose();
    }
}
