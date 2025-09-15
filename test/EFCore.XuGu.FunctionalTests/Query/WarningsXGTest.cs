// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class WarningsXGTest : WarningsTestBase<QueryNoClientEvalXGFixture>
    {
        public WarningsXGTest(QueryNoClientEvalXGFixture fixture)
            : base(fixture)
        {
        }
    }
}
