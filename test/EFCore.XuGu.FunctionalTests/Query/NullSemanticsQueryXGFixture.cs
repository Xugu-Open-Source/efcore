using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NullSemanticsQueryXGFixture : NullSemanticsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
    }
}
