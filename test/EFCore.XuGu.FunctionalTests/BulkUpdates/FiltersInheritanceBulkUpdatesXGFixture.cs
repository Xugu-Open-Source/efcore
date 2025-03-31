namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.BulkUpdates;

public class FiltersInheritanceBulkUpdatesXGFixture : InheritanceBulkUpdatesXGFixture
{
    protected override string StoreName
        => "FiltersInheritanceBulkUpdatesTest";

    protected override bool EnableFilters
        => true;
}
