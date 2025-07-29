using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class XGServiceCollectionExtensionsTest : RelationalServiceCollectionExtensionsTestBase
    {
        public XGServiceCollectionExtensionsTest()
            : base(XGTestHelpers.Instance)
        {
        }
    }
}
