namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.BulkUpdates;

public class TPCFiltersInheritanceBulkUpdatesXGFixture : TPCInheritanceBulkUpdatesXGFixture
{
    protected override string StoreName
        => "TPCFiltersInheritanceBulkUpdatesTest";

    protected override bool EnableFilters
        => true;
}
