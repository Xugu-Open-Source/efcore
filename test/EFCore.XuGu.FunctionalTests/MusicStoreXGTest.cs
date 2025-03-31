using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
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
