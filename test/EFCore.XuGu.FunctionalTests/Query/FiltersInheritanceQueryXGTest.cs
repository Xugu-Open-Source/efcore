using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class FiltersInheritanceQueryXGTest : FiltersInheritanceQueryTestBase<FiltersInheritanceQueryXGFixture>
    {
        public FiltersInheritanceQueryXGTest(FiltersInheritanceQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
