using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class XGServiceCollectionExtensionsTest : RelationalServiceCollectionExtensionsTestBase
    {
        public XGServiceCollectionExtensionsTest()
            : base(XGTestHelpers.Instance)
        {
        }
    }
}
