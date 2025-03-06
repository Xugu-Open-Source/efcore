// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class UpdatesXuGuDbFixture : UpdatesFixtureBase<XuGuDbTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public UpdatesXuGuDbFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXuGuDb()
                .BuildServiceProvider();
        }

        protected virtual string DatabaseName => "PartialUpdateXuGuDbTest";

        public override XuGuDbTestStore CreateTestStore()
        {
            return XuGuDbTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseXuGuDb(XuGuDbTestStore.CreateConnectionString(DatabaseName))
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new UpdatesContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureDeleted();
                        if (context.Database.EnsureCreated())
                        {
                            UpdatesModelInitializer.Seed(context);
                        }
                    }
                });
        }

        public override UpdatesContext CreateContext(XuGuDbTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseXuGuDb(testStore.Connection)
                .UseInternalServiceProvider(_serviceProvider);

            var context = new UpdatesContext(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
