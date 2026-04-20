// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public abstract class GraphUpdatesXGTestBase<TFixture> : GraphUpdatesTestBase<XGTestStore, TFixture>
        where TFixture : GraphUpdatesXGTestBase<TFixture>.GraphUpdatesXGFixtureBase, new()
    {
        protected GraphUpdatesXGTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public abstract class GraphUpdatesXGFixtureBase : GraphUpdatesFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            protected GraphUpdatesXGFixtureBase()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkXG()
                    .AddSingleton(TestXGModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            protected abstract string DatabaseName { get; }

            public override XGTestStore CreateTestStore()
            {
                return XGTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseXG(XGTestStore.CreateConnectionString(DatabaseName))
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

            public override DbContext CreateContext(XGTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseXG(testStore.Connection)
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new GraphUpdatesContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);
                return context;
            }
        }
    }
}
