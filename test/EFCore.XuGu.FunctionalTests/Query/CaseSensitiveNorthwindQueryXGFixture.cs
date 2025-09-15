// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class CaseSensitiveNorthwindQueryXGFixture<TModelCustomizer> : NorthwindQueryXGFixture<TModelCustomizer>
        where TModelCustomizer : ITestModelCustomizer, new()
    {
        protected override string StoreName => "NorthwindCs";
        protected override ITestStoreFactory TestStoreFactory => XGNorthwindTestStoreFactory.InstanceCs;
    }
}
