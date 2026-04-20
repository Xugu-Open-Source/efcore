// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class UpdatesXGFixture : UpdatesFixtureBase<XGTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public UpdatesXGFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXG()
                .BuildServiceProvider();
        }

        protected virtual string DatabaseName => "PartialUpdateXGTest";

        public override XGTestStore CreateTestStore()
        {
            return XGTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseXG(XGTestStore.CreateConnectionString(DatabaseName))
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

        public override UpdatesContext CreateContext(XGTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseXG(testStore.Connection)
                .UseInternalServiceProvider(_serviceProvider);

            var context = new UpdatesContext(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
