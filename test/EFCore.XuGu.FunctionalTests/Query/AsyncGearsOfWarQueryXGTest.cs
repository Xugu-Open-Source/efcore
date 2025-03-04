using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class AsyncGearsOfWarQueryXGTest : AsyncGearsOfWarQueryRelationalTestBase<GearsOfWarQueryXGFixture>
    {
        public AsyncGearsOfWarQueryXGTest(GearsOfWarQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;
    }
}
