using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class MusicStoreXGTest : MusicStoreTestBase<MusicStoreXGTest.MusicStoreXGFixture>
    {
        public MusicStoreXGTest(MusicStoreXGFixture fixture)
            : base(fixture)
        {
        }

        public class MusicStoreXGFixture : MusicStoreFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                XGTestHelpers.Instance.EnsureSufficientKeySpace(modelBuilder.Model, TestStore);
            }
        }
    }
}
