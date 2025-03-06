// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public abstract class GraphUpdatesXuGuDbTestBase<TFixture> : GraphUpdatesTestBase<XuGuDbTestStore, TFixture>
        where TFixture : GraphUpdatesXuGuDbTestBase<TFixture>.GraphUpdatesXuGuDbFixtureBase, new()
    {
        protected GraphUpdatesXuGuDbTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public abstract class GraphUpdatesXuGuDbFixtureBase : GraphUpdatesFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            protected GraphUpdatesXuGuDbFixtureBase()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkXuGuDb()
                    .AddSingleton(TestXuGuDbModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            protected abstract string DatabaseName { get; }

            public override XuGuDbTestStore CreateTestStore()
            {
                return XuGuDbTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseXuGuDb(XuGuDbTestStore.CreateConnectionString(DatabaseName))
                            .UseInternalServiceProvider(_serviceProvider);

                        using (var context = new GraphUpdatesContext(optionsBuilder.Options))
                        {
                            context.Database.EnsureDeleted();
                            if (context.Database.EnsureCreated())
                            {
                                Seed(context);
                            }
                        }
                    });
            }

            public override DbContext CreateContext(XuGuDbTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseXuGuDb(testStore.Connection)
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new GraphUpdatesContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);
                return context;
            }
        }
    }
}
