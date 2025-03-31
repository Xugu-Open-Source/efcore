using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class NotificationEntitiesXGTest : NotificationEntitiesTestBase<NotificationEntitiesXGTest.NotificationEntitiesXGFixture>
    {
        public NotificationEntitiesXGTest(NotificationEntitiesXGFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesXGFixture : NotificationEntitiesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
