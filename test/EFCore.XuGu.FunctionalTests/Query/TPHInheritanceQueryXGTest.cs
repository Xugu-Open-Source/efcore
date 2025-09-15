// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query;

public class TPHInheritanceQueryXGTest : TPHInheritanceQueryTestBase<TPHInheritanceQueryXGFixture>
{
    public TPHInheritanceQueryXGTest(TPHInheritanceQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture, testOutputHelper)
    {
    }
}
