// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.XuGu.Tests;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests;

public class ModelBuilding101XGTest : ModelBuilding101RelationalTestBase
{
    protected override DbContextOptionsBuilder ConfigureContext(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseXG(AppConfig.ServerVersion);
}
