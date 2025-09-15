// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query;

public class CaseSensitiveWithStringComparisonNorthwindQueryXGFixture<TModelCustomizer> : CaseSensitiveNorthwindQueryXGFixture<TModelCustomizer>
    where TModelCustomizer : ITestModelCustomizer, new()
{
    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var optionsBuilder = base.AddOptions(builder);
        new XGDbContextOptionsBuilder(optionsBuilder).EnableStringComparisonTranslations();
        return optionsBuilder;
    }
}
