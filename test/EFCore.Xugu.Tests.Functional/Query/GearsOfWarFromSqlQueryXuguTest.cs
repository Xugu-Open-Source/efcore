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
        [ConditionalFact(Skip = "XuguDB FromSql column order residual (driver E5021). Wave5 residual.")]
        public override void From_sql_queryable_simple_columns_out_of_order()
            => base.From_sql_queryable_simple_columns_out_of_order();
}
}

