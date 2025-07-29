using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class IncludeAsyncXGTest : IncludeAsyncTestBase<IncludeXGFixture>
    {
        public IncludeAsyncXGTest(IncludeXGFixture fixture)
            : base(fixture)
        {
        }
    }
}
