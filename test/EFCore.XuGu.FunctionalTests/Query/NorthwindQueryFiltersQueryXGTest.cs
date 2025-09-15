// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindQueryFiltersQueryXGTest : NorthwindQueryFiltersQueryTestBase<
        NorthwindQueryXGFixture<NorthwindQueryFiltersCustomizer>>
    {
        public NorthwindQueryFiltersQueryXGTest(
            NorthwindQueryXGFixture<NorthwindQueryFiltersCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Count_query(bool async)
        {
            await base.Count_query(async);

        AssertSql(
"""
@__ef_filter__TenantPrefix_0_startswith='B%' (Size = 40)

SELECT COUNT(*)
FROM `Customers` AS `c`
WHERE `c`.`CompanyName` LIKE @__ef_filter__TenantPrefix_0_startswith
""");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
