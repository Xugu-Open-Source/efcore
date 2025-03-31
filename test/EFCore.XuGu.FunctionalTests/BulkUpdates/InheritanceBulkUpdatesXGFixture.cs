using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.BulkUpdates;

public class InheritanceBulkUpdatesXGFixture : InheritanceBulkUpdatesRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => XGTestStoreFactory.Instance;
}
