// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Tests;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class KeysWithConvertersXGTest : KeysWithConvertersTestBase<
        KeysWithConvertersXGTest.KeysWithConvertersXGFixture>
    {
        public KeysWithConvertersXGTest(KeysWithConvertersXGFixture fixture)
            : base(fixture)
        {
        }

        public class KeysWithConvertersXGFixture : KeysWithConvertersFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => XGTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => builder.UseXG(AppConfig.ServerVersion, b => b.MinBatchSize(1));
        }
    }
}
