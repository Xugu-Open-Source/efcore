using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class PropertyValuesXGTest : PropertyValuesTestBase<PropertyValuesXGTest.PropertyValuesXGFixture>
    {
        public PropertyValuesXGTest(PropertyValuesXGFixture fixture)
            : base(fixture)
        {
        }

        public class PropertyValuesXGFixture : PropertyValuesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
