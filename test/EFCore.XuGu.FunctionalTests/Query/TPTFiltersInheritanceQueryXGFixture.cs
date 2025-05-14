// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class TPTFiltersInheritanceQueryXGFixture : TPTInheritanceQueryXGFixture
    {
        public override bool EnableFilters
            => true;
    }
}
