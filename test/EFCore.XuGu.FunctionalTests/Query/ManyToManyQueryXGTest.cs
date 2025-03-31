using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class ManyToManyQueryXGTest : ManyToManyQueryRelationalTestBase<ManyToManyQueryXGFixture>
    {
        public ManyToManyQueryXGTest(ManyToManyQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected override bool CanExecuteQueryString
            => true;
    }
}
