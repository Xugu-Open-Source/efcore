using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class UpdatesXGFixture : UpdatesRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
    }
}
