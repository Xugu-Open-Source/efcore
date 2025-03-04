using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class OverzealousInitializationXGTest
        : OverzealousInitializationTestBase<OverzealousInitializationXGTest.OverzealousInitializationXGFixture>
    {
        public OverzealousInitializationXGTest(OverzealousInitializationXGFixture fixture)
            : base(fixture)
        {
        }

        public class OverzealousInitializationXGFixture : OverzealousInitializationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
