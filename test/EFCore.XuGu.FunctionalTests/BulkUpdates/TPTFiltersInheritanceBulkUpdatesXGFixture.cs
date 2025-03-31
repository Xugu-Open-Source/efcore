namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.BulkUpdates;

public class TPTFiltersInheritanceBulkUpdatesXGFixture : TPTInheritanceBulkUpdatesXGFixture
{
    protected override string StoreName
        => "TPTFiltersInheritanceBulkUpdatesTest";

    protected override bool EnableFilters
        => true;
}
