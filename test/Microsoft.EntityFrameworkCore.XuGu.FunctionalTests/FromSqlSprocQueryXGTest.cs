// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class FromSqlSprocQueryXGTest : FromSqlSprocQueryTestBase<NorthwindSprocQueryXGFixture>
    {
        public override void From_sql_queryable_stored_procedure()
        {
            base.From_sql_queryable_stored_procedure();

            Assert.Equal(
                @"`SYSDBA`.`TenMostExpensiveProducts`",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_projection()
        {
            base.From_sql_queryable_stored_procedure_projection();

            Assert.Equal(
                @"`SYSDBA`.`TenMostExpensiveProducts`",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_with_parameter()
        {
            base.From_sql_queryable_stored_procedure_with_parameter();

            Assert.Equal(
                @":p0: ALFKI (Size = 4000)

`SYSDBA`.`CustOrderHist` :CustomerID = :p0",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_reprojection()
        {
            base.From_sql_queryable_stored_procedure_reprojection();

            Assert.Equal(
                @"`SYSDBA`.`TenMostExpensiveProducts`",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_composed()
        {
            base.From_sql_queryable_stored_procedure_composed();

            Assert.Equal(
                @"`SYSDBA`.`TenMostExpensiveProducts`",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_with_parameter_composed()
        {
            base.From_sql_queryable_stored_procedure_with_parameter_composed();

            Assert.Equal(
                @":p0: ALFKI (Size = 4000)

`SYSDBA`.`CustOrderHist` :CustomerID = :p0",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_take()
        {
            base.From_sql_queryable_stored_procedure_take();

            Assert.Equal(
                @"`SYSDBA`.`TenMostExpensiveProducts`",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_min()
        {
            base.From_sql_queryable_stored_procedure_min();

            Assert.Equal(
                @"`SYSDBA`.`TenMostExpensiveProducts`",
                Sql);
        }

        public override void From_sql_queryable_with_multiple_stored_procedures()
        {
            base.From_sql_queryable_with_multiple_stored_procedures();

            Assert.StartsWith(
                @"`SYSDBA`.`TenMostExpensiveProducts`

`SYSDBA`.`TenMostExpensiveProducts`

`SYSDBA`.`TenMostExpensiveProducts`",
                Sql);
        }

        public override void From_sql_queryable_stored_procedure_and_select()
        {
            base.From_sql_queryable_stored_procedure_and_select();

            Assert.StartsWith(
                @"`SYSDBA`.`Ten Most Expensive Products`

SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`SupplierID`, `p`.`UnitsInStock`
 FROM (
    SELECT * FROM Products
) `p`

SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`SupplierID`, `p`.`UnitsInStock`
 FROM (
    SELECT * FROM Products
) `p`",
                Sql);
        }

        public override void From_sql_queryable_select_and_stored_procedure()
        {
            base.From_sql_queryable_select_and_stored_procedure();

            Assert.StartsWith(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`SupplierID`, `p`.`UnitsInStock`
 FROM (
    SELECT * FROM Products
) `p`

`SYSDBA`.`TenMostExpensiveProducts`

`SYSDBA`.`TenMostExpensiveProducts`",
                Sql);
        }

        public FromSqlSprocQueryXGTest(
            NorthwindSprocQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        protected override string TenMostExpensiveProductsSproc => "`SYSDBA`.`TenMostExpensiveProducts`";

        protected override string CustomerOrderHistorySproc => "`SYSDBA`.`CustOrderHist` :CustomerID = {0}";

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
