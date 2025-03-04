using System.Threading.Tasks;
using EntityFrameworkCore.XuGu.Infrastructure;
using EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
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
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Collate_case_insensitive(bool async)
        {
            await base.Collate_case_insensitive(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] COLLATE Latin1_General_CI_AI = N'maria anders'");
        }
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Collate_case_sensitive(bool async)
        {
            await base.Collate_case_sensitive(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] COLLATE Latin1_General_CS_AS = N'maria anders'");
        }
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Collate_case_sensitive_constant(bool async)
        {
            await base.Collate_case_sensitive_constant(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] = N'maria anders' COLLATE Latin1_General_CS_AS");
        }

        public override async Task Like_literal(bool async)
        {
            await base.Like_literal(async);

            AssertSql(
                @"SELECT COUNT(*)
FROM `Customers` AS `c`
WHERE `c`.`ContactName` LIKE '%M%'");
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
