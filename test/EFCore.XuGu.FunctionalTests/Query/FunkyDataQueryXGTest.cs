using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class FunkyDataQueryXGTest : FunkyDataQueryTestBase<FunkyDataQueryXGTest.FunkyDataQueryXGFixture>
    {
        public FunkyDataQueryXGTest(FunkyDataQueryXGFixture fixture)
            : base(fixture)
        {
        }

        public class FunkyDataQueryXGFixture : FunkyDataQueryFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
