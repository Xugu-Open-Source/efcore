using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class ManyToManyQueryXGFixture : ManyToManyQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => XGTestStoreFactory.Instance;
    }
}
