using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests;

public abstract class FindXGTest : FindTestBase<FindXGTest.FindXGFixture>
{
    protected FindXGTest(FindXGFixture fixture)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public class FindXGTestSet : FindXGTest
    {
        public FindXGTestSet(FindXGFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindXGTestContext : FindXGTest
    {
        public FindXGTestContext(FindXGFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindXGTestNonGeneric : FindXGTest
    {
        public FindXGTestNonGeneric(FindXGFixture fixture)
            : base(fixture)
        {
        }

        protected override TestFinder Finder { get; } = new FindViaNonGenericContextFinder();
    }

    public class FindXGFixture : FindFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
    }
}
