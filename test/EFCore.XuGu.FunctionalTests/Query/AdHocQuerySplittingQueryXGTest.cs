// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query;

public class AdHocQuerySplittingQueryXGTest : AdHocQuerySplittingQueryTestBase
{
    protected override DbContextOptionsBuilder SetQuerySplittingBehavior(
        DbContextOptionsBuilder optionsBuilder,
        QuerySplittingBehavior splittingBehavior)
    {
        new XGDbContextOptionsBuilder(optionsBuilder).UseQuerySplittingBehavior(splittingBehavior);

        return optionsBuilder;
    }

    protected override DbContextOptionsBuilder ClearQuerySplittingBehavior(DbContextOptionsBuilder optionsBuilder)
    {
        var extension = optionsBuilder.Options.FindExtension<XGOptionsExtension>();
        if (extension == null)
        {
            extension = new XGOptionsExtension();
        }
        else
        {
            _querySplittingBehaviorFieldInfo.SetValue(extension, null);
        }

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    private static readonly FieldInfo _querySplittingBehaviorFieldInfo =
        typeof(RelationalOptionsExtension).GetField("_querySplittingBehavior", BindingFlags.NonPublic | BindingFlags.Instance);

    protected override ITestStoreFactory TestStoreFactory
        => XGTestStoreFactory.Instance;
}
