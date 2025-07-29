using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class CompiledQueryXGTest : CompiledQueryTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public CompiledQueryXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
