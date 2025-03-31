using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class TPTManyToManyQueryXGTest : TPTManyToManyQueryRelationalTestBase<TPTManyToManyQueryXGFixture>
    {
        public TPTManyToManyQueryXGTest(TPTManyToManyQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;
    }
}
