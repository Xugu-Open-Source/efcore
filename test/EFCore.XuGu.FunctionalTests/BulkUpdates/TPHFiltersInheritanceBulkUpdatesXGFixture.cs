// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.BulkUpdates;

public class TPHFiltersInheritanceBulkUpdatesXGFixture : TPHInheritanceBulkUpdatesXGFixture
{
    protected override string StoreName
        => "FiltersInheritanceBulkUpdatesTest";

    public override bool EnableFilters
        => true;
}
