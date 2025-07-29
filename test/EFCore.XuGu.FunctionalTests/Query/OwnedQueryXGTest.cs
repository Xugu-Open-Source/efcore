using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class OwnedQueryXGTest : RelationalOwnedQueryTestBase<OwnedQueryXGTest.OwnedQueryXGFixture>
    {
        public OwnedQueryXGTest(OwnedQueryXGFixture fixture)
            : base(fixture)
        {
        }

        public class OwnedQueryXGFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
