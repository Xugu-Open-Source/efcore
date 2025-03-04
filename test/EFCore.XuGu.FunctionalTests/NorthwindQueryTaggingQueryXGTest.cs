using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EntityFrameworkCore.XuGu.FunctionalTests.Query;
using Xunit.Abstractions;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    internal class NorthwindQueryTaggingQueryXGTest : NorthwindQueryTaggingQueryTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindQueryTaggingQueryXGTest(
            NorthwindQueryXGFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
