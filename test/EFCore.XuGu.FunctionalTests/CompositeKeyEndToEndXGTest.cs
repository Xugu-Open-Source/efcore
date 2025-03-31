using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class CompositeKeyEndToEndXGTest : CompositeKeyEndToEndTestBase<CompositeKeyEndToEndXGTest.CompositeKeyEndToEndXGFixture>
    {
        public CompositeKeyEndToEndXGTest(CompositeKeyEndToEndXGFixture fixture)
            : base(fixture)
        {
        }

        public class CompositeKeyEndToEndXGFixture : CompositeKeyEndToEndFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
