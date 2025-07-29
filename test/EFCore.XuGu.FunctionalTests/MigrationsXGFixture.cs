using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class MigrationsXGFixture : MigrationsFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => XGConnectionStringTestStoreFactory.Instance;
    }
}
