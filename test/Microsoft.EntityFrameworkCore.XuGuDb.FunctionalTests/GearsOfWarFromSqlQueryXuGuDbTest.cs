// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class GearsOfWarFromSqlQueryXuGuDbTest : GearsOfWarFromSqlQueryTestBase<XuGuDbTestStore, GearsOfWarQueryXuGuDbFixture>
    {
        public override void From_sql_queryable_simple_columns_out_of_order()
        {
            base.From_sql_queryable_simple_columns_out_of_order();

            Assert.Equal(
                @"SELECT ""Id"", ""Name"", ""IsAutomatic"", ""AmmunitionType"", ""OwnerFullName"", ""SynergyWithId"" FROM ""Weapon"" ORDER BY ""Name""",
                Sql);
        }

        public GearsOfWarFromSqlQueryXuGuDbTest(GearsOfWarQueryXuGuDbFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
