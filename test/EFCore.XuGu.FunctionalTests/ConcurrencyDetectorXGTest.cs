using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class ConcurrencyDetectorXGTest : ConcurrencyDetectorRelationalTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public ConcurrencyDetectorXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
