using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class QueryFilterFuncletizationXGTest
        : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationXGTest.QueryFilterFuncletizationXGFixture>
    {
        public QueryFilterFuncletizationXGTest(
            QueryFilterFuncletizationXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void DbContext_list_is_parameterized()
        {
            using var context = CreateContext();
            // Default value of TenantIds is null InExpression over null values throws
            Assert.Throws<NullReferenceException>(() => context.Set<ListFilter>().ToList());

            context.TenantIds = new List<int>();
            var query = context.Set<ListFilter>().ToList();
            Assert.Empty(query);

            context.TenantIds = new List<int> { 1 };
            query = context.Set<ListFilter>().ToList();
            Assert.Single(query);

            context.TenantIds = new List<int> { 2, 3 };
            query = context.Set<ListFilter>().ToList();
            Assert.Equal(2, query.Count);

            AssertSql(
                @"SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE FALSE",
                //
                @"SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE `l`.`Tenant` = 1",
                //
                @"SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE `l`.`Tenant` IN (2, 3)");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class QueryFilterFuncletizationXGFixture : QueryFilterFuncletizationRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
