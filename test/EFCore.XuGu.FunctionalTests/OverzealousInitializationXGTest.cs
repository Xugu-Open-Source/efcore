// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class OverzealousInitializationXGTest
        : OverzealousInitializationTestBase<OverzealousInitializationXGTest.OverzealousInitializationXGFixture>
    {
        public OverzealousInitializationXGTest(OverzealousInitializationXGFixture fixture)
            : base(fixture)
        {
        }

        public class OverzealousInitializationXGFixture : OverzealousInitializationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
