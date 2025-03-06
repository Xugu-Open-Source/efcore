// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class FromSqlQueryXuGuDbTest : FromSqlQueryTestBase<NorthwindQueryXuGuDbFixture>
    {
        public override void From_sql_queryable_simple()
        {
            base.From_sql_queryable_simple();

            Assert.Equal(
                @"SELECT * FROM ""Customers"" WHERE ""ContactName"" LIKE '%z%'",
                Sql);
        }

        public override void From_sql_queryable_simple_columns_out_of_order()
        {
            base.From_sql_queryable_simple_columns_out_of_order();

            Assert.Equal(
                @"SELECT ""Region"", ""PostalCode"", ""Phone"", ""Fax"", ""CustomerID"", ""Country"", ""ContactTitle"", ""ContactName"", ""CompanyName"", ""City"", ""Address"" FROM ""Customers""",
                Sql);
        }

        public override void From_sql_queryable_simple_columns_out_of_order_and_extra_columns()
        {
            base.From_sql_queryable_simple_columns_out_of_order_and_extra_columns();

            Assert.Equal(
                @"SELECT ""Region"", ""PostalCode"", ""PostalCode"" AS ""Foo"", ""Phone"", ""Fax"", ""CustomerID"", ""Country"", ""ContactTitle"", ""ContactName"", ""CompanyName"", ""City"", ""Address"" FROM ""Customers""",
                Sql);
        }

        public override void From_sql_queryable_composed()
        {
            base.From_sql_queryable_composed();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM (
    SELECT * FROM ""Customers""
) `c`
 WHERE `c`.`ContactName` LIKE ('%' || 'z') || '%'",
                Sql);
        }

        public override void From_sql_queryable_multiple_composed()
        {
            base.From_sql_queryable_multiple_composed();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM (
    SELECT * FROM ""Customers""
) `c`
CROSS JOIN (
    SELECT * FROM ""Orders""
) `o`
 WHERE `c`.`CustomerID` = `o`.`CustomerID`",
                Sql);
        }

        public override void From_sql_queryable_multiple_composed_with_closure_parameters()
        {
            base.From_sql_queryable_multiple_composed_with_closure_parameters();

            Assert.Equal(
                @":__8__locals1_startDate_1: 01/01/1997 00:00:00 (DbType = DateTime)
:__8__locals1_endDate_2: 01/01/1998 00:00:00 (DbType = DateTime)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM (
    SELECT * FROM ""Customers""
) `c`
CROSS JOIN (
    SELECT * FROM ""Orders"" WHERE ""OrderDate"" BETWEEN :__8__locals1_startDate_1 AND :__8__locals1_endDate_2
) `o`
 WHERE `c`.`CustomerID` = `o`.`CustomerID`",
                Sql);
        }

        public override void From_sql_queryable_multiple_composed_with_parameters_and_closure_parameters()
        {
            base.From_sql_queryable_multiple_composed_with_parameters_and_closure_parameters();

            Assert.Equal(
                @":p0: London (Size = 8000) (DbType = AnsiString)
:__8__locals1_startDate_1: 01/01/1997 00:00:00 (DbType = DateTime)
:__8__locals1_endDate_2: 01/01/1998 00:00:00 (DbType = DateTime)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM (
    SELECT * FROM ""Customers"" WHERE ""City"" = :p0
) `c`
CROSS JOIN (
    SELECT * FROM ""Orders"" WHERE ""OrderDate"" BETWEEN :__8__locals1_startDate_1 AND :__8__locals1_endDate_2
) `o`
 WHERE `c`.`CustomerID` = `o`.`CustomerID`",
                Sql);
        }

        public override void From_sql_queryable_multiple_line_query()
        {
            base.From_sql_queryable_multiple_line_query();

            Assert.Equal(
                @"SELECT *
FROM ""Customers""
WHERE ""City"" = 'London'",
                Sql);
        }

        public override void From_sql_queryable_composed_multiple_line_query()
        {
            base.From_sql_queryable_composed_multiple_line_query();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM (
    SELECT *
    FROM ""Customers""
) `c`
 WHERE `c`.`City` = 'London'",
                Sql);
        }

        public override void From_sql_queryable_with_parameters()
        {
            base.From_sql_queryable_with_parameters();

            Assert.Equal(
                @":p0: London (Size = 8000) (DbType = AnsiString)
:p1: Sales Representative (Size = 8000) (DbType = AnsiString)

SELECT * FROM ""Customers"" WHERE ""City"" = :p0 AND ""ContactTitle"" = :p1",
                Sql);
        }

        public override void From_sql_queryable_with_parameters_inline()
        {
            base.From_sql_queryable_with_parameters_inline();

            Assert.Equal(
                @":p0: London (Size = 8000) (DbType = AnsiString)
:p1: Sales Representative (Size = 8000) (DbType = AnsiString)

SELECT * FROM ""Customers"" WHERE ""City"" = :p0 AND ""ContactTitle"" = :p1",
                Sql);
        }

        public override void From_sql_queryable_with_null_parameter()
        {
            base.From_sql_queryable_with_null_parameter();

            Assert.Equal(
                @":p0:  (Nullable = false) (DbType = String)

SELECT * FROM ""Employees"" WHERE ""ReportsTo"" = :p0 OR (""ReportsTo"" IS NULL AND :p0 IS NULL)",
                Sql);
        }

        public override void From_sql_queryable_with_parameters_and_closure()
        {
            base.From_sql_queryable_with_parameters_and_closure();

            Assert.Equal(
                @":p0: London (Size = 8000) (DbType = AnsiString)
:__contactTitle_1: Sales Representative (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM (
    SELECT * FROM ""Customers"" WHERE ""City"" = :p0
) `c`
 WHERE `c`.`ContactTitle` = :__contactTitle_1",
                Sql);
        }

        public override void From_sql_queryable_simple_cache_key_includes_query_string()
        {
            base.From_sql_queryable_simple_cache_key_includes_query_string();

            Assert.Equal(
                @"SELECT * FROM ""Customers"" WHERE ""City"" = 'London'

SELECT * FROM ""Customers"" WHERE ""City"" = 'Seattle'",
                Sql);
        }

        public override void From_sql_queryable_with_parameters_cache_key_includes_parameters()
        {
            base.From_sql_queryable_with_parameters_cache_key_includes_parameters();

            Assert.Equal(
                @":p0: London (Size = 8000) (DbType = AnsiString)
:p1: Sales Representative (Size = 8000) (DbType = AnsiString)

SELECT * FROM ""Customers"" WHERE ""City"" = :p0 AND ""ContactTitle"" = :p1

:p0: Madrid (Size = 8000) (DbType = AnsiString)
:p1: Accounting Manager (Size = 8000) (DbType = AnsiString)

SELECT * FROM ""Customers"" WHERE ""City"" = :p0 AND ""ContactTitle"" = :p1",
                Sql);
        }

        public override void From_sql_queryable_simple_as_no_tracking_not_composed()
        {
            base.From_sql_queryable_simple_as_no_tracking_not_composed();

            Assert.Equal(
                @"SELECT * FROM ""Customers""",
                Sql);
        }

        public override void From_sql_queryable_simple_projection_composed()
        {
            base.From_sql_queryable_simple_projection_composed();

            Assert.Equal(
                @"SELECT `p`.`ProductName`
 FROM (
    SELECT *
     FROM Products
     WHERE Discontinued <> 1
    AND ((UnitsInStock + UnitsOnOrder) < ReorderLevel)
) `p`",
                Sql);
        }

        public override void From_sql_queryable_simple_include()
        {
            base.From_sql_queryable_simple_include();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM (
    SELECT * FROM ""Customers""
) `c`
ORDER BY `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM (
        SELECT * FROM ""Customers""
    ) `c`
     WHERE `o`.`CustomerID` = `c`.`CustomerID`)
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void From_sql_queryable_simple_composed_include()
        {
            base.From_sql_queryable_simple_composed_include();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM (
    SELECT * FROM ""Customers""
) `c`
 WHERE `c`.`City` = 'London'
ORDER BY `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM (
        SELECT * FROM ""Customers""
    ) `c`
     WHERE (`c`.`City` = 'London') AND (`o`.`CustomerID` = `c`.`CustomerID`))
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void From_sql_annotations_do_not_affect_successive_calls()
        {
            base.From_sql_annotations_do_not_affect_successive_calls();

            Assert.Equal(
                @"SELECT * FROM ""Customers"" WHERE ""ContactName"" LIKE '%z%'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        public override void From_sql_composed_with_nullable_predicate()
        {
            base.From_sql_composed_with_nullable_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM (
    SELECT * FROM ""Customers""
) `c`
 WHERE (`c`.`ContactName` = `c`.`CompanyName`) OR (`c`.`ContactName` IS NULL AND `c`.`CompanyName` IS NULL)",
                Sql);
        }

        public override void From_sql_with_dbParameter()
        {
            //base.From_sql_with_dbParameter();

            using (var context = CreateContext())
            {
                var parameter = CreateDbParameter(":city", "London");

                var actual = context.Customers
                    .FromSql(@"SELECT * FROM ""Customers"" WHERE ""City"" = :city", parameter)
                    .ToArray();

                Assert.Equal(6, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
            }

            Assert.Equal(
                @":city: London (Nullable = false) (DbType = Object)

SELECT * FROM ""Customers"" WHERE ""City"" = :city",
                Sql);
        }

        public override void From_sql_with_dbParameter_mixed()
        {
            //base.From_sql_with_dbParameter_mixed();

            using (var context = CreateContext())
            {
                var city = "London";
                var title = "Sales Representative";

                var titleParameter = CreateDbParameter(":title", title);

                var actual = context.Customers
                    .FromSql(@"SELECT * FROM ""Customers"" WHERE ""City"" = {0} AND ""ContactTitle"" = :title",
                        city,
                        titleParameter)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));

                var cityParameter = CreateDbParameter(":city", city);

                actual = context.Customers
                    .FromSql(@"SELECT * FROM ""Customers"" WHERE ""City"" = :city AND ""ContactTitle"" = {1}",
                        cityParameter,
                        title)
                    .ToArray();

                Assert.Equal(3, actual.Length);
                Assert.True(actual.All(c => c.City == "London"));
                Assert.True(actual.All(c => c.ContactTitle == "Sales Representative"));
            }

            Assert.Equal(
                @":p0: London (Size = 8000) (DbType = AnsiString)
:title: Sales Representative (Nullable = false) (DbType = Object)

SELECT * FROM ""Customers"" WHERE ""City"" = :p0 AND ""ContactTitle"" = :title

:city: London (Nullable = false) (DbType = Object)
:p1: Sales Representative (Size = 8000) (DbType = AnsiString)

SELECT * FROM ""Customers"" WHERE ""City"" = :city AND ""ContactTitle"" = :p1",
                Sql);
        }

        public FromSqlQueryXuGuDbTest(NorthwindQueryXuGuDbFixture fixture)
            : base(fixture)
        {
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new XGParameters
            {
                ParameterName = name,
                Value = value
            };

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
