using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Xunit.Abstractions;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class TPTRelationshipsQueryXGTest
        : TPTRelationshipsQueryTestBase<TPTRelationshipsQueryXGTest.TPTRelationshipsQueryXGFixture>
    {
        public TPTRelationshipsQueryXGTest(
            TPTRelationshipsQueryXGFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
            => fixture.TestSqlLoggerFactory.Clear();

        public class TPTRelationshipsQueryXGFixture : TPTRelationshipsQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory
                => XGTestStoreFactory.Instance;
        }
    }
}
