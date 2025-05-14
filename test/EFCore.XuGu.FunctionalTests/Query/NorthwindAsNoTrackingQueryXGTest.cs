// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindAsNoTrackingQueryXGTest : NorthwindAsNoTrackingQueryTestBase<
        NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindAsNoTrackingQueryXGTest(
            NorthwindQueryXGFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact]
        public virtual void testAny()
        {
            using var db = CreateContext();

            
            bool any= db.Products.Any();
        }
    }
}
