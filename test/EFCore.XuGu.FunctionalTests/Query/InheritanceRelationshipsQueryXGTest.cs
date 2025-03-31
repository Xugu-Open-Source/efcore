using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class InheritanceRelationshipsQueryXGTest : InheritanceRelationshipsQueryRelationalTestBase<InheritanceRelationshipsQueryXGFixture>
    {
        public InheritanceRelationshipsQueryXGTest(InheritanceRelationshipsQueryXGFixture fixture)
            : base(fixture)
        {
        }
    }
}
