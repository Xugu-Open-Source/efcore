// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class NotificationEntitiesXuGuDbTest
        : NotificationEntitiesTestBase<NotificationEntitiesXuGuDbTest.NotificationEntitiesXuGuDbFixture>
    {
        public NotificationEntitiesXuGuDbTest(NotificationEntitiesXuGuDbFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesXuGuDbFixture : NotificationEntitiesFixtureBase
        {
            private readonly DbContextOptions _options;

            public NotificationEntitiesXuGuDbFixture()
            {
                _options = new DbContextOptionsBuilder()
                    .UseXuGuDb(XuGuDbTestStore.CreateConnectionString("NotificationEntities"))
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkXuGuDb()
                        .AddSingleton(TestXuGuDbModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider())
                    .Options;

                EnsureCreated();
            }

            public override DbContext CreateContext()
                => new DbContext(_options);
        }
    }
}
