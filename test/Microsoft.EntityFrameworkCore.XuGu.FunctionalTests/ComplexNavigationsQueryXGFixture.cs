// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class ComplexNavigationsQueryXGFixture
        : ComplexNavigationsQueryRelationalFixture<XGTestStore>
    {
        public static readonly string DatabaseName = "ComplexNavigations";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString
            = XGTestStore.CreateConnectionString(DatabaseName);

        public ComplexNavigationsQueryXGFixture()
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
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseXG(_connectionString)
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new ComplexNavigationsContext(optionsBuilder.Options))
                    {
                        // TODO: Delete DB if model changed
                        context.Database.EnsureDeleted();

                        if (context.Database.EnsureCreated())
                        {
                            ComplexNavigationsModelInitializer.Seed(context);
                        }

                        TestSqlLoggerFactory.Reset();
                    }
                });
        }

        public override ComplexNavigationsContext CreateContext(XGTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseXG(testStore.Connection, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_serviceProvider);

            var context = new ComplexNavigationsContext(optionsBuilder.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
