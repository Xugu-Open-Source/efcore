// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class MappingQueryXGFixture : MappingQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly XGTestStore _testDatabase;

        public MappingQueryXGFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXG()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _testDatabase = XGNorthwindContext.GetSharedStore();

            _options = new DbContextOptionsBuilder()
                .UseModel(CreateModel())
                .UseXG(_testDatabase.ConnectionString)
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public DbContext CreateContext()
        {
            var context = new DbContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }

        public void Dispose() => _testDatabase.Dispose();

        protected override string DatabaseSchema { get; } = "SYSDBA";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MappingQueryTestBase.MappedCustomer>(e =>
                {
                    e.Property(c => c.CompanyName2).Metadata.XG().ColumnName = "CompanyName";
                    e.Metadata.XG().TableName = "Customers";
                    e.Metadata.XG().Schema = "SYSDBA";
                });
        }
    }
}
