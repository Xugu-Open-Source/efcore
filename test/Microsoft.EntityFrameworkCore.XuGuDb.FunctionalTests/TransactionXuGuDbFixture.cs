// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class TransactionXuGuDbFixture : TransactionFixtureBase<XuGuDbTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionXuGuDbFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXuGuDb()
                .AddSingleton(TestXuGuDbModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();
        }

        public override XuGuDbTestStore CreateTestStore()
        {
            var db = XuGuDbTestStore.CreateScratch();

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

        public override DbContext CreateContext(XuGuDbTestStore testStore)
            => new DbContext(new DbContextOptionsBuilder()
                .UseXuGuDb(testStore.ConnectionString)
                .UseInternalServiceProvider(_serviceProvider).Options);

        public override DbContext CreateContext(DbConnection connection)
            => new DbContext(new DbContextOptionsBuilder()
                .UseXuGuDb(connection)
                .UseInternalServiceProvider(_serviceProvider).Options);
    }
}
