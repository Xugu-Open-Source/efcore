// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.DebugServices;

public class DebugRelationalCommandBuilder : RelationalCommandBuilder
{
    public DebugRelationalCommandBuilder([NotNull] RelationalCommandBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    public override IRelationalCommand Build()
        => new DebugRelationalCommand(Dependencies, base.Build().CommandText, Parameters);
}
