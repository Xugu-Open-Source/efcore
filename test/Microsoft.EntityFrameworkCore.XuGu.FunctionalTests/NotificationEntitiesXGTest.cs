// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class NotificationEntitiesXGTest
        : NotificationEntitiesTestBase<NotificationEntitiesXGTest.NotificationEntitiesXGFixture>
    {
        public NotificationEntitiesXGTest(NotificationEntitiesXGFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesXGFixture : NotificationEntitiesFixtureBase
        {
            private readonly DbContextOptions _options;

            public NotificationEntitiesXGFixture()
            {
                _options = new DbContextOptionsBuilder()
                    .UseXG(XGTestStore.CreateConnectionString("NotificationEntities"))
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkXG()
                        .AddSingleton(TestXGModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider())
                    .Options;

                EnsureCreated();
            }

            public override DbContext CreateContext()
                => new DbContext(_options);
        }
    }
}
