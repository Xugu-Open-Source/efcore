using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class TPTManyToManyQueryXGFixture : TPTManyToManyQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => XGTestStoreFactory.Instance;
    }
}
