// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public partial class NorthwindDbFunctionsQueryXGTest : NorthwindDbFunctionsQueryRelationalTestBase<
        NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindDbFunctionsQueryXGTest(
            NorthwindQueryXGFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Like_literal(bool async)
        {
            await  AssertCount(
                    async,
                    ss => ss.Set<Customer>(),
                    ss => ss.Set<Customer>(),
                    c => EF.Functions.Like(c.ContactName, "%M%") || EF.Functions.Like(c.ContactName, "%m%"),
                    c => c.ContactName.Contains("M") || c.ContactName.Contains("m"));

            AssertSql(
                    @"SELECT COUNT(*)
FROM `Customers` AS `c`
WHERE (`c`.`ContactName` LIKE '%M%') OR (`c`.`ContactName` LIKE '%m%')");
        }

        public override async Task Like_identity(bool async)
        {
            await base.Like_identity(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM `Customers` AS `c`
WHERE `c`.`ContactName` LIKE `c`.`ContactName`");
        }

        public override async Task Like_literal_with_escape(bool async)
        {
            await base.Like_literal_with_escape(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM `Customers` AS `c`
WHERE `c`.`ContactName` LIKE '!%' ESCAPE '!'");
        }

        protected override string CaseInsensitiveCollation
            => "utf8mb4_general_ci";

        protected override string CaseSensitiveCollation
            => "utf8mb4_bin";

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
