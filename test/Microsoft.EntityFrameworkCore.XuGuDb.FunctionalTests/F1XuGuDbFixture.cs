// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ConcurrencyModel;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class F1XuGuDbFixture : F1RelationalFixture<XuGuDbTestStore>
    {
        public static readonly string DatabaseName = "OptimisticConcurrencyTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = XuGuDbTestStore.CreateConnectionString(DatabaseName);

        public F1XuGuDbFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXuGuDb()
                .AddSingleton(TestXuGuDbModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override XuGuDbTestStore CreateTestStore()
        {
            return XuGuDbTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseXuGuDb(_connectionString)
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new F1Context(optionsBuilder.Options))
                    {
                        // TODO: Delete DB if model changed
                        context.Database.EnsureDeleted();
                        if (context.Database.EnsureCreated())
                        {
                            ConcurrencyModelInitializer.Seed(context);
                        }

                        TestSqlLoggerFactory.Reset();
                    }
                });
        }

        public override F1Context CreateContext(XuGuDbTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseXuGuDb(testStore.Connection)
                .UseInternalServiceProvider(_serviceProvider);

            var context = new F1Context(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
