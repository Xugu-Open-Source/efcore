using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class TPTManyToManyQueryXGFixture : TPTManyToManyQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => XGTestStoreFactory.Instance;
    }
}
