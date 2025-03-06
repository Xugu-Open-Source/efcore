// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class NullKeysXuGuDbTest : NullKeysTestBase<NullKeysXuGuDbTest.NullKeysXuGuDbFixture>
    {
        public NullKeysXuGuDbTest(NullKeysXuGuDbFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysXuGuDbFixture : NullKeysFixtureBase
        {
            private readonly DbContextOptions _options;

            public NullKeysXuGuDbFixture()
            {
                _options = new DbContextOptionsBuilder()
                    .UseXuGuDb(XuGuDbTestStore.CreateConnectionString("StringsContext"))
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
