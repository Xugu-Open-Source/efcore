// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class LoadXGTest : LoadTestBase<LoadXGTest.LoadXGFixture>
    {
        public LoadXGTest(LoadXGFixture fixture)
            : base(fixture)
        {
        }

        public class LoadXGFixture : LoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder);
        }
    }
}
