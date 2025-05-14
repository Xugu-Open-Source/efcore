// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class FieldsOnlyLoadXGTest : FieldsOnlyLoadTestBase<FieldsOnlyLoadXGTest.FieldsOnlyLoadXGFixture>
    {
        public FieldsOnlyLoadXGTest(FieldsOnlyLoadXGFixture fixture)
            : base(fixture)
        {
        }

        public class FieldsOnlyLoadXGFixture : FieldsOnlyLoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => XGTestStoreFactory.Instance;
        }
    }
}
