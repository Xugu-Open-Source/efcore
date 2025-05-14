// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindAsTrackingQueryXGTest : NorthwindAsTrackingQueryTestBase<
        NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindAsTrackingQueryXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
