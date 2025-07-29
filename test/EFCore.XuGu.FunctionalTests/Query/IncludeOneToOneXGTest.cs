using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class IncludeOneToOneXGTest : IncludeOneToOneTestBase<IncludeOneToOneXGTest.OneToOneQueryXGFixture>
    {
        public IncludeOneToOneXGTest(OneToOneQueryXGFixture fixture)
            : base(fixture)
        {
        }

        public class OneToOneQueryXGFixture : OneToOneQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory =>
                (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }
    }
}
