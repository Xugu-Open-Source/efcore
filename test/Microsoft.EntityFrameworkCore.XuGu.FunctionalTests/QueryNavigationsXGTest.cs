// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    [MonoVersionCondition(Min = "4.2.0", SkipReason = "Queries fail on Mono < 4.2.0 due to differences in the implementation of LINQ")]
    public class QueryNavigationsXGTest : QueryNavigationsTestBase<NorthwindQueryXGFixture>
    {
        [Fact]
        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Deep()
        {
            base.Select_Where_Navigation_Deep();

            Assert.StartsWith(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`, `od.Order`.`OrderID`, `od.Order`.`CustomerID`, `od.Order`.`EmployeeID`, `od.Order`.`OrderDate`, `od.Order.Customer`.`CustomerID`, `od.Order.Customer`.`Address`, `od.Order.Customer`.`City`, `od.Order.Customer`.`CompanyName`, `od.Order.Customer`.`ContactName`, `od.Order.Customer`.`ContactTitle`, `od.Order.Customer`.`Country`, `od.Order.Customer`.`Fax`, `od.Order.Customer`.`Phone`, `od.Order.Customer`.`PostalCode`, `od.Order.Customer`.`Region`
 FROM `OrderDetails` `od`
INNER JOIN `Orders` `od.Order` ON `od`.`OrderID` = `od.Order`.`OrderID`
LEFT JOIN `Customers` `od.Order.Customer` ON `od.Order`.`CustomerID` = `od.Order.Customer`.`CustomerID`
ORDER BY `od`.`OrderID`, `od`.`ProductID`, `od.Order`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Take_Select_Navigation()
        {
            base.Take_Select_Navigation();

            Assert.StartsWith(
                @":__p_0: 2

SELECT `t`.`CustomerID`
 FROM (
    SELECT `c0`.*
     FROM `Customers` `c0` LIMIT :__p_0
) `t`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
",
                Sql);
        }

        [Fact]
        public override void Select_collection_FirstOrDefault_project_single_column1()
        {
            base.Select_collection_FirstOrDefault_project_single_column1();

            Assert.Equal(
                @":__p_0: 2

SELECT (
    SELECT `o`.`CustomerID`
    FROM `Orders` `o`
    WHERE `t`.`CustomerID` = `o`.`CustomerID` LIMIT 1
)
FROM (
    SELECT `c0`.*
    FROM `Customers` `c0` LIMIT :__p_0
) `t`",
                Sql);
        }

        [Fact]
        public override void Select_collection_FirstOrDefault_project_single_column2()
        {
            base.Select_collection_FirstOrDefault_project_single_column2();

            Assert.Equal(
                @":__p_0: 2

SELECT (
    SELECT `o`.`CustomerID`
     FROM `Orders` `o`
    WHERE `t`.`CustomerID` = `o`.`CustomerID` LIMIT 1
)
 FROM (
    SELECT `c0`.*
     FROM `Customers` `c0` LIMIT :__p_0
) `t`",
                Sql);
        }

        [Fact]
        public override void Select_collection_FirstOrDefault_project_anonymous_type()
        {
            base.Select_collection_FirstOrDefault_project_anonymous_type();

            Assert.StartsWith(
                @":__p_0: 2

SELECT `t`.`CustomerID`
 FROM (
    SELECT `c0`.*
     FROM `Customers` `c0` LIMIT :__p_0
) `t`

SELECT `o`.`CustomerID`, `o`.`OrderID`
 FROM `Orders` `o`

SELECT `o`.`CustomerID`, `o`.`OrderID`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Select_collection_FirstOrDefault_project_entity()
        {
            base.Select_collection_FirstOrDefault_project_entity();

            Assert.StartsWith(
                @":__p_0: 2

SELECT `t`.`CustomerID`
 FROM (
    SELECT `c0`.*
     FROM `Customers` `c0` LIMIT :__p_0
) `t`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Skip_Select_Navigation()
        {
            base.Skip_Select_Navigation();

            if (TestEnvironment.GetFlag(nameof(XGCondition.SupportsOffset)) ?? true)
            {
                Assert.StartsWith(
                    @":__p_0: 20

SELECT `c`.`CustomerID`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`
LIMIT 999999999
OFFSET :__p_0

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
ORDER BY `o`.`OrderID`
",
                    Sql);
            }
        }

        [Fact]
        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_Navigation()
        {
            base.Select_Navigation();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_Navigations()
        {
            base.Select_Navigations();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Multiple_Access()
        {
            base.Select_Where_Navigation_Multiple_Access();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_Navigations_Where_Navigations()
        {
            base.Select_Navigations_Where_Navigations();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_count_plus_sum()
        {
            //base.Select_count_plus_sum();
            AssertQuery<Order>(os => os.Select(o => new
            {
                Total = o.OrderDetails.Sum(od => (decimal)od.Quantity) + o.OrderDetails.Count()
            }));

            Assert.StartsWith(
                @"SELECT (
    SELECT SUM(`od0`.`Quantity`)
     FROM `OrderDetails` `od0`
     WHERE `o`.`OrderID` = `od0`.`OrderID`
) + (
    SELECT COUNT(*)
     FROM `OrderDetails` `o1`
     WHERE `o`.`OrderID` = `o1`.`OrderID`
)
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Singleton_Navigation_With_Member_Access()
        {
            base.Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();

            Assert.StartsWith(
                @"",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();

            Assert.StartsWith(
                @"",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

            Assert.Equal(
                @"SELECT `o1`.`OrderID`, `o1`.`CustomerID`, `o1`.`EmployeeID`, `o1`.`OrderDate`, `o2`.`OrderID`, `o2`.`CustomerID`, `o2`.`EmployeeID`, `o2`.`OrderDate`
 FROM `Orders` `o1`
CROSS JOIN `Orders` `o2`
 WHERE (`o1`.`CustomerID` = `o2`.`CustomerID`) OR (`o1`.`CustomerID` IS NULL AND `o2`.`CustomerID` IS NULL)",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` IS NULL",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Null_Deep()
        {
            base.Select_Where_Navigation_Null_Deep();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`, `e.Manager`.`EmployeeID`, `e.Manager`.`City`, `e.Manager`.`Country`, `e.Manager`.`FirstName`, `e.Manager`.`ReportsTo`, `e.Manager`.`Title`
 FROM `Employees` `e`
LEFT JOIN `Employees` `e.Manager` ON `e`.`ReportsTo` = `e.Manager`.`EmployeeID`
ORDER BY `e`.`ReportsTo`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` IS NULL",
                Sql);
        }

        [Fact]
        public override void Select_collection_navigation_simple()
        {
            base.Select_collection_navigation_simple();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` LIKE 'A' || '%'

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Select_collection_navigation_multi_part()
        {
            base.Select_collection_navigation_multi_part();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
 WHERE `o`.`CustomerID` = 'ALFKI'
ORDER BY `o`.`CustomerID`

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_any()
        {
            base.Collection_select_nav_prop_any();

            Assert.Equal(
                @"SELECT (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
             FROM `Orders` `o0`
             WHERE `c`.`CustomerID` = `o0`.`CustomerID`)
        THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
    END
)
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_predicate()
        {
            base.Collection_select_nav_prop_predicate();

            Assert.Equal(
                @"SELECT CASE
    WHEN (
        SELECT COUNT(*)
         FROM `Orders` `o`
         WHERE `c`.`CustomerID` = `o`.`CustomerID`
    ) > 0
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Collection_where_nav_prop_any()
        {
            base.Collection_where_nav_prop_any();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE `c`.`CustomerID` = `o`.`CustomerID`)",
                Sql);
        }

        [Fact]
        public override void Collection_where_nav_prop_any_predicate()
        {
            base.Collection_where_nav_prop_any_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE (`o`.`OrderID` > 0) AND (`c`.`CustomerID` = `o`.`CustomerID`))",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_all()
        {
            base.Collection_select_nav_prop_all();

            Assert.Equal(
                @"SELECT (
    SELECT CASE
        WHEN NOT EXISTS (
            SELECT 1
             FROM `Orders` `o0`
             WHERE ((`c`.`CustomerID` = `o0`.`CustomerID`) AND `o0`.`CustomerID` IS NOT NULL) AND NOT ((`o0`.`CustomerID` = 'ALFKI') AND `o0`.`CustomerID` IS NOT NULL))
        THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
    END
)
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_all_client()
        {
            base.Collection_select_nav_prop_all_client();

            Assert.StartsWith(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`

SELECT `o1`.`OrderID`, `o1`.`CustomerID`, `o1`.`EmployeeID`, `o1`.`OrderDate`
 FROM `Orders` `o1`",
                Sql);
        }

        [Fact]
        public override void Collection_where_nav_prop_all()
        {
            base.Collection_where_nav_prop_all();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE NOT EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE ((`c`.`CustomerID` = `o`.`CustomerID`) AND `o`.`CustomerID` IS NOT NULL) AND NOT ((`o`.`CustomerID` = 'ALFKI') AND `o`.`CustomerID` IS NOT NULL))",
                Sql);
        }

        [Fact]
        public override void Collection_where_nav_prop_all_client()
        {
            base.Collection_where_nav_prop_all_client();

            Assert.StartsWith(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_count()
        {
            //base.Collection_select_nav_prop_count();
            AssertQuery<Customer>(
                cs => from c in cs
                      select new { Count=c.Orders.LongCount() },
                cs => from c in cs
                      select new { Count=(c.Orders ?? new List<Order>()).LongCount() });

            Assert.Equal(
                @"SELECT (
    SELECT COUNT(*)
     FROM `Orders` `o0`
     WHERE `c`.`CustomerID` = `o0`.`CustomerID`
)
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Collection_where_nav_prop_count()
        {
            //base.Collection_where_nav_prop_count();

            AssertQuery<Customer>(
                cs => from c in cs
                      where c.Orders.LongCount() > 5
                      select c,
                cs => from c in cs
                      where (c.Orders ?? new List<Order>()).LongCount() > 5
                      select c,
                entryCount: 63);

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE (
    SELECT COUNT(*)
     FROM `Orders` `o`
     WHERE `c`.`CustomerID` = `o`.`CustomerID`
) > 5",
                Sql);
        }

        [Fact]
        public override void Collection_where_nav_prop_count_reverse()
        {
            //base.Collection_where_nav_prop_count_reverse();

            AssertQuery<Customer>(
                cs => from c in cs
                      where 5 < c.Orders.LongCount()
                      select c,
                cs => from c in cs
                      where 5 < (c.Orders ?? new List<Order>()).LongCount()
                      select c,
                entryCount: 63);

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE 5 < (
    SELECT COUNT(*)
     FROM `Orders` `o`
     WHERE `c`.`CustomerID` = `o`.`CustomerID`
)",
                Sql);
        }

        [Fact]
        public override void Collection_orderby_nav_prop_count()
        {
            //base.Collection_orderby_nav_prop_count();

            AssertQuery<Customer>(
                cs => from c in cs
                      orderby c.Orders.LongCount()
                      select c,
                cs => from c in cs
                      orderby (c.Orders ?? new List<Order>()).LongCount()
                      select c,
                entryCount: 91);

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY (
    SELECT COUNT(*)
     FROM `Orders` `o`
     WHERE `c`.`CustomerID` = `o`.`CustomerID`
)",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_long_count()
        {
            base.Collection_select_nav_prop_long_count();

            Assert.Equal(
                @"SELECT (
    SELECT COUNT(*)
     FROM `Orders` `o0`
     WHERE `c`.`CustomerID` = `o0`.`CustomerID`
)
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Select_multiple_complex_projections()
        {
            //base.Select_multiple_complex_projections();

            AssertQuery<Order>(
                os => from o in os
                      where o.CustomerID.StartsWith("A")
                      select new
                      {
                          collection1 = o.OrderDetails.LongCount(),
                          scalar1 = o.OrderDate,
                          any = o.OrderDetails.Select(od => od.UnitPrice).Any(up => up > 10),
                          conditional = o.CustomerID == "ALFKI" ? "50" : "10",
                          scalar2 = (int?)o.OrderID,
                          all = o.OrderDetails.All(od => od.OrderID == 42),
                          collection2 = o.OrderDetails.LongCount(),
                      });

            Assert.Equal(
                @"SELECT (
    SELECT COUNT(*)
     FROM `OrderDetails` `o2`
     WHERE `o`.`OrderID` = `o2`.`OrderID`
), `o`.`OrderDate`, (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
             FROM `OrderDetails` `od1`
             WHERE (`od1`.`UnitPrice` > 10.0) AND (`o`.`OrderID` = `od1`.`OrderID`))
        THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
    END
), CASE
    WHEN `o`.`CustomerID` = 'ALFKI'
    THEN '50' ELSE '10'
END, `o`.`OrderID`, (
    SELECT CASE
        WHEN NOT EXISTS (
            SELECT 1
             FROM `OrderDetails` `od2`
             WHERE (`o`.`OrderID` = `od2`.`OrderID`) AND (`od2`.`OrderID` <> 42))
        THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
    END
), (
    SELECT COUNT_BIG(*)
     FROM `OrderDetails` `o3`
     WHERE `o`.`OrderID` = `o3`.`OrderID`
)
 FROM `Orders` `o`
 WHERE `o`.`CustomerID` LIKE 'A' || '%'",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_sum()
        {
            //base.Collection_select_nav_prop_sum();

            AssertQuery<Customer>(
                cs => from c in cs
                      select new { Sum = c.Orders.Sum(o => (decimal)o.OrderID) },
                cs => from c in cs
                      select new { Sum = (c.Orders ?? new List<Order>()).Sum(o => (decimal)o.OrderID) });

            Assert.Equal(
                @"SELECT (
    SELECT SUM(`o0`.`OrderID`)
     FROM `Orders` `o0`
     WHERE `c`.`CustomerID` = `o0`.`CustomerID`
)
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Collection_where_nav_prop_sum()
        {
            base.Collection_where_nav_prop_sum();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE (
    SELECT SUM(`o`.`OrderID`)
     FROM `Orders` `o`
     WHERE `c`.`CustomerID` = `o`.`CustomerID`
) > 1000",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_first_or_default()
        {
            base.Collection_select_nav_prop_first_or_default();

            // TODO: Projection sub-query lifting
            Assert.StartsWith(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_first_or_default_then_nav_prop()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop();

            // TODO: Projection sub-query lifting
            Assert.StartsWith(
                @"SELECT `e`.`CustomerID`
 FROM `Customers` `e`
 WHERE `e`.`CustomerID` LIKE 'A' || '%'

SELECT `e0`.`OrderID`, `e0`.`CustomerID`, `e0`.`EmployeeID`, `e0`.`OrderDate`, `e.Customer`.`CustomerID`, `e.Customer`.`Address`, `e.Customer`.`City`, `e.Customer`.`CompanyName`, `e.Customer`.`ContactName`, `e.Customer`.`ContactTitle`, `e.Customer`.`Country`, `e.Customer`.`Fax`, `e.Customer`.`Phone`, `e.Customer`.`PostalCode`, `e.Customer`.`Region`
 FROM `Orders` `e0`
LEFT JOIN `Customers` `e.Customer` ON `e0`.`CustomerID` = `e.Customer`.`CustomerID`
 WHERE `e0`.`OrderID` IN (10643, 10692, 10702, 10835, 10952, 11011)
ORDER BY `e0`.`CustomerID`

SELECT `e0`.`OrderID`, `e0`.`CustomerID`, `e0`.`EmployeeID`, `e0`.`OrderDate`, `e.Customer`.`CustomerID`, `e.Customer`.`Address`, `e.Customer`.`City`, `e.Customer`.`CompanyName`, `e.Customer`.`ContactName`, `e.Customer`.`ContactTitle`, `e.Customer`.`Country`, `e.Customer`.`Fax`, `e.Customer`.`Phone`, `e.Customer`.`PostalCode`, `e.Customer`.`Region`
 FROM `Orders` `e0`
LEFT JOIN `Customers` `e.Customer` ON `e0`.`CustomerID` = `e.Customer`.`CustomerID`
 WHERE `e0`.`OrderID` IN (10643, 10692, 10702, 10835, 10952, 11011)
ORDER BY `e0`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_first_or_default_then_nav_prop_nested()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop_nested();

            Assert.StartsWith(
                @"SELECT 1
 FROM `Customers` `e`
 WHERE `e`.`CustomerID` LIKE 'A' || '%'

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
 WHERE `o`.`CustomerID` = 'ALFKI'
ORDER BY `o`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
 WHERE `o`.`CustomerID` = 'ALFKI'
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_single_or_default_then_nav_prop_nested()
        {
            base.Collection_select_nav_prop_single_or_default_then_nav_prop_nested();

            Assert.StartsWith(
                @"SELECT 1
 FROM `Customers` `e`
 WHERE `e`.`CustomerID` LIKE 'A' || '%'

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
 WHERE `o`.`OrderID` = 10643
ORDER BY `o`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
 WHERE `o`.`OrderID` = 10643
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method();

            Assert.StartsWith(
                @"SELECT 1
 FROM `Customers` `e`
 WHERE `e`.`CustomerID` LIKE 'A' || '%'

SELECT `oo`.`OrderID`, `oo`.`CustomerID`, `oo`.`EmployeeID`, `oo`.`OrderDate`, `oo.Customer`.`CustomerID`, `oo.Customer`.`Address`, `oo.Customer`.`City`, `oo.Customer`.`CompanyName`, `oo.Customer`.`ContactName`, `oo.Customer`.`ContactTitle`, `oo.Customer`.`Country`, `oo.Customer`.`Fax`, `oo.Customer`.`Phone`, `oo.Customer`.`PostalCode`, `oo.Customer`.`Region`
 FROM `Orders` `oo`
LEFT JOIN `Customers` `oo.Customer` ON `oo`.`CustomerID` = `oo.Customer`.`CustomerID`
 WHERE `oo`.`CustomerID` = 'ALFKI'
ORDER BY `oo`.`CustomerID`

SELECT `oo`.`OrderID`, `oo`.`CustomerID`, `oo`.`EmployeeID`, `oo`.`OrderDate`, `oo.Customer`.`CustomerID`, `oo.Customer`.`Address`, `oo.Customer`.`City`, `oo.Customer`.`CompanyName`, `oo.Customer`.`ContactName`, `oo.Customer`.`ContactTitle`, `oo.Customer`.`Country`, `oo.Customer`.`Fax`, `oo.Customer`.`Phone`, `oo.Customer`.`PostalCode`, `oo.Customer`.`Region`
 FROM `Orders` `oo`
LEFT JOIN `Customers` `oo.Customer` ON `oo`.`CustomerID` = `oo.Customer`.`CustomerID`
 WHERE `oo`.`CustomerID` = 'ALFKI'
ORDER BY `oo`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Collection_select_nav_prop_first_or_default_then_nav_prop_nested_with_orderby()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop_nested_with_orderby();

            Assert.Equal(
                @"SELECT 1
 FROM `Customers` `e`
 WHERE `e`.`CustomerID` LIKE 'A' || '%'

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
 WHERE `o`.`CustomerID` = 'ALFKI'
ORDER BY `o`.`CustomerID`, `o`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
 WHERE `o`.`CustomerID` = 'ALFKI'
ORDER BY `o`.`CustomerID`, `o`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
 WHERE `o`.`CustomerID` = 'ALFKI'
ORDER BY `o`.`CustomerID`, `o`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
 WHERE `o`.`CustomerID` = 'ALFKI'
ORDER BY `o`.`CustomerID`, `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Navigation_fk_based_inside_contains()
        {
            base.Navigation_fk_based_inside_contains();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE `o`.`CustomerID` IN ('ALFKI')",
                Sql);
        }

        [Fact]
        public override void Navigation_inside_contains()
        {
            base.Navigation_inside_contains();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Navigation_inside_contains_nested()
        {
            base.Navigation_inside_contains_nested();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`, `od.Order`.`OrderID`, `od.Order`.`CustomerID`, `od.Order`.`EmployeeID`, `od.Order`.`OrderDate`, `od.Order.Customer`.`CustomerID`, `od.Order.Customer`.`Address`, `od.Order.Customer`.`City`, `od.Order.Customer`.`CompanyName`, `od.Order.Customer`.`ContactName`, `od.Order.Customer`.`ContactTitle`, `od.Order.Customer`.`Country`, `od.Order.Customer`.`Fax`, `od.Order.Customer`.`Phone`, `od.Order.Customer`.`PostalCode`, `od.Order.Customer`.`Region`
 FROM `OrderDetails` `od`
INNER JOIN `Orders` `od.Order` ON `od`.`OrderID` = `od.Order`.`OrderID`
LEFT JOIN `Customers` `od.Order.Customer` ON `od.Order`.`CustomerID` = `od.Order.Customer`.`CustomerID`
ORDER BY `od.Order`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Navigation_from_join_clause_inside_contains()
        {
            base.Navigation_from_join_clause_inside_contains();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `OrderDetails` `od`
INNER JOIN `Orders` `o` ON `od`.`OrderID` = `o`.`OrderID`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Where_subquery_on_navigation()
        {
            base.Where_subquery_on_navigation();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE EXISTS (
    SELECT 1
     FROM (
        SELECT `o`.`OrderID`, `o`.`ProductID`
         FROM `OrderDetails` `o`
         WHERE `p`.`ProductID` = `o`.`ProductID`
    ) `t00`
    INNER JOIN (
        SELECT `orderDetail`.`OrderID`, `orderDetail`.`ProductID`
         FROM `OrderDetails` `orderDetail`
         WHERE `orderDetail`.`Quantity` = 1 LIMIT 1
    ) `t1` ON (`t00`.`OrderID` = `t1`.`OrderID`) AND (`t00`.`ProductID` = `t1`.`ProductID`))",
                Sql);
        }

        [Fact]
        public override void Where_subquery_on_navigation2()
        {
            base.Where_subquery_on_navigation2();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE EXISTS (
    SELECT 1
     FROM (
        SELECT `o`.`OrderID`, `o`.`ProductID`
         FROM `OrderDetails` `o`
         WHERE `p`.`ProductID` = `o`.`ProductID`
    ) `t00`
    INNER JOIN (
        SELECT `o0`.`OrderID`, `o0`.`ProductID`
         FROM `OrderDetails` `o0`
        ORDER BY `o0`.`OrderID` DESC, `o0`.`ProductID` LIMIT 1
    ) `t1` ON (`t00`.`OrderID` = `t1`.`OrderID`) AND (`t00`.`ProductID` = `t1`.`ProductID`))",
                Sql);
        }

        [Fact]
        public override void Where_subquery_on_navigation_client_eval()
        {
            base.Where_subquery_on_navigation_client_eval();

            Assert.StartsWith(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`

SELECT `o3`.`OrderID`
 FROM `Orders` `o3`

SELECT `o2`.`CustomerID`, `o2`.`OrderID`
 FROM `Orders` `o2`

SELECT `o3`.`OrderID`
 FROM `Orders` `o3`",
                Sql);

        }

        [Fact]
        public override void Navigation_in_subquery_referencing_outer_query()
        {
            //base.Navigation_in_subquery_referencing_outer_query();
            using (var context = CreateContext())
            {
                var query = from o in context.Orders
                                // ReSharper disable once UseMethodAny.0
                            where (from od in context.OrderDetails
                                   where o.Customer.Country == od.Order.Customer.Country
                                   select od).LongCount() > 0
                            select o;

                var result = query.ToList();

                Assert.Equal(830, result.Count);
            }

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
INNER JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
 WHERE (
    SELECT COUNT(*)
     FROM `OrderDetails` `od`
    INNER JOIN `Orders` `od.Order` ON `od`.`OrderID` = `od.Order`.`OrderID`
    INNER JOIN `Customers` `od.Order.Customer` ON `od.Order`.`CustomerID` = `od.Order.Customer`.`CustomerID`
     WHERE (`o.Customer`.`Country` = `od.Order.Customer`.`Country`) OR (`o.Customer`.`Country` IS NULL AND `od.Order.Customer`.`Country` IS NULL)
) > 0",
                Sql);
        }

        [Fact]
        public override void GroupBy_on_nav_prop()
        {
            base.GroupBy_on_nav_prop();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Where_nav_prop_group_by()
        {
            //base.Where_nav_prop_group_by();

            AssertQuery<OrderDetail, IGrouping<short, OrderDetail>>(
                ods => from od in ods
                       where od.Order.CustomerID == "ALFKI"
                       group od by od.Quantity,
                asserter: (l2oItems, efItems) =>
                {
                    foreach (var pair in
                        from l2oItem in l2oItems
                        join efItem in efItems on l2oItem.Key equals efItem.Key
                        select new { l2oItem, efItem })
                    {
                        Assert.Equal(
                            pair.l2oItem.Select(i => i.OrderID).OrderBy(i => i),
                            pair.efItem.Select(i => i.OrderID).OrderBy(i => i));
                    }
                });

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` `od`
INNER JOIN `Orders` `od.Order` ON `od`.`OrderID` = `od.Order`.`OrderID`
 WHERE `od.Order`.`CustomerID` = 'ALFKI'
ORDER BY `od`.`Quantity`",
                Sql);
        }

        [Fact]
        public override void Let_group_by_nav_prop()
        {
            base.Let_group_by_nav_prop();

            Assert.Equal(
                @"",
                Sql);
        }

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private static string Sql => TestSqlLoggerFactory.Sql;

        public QueryNavigationsXGTest(NorthwindQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        protected new void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> query,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class
        {
            AssertQuery(query, query, assertOrder, entryCount, asserter);
        }
    }
}
