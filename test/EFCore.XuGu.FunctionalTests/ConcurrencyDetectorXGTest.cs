using EntityFrameworkCore.XuGu.FunctionalTests.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class ConcurrencyDetectorXGTest : ConcurrencyDetectorRelationalTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public ConcurrencyDetectorXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }
    }
}
