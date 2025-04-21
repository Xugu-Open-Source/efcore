using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class ManyToManyTrackingXGTest
        : ManyToManyTrackingXGTestBase<ManyToManyTrackingXGTest.ManyToManyTrackingXGFixture>
    {
        private readonly ManyToManyTrackingXGFixture mfixture;
        public ManyToManyTrackingXGTest(ManyToManyTrackingXGFixture fixture)
            : base(fixture)
        {
            mfixture = fixture;
        }

        public class ManyToManyTrackingXGFixture : ManyToManyTrackingXGFixtureBase
        {
            protected override string StoreName
                => "ManyToManyTrackingXGTest";
        }
    }
}
