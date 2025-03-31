namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query;

public class TPCFiltersInheritanceQueryXGFixture : TPCInheritanceQueryXGFixture
{
    protected override bool EnableFilters
        => true;
}
