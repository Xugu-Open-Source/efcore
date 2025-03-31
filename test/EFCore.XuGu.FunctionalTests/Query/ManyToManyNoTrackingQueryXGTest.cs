using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class ManyToManyNoTrackingQueryXGTest
        : ManyToManyNoTrackingQueryRelationalTestBase<ManyToManyQueryXGFixture>
    {
        public ManyToManyNoTrackingQueryXGTest(ManyToManyQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;
    }
}
