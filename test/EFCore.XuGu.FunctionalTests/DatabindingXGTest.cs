// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class DatabindingXGTest : DataBindingTestBase<F1XGFixture>
    {
        public DatabindingXGTest(F1XGFixture fixture)
            : base(fixture)
        {
        }
    }
}
