using System;
using System.Linq;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;
using XuguClient;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class FromSqlSprocQueryXGTest : FromSqlSprocQueryTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public FromSqlSprocQueryXGTest(
            NorthwindQueryXGFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        protected override string TenMostExpensiveProductsSproc => "CALL `Ten Most Expensive Products`()";
        protected override string CustomerOrderHistorySproc => "CALL `CustOrderHist` ({0})";
    }
}
