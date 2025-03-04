using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindAsNoTrackingQueryXGTest : NorthwindAsNoTrackingQueryTestBase<
        NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindAsNoTrackingQueryXGTest(
            NorthwindQueryXGFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
