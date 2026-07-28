// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query
{
    public class GearsOfWarFromSqlQueryXGTest : GearsOfWarFromSqlQueryTestBase<GearsOfWarQueryXuguFixture>
    {
        public GearsOfWarFromSqlQueryXGTest(GearsOfWarQueryXuguFixture fixture)
            : base(fixture)
        {
        }
    }
}

