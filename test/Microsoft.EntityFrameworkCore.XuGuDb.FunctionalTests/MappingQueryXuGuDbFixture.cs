// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class MappingQueryXuGuDbFixture : MappingQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly XuGuDbTestStore _testDatabase;

        public MappingQueryXuGuDbFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXuGuDb()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _testDatabase = XuGuDbNorthwindContext.GetSharedStore();

            _options = new DbContextOptionsBuilder()
                .UseModel(CreateModel())
                .UseXuGuDb(_testDatabase.ConnectionString)
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
                    e.Property(c => c.CompanyName2).Metadata.XuGuDb().ColumnName = "CompanyName";
                    e.Metadata.XuGuDb().TableName = "Customers";
                    e.Metadata.XuGuDb().Schema = "SYSDBA";
                });
        }
    }
}
