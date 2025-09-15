// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.DebugServices;

public class DebugRelationalCommand : RelationalCommand
{
    public DebugRelationalCommand(
        [NotNull] RelationalCommandBuilderDependencies dependencies,
        [NotNull] string commandText,
        [NotNull] IReadOnlyList<IRelationalParameter> parameters)
        : base(dependencies, commandText, parameters)
    {
    }

    protected override RelationalDataReader CreateRelationalDataReader()
        => new DebugRelationalDataReader();
}
