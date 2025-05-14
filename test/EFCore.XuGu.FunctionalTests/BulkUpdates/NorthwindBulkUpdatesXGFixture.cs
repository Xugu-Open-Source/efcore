// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Microsoft.EntityFrameworkCore.BulkUpdates;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.BulkUpdates;

public class NorthwindBulkUpdatesXGFixture<TModelCustomizer> : NorthwindBulkUpdatesFixture<TModelCustomizer>
    where TModelCustomizer : IModelCustomizer, new()
{
    protected override ITestStoreFactory TestStoreFactory
        => XGNorthwindTestStoreFactory.Instance;

    protected override Type ContextType
        => typeof(NorthwindXGContext);
}
