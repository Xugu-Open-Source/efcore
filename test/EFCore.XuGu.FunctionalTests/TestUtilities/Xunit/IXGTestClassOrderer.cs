// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Collections.Generic;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities.Xunit;

public interface IXGTestClassOrderer
{
    IEnumerable<ITestClass> OrderTestClasses(IEnumerable<ITestClass> testClasses);
}
