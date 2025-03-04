using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using EntityFrameworkCore.XuGu.Tests;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class KeysWithConvertersXGTest : KeysWithConvertersTestBase<
        KeysWithConvertersXGTest.KeysWithConvertersXGFixture>
    {
        public KeysWithConvertersXGTest(KeysWithConvertersXGFixture fixture)
            : base(fixture)
        {
        }

        public class KeysWithConvertersXGFixture : KeysWithConvertersFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => XGTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => builder.UseXG(AppConfig.ServerVersion, b => b.MinBatchSize(1));
        }
    }
}
