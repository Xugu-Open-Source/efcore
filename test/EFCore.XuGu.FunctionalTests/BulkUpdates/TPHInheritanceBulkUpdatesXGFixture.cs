// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.BulkUpdates;

public class TPHInheritanceBulkUpdatesXGFixture : TPHInheritanceBulkUpdatesFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => XGTestStoreFactory.Instance;
}
