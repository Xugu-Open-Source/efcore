// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindNavigationsQueryXGTest : NorthwindNavigationsQueryRelationalTestBase<
        NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindNavigationsQueryXGTest(
            NorthwindQueryXGFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory(Skip = "Issue #573")]
        public override Task Where_subquery_on_navigation(bool async)
        {
            return base.Where_subquery_on_navigation(async);
        }

        [ConditionalTheory(Skip = "Issue #573")]
        public override Task Where_subquery_on_navigation2(bool async)
        {
            return base.Where_subquery_on_navigation2(async);
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.OuterReferenceInMultiLevelSubquery))]
        public override Task Navigation_in_subquery_referencing_outer_query_with_client_side_result_operator_and_count(bool async)
        {
            return base.Navigation_in_subquery_referencing_outer_query_with_client_side_result_operator_and_count(async);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
