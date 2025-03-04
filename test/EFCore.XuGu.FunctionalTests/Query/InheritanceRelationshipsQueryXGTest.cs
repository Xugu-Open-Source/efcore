using Microsoft.EntityFrameworkCore.Query;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class InheritanceRelationshipsQueryXGTest : InheritanceRelationshipsQueryRelationalTestBase<InheritanceRelationshipsQueryXGFixture>
    {
        public InheritanceRelationshipsQueryXGTest(InheritanceRelationshipsQueryXGFixture fixture)
            : base(fixture)
        {
        }
    }
}
