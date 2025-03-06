// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class OneToOneQueryXuGuDbFixture : OneToOneQueryFixtureBase, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly XuGuDbTestStore _testStore;

        public OneToOneQueryXuGuDbFixture()
        {
            _testStore = XuGuDbTestStore.CreateScratch();

            _options = new DbContextOptionsBuilder()
                .UseXuGuDb(_testStore.ConnectionString)
                .UseInternalServiceProvider(new ServiceCollection()
                    .AddEntityFrameworkXuGuDb()
                    .AddSingleton(TestXuGuDbModelSource.GetFactory(OnModelCreating))
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
