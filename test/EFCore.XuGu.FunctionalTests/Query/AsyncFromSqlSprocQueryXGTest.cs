using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class AsyncFromSqlSprocQueryXGTest : AsyncFromSqlSprocQueryTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public AsyncFromSqlSprocQueryXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        protected override string TenMostExpensiveProductsSproc => "CALL `Ten Most Expensive Products`()";

        protected override string CustomerOrderHistorySproc => "CALL `CustOrderHist` ({0})";
    }
}
