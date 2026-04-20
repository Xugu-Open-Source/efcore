// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class TransactionXGFixture : TransactionFixtureBase<XGTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionXGFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXG()
                .AddSingleton(TestXGModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();
        }

        public override XGTestStore CreateTestStore()
        {
            var db = XGTestStore.CreateScratch();

            using (var command = db.Connection.CreateCommand())
            {
                command.CommandText = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE";
                command.ExecuteNonQuery();
            }

            using (var command = db.Connection.CreateCommand())
            {
                command.CommandText = "SET TRANSACTION ISOLATION LEVEL READ ONLY;";
                command.ExecuteNonQuery();
            }

            using (var context = CreateContext(db))
            {
                Seed(context);
            }

            return db;
        }

        public override DbContext CreateContext(XGTestStore testStore)
            => new DbContext(new DbContextOptionsBuilder()
                .UseXG(testStore.ConnectionString)
                .UseInternalServiceProvider(_serviceProvider).Options);

        public override DbContext CreateContext(DbConnection connection)
            => new DbContext(new DbContextOptionsBuilder()
                .UseXG(connection)
                .UseInternalServiceProvider(_serviceProvider).Options);
    }
}
