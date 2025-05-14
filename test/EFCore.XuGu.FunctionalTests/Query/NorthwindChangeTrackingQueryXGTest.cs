// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindChangeTrackingQueryXGTest : NorthwindChangeTrackingQueryTestBase<
        NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindChangeTrackingQueryXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override NorthwindContext CreateNoTrackingContext()
            => new NorthwindXGContext(
                new DbContextOptionsBuilder(Fixture.CreateOptions())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);
    }
}
