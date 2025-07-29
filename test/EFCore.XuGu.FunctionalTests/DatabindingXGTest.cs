using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class DatabindingXGTest : DatabindingTestBase<F1XGFixture>
    {
        public DatabindingXGTest(F1XGFixture fixture)
            : base(fixture)
        {
        }
    }
}
