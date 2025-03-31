using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class TPTInheritanceQueryXGTest : TPTInheritanceQueryTestBase<TPTInheritanceQueryXGFixture>
    {
        public TPTInheritanceQueryXGTest(TPTInheritanceQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
