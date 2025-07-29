using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class AsTrackingXGTest : AsTrackingTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public AsTrackingXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
