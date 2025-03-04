using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class LoadXGTest : LoadTestBase<LoadXGTest.LoadXGFixture>
    {
        public LoadXGTest(LoadXGFixture fixture)
            : base(fixture)
        {
        }

        public class LoadXGFixture : LoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder);
        }
    }
}
