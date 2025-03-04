using Microsoft.EntityFrameworkCore.Query;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class WarningsXGTest : WarningsTestBase<QueryNoClientEvalXGFixture>
    {
        public WarningsXGTest(QueryNoClientEvalXGFixture fixture)
            : base(fixture)
        {
        }
    }
}
