using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindKeylessEntitiesQueryXGTest : NorthwindKeylessEntitiesQueryRelationalTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindKeylessEntitiesQueryXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        [ConditionalFact(Skip = "https://github.com/dotnet/efcore/issues/21627")]
        public override void KeylessEntity_with_nav_defining_query()
            => base.KeylessEntity_with_nav_defining_query();
    }
}
