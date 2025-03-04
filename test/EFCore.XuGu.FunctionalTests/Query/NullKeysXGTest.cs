using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NullKeysXGTest : NullKeysTestBase<NullKeysXGTest.NullKeysXGFixture>
    {
        public NullKeysXGTest(NullKeysXGFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysXGFixture : NullKeysFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
