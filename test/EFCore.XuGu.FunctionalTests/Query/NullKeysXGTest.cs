// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NullKeysXGTest : NullKeysTestBase<NullKeysXGTest.NullKeysXGFixture>
    {
        public NullKeysXGTest(NullKeysXGFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysXGFixture : NullKeysFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
