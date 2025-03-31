using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class ComplexNavigationsCollectionsSplitSharedTypeQueryXGTest : ComplexNavigationsCollectionsSplitSharedTypeQueryRelationalTestBase<ComplexNavigationsSharedTypeQueryXGFixture>
    {
        public ComplexNavigationsCollectionsSplitSharedTypeQueryXGTest(
            ComplexNavigationsSharedTypeQueryXGFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
    }
}
