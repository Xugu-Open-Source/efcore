namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class TPTFiltersInheritanceQueryXGFixture : TPTInheritanceQueryXGFixture
    {
        protected override bool EnableFilters
            => true;
    }
}
