// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.NullSemanticsModel;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class NullSemanticsQueryXGFixture : NullSemanticsQueryRelationalFixture<XGTestStore>
    {
        public static readonly string DatabaseName = "NullSemanticsQueryTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = XGTestStore.CreateConnectionString(DatabaseName);

        public NullSemanticsQueryXGFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXG()
                .AddSingleton(TestXGModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override XGTestStore CreateTestStore()
        {
            return XGTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = new NullSemanticsContext(new DbContextOptionsBuilder()
                        .UseXG(_connectionString)
                        .UseInternalServiceProvider(_serviceProvider).Options))
                    {
                        // TODO: Delete DB if model changed

                        if (context.Database.EnsureCreated())
                        {
                            NullSemanticsModelInitializer.Seed(context);
                        }

                        TestSqlLoggerFactory.Reset();
                    }
                });
        }

        public override NullSemanticsContext CreateContext(XGTestStore testStore, bool useRelationalNulls)
        {
            var context = new NullSemanticsContext(new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(_serviceProvider)
                .UseXG(
                    testStore.Connection,
                    b =>
                        {
                            if (useRelationalNulls)
                            {
                                b.UseRelationalNulls();
                            }
                        }).Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
