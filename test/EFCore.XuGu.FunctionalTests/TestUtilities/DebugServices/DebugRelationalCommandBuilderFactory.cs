// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.DebugServices;

public class DebugRelationalCommandBuilderFactory : RelationalCommandBuilderFactory
{
    public DebugRelationalCommandBuilderFactory([NotNull] RelationalCommandBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    public override IRelationalCommandBuilder Create()
        => new DebugRelationalCommandBuilder(Dependencies);
}
