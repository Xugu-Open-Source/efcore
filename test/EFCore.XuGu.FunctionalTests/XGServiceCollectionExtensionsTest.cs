// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class XGServiceCollectionExtensionsTest : RelationalServiceCollectionExtensionsTestBase
    {
        public XGServiceCollectionExtensionsTest()
            : base(XGTestHelpers.Instance)
        {
        }
    }
}
