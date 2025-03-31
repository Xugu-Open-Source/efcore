using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class GearsOfWarFromSqlQueryXGTest : GearsOfWarFromSqlQueryTestBase<GearsOfWarQueryXGFixture>
    {
        public GearsOfWarFromSqlQueryXGTest(GearsOfWarQueryXGFixture fixture)
            : base(fixture)
        {
        }
    }
}
