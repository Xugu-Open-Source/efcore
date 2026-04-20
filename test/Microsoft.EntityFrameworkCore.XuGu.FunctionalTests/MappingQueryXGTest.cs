// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class MappingQueryXGTest : MappingQueryTestBase, IClassFixture<MappingQueryXGFixture>
    {
        [Fact]
        public override void All_customers()
        {
            base.All_customers();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`CompanyName`
 FROM `SYSDBA`.`Customers` `c`",
                Sql);
        }

        [Fact]
        public override void All_employees()
        {
            base.All_employees();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`
 FROM `SYSDBA`.`Employees` `e`",
                Sql);
        }

        [Fact]
        public override void All_orders()
        {
            base.All_orders();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`ShipVia`
 FROM `SYSDBA`.`Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Project_nullable_enum()
        {
            base.Project_nullable_enum();

            Assert.Equal(
                @"SELECT `o`.`ShipVia`
 FROM `SYSDBA`.`Orders` `o`",
                Sql);
        }

        private readonly MappingQueryXGFixture _fixture;

        public MappingQueryXGTest(MappingQueryXGFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext() => _fixture.CreateContext();

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
