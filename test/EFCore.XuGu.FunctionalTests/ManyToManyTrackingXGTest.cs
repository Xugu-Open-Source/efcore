namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class ManyToManyTrackingXGTest
        : ManyToManyTrackingXGTestBase<ManyToManyTrackingXGTest.ManyToManyTrackingXGFixture>
    {
        public ManyToManyTrackingXGTest(ManyToManyTrackingXGFixture fixture)
            : base(fixture)
        {
        }

        public class ManyToManyTrackingXGFixture : ManyToManyTrackingXGFixtureBase
        {
        }
    }
}
