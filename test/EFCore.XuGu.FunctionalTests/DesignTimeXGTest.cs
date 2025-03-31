using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Design.Internal;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class DesignTimeXGTest : DesignTimeTestBase<DesignTimeXGTest.DesignTimeXGFixture>
    {
        public DesignTimeXGTest(DesignTimeXGFixture fixture)
            : base(fixture)
        {
        }

        protected override Assembly ProviderAssembly
            => typeof(XGDesignTimeServices).Assembly;

        public class DesignTimeXGFixture : DesignTimeFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => XGTestStoreFactory.Instance;
        }
    }
}
