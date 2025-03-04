using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class TPTFiltersInheritanceQueryXGTest : TPTFiltersInheritanceQueryTestBase<TPTFiltersInheritanceQueryXGFixture>
    {
        public TPTFiltersInheritanceQueryXGTest(TPTFiltersInheritanceQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
