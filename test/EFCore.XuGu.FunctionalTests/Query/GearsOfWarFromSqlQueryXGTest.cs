using Microsoft.EntityFrameworkCore.Query;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class GearsOfWarFromSqlQueryXGTest : GearsOfWarFromSqlQueryTestBase<GearsOfWarQueryXGFixture>
    {
        public GearsOfWarFromSqlQueryXGTest(GearsOfWarQueryXGFixture fixture)
            : base(fixture)
        {
        }
    }
}
