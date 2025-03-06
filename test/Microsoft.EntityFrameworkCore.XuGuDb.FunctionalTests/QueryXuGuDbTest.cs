// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version `2`.`0`. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Xunit;
using Xunit.Abstractions;

#if NETCOREAPP1_0
using System.Threading;
#endif

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    [MonoVersionCondition(Min = "4.2.0", SkipReason = "Queries fail on Mono < 4.2.0 due to differences in the implementation of LINQ")]
    public class QueryXuGuDbTest : QueryTestBase<NorthwindQueryXuGuDbFixture>
    {
        public QueryXuGuDbTest(NorthwindQueryXuGuDbFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        [Fact]
        public override void Local_array()
        {
            base.Local_array();

            Assert.Equal(
                @"?: ALFKI (Size = 450) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = ? LIMIT 2",
                Sql);
        }

        [Fact]
        public override void Entity_equality_self()
        {
            base.Entity_equality_self();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Entity_equality_local()
        {
            base.Entity_equality_local();

            Assert.Equal(
                @"?: ANATR (Nullable = false) (Size = 450) (DbType = Object)

SELECT `c`.`CustomerID`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = ?",
                Sql);
        }

        [Fact]
        public override void Entity_equality_local_inline()
        {
            base.Entity_equality_local_inline();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ANATR'",
                Sql);
        }

        [Fact]
        public override void Queryable_reprojection()
        {
            base.Queryable_reprojection();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Default_if_empty_top_level()
        {
            base.Default_if_empty_top_level();

            Assert.Equal(
                @"SELECT `t`.`EmployeeID`, `t`.`City`, `t`.`Country`, `t`.`FirstName`, `t`.`ReportsTo`, `t`.`Title`
 FROM (
    SELECT NULL `empty`
) `empty0`
LEFT JOIN (
    SELECT `c`.`EmployeeID`, `c`.`City`, `c`.`Country`, `c`.`FirstName`, `c`.`ReportsTo`, `c`.`Title`
     FROM `Employees` `c`
     WHERE `c`.`EmployeeID` = -1
) `t` ON 1 = 1",
                Sql);
        }

        [Fact]
        public override void Default_if_empty_top_level_positive()
        {
            base.Default_if_empty_top_level_positive();

            Assert.Equal(
                @"SELECT `t`.`EmployeeID`, `t`.`City`, `t`.`Country`, `t`.`FirstName`, `t`.`ReportsTo`, `t`.`Title`
 FROM (
    SELECT NULL `empty`
) `empty0`
LEFT JOIN (
    SELECT `c`.`EmployeeID`, `c`.`City`, `c`.`Country`, `c`.`FirstName`, `c`.`ReportsTo`, `c`.`Title`
     FROM `Employees` `c`
     WHERE `c`.`EmployeeID` > 0
) `t` ON 1 = 1",
                Sql);
        }

        [Fact]
        public override void Default_if_empty_top_level_arg()
        {
            base.Default_if_empty_top_level_arg();

            Assert.Equal(
                @"SELECT `c`.`EmployeeID`, `c`.`City`, `c`.`Country`, `c`.`FirstName`, `c`.`ReportsTo`, `c`.`Title`
 FROM `Employees` `c`
 WHERE `c`.`EmployeeID` = -1",
                Sql);
        }

        [Fact]
        public override void Default_if_empty_top_level_projection()
        {
            base.Default_if_empty_top_level_projection();

            Assert.Equal(
                @"SELECT `t`.`EmployeeID`
 FROM (
    SELECT NULL `empty`
) `empty0`
LEFT JOIN (
    SELECT `e`.`EmployeeID`
     FROM `Employees` `e`
     WHERE `e`.`EmployeeID` = -1
) `t` ON 1 = 1",
                Sql);
        }

        [Fact]
        public override void Where_query_composition()
        {
            base.Where_query_composition();

            Assert.Equal(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`
 WHERE `e1`.`FirstName` = (SELECT `e`.`FirstName` FROM (SELECT `e`.`FirstName`
 FROM `Employees` `e`
ORDER BY `e`.`EmployeeID`) WHERE ROWNUM <=1)",
                Sql);
        }

        [Fact]
        public override void Where_query_composition_is_null()
        {
            base.Where_query_composition_is_null();

            Assert.StartsWith(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`

SELECT `e2`.`EmployeeID`, `e2`.`City`, `e2`.`Country`, `e2`.`FirstName`, `e2`.`ReportsTo`, `e2`.`Title`
 FROM `Employees` `e2`",
                Sql);
        }

        [Fact]
        public override void Where_query_composition_is_not_null()
        {
            base.Where_query_composition_is_null();

            Assert.StartsWith(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`

SELECT `e2`.`EmployeeID`, `e2`.`City`, `e2`.`Country`, `e2`.`FirstName`, `e2`.`ReportsTo`, `e2`.`Title`
 FROM `Employees` `e2`",
                Sql);
        }

        [Fact]
        public override void Where_query_composition_entity_equality_one_element_SingleOrDefault()
        {
            base.Where_query_composition_entity_equality_one_element_SingleOrDefault();

            Assert.StartsWith(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`

SELECT `e20`.`EmployeeID`
 FROM `Employees` `e20`",
                Sql);
        }

        [Fact]
        public override void Where_query_composition_entity_equality_one_element_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_one_element_FirstOrDefault();

            Assert.Equal(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`
 WHERE (
    SELECT `e2`.`EmployeeID`
     FROM `Employees` `e2`
     WHERE `e2`.`EmployeeID` = `e1`.`ReportsTo` LIMIT 1
) = 0",
                Sql);
        }

        [Fact]
        public override void Where_query_composition_entity_equality_no_elements_SingleOrDefault()
        {
            base.Where_query_composition_entity_equality_no_elements_SingleOrDefault();

            Assert.StartsWith(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`

SELECT `e20`.`EmployeeID`
 FROM `Employees` `e20`
 WHERE `e20`.`EmployeeID` = 42 LIMIT 2", Sql);
        }

        [Fact]
        public override void Where_query_composition_entity_equality_no_elements_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_no_elements_FirstOrDefault();

            Assert.Equal(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`
 WHERE (
    SELECT `e2`.`EmployeeID`
     FROM `Employees` `e2`
     WHERE `e2`.`EmployeeID` = 42 LIMIT 1
) = 0",
                Sql);
        }

        [Fact]
        public override void Where_query_composition_entity_equality_multiple_elements_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_multiple_elements_FirstOrDefault();

            Assert.Equal(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`
 WHERE (
    SELECT `e2`.`EmployeeID`
     FROM `Employees` `e2`
     WHERE (`e2`.`EmployeeID` <> `e1`.`ReportsTo`) OR `e1`.`ReportsTo` IS NULL LIMIT 1
) = 0",
                Sql);
        }

        [Fact]
        public override void Where_query_composition2()
        {
            //base.Where_query_composition2();

            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    where e1.FirstName ==
                          (from e2 in es.OrderBy(e => e.EmployeeID)
                           select e2)
                              .First().FirstName
                    select e1,
                entryCount: 1);

            Assert.StartsWith(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`
 WHERE `e1`.`FirstName` = (SELECT `e`.`FirstName` FROM (SELECT `e`.`FirstName`
 FROM `Employees` `e`
ORDER BY `e`.`EmployeeID`) WHERE ROWNUM <=1)",
                Sql);
        }

        [Fact]
        public override void Where_shadow_subquery_first()
        {
            base.Where_shadow_subquery_first();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`Title` = (SELECT `e2`.`Title` FROM (SELECT `e2`.`Title`
 FROM `Employees` `e2`
ORDER BY `e2`.`Title`) WHERE ROWNUM <=1)",
                Sql);
        }

        [Fact]
        public override void Select_Subquery_Single()
        {
            base.Select_Subquery_Single();

            Assert.Equal(
                @"?: 2

SELECT `od`.`OrderID`
 FROM `OrderDetails` `od` LIMIT ?

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Subquery_Deep_Single()
        {
            base.Select_Where_Subquery_Deep_Single();

            Assert.StartsWith(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` `od`

SELECT `o0`.`OrderID`, `o0`.`CustomerID`
 FROM `Orders` `o0`

SELECT `c2`.`CustomerID`, `c2`.`City`
 FROM `Customers` `c2`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Subquery_Deep_First()
        {
            base.Select_Where_Subquery_Deep_First();

            Assert.Equal(
                @"?: 2

SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` `od`
 WHERE (
    SELECT (
        SELECT `c`.`City`
         FROM `Customers` `c`
         WHERE `o`.`CustomerID` = `c`.`CustomerID` LIMIT 1
    )
     FROM `Orders` `o`
     WHERE `od`.`OrderID` = `o`.`OrderID` LIMIT 1
) = 'Seattle' LIMIT ?",
                Sql);
        }

        [Fact]
        public override void Select_Where_Subquery_Equality()
        {
            base.Select_Where_Subquery_Equality();

            Assert.StartsWith(
                @"?: 2

SELECT `t`.`OrderID`, `t`.`CustomerID`, `t`.`EmployeeID`, `t`.`OrderDate`
FROM (
    SELECT TOP(?) `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
    FROM `Orders` `o0`
) `t`

SELECT `t1`.`OrderID`
FROM (
    SELECT TOP(2) `o4`.`OrderID`, `o4`.`ProductID`, `o4`.`Discount`, `o4`.`Quantity`, `o4`.`UnitPrice`
    FROM `OrderDetails` `o4`
) `t1`

SELECT `c5`.`CustomerID`, `c5`.`Country`
FROM `Customers` `c5`

SELECT o23.OrderID, `c6`.`Country`
FROM `Orders` `o23`
INNER JOIN `Customers` AS `c6` ON `o23`.`CustomerID` = `c6`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Where_subquery_anon()
        {
            base.Where_subquery_anon();

            Assert.Equal(
                @"?: 9

SELECT `t`.`EmployeeID`, `t`.`City`, `t`.`Country`, `t`.`FirstName`, `t`.`ReportsTo`, `t`.`Title`, `t0`.`OrderID`, `t0`.`CustomerID`, `t0`.`EmployeeID`, `t0`.`OrderDate`
 FROM (
    SELECT `e0`.`EmployeeID`, `e0`.`City`, `e0`.`Country`, `e0`.`FirstName`, `e0`.`ReportsTo`, `e0`.`Title`
     FROM `Employees` `e0` LIMIT ?
) `t`
CROSS JOIN (
    SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
     FROM `Orders` `o0` LIMIT 1000
) `t0`",
                Sql);
        }

        [Fact]
        public override void Where_subquery_correlated()
        {
            base.Where_subquery_correlated();

            Assert.Equal(
                @"SELECT `c1`.`CustomerID`, `c1`.`Address`, `c1`.`City`, `c1`.`CompanyName`, `c1`.`ContactName`, `c1`.`ContactTitle`, `c1`.`Country`, `c1`.`Fax`, `c1`.`Phone`, `c1`.`PostalCode`, `c1`.`Region`
 FROM `Customers` `c1`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c2`
     WHERE `c1`.`CustomerID` = `c2`.`CustomerID`)",
                Sql);
        }

        [Fact]
        public override void Where_subquery_correlated_client_eval()
        {
            base.Where_subquery_correlated_client_eval();

            Assert.StartsWith(
                @"SELECT `c1`.`CustomerID`, `c1`.`Address`, `c1`.`City`, `c1`.`CompanyName`, `c1`.`ContactName`, `c1`.`ContactTitle`, `c1`.`Country`, `c1`.`Fax`, `c1`.`Phone`, `c1`.`PostalCode`, `c1`.`Region`
 FROM `Customers` `c1`

SELECT `c2`.`CustomerID`, `c2`.`Address`, `c2`.`City`, `c2`.`CompanyName`, `c2`.`ContactName`, `c2`.`ContactTitle`, `c2`.`Country`, `c2`.`Fax`, `c2`.`Phone`, `c2`.`PostalCode`, `c2`.`Region`
 FROM `Customers` `c2`",
                Sql);
        }

        [Fact]
        public override void OrderBy_SelectMany()
        {
            base.OrderBy_SelectMany();


            Assert.StartsWith(
                @"SELECT `c`.`CustomerID`, `c`.`ContactName`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`

SELECT `o`.`CustomerID`, `o`.`OrderID`
 FROM `Orders` `o`
ORDER BY `o`.`OrderID`",
                Sql);
        }

        [Fact]
        public override void Let_any_subquery_anonymous()
        {
            base.Let_any_subquery_anonymous();

            Assert.StartsWith(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
             FROM `Orders`  `o0`
             WHERE `o0`.`CustomerID` = `c`.`CustomerID`)
        THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
    END
)
 FROM `Customers`  `c`

SELECT `o1`.`OrderID`, `o1`.`CustomerID`, `o1`.`EmployeeID`, `o1`.`OrderDate`
 FROM `Orders` `o1`",
                Sql);
        }

        [Fact]
        public override void GroupBy_join_default_if_empty_anonymous()
        {
            base.GroupBy_join_default_if_empty_anonymous();

            Assert.Equal(
                @"SELECT `orders`.`OrderID`, `orders`.`CustomerID`, `orders`.`EmployeeID`, `orders`.`OrderDate`, `orderDetail`.`OrderID`, `orderDetail`.`ProductID`, `orderDetail`.`Discount`, `orderDetail`.`Quantity`, `orderDetail`.`UnitPrice`
FROM `Orders` `orders`
LEFT JOIN `OrderDetails` `orderDetail` ON `orders`.`OrderID` = `orderDetail`.`OrderID`
ORDER BY `orders`.`OrderID`",
                Sql);
        }

        [Fact]
        public override void Where_simple_closure()
        {
            base.Where_simple_closure();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?",
                Sql);
        }

        [Fact]
        public override void Where_simple_closure_constant()
        {
            base.Where_simple_closure_constant();

            Assert.Equal(
                @"?: True (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE ? = 1",
                Sql);
        }

        [Fact]
        public override void Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            base.Where_simple_closure_via_query_cache_nullable_type_reverse();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` IS NULL

?: 5 (Nullable = true)

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` = ?

?: 2 (Nullable = true)

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` = ?",
                Sql);
        }

        [Fact]
        public override void Where_simple_closure_via_query_cache_nullable_type()
        {
            base.Where_simple_closure_via_query_cache_nullable_type();

            Assert.Equal(
                @"?: 2 (Nullable = true)

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` = ?

?: 5 (Nullable = true)

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` = ?

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` IS NULL",
                Sql);
        }

        [Fact]
        public override void Where_new_instance_field_access_closure_via_query_cache()
        {
            base.Where_new_instance_field_access_closure_via_query_cache();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?

?: Seattle (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?",
                Sql);
        }

        [Fact]
        public override void Where_nested_property_access_closure_via_query_cache()
        {
            base.Where_nested_property_access_closure_via_query_cache();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?

?: Seattle (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?",
                Sql);
        }

        [Fact]
        public override void Where_nested_field_access_closure_via_query_cache()
        {
            base.Where_nested_field_access_closure_via_query_cache();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?

?: Seattle (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?",
                Sql);
        }

        [Fact]
        public override void Where_static_property_access_closure_via_query_cache()
        {
            base.Where_static_property_access_closure_via_query_cache();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?

?: Seattle (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?",
                Sql);
        }

        [Fact]
        public override void Where_property_access_closure_via_query_cache()
        {
            base.Where_property_access_closure_via_query_cache();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?

?: Seattle (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?",
                Sql);
        }

        [Fact]
        public override void Where_static_field_access_closure_via_query_cache()
        {
            base.Where_static_field_access_closure_via_query_cache();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?

?: Seattle (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?",
                Sql);
        }

        [Fact]
        public override void Where_field_access_closure_via_query_cache()
        {
            base.Where_field_access_closure_via_query_cache();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?

?: Seattle (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?",
                Sql);
        }

        [Fact]
        public override void Where_method_call_closure_via_query_cache()
        {
            base.Where_method_call_closure_via_query_cache();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?

?: Seattle (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?",
                Sql);
        }

        [Fact]
        public override void Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_reverse_closure_via_query_cache();

            Assert.Equal(
                @"?: 1 (Nullable = true)

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`EmployeeID` > ?

?: 5 (Nullable = true)

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`EmployeeID` > ?",
                Sql);
        }

        [Fact]
        public override void Where_method_call_nullable_type_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_closure_via_query_cache();

            Assert.Equal(
                @"?: 2

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` = ?

?: 5

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` = ?",
                Sql);
        }

        [Fact]
        public override void Where_simple_closure_via_query_cache()
        {
            base.Where_simple_closure_via_query_cache();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?

?: Seattle (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = ?",
                Sql);
        }

        [Fact]
        public override void Where_subquery_closure_via_query_cache()
        {
            base.Where_subquery_closure_via_query_cache();

            Assert.Equal(
                @"?: ALFKI (Size = 450) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE (`o`.`CustomerID` = ?) AND (`o`.`CustomerID` = `c`.`CustomerID`))

?: ANATR (Size = 450) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE (`o`.`CustomerID` = ?) AND (`o`.`CustomerID` = `c`.`CustomerID`))",
                Sql);
        }

        [Fact]
        public override void Count_with_predicate()
        {
            //base.Count_with_predicate();
            AssertQuery<Order>(
                os => os.LongCount(o => o.CustomerID == "ALFKI"));
            Assert.Equal(
                @"SELECT COUNT(*)
 FROM `Orders` `o`
 WHERE `o`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void Where_OrderBy_Count()
        {
            //base.Where_OrderBy_Count();
            AssertQuery<Order>(os => os.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).LongCount());
            Assert.Equal(
                @"SELECT COUNT(*)
 FROM `Orders` `o`
 WHERE `o`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void OrderBy_Where_Count()
        {
            //base.OrderBy_Where_Count();
            AssertQuery<Order>(os => os.OrderBy(o => o.OrderID).Where(o => o.CustomerID == "ALFKI").LongCount());
            Assert.Equal(
                @"SELECT COUNT(*)
 FROM `Orders` `o`
 WHERE `o`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void OrderBy_Count_with_predicate()
        {
            //base.OrderBy_Count_with_predicate();
            AssertQuery<Order>(os => os.OrderBy(o => o.OrderID).LongCount(o => o.CustomerID == "ALFKI"));
            Assert.Equal(
                @"SELECT COUNT(*)
 FROM `Orders` `o`
 WHERE `o`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void OrderBy_Where_Count_with_predicate()
        {
            //base.OrderBy_Where_Count_with_predicate();
            AssertQuery<Order>(os => os.OrderBy(o => o.OrderID).Where(o => o.OrderID > 10).LongCount(o => o.CustomerID != "ALFKI"));
            Assert.Equal(
                @"SELECT COUNT(*)
 FROM `Orders` `o`
 WHERE (`o`.`OrderID` > 10) AND ((`o`.`CustomerID` <> 'ALFKI') OR `o`.`CustomerID` IS NULL)",
                Sql);
        }

        [Fact]
        public override void Where_OrderBy_Count_client_eval()
        {
            //base.Where_OrderBy_Count_client_eval();
            AssertQuery<Order>(os => os.Where(o => ClientEvalPredicate(o)).OrderBy(o => ClientEvalSelectorStateless()).LongCount());

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Where_OrderBy_Count_client_eval_mixed()
        {
            //base.Where_OrderBy_Count_client_eval_mixed();
            AssertQuery<Order>(os => os.Where(o => o.OrderID > 10).OrderBy(o => ClientEvalPredicate(o)).LongCount());

            Assert.Equal(
                @"SELECT COUNT(*)
 FROM `Orders` `o`
 WHERE `o`.`OrderID` > 10",
                Sql);
        }

        [Fact]
        public override void OrderBy_Where_Count_client_eval()
        {
            //base.OrderBy_Where_Count_client_eval();

            AssertQuery<Order>(os => os.OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o)).LongCount());

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void OrderBy_Where_Count_client_eval_mixed()
        {
            base.OrderBy_Where_Count_client_eval_mixed();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void OrderBy_Count_with_predicate_client_eval()
        {
            base.OrderBy_Count_with_predicate_client_eval();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void OrderBy_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Count_with_predicate_client_eval_mixed();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void OrderBy_Where_Count_with_predicate_client_eval()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void OrderBy_Where_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval_mixed();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` <> 'ALFKI') OR `o`.`CustomerID` IS NULL",
                Sql);
        }

        [Fact]
        public override void OrderBy_client_Take()
        {
            //base.OrderBy_client_Take();
            AssertQuery<Employee>(es => es.OrderBy(o => ClientEvalSelectorStateless()).Take(10), entryCount: 9);

            Assert.Equal(
                @"?: 42
?: 10

SELECT `o`.`EmployeeID`, `o`.`City`, `o`.`Country`, `o`.`FirstName`, `o`.`ReportsTo`, `o`.`Title`
 FROM `Employees` `o`
ORDER BY ? LIMIT ?",
                Sql);
        }

        [Fact]
        public override void OrderBy_arithmetic()
        {
            base.OrderBy_arithmetic();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
ORDER BY `e`.`EmployeeID` - `e`.`EmployeeID`",
                Sql);
        }

        [Fact]
        public override void OrderBy_condition_comparison()
        {
            base.OrderBy_condition_comparison();

            Assert.Equal(
               @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
ORDER BY CASE
    WHEN `p`.`UnitsInStock` > 0
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END, `p`.`ProductID`",
               Sql);
        }

        [Fact]
        public override void OrderBy_ternary_conditions()
        {
            base.OrderBy_ternary_conditions();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
ORDER BY CASE
    WHEN ((`p`.`UnitsInStock` > 10) AND (`p`.`ProductID` > 40)) OR ((`p`.`UnitsInStock` <= 10) AND (`p`.`ProductID` <= 40))
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END, `p`.`ProductID`",
                Sql);
        }

        [Fact]
        public override void OrderBy_any()
        {
            base.OrderBy_any();

            Assert.Equal(
                @"SELECT `p`.`CustomerID`, `p`.`Address`, `p`.`City`, `p`.`CompanyName`, `p`.`ContactName`, `p`.`ContactTitle`, `p`.`Country`, `p`.`Fax`, `p`.`Phone`, `p`.`PostalCode`, `p`.`Region`
 FROM `Customers` `p`
ORDER BY (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
             FROM `Orders` `o`
             WHERE (`o`.`OrderID` > 11000) AND (`p`.`CustomerID` = `o`.`CustomerID`))
        THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
    END
), `p`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Sum_with_no_arg()
        {
            base.Sum_with_no_arg();

            Assert.Equal(
                @"SELECT SUM(`o`.`OrderID`)
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Sum_with_arg()
        {
            base.Sum_with_arg();

            Assert.Equal(
                @"SELECT SUM(`o`.`OrderID`)
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Sum_with_arg_expression()
        {
            base.Sum_with_arg_expression();

            Assert.Equal(
                @"SELECT SUM(`o`.`OrderID` + `o`.`OrderID`)
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Sum_with_binary_expression()
        {
            base.Sum_with_binary_expression();

            Assert.Equal(
                @"SELECT SUM(`o`.`OrderID` * 2)
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Sum_with_division_on_decimal()
        {
            base.Sum_with_division_on_decimal();

            Assert.Equal(@"SELECT SUM(`od`.`Quantity` / 2.09)
 FROM `OrderDetails` `od`", Sql);
        }
        
        [Fact]
        public override void Sum_with_division_on_decimal_no_significant_digits()
        {
            base.Sum_with_division_on_decimal_no_significant_digits();

            Assert.Equal(@"SELECT SUM(`od`.`Quantity` / 2.0)
 FROM `OrderDetails` `od`", Sql);
        }

        [Fact]
        public override void Min_with_no_arg()
        {
            base.Min_with_no_arg();

            Assert.Equal(
                @"SELECT MIN(`o`.`OrderID`)
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Min_with_arg()
        {
            base.Min_with_arg();

            Assert.Equal(
                @"SELECT MIN(`o`.`OrderID`)
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Max_with_no_arg()
        {
            base.Max_with_no_arg();

            Assert.Equal(
                @"SELECT MAX(`o`.`OrderID`)
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Max_with_arg()
        {
            base.Max_with_arg();

            Assert.Equal(
                @"SELECT MAX(`o`.`OrderID`)
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Distinct_Count()
        {
            //base.Distinct_Count();

            AssertQuery<Customer>(
                cs => cs.Distinct().LongCount());

            Assert.Equal(
                @"SELECT COUNT(*)
 FROM (
    SELECT DISTINCT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
     FROM `Customers` `c`
) `t`",
                Sql);
        }

        [Fact]
        public override void Select_Distinct_Count()
        {
            //base.Select_Distinct_Count();

            AssertQuery<Customer>(
                cs => cs.Select(c => c.City).Distinct().LongCount());

            Assert.Equal(
                @"SELECT COUNT(*)
 FROM (
    SELECT DISTINCT `c`.`City`
     FROM `Customers` `c`
) `t`",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Skip()
        {
            base.Skip();

            Assert.Equal(
                @"?: 5

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`
LIMIT 999999999
OFFSET ?",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Skip_no_orderby()
        {
            base.Skip_no_orderby();

            Assert.Equal(
                @"?: 5

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY ROWID
LIMIT 999999999
OFFSET ?",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Skip_Take()
        {
            base.Skip_Take();

            Assert.Equal(
                @"?: 10
?: 5

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`ContactName`
LIMIT ?
OFFSET ?",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Join_Customers_Orders_Skip_Take()
        {
            base.Join_Customers_Orders_Skip_Take();

            Assert.Equal(
                @"?: 5
?: 10

SELECT `c`.`ContactName`, `o`.`OrderID`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
ORDER BY `o`.`OrderID`
LIMIT ?
OFFSET ?",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Join_Customers_Orders_Projection_With_String_Concat_Skip_Take()
        {
            base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take();

            Assert.Equal(
                @"?: 5
?: 10

SELECT (`c`.`ContactName` || ' ') || `c`.`ContactTitle`, `o`.`OrderID`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
ORDER BY `o`.`OrderID`
LIMIT ?
OFFSET ?",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Take_Skip()
        {
            base.Take_Skip();

            Assert.Equal(
                @"?: 10
?: 5

SELECT `t`.*
 FROM (
    SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
     FROM `Customers` `c`
    ORDER BY `c`.`ContactName` LIMIT ?
) `t`
ORDER BY `t`.`ContactName`
LIMIT 999999999
OFFSET ?",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();

            Assert.Equal(
                @"?: 10
@?: 5

SELECT DISTINCT t0.*
 FROM (
    SELECT t.*
     FROM (
        SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
         FROM `Customers` `c`
        ORDER BY `c`.`ContactName` LIMIT ?
    ) `t`
    ORDER BY `t`.`ContactName`
    LIMIT 999999999
    OFFSET ?
) `t0`",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Take_Skip_Distinct_Caching()
        {
            base.Take_Skip_Distinct_Caching();

            Assert.Equal(
                @"?: 10
?: 5

SELECT DISTINCT t0.*
 FROM (
    SELECT t.*
     FROM (
        SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
         FROM `Customers` `c`
        ORDER BY `c`.`ContactName` LIMIT ?
    ) `t`
    ORDER BY `t`.`ContactName`
    LIMIT 999999999
    OFFSET ?
) `t0`

?: 15
?: 10

SELECT DISTINCT t0.*
 FROM (
    SELECT t.*
     FROM (
        SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
         FROM `Customers` `c`
        ORDER BY `c`.`ContactName` LIMIT ?
    ) `t`
    ORDER BY `t`.`ContactName`
    LIMIT 999999999
    OFFSET ?
) `t0`",
                Sql);
        }

        public void Skip_when_no_OrderBy()
        {
            Assert.Throws<Exception>(() => AssertQuery<Customer>(cs => cs.Skip(5).Take(10)));
        }

        [Fact]
        public override void Take_Distinct_Count()
        {
            //base.Take_Distinct_Count();

            AssertQuery<Order>(
                os => os.Take(5).Distinct().LongCount());

            Assert.Equal(
                @"?: 5

SELECT COUNT(*)
 FROM (
    SELECT DISTINCT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
     FROM `Orders` `o` LIMIT ?
) `t`",
                Sql);
        }

        [Fact]
        public override void Take_Where_Distinct_Count()
        {
            //base.Take_Where_Distinct_Count();

            AssertQuery<Order>(
                os => os.Where(o => o.CustomerID == "FRANK").Take(5).Distinct().LongCount());

            Assert.Equal(
                @"?: 5

SELECT COUNT(*)
 FROM (
    SELECT DISTINCT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
     FROM `Orders` `o`
     WHERE `o`.`CustomerID` = 'FRANK' LIMIT ?
) `t`",
                Sql);
        }

        [Fact]
        public override void Queryable_simple()
        {
            base.Queryable_simple();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Queryable_simple_anonymous()
        {
            base.Queryable_simple_anonymous();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Queryable_nested_simple()
        {
            base.Queryable_nested_simple();

            Assert.Equal(
                @"SELECT `c3`.`CustomerID`, `c3`.`Address`, `c3`.`City`, `c3`.`CompanyName`, `c3`.`ContactName`, `c3`.`ContactTitle`, `c3`.`Country`, `c3`.`Fax`, `c3`.`Phone`, `c3`.`PostalCode`, `c3`.`Region`
 FROM `Customers` `c3`",
                Sql);
        }

        [Fact]
        public override void Queryable_simple_anonymous_projection_subquery()
        {
            base.Queryable_simple_anonymous_projection_subquery();

            Assert.Equal(
                @"?: 91

SELECT `t`.`City`
 FROM (
    SELECT `c0`.*
     FROM `Customers` `c0` LIMIT ?
) `t`",
                Sql);
        }

        [Fact]
        public override void Queryable_simple_anonymous_subquery()
        {
            base.Queryable_simple_anonymous_subquery();

            Assert.Equal(
                @"?: 91

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c` LIMIT ?",
                Sql);
        }

        [Fact]
        public override void Take_simple()
        {
            base.Take_simple();

            Assert.Equal(
                @"?: 10

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID` LIMIT ?",
                Sql);
        }

        [Fact]
        public override void Take_simple_parameterized()
        {
            base.Take_simple_parameterized();

            Assert.Equal(
                @"?: 10

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID` LIMIT ?",
                Sql);
        }

        [Fact]
        public override void Take_simple_projection()
        {
            base.Take_simple_projection();

            Assert.Equal(
                @"?: 10

SELECT `c`.`City`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID` LIMIT ?",
                Sql);
        }

        [Fact]
        public override void Take_subquery_projection()
        {
            base.Take_subquery_projection();

            Assert.Equal(
                @"?: 2

SELECT `t`.`City`
 FROM (
    SELECT `c0`.*
     FROM `Customers` `c0`
    ORDER BY `c0`.`CustomerID` LIMIT ?
) `t`",
                Sql);
        }

        [Fact]
        public override void OrderBy_Take_Count()
        {
            //base.OrderBy_Take_Count();

            AssertQuery<Order>(
                os => os.OrderBy(o => o.OrderID).Take(5).LongCount());

            Assert.Equal(
                @"?: 5

SELECT COUNT(*)
 FROM (
    SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
     FROM `Orders` `o`
    ORDER BY `o`.`OrderID` LIMIT ?
) `t`",
                Sql);
        }

        [Fact]
        public override void Take_OrderBy_Count()
        {
            //base.Take_OrderBy_Count();

            AssertQuery<Order>(
                os => os.Take(5).OrderBy(o => o.OrderID).LongCount());

            Assert.Equal(
                @"?: 5

SELECT COUNT(*)
 FROM (
    SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
     FROM `Orders` `o0` LIMIT ?
) `t`",
                Sql);
        }

        [Fact]
        public override void Any_simple()
        {
            base.Any_simple();

            Assert.Equal(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
         FROM `Customers` `c`)
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Any_predicate()
        {
            base.Any_predicate();

            Assert.Equal(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
         FROM `Customers` `c`
         WHERE `c`.`ContactName` LIKE 'A' + '%')
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Any_nested_negated()
        {
            base.Any_nested_negated();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE NOT EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE `o`.`CustomerID` LIKE 'A' || '%')",
                Sql);
        }

        [Fact]
        public override void Any_nested_negated2()
        {
            base.Any_nested_negated2();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE ((`c`.`City` <> 'London') OR `c`.`City` IS NULL) AND NOT EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE `o`.`CustomerID` LIKE 'A' || '%')",
                Sql);
        }

        [Fact]
        public override void Any_nested_negated3()
        {
            base.Any_nested_negated3();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE NOT EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE `o`.`CustomerID` LIKE 'A' || '%') AND ((`c`.`City` <> 'London') OR `c`.`City` IS NULL)",
                Sql);
        }

        [Fact]
        public override void Any_nested()
        {
            base.Any_nested();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE `o`.`CustomerID` LIKE 'A' || '%')",
                Sql);
        }

        [Fact]
        public override void Any_nested2()
        {
            base.Any_nested2();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE ((`c`.`City` <> 'London') OR `c`.`City` IS NULL) AND EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE `o`.`CustomerID` LIKE 'A' || '%')",
                Sql);
        }

        [Fact]
        public override void Any_nested3()
        {
            base.Any_nested3();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE `o`.`CustomerID` LIKE 'A' || '%') AND ((`c`.`City` <> 'London') OR `c`.`City` IS NULL)",
                Sql);
        }

        [Fact]
        public override void Any_with_multiple_conditions_still_uses_exists()
        {
            base.Any_with_multiple_conditions_still_uses_exists();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE (`c`.`City` = 'London') AND EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE (`o`.`EmployeeID` = 1) AND (`c`.`CustomerID` = `o`.`CustomerID`))",
                Sql);
        }

        [Fact]
        public override void All_top_level()
        {
            base.All_top_level();

            Assert.Equal(
                @"SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
         FROM `Customers` `c`
         WHERE NOT (`c`.`ContactName` LIKE 'A' || '%'))
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Select_scalar()
        {
            base.Select_scalar();

            Assert.Equal(
                @"SELECT `c`.`City`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Select_anonymous_one()
        {
            base.Select_anonymous_one();

            Assert.Equal(
                @"SELECT `c`.`City`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Select_anonymous_two()
        {
            base.Select_anonymous_two();

            Assert.Equal(
                @"SELECT `c`.`City`, `c`.`Phone`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Select_anonymous_three()
        {
            base.Select_anonymous_three();

            Assert.Equal(
                @"SELECT `c`.`City`, `c`.`Phone`, `c`.`Country`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Select_anonymous_bool_constant_true()
        {
            base.Select_anonymous_bool_constant_true();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Select_anonymous_constant_in_expression()
        {
            base.Select_anonymous_constant_in_expression();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, LEN(`c`.`CustomerID`) + 5
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Select_anonymous_conditional_expression()
        {
            base.Select_anonymous_conditional_expression();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, CASE
    WHEN `p`.`UnitsInStock` > 0
    THEN CAST(`1` AS BIT) ELSE CAST(`0` AS BIT)
END
 FROM `Products` `p`",
                Sql);
        }

        [Fact]
        public override void Select_scalar_primitive_after_take()
        {
            base.Select_scalar_primitive_after_take();

            Assert.Equal(
                @"?: 9

SELECT `t`.`EmployeeID`
 FROM (
    SELECT `e0`.*
     FROM `Employees` `e0` LIMIT ?
) `t`",
                Sql);
        }

        [Fact]
        public override void Select_constant_null_string()
        {
            base.Select_constant_null_string();

            Assert.Equal(
                @"SELECT 1
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Select_local()
        {
            base.Select_local();

            Assert.Equal(
                @"?: 10

SELECT ?
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_simple()
        {
            base.Where_simple();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = 'London'",
                Sql);
        }

        [Fact]
        public override void Where_simple_shadow()
        {
            base.Where_simple_shadow();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`Title` = 'Sales Representative'",
                Sql);
        }

        [Fact]
        public override void Where_simple_shadow_projection()
        {
            base.Where_simple_shadow_projection();

            Assert.Equal(
                @"SELECT `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`Title` = 'Sales Representative'",
                Sql);
        }

        [Fact]
        public override void Where_comparison_nullable_type_not_null()
        {
            base.Where_comparison_nullable_type_not_null();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` = 2",
                Sql);
        }

        [Fact]
        public override void Where_comparison_nullable_type_null()
        {
            base.Where_comparison_nullable_type_null();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` IS NULL",
                Sql);
        }

        [Fact]
        public override void Where_client()
        {
            base.Where_client();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_client_and_server_top_level()
        {
            base.Where_client_and_server_top_level();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` <> 'AROUT'",
                Sql);
        }

        [Fact]
        public override void Where_client_or_server_top_level()
        {
            base.Where_client_or_server_top_level();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_client_and_server_non_top_level()
        {
            base.Where_client_and_server_non_top_level();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_client_deep_inside_predicate_and_server_top_level()
        {
            base.Where_client_deep_inside_predicate_and_server_top_level();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` <> 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void First_client_predicate()
        {
            base.First_client_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Last()
        {
            base.Last();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`ContactName` DESC LIMIT 1",
                Sql);
        }

        [Fact]
        public override void Last_when_no_order_by()
        {
            base.Last_when_no_order_by();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void Last_Predicate()
        {
            base.Last_Predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = 'London'
ORDER BY `c`.`ContactName` DESC LIMIT 1",
                Sql);
        }

        [Fact]
        public override void Where_Last()
        {
            base.Where_Last();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = 'London'
ORDER BY `c`.`ContactName` DESC LIMIT 1",
                Sql);
        }

        [Fact]
        public override void LastOrDefault()
        {
            base.LastOrDefault();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`ContactName` DESC LIMIT 1",
                Sql);
        }

        [Fact]
        public override void LastOrDefault_Predicate()
        {
            base.LastOrDefault_Predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = 'London'
ORDER BY `c`.`ContactName` DESC LIMIT 1",
                Sql);
        }

        [Fact]
        public override void Where_LastOrDefault()
        {
            base.Where_LastOrDefault();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = 'London'
ORDER BY `c`.`ContactName` DESC LIMIT 1",
                Sql);
        }

        [Fact]
        public override void Where_equals_method_string()
        {
            base.Where_equals_method_string();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` = 'London'",
                Sql);
        }

        [Fact]
        public override void Where_equals_method_int()
        {
            base.Where_equals_method_int();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`EmployeeID` = 1",
                Sql);
        }

        [Fact]
        public override void Where_equals_using_object_overload_on_mismatched_types()
        {
            base.Where_equals_using_object_overload_on_mismatched_types();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE 0 = 1",
                Sql);

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: 'e.EmployeeID', '__longPrm_0'. This comparison will always return 'false'."));
        }

        [Fact]
        public override void Where_equals_using_int_overload_on_mismatched_types()
        {
            base.Where_equals_using_int_overload_on_mismatched_types();

            Assert.Equal(
                @"?: 1 (DbType = Int32)

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`EmployeeID` = ?",
                Sql);
        }

        [Fact]
        public override void Where_equals_on_mismatched_types_int_nullable_int()
        {
            base.Where_equals_on_mismatched_types_int_nullable_int();

            Assert.Equal(
                @"?: 2

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` = ?

?: 2

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE ? = `e`.`ReportsTo`",
                Sql);
        }

        [Fact]
        public override void Where_equals_on_mismatched_types_nullable_int_long()
        {
            base.Where_equals_on_mismatched_types_nullable_int_long();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE 0 = 1

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE 0 = 1",
                Sql);

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: 'e.ReportsTo', '__longPrm_0'. This comparison will always return 'false'."));

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: '__longPrm_0', 'e.ReportsTo'. This comparison will always return 'false'."));
        }

        [Fact]
        public override void Where_equals_on_mismatched_types_nullable_long_nullable_int()
        {
            base.Where_equals_on_mismatched_types_nullable_long_nullable_int();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE 0 = 1

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE 0 = 1",
                Sql);

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: '__nullableLongPrm_0', 'e.ReportsTo'. This comparison will always return 'false'."));

            Assert.True(TestSqlLoggerFactory.Log.Contains(
                "Possible unintended use of method Equals(object) for arguments of different types: 'e.ReportsTo', '__nullableLongPrm_0'. This comparison will always return 'false'."));
        }

        [Fact]
        public override void Where_equals_on_matched_nullable_int_types()
        {
            base.Where_equals_on_matched_nullable_int_types();

            Assert.Equal(
                @"?: 2 (Nullable = true)

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE ? = `e`.`ReportsTo`

?: 2 (Nullable = true)

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` = ?",
                Sql);
        }

        [Fact]
        public override void Where_equals_on_null_nullable_int_types()
        {
            base.Where_equals_on_null_nullable_int_types();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` IS NULL

SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
 WHERE `e`.`ReportsTo` IS NULL",
                Sql);
        }

        [Fact]
        public override void Where_string_length()
        {
            base.Where_string_length();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE LEN(`c`.`City`) = 6",
                Sql);
        }

        [Fact]
        public override void Where_datetime_date_component()
        {
            base.Where_datetime_date_component();

            Assert.Equal(
              @"?: 05/04/1998 00:00:00 (DbType = DateTime)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE CAST(`o`.`OrderDate` AS Date) = ?",
              Sql);
        }

        [Fact]
        public override void Where_datetime_day_component()
        {
            base.Where_datetime_day_component();

            Assert.Equal(
@"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE DAY(`o`.`OrderDate`) = 4",
              Sql);

        }

        [Fact]
        public override void Where_datetime_year_component()
        {
            base.Where_datetime_year_component();

            Assert.Equal(
@"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE YEAR(`o`.`OrderDate`) = 1998",
              Sql);

        }

        [Fact]
        public override void Where_datetime_dayOfYear_component()
        {
            base.Where_datetime_dayOfYear_component();

            Assert.Equal(
@"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE DAYOFYEAR(`o`.`OrderDate`) = 68",
              Sql);

        }

        [Fact]
        public override void Where_datetime_month_component()
        {
            base.Where_datetime_month_component();

            Assert.Equal(
@"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE MONTH(`o`.`OrderDate`) = 4",
              Sql);
        }

        [Fact]
        public override void Where_datetime_hour_component()
        {
            base.Where_datetime_hour_component();

            Assert.Equal(
@"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE HOUR(`o`.`OrderDate`) = 14",
              Sql);
        }

        [Fact]
        public override void Where_datetime_minute_component()
        {
            base.Where_datetime_minute_component();

            Assert.Equal(
@"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE MINUTE(`o`.`OrderDate`) = 23",
              Sql);
        }

        [Fact]
        public override void Where_datetime_second_component()
        {
            base.Where_datetime_second_component();

            Assert.Equal(
@"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE SECOND(`o`.`OrderDate`) = 44",
              Sql);
        }

        [Fact]
        public override void Where_datetime_millisecond_component()
        {
            base.Where_datetime_millisecond_component();

            Assert.Equal(
@"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE MICROSECOND(`o`.`OrderDate`) / 1000 = 88",
              Sql);
        }

        [Fact]
        public override void Where_datetime_now()
        {
            base.Where_datetime_now();

            Assert.Equal(
                @"?: 04/10/2015 00:00:00 (DbType = DateTime)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE NOW() <> ?",
                Sql);
        }

        [Fact]
        public override void Where_datetime_utcnow()
        {
            base.Where_datetime_utcnow();

            Assert.Equal(
                @"?: 04/10/2015 00:00:00 (DbType = DateTime)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE UTC_DATE() <> ?",
                Sql);
        }

        [Fact]
        public override void Where_is_null()
        {
            base.Where_is_null();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` IS NULL",
                Sql);
        }

        [Fact]
        public override void Where_is_not_null()
        {
            base.Where_is_not_null();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` IS NOT NULL",
                Sql);
        }

        [Fact]
        public override void Where_null_is_null()
        {
            base.Where_null_is_null();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_constant_is_null()
        {
            base.Where_constant_is_null();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE 0 = 1",
                Sql);
        }

        [Fact]
        public override void Where_null_is_not_null()
        {
            base.Where_null_is_not_null();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE 0 = 1",
                Sql);
        }

        [Fact]
        public override void Where_constant_is_not_null()
        {
            base.Where_constant_is_not_null();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_simple_reversed()
        {
            base.Where_simple_reversed();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE 'London' = `c`.`City`",
                Sql);
        }

        [Fact]
        public override void Where_identity_comparison()
        {
            base.Where_identity_comparison();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE (`c`.`City` = `c`.`City`) OR (`c`.`City` IS NULL AND `c`.`City` IS NULL)",
                Sql);
        }

        [Fact]
        public override void Where_select_many_or()
        {
            base.Where_select_many_or();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE (`c`.`City` = 'London') OR (`e`.`City` = 'London')",
                Sql);
        }

        [Fact]
        public override void Where_select_many_or2()
        {
            base.Where_select_many_or2();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE `c`.`City` IN ('London', 'Berlin')",
                Sql);
        }

        [Fact]
        public override void Where_select_many_or3()
        {
            base.Where_select_many_or3();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE `c`.`City` IN ('London', 'Berlin', 'Seattle')",
                Sql);
        }

        [Fact]
        public override void Where_select_many_or4()
        {
            base.Where_select_many_or4();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE `c`.`City` IN ('London', 'Berlin', 'Seattle', 'Lisboa')",
                Sql);
        }

        [Fact]
        public override void Where_select_many_or_with_parameter()
        {
            base.Where_select_many_or_with_parameter();

            Assert.Equal(
                @"?: London (Size = 4000) (DbType = Object)
?: Lisboa (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE `c`.`City` IN (?, 'Berlin', 'Seattle', ?)",
                Sql);
        }

        [Fact]
        public override void Where_in_optimization_multiple()
        {
            base.Where_in_optimization_multiple();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE (`c`.`City` IN ('London', 'Berlin') OR (`c`.`CustomerID` = 'ALFKI')) OR (`c`.`CustomerID` = 'ABCDE')",
                Sql);
        }

        [Fact]
        public override void Where_not_in_optimization1()
        {
            base.Where_not_in_optimization1();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE ((`c`.`City` <> 'London') OR `c`.`City` IS NULL) AND ((`e`.`City` <> 'London') OR `e`.`City` IS NULL)",
                Sql);
        }

        [Fact]
        public override void Where_not_in_optimization2()
        {
            base.Where_not_in_optimization2();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE `c`.`City` NOT IN ('London', 'Berlin')",
                Sql);
        }

        [Fact]
        public override void Where_not_in_optimization3()
        {
            base.Where_not_in_optimization3();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE `c`.`City` NOT IN ('London', 'Berlin', 'Seattle')",
                Sql);
        }

        [Fact]
        public override void Where_not_in_optimization4()
        {
            base.Where_not_in_optimization4();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE `c`.`City` NOT IN ('London', 'Berlin', 'Seattle', 'Lisboa')",
                Sql);
        }

        [Fact]
        public override void Where_select_many_and()
        {
            base.Where_select_many_and();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE ((`c`.`City` = 'London') AND (`c`.`Country` = 'UK')) AND ((`e`.`City` = 'London') AND (`e`.`Country` = 'UK'))",
                Sql);
        }

        [Fact]
        public override void Select_project_filter()
        {
            base.Select_project_filter();

            Assert.Equal(
                @"SELECT `c`.`CompanyName`
 FROM `Customers` `c`
 WHERE `c`.`City` = 'London'",
                Sql);
        }

        [Fact]
        public override void Select_project_filter2()
        {
            base.Select_project_filter2();

            Assert.Equal(
                @"SELECT `c`.`City`
 FROM `Customers` `c`
 WHERE `c`.`City` = 'London'",
                Sql);
        }

        [Fact]
        public override void SelectMany_mixed()
        {
            base.SelectMany_mixed();

            Assert.Equal(3763, Sql.Replace("\r", "").Replace("\n", "").Length); // new-line insensitive assertion
            Assert.StartsWith(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void SelectMany_simple_subquery()
        {
            base.SelectMany_simple_subquery();

            Assert.Equal(
                @"?: 9

SELECT `t`.`EmployeeID`, `t`.`City`, `t`.`Country`, `t`.`FirstName`, `t`.`ReportsTo`, `t`.`Title`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM (
    SELECT `e0`.`EmployeeID`, `e0`.`City`, `e0`.`Country`, `e0`.`FirstName`, `e0`.`ReportsTo`, `e0`.`Title`
     FROM `Employees` `e0` LIMIT ?
) `t`
CROSS JOIN `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void SelectMany_simple1()
        {
            base.SelectMany_simple1();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Employees` `e`
CROSS JOIN `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void SelectMany_simple2()
        {
            base.SelectMany_simple2();

            Assert.Equal(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e2`.`FirstName`
 FROM `Employees` `e1`
CROSS JOIN `Customers` `c`
CROSS JOIN `Employees` `e2`",
                Sql);
        }

        [Fact]
        public override void SelectMany_entity_deep()
        {
            base.SelectMany_entity_deep();

            Assert.Equal(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`, `e2`.`EmployeeID`, `e2`.`City`, `e2`.`Country`, `e2`.`FirstName`, `e2`.`ReportsTo`, `e2`.`Title`, `e3`.`EmployeeID`, `e3`.`City`, `e3`.`Country`, `e3`.`FirstName`, `e3`.`ReportsTo`, `e3`.`Title`, `e4`.`EmployeeID`, `e4`.`City`, `e4`.`Country`, `e4`.`FirstName`, `e4`.`ReportsTo`, `e4`.`Title`
 FROM `Employees` `e1`
CROSS JOIN `Employees` `e2`
CROSS JOIN `Employees` `e3`
CROSS JOIN `Employees` `e4`",
                Sql);
        }

        [Fact]
        public override void SelectMany_projection1()
        {
            base.SelectMany_projection1();

            Assert.Equal(
                @"SELECT `e1`.`City`, `e2`.`Country`
 FROM `Employees` `e1`
CROSS JOIN `Employees` `e2`",
                Sql);
        }

        [Fact]
        public override void SelectMany_projection2()
        {
            base.SelectMany_projection2();

            Assert.Equal(
                @"SELECT `e1`.`City`, `e2`.`Country`, `e3`.`FirstName`
 FROM `Employees` `e1`
CROSS JOIN `Employees` `e2`
CROSS JOIN `Employees` `e3`",
                Sql);
        }

        [Fact]
        public override void SelectMany_Count()
        {
            //base.SelectMany_Count();

            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 from o in os
                 select c.CustomerID).LongCount());

            Assert.Equal(
                @"SELECT COUNT(*)
 FROM `Customers` `c`
CROSS JOIN `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void SelectMany_LongCount()
        {
            base.SelectMany_LongCount();

            Assert.Equal(
                @"SELECT COUNT(*)
 FROM `Customers` `c`
CROSS JOIN `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void SelectMany_OrderBy_ThenBy_Any()
        {
            base.SelectMany_OrderBy_ThenBy_Any();

            Assert.Equal(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
         FROM `Customers` `c`
        CROSS JOIN `Orders` `o`)
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Join_customers_orders_projection()
        {
            base.Join_customers_orders_projection();

            Assert.Equal(
                @"SELECT `c`.`ContactName`, `o`.`OrderID`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Join_customers_orders_entities()
        {
            base.Join_customers_orders_entities();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Join_composite_key()
        {
            base.Join_composite_key();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON (`c`.`CustomerID` = `o`.`CustomerID`) AND (`c`.`CustomerID` = `o`.`CustomerID`)",
                Sql);
        }

        [Fact]
        public override void Join_client_new_expression()
        {
            base.Join_client_new_expression();

            // See issue#4458
            Assert.Contains(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);

            Assert.Contains(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Join_select_many()
        {
            base.Join_select_many();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
CROSS JOIN `Employees` `e`",
                Sql);
        }

        [Fact]
        public override void Client_Join_select_many()
        {
            base.Client_Join_select_many();

            // See issue#4458
            Assert.Contains(
                @"SELECT `e2`.`EmployeeID`, `e2`.`City`, `e2`.`Country`, `e2`.`FirstName`, `e2`.`ReportsTo`, `e2`.`Title`
 FROM `Employees` `e2`",
                Sql);

            Assert.Contains(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`",
                Sql);

            Assert.Contains(
                @"SELECT `e3`.`EmployeeID`, `e3`.`City`, `e3`.`Country`, `e3`.`FirstName`, `e3`.`ReportsTo`, `e3`.`Title`
 FROM `Employees` `e3`",
                Sql);
        }

        [Fact]
        public override void Join_Where_Count()
        {
            //base.Join_Where_Count();

            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 join o in os on c.CustomerID equals o.CustomerID
                 where c.CustomerID == "ALFKI"
                 select c).LongCount());

            Assert.Equal(
                @"SELECT COUNT(*)
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
 WHERE `c`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void Join_OrderBy_Count()
        {
            //base.Join_OrderBy_Count();

            AssertQuery<Customer, Order>((cs, os) =>
                (from c in cs
                 join o in os on c.CustomerID equals o.CustomerID
                 orderby c.CustomerID
                 select c).LongCount());

            Assert.Equal(
                @"SELECT COUNT(*)
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Join_customers_orders_with_subquery()
        {
            base.Join_customers_orders_with_subquery();

            // See issue#4458
            Assert.Contains(
                @"SELECT `o2`.`CustomerID`, `o2`.`OrderID`
 FROM `Orders` `o2`
ORDER BY `o2`.`OrderID`",
                Sql);

            Assert.Contains(
                @"SELECT `c`.`CustomerID`, `c`.`ContactName`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Join_customers_orders_with_subquery_with_take()
        {
            base.Join_customers_orders_with_subquery_with_take();

            Assert.Equal(
                @"?: 5

SELECT `c`.`ContactName`, `t`.`OrderID`
 FROM `Customers` `c`
INNER JOIN (
    SELECT `o20`.*
     FROM `Orders` `o20`
    ORDER BY `o20`.`OrderID` LIMIT ?
) `t` ON `c`.`CustomerID` = `t`.`CustomerID`
 WHERE `t`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void Join_customers_orders_with_subquery_anonymous_property_method()
        {
            base.Join_customers_orders_with_subquery_anonymous_property_method();

            // See issue#4458
            Assert.Contains(
                @"SELECT `o2`.`OrderID`, `o2`.`CustomerID`, `o2`.`EmployeeID`, `o2`.`OrderDate`
 FROM `Orders` `o2`
ORDER BY `o2`.`OrderID`",
                Sql);

            Assert.Contains(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Join_customers_orders_with_subquery_anonymous_property_method_with_take()
        {
            base.Join_customers_orders_with_subquery_anonymous_property_method_with_take();

            // See issue#4458
            Assert.Contains(
                @"?: 5

SELECT `o2`.`OrderID`, `o2`.`CustomerID`, `o2`.`EmployeeID`, `o2`.`OrderDate`
 FROM `Orders` `o2`
ORDER BY `o2`.`OrderID` LIMIT ?",
                Sql);

            Assert.Contains(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Join_customers_orders_with_subquery_predicate()
        {
            base.Join_customers_orders_with_subquery_predicate();

            // See issue#4458
            Assert.Contains(
                @"SELECT `o2`.`CustomerID`, `o2`.`OrderID`
 FROM `Orders` `o2`
 WHERE `o2`.`OrderID` > 0
ORDER BY `o2`.`OrderID`",
                Sql);

            Assert.Contains(
                @"SELECT `c`.`CustomerID`, `c`.`ContactName`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Join_customers_orders_with_subquery_predicate_with_take()
        {
            base.Join_customers_orders_with_subquery_predicate_with_take();

            Assert.Equal(
                @"?: 5

SELECT `c`.`ContactName`, `t`.`OrderID`
 FROM `Customers` `c`
INNER JOIN (
    SELECT `o20`.*
     FROM `Orders` `o20`
     WHERE `o20`.`OrderID` > 0
    ORDER BY `o20`.`OrderID` LIMIT ?
) `t` ON `c`.`CustomerID` = `t`.`CustomerID`
 WHERE `t`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void Join_customers_orders_select()
        {
            base.Join_customers_orders_select();

            Assert.Equal(
                @"SELECT `c`.`ContactName`, `o`.`OrderID`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Multiple_joins_Where_Order_Any()
        {
            base.Multiple_joins_Where_Order_Any();

            Assert.Equal(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
         FROM `Customers` `c`
        INNER JOIN `Orders` `or` ON `c`.`CustomerID` = `or`.`CustomerID`
        INNER JOIN `OrderDetails` `od` ON `or`.`OrderID` = `od`.`OrderID`
         WHERE `c`.`City` = 'London')
    THEN CAST(`1` AS BIT) ELSE CAST(`0` AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Where_join_select()
        {
            base.Where_join_select();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
 WHERE `c`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void Where_orderby_join_select()
        {
            base.Where_orderby_join_select();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
 WHERE `c`.`CustomerID` <> 'ALFKI'
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Where_join_orderby_join_select()
        {
            base.Where_join_orderby_join_select();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
INNER JOIN `OrderDetails` `od` ON `o`.`OrderID` = `od`.`OrderID`
 WHERE `c`.`CustomerID` <> 'ALFKI'
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Where_select_many()
        {
            base.Where_select_many();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
CROSS JOIN `Orders` `o`
 WHERE `c`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void Where_orderby_select_many()
        {
            base.Where_orderby_select_many();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
CROSS JOIN `Orders` `o`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupBy_simple()
        {
            base.GroupBy_simple();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupBy_Distinct()
        {
            base.GroupBy_Distinct();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupBy_Count()
        {
            //base.GroupBy_Count();

            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.LongCount()));

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupBy_LongCount()
        {
            base.GroupBy_LongCount();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_GroupBy()
        {
            base.Select_GroupBy();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`
 FROM `Orders` `o`
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Select_GroupBy_SelectMany()
        {
            base.Select_GroupBy_SelectMany();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`
 FROM `Orders` `o`
ORDER BY `o`.`OrderID`",
                Sql);
        }

        [Fact]
        public override void SelectMany_cartesian_product_with_ordering()
        {
            base.SelectMany_cartesian_product_with_ordering();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `e`.`City`
 FROM `Customers` `c`
CROSS JOIN `Employees` `e`
 WHERE (`c`.`City` = `e`.`City`) OR (`c`.`City` IS NULL AND `e`.`City` IS NULL)
ORDER BY `e`.`City`, `c`.`CustomerID` DESC",
                Sql);
        }

        [Fact]
        public override void GroupJoin_DefaultIfEmpty()
        {
            base.GroupJoin_DefaultIfEmpty();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Customers` `c`
LEFT JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupJoin_DefaultIfEmpty2()
        {
            base.GroupJoin_DefaultIfEmpty2();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Employees` `e`
LEFT JOIN `Orders` `o` ON `e`.`EmployeeID` = `o`.`EmployeeID`
ORDER BY `e`.`EmployeeID`",
                Sql);
        }

        [Fact]
        public override void GroupJoin_DefaultIfEmpty3()
        {
            base.GroupJoin_DefaultIfEmpty3();

            Assert.Equal(
                @"?: 1

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM (
    SELECT `c0`.*
     FROM `Customers` `c0` LIMIT ?
) `t`
LEFT JOIN `Orders` `o` ON `t`.`CustomerID` = `o`.`CustomerID`
ORDER BY `t`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupJoin_simple()
        {
            base.GroupJoin_simple();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Customers` `c`
LEFT JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupJoin_simple2()
        {
            base.GroupJoin_simple2();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Customers` `c`
LEFT JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupJoin_simple_ordering()
        {
            base.GroupJoin_simple_ordering();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Customers` `c`
LEFT JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
ORDER BY `c`.`City`, `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupJoin_simple_subquery()
        {
            base.GroupJoin_simple_subquery();

            Assert.Equal(
                @"?: 4

SELECT `t`.`OrderID`, `t`.`CustomerID`, `t`.`EmployeeID`, `t`.`OrderDate`
 FROM `Customers` `c`
LEFT JOIN (
    SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
     FROM `Orders` `o0`
    ORDER BY `o0`.`OrderID` LIMIT ?
) `t` ON `c`.`CustomerID` = `t`.`CustomerID`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupJoin_customers_orders_count()
        {
            base.GroupJoin_customers_orders_count();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Customers` `c`
LEFT JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupJoin_tracking_groups()
        {
            base.GroupJoin_tracking_groups();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Customers` `c`
LEFT JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void GroupJoin_simple3()
        {
            base.GroupJoin_simple3();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Customers` `c`
LEFT JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void SelectMany_Joined_DefaultIfEmpty()
        {
            base.SelectMany_Joined_DefaultIfEmpty();

            Assert.StartsWith(
                @"SELECT `t1`.`OrderID`, `t1`.`CustomerID`, `t1`.`EmployeeID`, `t1`.`OrderDate`, `c`.`ContactName`
 FROM `Customers` `c`
CROSS APPLY (
    SELECT `t`.`OrderID`, `t`.`CustomerID`, `t`.`EmployeeID`, `t`.`OrderDate`
     FROM (
        SELECT NULL `empty`
    ) `empty0`
    LEFT JOIN (
        SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
         FROM `Orders` `o0`
         WHERE `o0`.`CustomerID` = `c`.`CustomerID`
    ) `t` ON 1 = 1
) `t1`",
                Sql);
        }

        [Fact]
        public override void SelectMany_Joined_DefaultIfEmpty2()
        {
            base.SelectMany_Joined_DefaultIfEmpty2();

            Assert.StartsWith(
                @"SELECT `t1`.`OrderID`, `t1`.`CustomerID`, `t1`.`EmployeeID`, `t1`.`OrderDate`
 FROM `Customers` `c`
CROSS APPLY (
    SELECT `t`.`OrderID`, `t`.`CustomerID`, `t`.`EmployeeID`, `t`.`OrderDate`
     FROM (
        SELECT NULL `empty`
    ) `empty0`
    LEFT JOIN (
        SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
         FROM `Orders` `o0`
         WHERE `o0`.`CustomerID` = `c`.`CustomerID`
    ) `t` ON 1 = 1
) `t1`",
                Sql);
        }

        [Fact]
        public override void SelectMany_Joined_Take()
        {
            base.SelectMany_Joined_Take();

            Assert.Equal(
                @"SELECT `t`.`OrderID`, `t`.`CustomerID`, `t`.`EmployeeID`, `t`.`OrderDate`, `c`.`ContactName`
 FROM `Customers` `c`
CROSS APPLY (
    SELECT TOP(1000) `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
     FROM `Orders` `o0`
     WHERE `o0`.`CustomerID` = `c`.`CustomerID`
) AS t",
                Sql);
        }

        [Fact]
        public override void Take_with_single()
        {
            base.Take_with_single();

            Assert.Equal(
                @"?: 1

SELECT `t`.*
 FROM (
    SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
     FROM `Customers` `c`
    ORDER BY `c`.`CustomerID` LIMIT ?
) `t` LIMIT 2",
                Sql);
        }

        [Fact]
        public override void Take_with_single_select_many()
        {
            base.Take_with_single_select_many();

            Assert.Equal(
                @"?: 1

SELECT `t`.*
 FROM (
    SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `o`.`OrderID`, `o`.`CustomerID` `c0`, `o`.`EmployeeID`, `o`.`OrderDate`
     FROM `Customers` `c`
    CROSS JOIN `Orders` `o`
    ORDER BY `c`.`CustomerID`, `o`.`OrderID` LIMIT ?
) `t` LIMIT 2",
                Sql);
        }

        [Fact]
        public override void Distinct()
        {
            base.Distinct();

            Assert.Equal(
                @"SELECT DISTINCT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Distinct_Scalar()
        {
            base.Distinct_Scalar();

            Assert.Equal(
                @"SELECT DISTINCT `c`.`City`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Distinct_Skip() => base.Distinct_Skip();

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Distinct_Skip_Take() => base.Distinct_Skip_Take();

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Skip_Distinct() => base.Skip_Distinct();

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Skip_Take_Distinct() => base.Skip_Take_Distinct();

        [Fact]
        public override void OrderBy()
        {
            base.OrderBy();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void OrderBy_anon()
        {
            base.OrderBy_anon();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void OrderBy_anon2()
        {
            base.OrderBy_anon2();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void OrderBy_client_mixed()
        {
            base.OrderBy_client_mixed();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void OrderBy_multiple_queries()
        {
            base.OrderBy_multiple_queries();

            // See issue#4458
            Assert.Contains(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);

            Assert.Contains(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void OrderBy_Distinct()
        {
            base.OrderBy_Distinct();

            Assert.Equal(
                @"SELECT DISTINCT `c`.`City`
 FROM `Customers` `c`", // Ordering not preserved by distinct when ordering columns not projected.
                Sql);
        }

        [Fact]
        public override void Distinct_OrderBy()
        {
            base.Distinct_OrderBy();

            Assert.Equal(
                @"SELECT `t`.`Country`
 FROM (
    SELECT DISTINCT `c0`.`Country`
     FROM `Customers` `c0`
) `t`
ORDER BY `t`.`Country`",
                Sql);
        }

        [Fact]
        public override void Distinct_OrderBy2()
        {
            base.Distinct_OrderBy2();

            Assert.Equal(
                @"SELECT `t`.`CustomerID`, `t`.`Address`, `t`.`City`, `t`.`CompanyName`, `t`.`ContactName`, `t`.`ContactTitle`, `t`.`Country`, `t`.`Fax`, `t`.`Phone`, `t`.`PostalCode`, `t`.`Region`
 FROM (
    SELECT DISTINCT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
     FROM `Customers` `c0`
) `t`
ORDER BY `t`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Distinct_OrderBy3()
        {
            base.Distinct_OrderBy3();

            Assert.Equal(
                @"SELECT `t`.`CustomerID`
 FROM (
    SELECT DISTINCT `c0`.`CustomerID`
     FROM `Customers` `c0`
) `t`
ORDER BY `t`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Take_Distinct()
        {
            base.Take_Distinct();

            Assert.Equal(
                @"?: 5

SELECT DISTINCT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
ORDER BY `o`.`OrderID` LIMIT ?",
                Sql);
        }

        [Fact]
        public override void OrderBy_shadow()
        {
            base.OrderBy_shadow();

            Assert.Equal(
                @"SELECT `e`.`EmployeeID`, `e`.`City`, `e`.`Country`, `e`.`FirstName`, `e`.`ReportsTo`, `e`.`Title`
 FROM `Employees` `e`
ORDER BY `e`.`Title`, `e`.`EmployeeID`",
                Sql);
        }

        [Fact]
        public override void OrderBy_multiple()
        {
            base.OrderBy_multiple();

            Assert.Equal(
                @"SELECT `c`.`City`
 FROM `Customers` `c`
ORDER BY `c`.`Country`, `c`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void OrderBy_ThenBy_Any()
        {
            base.OrderBy_ThenBy_Any();

            Assert.Equal(
                @"SELECT CASE
    WHEN EXISTS (
        SELECT 1
         FROM `Customers` `c`)
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void OrderBy_correlated_subquery_lol()
        {
            base.OrderBy_correlated_subquery_lol();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
             FROM `Customers` `c2`
             WHERE `c2`.`CustomerID` = `c`.`CustomerID`)
        THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
    END
)",
                Sql);
        }

        [Fact]
        public override void OrderBy_correlated_subquery_lol2()
        {
            base.OrderBy_correlated_subquery_lol2();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (
    SELECT `c`.`City`
     FROM `Customers` `c`
    ORDER BY (
        SELECT CASE
            WHEN EXISTS (
                SELECT 1
                 FROM `Customers` `c2`
                 WHERE `c2`.`CustomerID` = 'ALFKI')
            THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
        END
    ) LIMIT 1
) <> 'Nowhere'",
                Sql);
        }

        [Fact]
        public override void Where_subquery_recursive_trivial()
        {
            base.Where_subquery_recursive_trivial();

            Assert.Equal(
                @"SELECT `e1`.`EmployeeID`, `e1`.`City`, `e1`.`Country`, `e1`.`FirstName`, `e1`.`ReportsTo`, `e1`.`Title`
 FROM `Employees` `e1`
 WHERE EXISTS (
    SELECT 1
     FROM `Employees` `e2`
     WHERE EXISTS (
        SELECT 1
         FROM `Employees` `e3`))
ORDER BY `e1`.`EmployeeID`",
                Sql);
        }

        [Fact]
        public override void Where_false()
        {
            base.Where_false();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE 0 = 1",
                Sql);
        }

        [Fact]
        public override void Where_default()
        {
            base.Where_default();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`Fax` IS NULL",
                Sql);
        }

        [Fact]
        public override void Where_expression_invoke()
        {
            base.Where_expression_invoke();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void Where_ternary_boolean_condition()
        {
            base.Where_ternary_boolean_condition();

            Assert.Contains(
                    @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE ((@__flag_0 = 1) AND (`p`.`UnitsInStock` >= 20)) OR ((@__flag_0 <> 1) AND (`p`.`UnitsInStock` < 20))",
                    Sql);
        }

        [Fact]
        public override void Where_ternary_boolean_condition_with_another_condition()
        {
            base.Where_ternary_boolean_condition_with_another_condition();

            Assert.Equal(
                    @"?: 15
?: True

SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE (`p`.`ProductID` < ?) AND (((? = 1) AND (`p`.`UnitsInStock` >= 20)) OR ((? <> 1) AND (`p`.`UnitsInStock` < 20)))",
                    Sql);
        }

        [Fact]
        public override void Where_ternary_boolean_condition_with_false_as_result()
        {
            base.Where_ternary_boolean_condition_with_false_as_result();

            Assert.Contains(
                    @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE (? = 1) AND (`p`.`UnitsInStock` >= 20)",
                    Sql);
        }

        [Fact]
        public override void Where_concat_string_int_comparison1()
        {
            base.Where_concat_string_int_comparison1();

            Assert.Equal(
                @"?: 10 (Size = -1) (DbType = Object)

SELECT `c`.`CustomerID`
 FROM `Customers` `c`
 WHERE (`c`.`CustomerID` || CAST(? AS varchar)) = `c`.`CompanyName`",
                Sql);
        }

        [Fact]
        public override void Where_concat_string_int_comparison2()
        {
            base.Where_concat_string_int_comparison2();

            Assert.Equal(
                @"?: 10 (Size = -1) (DbType = Object)

SELECT `c`.`CustomerID`
 FROM `Customers` `c`
 WHERE (CAST(? AS varchar) || `c`.`CustomerID`) = `c`.`CompanyName`",
                Sql);
        }

        [Fact]
        public override void Where_concat_string_int_comparison3()
        {
            base.Where_concat_string_int_comparison3();

            Assert.Equal(
                @"?: 10 (Size = -1) (DbType = Object)
?: 21 (Size = -1) (DbType = Object)

SELECT `c`.`CustomerID`
 FROM `Customers` `c`
 WHERE (((CAST(? + 20 AS varchar) || `c`.`CustomerID`) || CAST(? AS varchar)) || CAST(42 AS varchar)) = `c`.`CompanyName`",
                Sql);
        }

        [Fact]
        public override void Where_primitive()
        {
            base.Where_primitive();

            Assert.Equal(
                @"?: 9

SELECT `t`.`EmployeeID`
 FROM (
    SELECT `e0`.`EmployeeID`
     FROM `Employees` `e0` LIMIT ?
) `t`
 WHERE `t`.`EmployeeID` = 5",
                Sql);
        }

        [Fact]
        public override void Where_bool_member()
        {
            base.Where_bool_member();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` = 1",
                Sql);
        }

        [Fact]
        public override void Where_bool_member_false()
        {
            base.Where_bool_member_false();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` = 0",
                Sql);
        }

        [Fact]
        public override void Where_bool_client_side_negated()
        {
            base.Where_bool_client_side_negated();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` = 1",
                Sql);
        }

        [Fact]
        public override void Where_bool_member_negated_twice()
        {
            base.Where_bool_member_negated_twice();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` = 1",
                Sql);
        }

        [Fact]
        public override void Where_bool_member_shadow()
        {
            base.Where_bool_member_shadow();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` = 1",
                Sql);
        }

        [Fact]
        public override void Where_bool_member_false_shadow()
        {
            base.Where_bool_member_false_shadow();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` = 0",
                Sql);
        }

        [Fact]
        public override void Where_bool_member_equals_constant()
        {
            base.Where_bool_member_equals_constant();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` = 1",
                Sql);
        }

        [Fact]
        public override void Where_bool_member_in_complex_predicate()
        {
            base.Where_bool_member_in_complex_predicate();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE ((`p`.`ProductID` > 100) AND (`p`.`Discontinued` = 1)) OR (`p`.`Discontinued` = 1)",
                Sql);
        }

        [Fact]
        public override void Where_bool_member_compared_to_binary_expression()
        {
            base.Where_bool_member_compared_to_binary_expression();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` = CASE
    WHEN `p`.`ProductID` > 50
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Where_not_bool_member_compared_to_binary_expression()
        {
            base.Where_not_bool_member_compared_to_binary_expression();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` <> CASE
    WHEN `p`.`ProductID` > 50
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Where_not_bool_member_compared_to_not_bool_member()
        {
            base.Where_not_bool_member_compared_to_not_bool_member();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` = `p`.`Discontinued`",
                Sql);
        }

        [Fact]
        public override void Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE CASE
    WHEN `p`.`ProductID` > 50
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END = CASE
    WHEN `p`.`ProductID` > 20
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Where_bool_parameter()
        {
            base.Where_bool_parameter();

            Assert.Equal(
                @"?: True

SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE ? = 1",
                Sql);
        }

        [Fact]
        public override void Where_bool_parameter_compared_to_binary_expression()
        {
            base.Where_bool_parameter_compared_to_binary_expression();

            Assert.Equal(
                @"?: True

SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE CASE
    WHEN `p`.`ProductID` > 50
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END <> ?",
                Sql);
        }

        [Fact]
        public override void Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            base.Where_bool_member_and_parameter_compared_to_binary_expression_nested();

            Assert.Equal(
                @"?: True

SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`Discontinued` = CASE
    WHEN CASE
        WHEN `p`.`ProductID` > 50
        THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
    END <> ?
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Where_de_morgan_or_optimizated()
        {
            base.Where_de_morgan_or_optimizated();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE (`p`.`Discontinued` = 0) AND (`p`.`ProductID` >= 20)",
                Sql);
        }

        [Fact]
        public override void Where_de_morgan_and_optimizated()
        {
            base.Where_de_morgan_and_optimizated();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE (`p`.`Discontinued` = 0) OR (`p`.`ProductID` >= 20)",
                Sql);
        }

        [Fact]
        public override void Where_complex_negated_expression_optimized()
        {
            base.Where_complex_negated_expression_optimized();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE ((`p`.`Discontinued` = 0) AND (`p`.`ProductID` < 60)) AND (`p`.`ProductID` > 30)",
                Sql);
        }

        [Fact]
        public override void Where_short_member_comparison()
        {
            base.Where_short_member_comparison();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE `p`.`UnitsInStock` > 10",
                Sql);
        }

        [Fact]
        public override void Where_comparison_to_nullable_bool()
        {
            base.Where_comparison_to_nullable_bool();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` LIKE '%' + 'KI'",
                Sql);
        }

        [Fact]
        public override void Where_true()
        {
            base.Where_true();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_compare_constructed_equal()
        {
            base.Where_compare_constructed_equal();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_compare_constructed_multi_value_equal()
        {
            base.Where_compare_constructed_multi_value_equal();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_compare_constructed_multi_value_not_equal()
        {
            base.Where_compare_constructed_multi_value_not_equal();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_compare_constructed()
        {
            base.Where_compare_constructed();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Where_compare_null()
        {
            base.Where_compare_null();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`City` IS NULL AND (`c`.`Country` = 'UK')",
                Sql);
        }

        [Fact]
        public override void Where_Is_on_same_type()
        {
            base.Where_Is_on_same_type();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Single_Predicate()
        {
            base.Single_Predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI' LIMIT 2",
                Sql);
        }

        [Fact]
        public override void Projection_when_arithmetic_expression_precendence()
        {
            base.Projection_when_arithmetic_expression_precendence();

            Assert.Equal(
                @"SELECT `o`.`OrderID` / (`o`.`OrderID` / 2), (`o`.`OrderID` / `o`.`OrderID`) / 2
 FROM `Orders` `o`",
                Sql);
        }

        // TODO: Complex projection translation.

        //        public override void Projection_when_arithmetic_expressions()
        //        {
        //            base.Projection_when_arithmetic_expressions();
        //
        //            Assert.Equal(
        //                @"SELECT `o`.`OrderID`, `o`.`OrderID` * 2, `o`.`OrderID` + 23, 100000 - `o`.`OrderID`, `o`.`OrderID` / (`o`.`OrderID` / 2)
        // FROM `Orders` `o`",
        //                Sql);
        //        }
        //
        //        public override void Projection_when_arithmetic_mixed()
        //        {
        //            //base.Projection_when_arithmetic_mixed();
        //        }
        //
        //        public override void Projection_when_arithmetic_mixed_subqueries()
        //        {
        //            //base.Projection_when_arithmetic_mixed_subqueries();
        //        }

        [Fact]
        public override void Projection_when_null_value()
        {
            base.Projection_when_null_value();

            Assert.Equal(
                @"SELECT `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE 'M' || '%'",
                Sql);
        }

        [Fact]
        public override void String_StartsWith_Identity()
        {
            base.String_StartsWith_Identity();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE `c`.`ContactName` || '%'",
                Sql);
        }

        [Fact]
        public override void String_StartsWith_Column()
        {
            base.String_StartsWith_Column();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE `c`.`ContactName` || '%'",
                Sql);
        }

        [Fact]
        public override void String_StartsWith_MethodCall()
        {
            base.String_StartsWith_MethodCall();

            Assert.Equal(
                @"?: M (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE ? || '%'",
                Sql);
        }

        [Fact]
        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE '%' || 'b'",
                Sql);
        }

        [Fact]
        public override void String_EndsWith_Identity()
        {
            base.String_EndsWith_Identity();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE '%' || `c`.`ContactName`",
                Sql);
        }

        [Fact]
        public override void String_EndsWith_Column()
        {
            base.String_EndsWith_Column();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE '%' || `c`.`ContactName`",
                Sql);
        }

        [Fact]
        public override void String_EndsWith_MethodCall()
        {
            base.String_EndsWith_MethodCall();

            Assert.Equal(
                @"?: m (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE '%' || ?",
                Sql);
        }

        [Fact]
        public override void String_Contains_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE (N'%' || 'M') || '%'",
                Sql);
        }

        [Fact]
        public override void String_Contains_Identity()
        {
            base.String_Contains_Identity();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE ('%' || `c`.`ContactName`) || '%'",
                Sql);
        }

        [Fact]
        public override void String_Contains_Column()
        {
            base.String_Contains_Column();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE ('%' || `c`.`ContactName`) || '%'",
                Sql);
        }

        [Fact]
        public override void String_Contains_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1().ToLower()) || c.ContactName.Contains(LocalMethod1().ToUpper())), // case-sensitive
                entryCount: 34);

            Assert.Equal(
                @"?: M (Size = 4000) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE (N'%' || ?) || '%'",
                Sql);
        }

        [Fact]
        public override void String_Compare_simple_zero()
        {
            base.String_Compare_simple_zero();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` <> 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` > 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` <= 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` > 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` <= 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void String_Compare_simple_one()
        {
            base.String_Compare_simple_one();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` > 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` < 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` <= 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` <= 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` >= 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` >= 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void String_Compare_simple_client()
        {
            base.String_Compare_simple_client();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void String_Compare_nested()
        {
            base.String_Compare_nested();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'M' || `c`.`CustomerID`

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` <> UPPER(`c`.`CustomerID`)

?: ALF (Size = 8000) (DbType = AnsiString)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` > REPLACE('ALFKI', ?, `c`.`CustomerID`)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` <= 'M' || `c`.`CustomerID`

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` > UPPER(`c`.`CustomerID`)

?: ALF (Size = 8000) (DbType = AnsiString)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` < REPLACE('ALFKI', ?, `c`.`CustomerID`)",
                Sql);
        }

        [Fact]
        public override void String_Compare_multi_predicate()
        {
            base.String_Compare_multi_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` >= 'ALFKI' AND `c`.`CustomerID` < 'CACTU'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactTitle` = 'Owner' AND `c`.`Country` <> 'USA'",
                Sql);
        }

        [Fact]
        public override void Where_math_abs1()
        {
            base.Where_math_abs1();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE ABS(`od`.`ProductID`) > 10",
                Sql);
        }

        [Fact]
        public override void Where_math_abs2()
        {
            base.Where_math_abs2();

            //AssertQuery<OrderDetail>(
            //    ods => ods.Select(i=>new OrderDetail {OrderID=i.OrderID,ProductID=i.ProductID,Discount=i.Discount,Quantity=i.Quantity,UnitPrice=i.UnitPrice }).Where(od => Math.Abs(od.Quantity) > 10).Select(i => i.ProductID),
            //    entryCount: 1547);

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE ABS(`od`.`Quantity`) > 10",
                Sql);
        }

        [Fact]
        public override void Where_math_abs3()
        {
            base.Where_math_abs3();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE ABS(`od`.`UnitPrice`) > `10`.`0`",
                Sql);
        }

        [Fact]
        public override void Where_math_abs_uncorrelated()
        {
            base.Where_math_abs_uncorrelated();

            Assert.Equal(
                @"?: 10

SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE ? < `od`.`ProductID`",
                Sql);
        }

        [Fact]
        public override void Where_math_ceiling1()
        {
            base.Where_math_ceiling1();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE CEILING(`od`.`Discount`) > 0E0",
                Sql);
        }

        [Fact]
        public override void Where_math_ceiling2()
        {
            base.Where_math_ceiling2();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE CEILING(`od`.`UnitPrice`) > 10.0",
                Sql);
        }

        [Fact]
        public override void Where_math_floor()
        {
            base.Where_math_floor();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE FLOOR(`od`.`UnitPrice`) > 10.0",
                Sql);
        }

        [Fact]
        public override void Where_query_composition4()
        {
            base.Where_query_composition4();

            Assert.StartsWith(
                @"SELECT `c1`.`CustomerID`, `c1`.`Address`, `c1`.`City`, `c1`.`CompanyName`, `c1`.`ContactName`, `c1`.`ContactTitle`, `c1`.`Country`, `c1`.`Fax`, `c1`.`Phone`, `c1`.`PostalCode`, `c1`.`Region`
 FROM `Customers` `c1`

SELECT 1
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`

SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
 FROM `Customers` `c0`",
                Sql);
        }

        [Fact]
        public override void Where_math_power()
        {
            base.Where_math_power();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE POWER(`od`.`Discount`, 2E0) > 0.0500000007450581E0",
                Sql);
        }

        [Fact]
        public override void Where_math_round()
        {
            base.Where_math_round();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE ROUND(`od`.`UnitPrice`, 0) > 10.0",
                Sql);
        }

        [Fact]
        public override void Where_math_truncate()
        {
            base.Where_math_truncate();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE ROUND(`od`.`UnitPrice`, 0, 1) > 10.0",
                Sql);
        }

        [Fact]
        public override void Where_guid_newguid()
        {
            base.Where_guid_newguid();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`
 FROM `OrderDetails` od
 WHERE NEWID() <> '00000000-0000-0000-0000-000000000000'",
                Sql);
        }

        [Fact]
        public override void Where_functions_nested()
        {
            base.Where_functions_nested();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE POWER(LEN(`c`.`CustomerID`), 2E0) = 25E0",
                Sql);
        }

        [Fact]
        public override void Where_string_to_lower()
        {
            base.Where_string_to_lower();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE LOWER(`c`.`CustomerID`) = 'alfki'",
                Sql);
        }

        [Fact]
        public override void Where_string_to_upper()
        {
            base.Where_string_to_upper();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE UPPER(`c`.`CustomerID`) = 'ALFKI'",
                Sql);
        }

        [Fact]
        public override void Convert_ToByte()
        {
            base.Convert_ToByte();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS tinyint) AS tinyint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS numeric) AS tinyint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS tinyint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS tinyint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS smallint) AS tinyint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS int) AS tinyint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS bigint) AS tinyint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS varchar) AS tinyint) >= 0)",
                Sql);
        }

        [Fact]
        public override void Convert_ToDecimal()
        {
            base.Convert_ToDecimal();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS tinyint) AS numeric) >= 0.0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS numeric) AS numeric) >= 0.0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS numeric) >= 0.0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS numeric) >= 0.0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS smallint) AS numeric) >= 0.0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS int) AS numeric) >= 0.0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS bigint) AS numeric) >= 0.0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS varchar) AS numeric) >= 0.0)",
                Sql);
        }

        [Fact]
        public override void Convert_ToDouble()
        {
            base.Convert_ToDouble();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS tinyint) AS float) >= 0E0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS numeric) AS float) >= 0E0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS float) >= 0E0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS float) >= 0E0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS smallint) AS float) >= 0E0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS int) AS float) >= 0E0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS bigint) AS float) >= 0E0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS varchar) AS float) >= 0E0)",
                Sql);
        }

        [Fact]
        public override void Convert_ToInt16()
        {
            base.Convert_ToInt16();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS tinyint) AS smallint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS numeric) AS smallint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS smallint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS smallint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS smallint) AS smallint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS int) AS smallint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS bigint) AS smallint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS varchar) AS smallint) >= 0)",
                Sql);
        }

        [Fact]
        public override void Convert_ToInt32()
        {
            base.Convert_ToInt32();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS tinyint) AS int) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS numeric) AS int) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS int) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS int) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS smallint) AS int) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS int) AS int) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS bigint) AS int) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS varchar) AS int) >= 0)",
                Sql);
        }

        [Fact]
        public override void Convert_ToInt64()
        {
            base.Convert_ToInt64();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS tinyint) AS bigint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS numeric) AS bigint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS bigint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS bigint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS smallint) AS bigint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS int) AS bigint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS bigint) AS bigint) >= 0)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS varchar) AS bigint) >= 0)",
                Sql);
        }

        [Fact]
        public override void Convert_ToString()
        {
            base.Convert_ToString();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS tinyint) AS varchar) <> '10')

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS numeric) AS varchar) <> '10')

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS varchar) <> '10')

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS float) AS varchar) <> '10')

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS smallint) AS varchar) <> '10')

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS int) AS varchar) <> '10')

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS bigint) AS varchar) <> '10')

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'ALFKI') AND (CAST(CAST(`o`.`OrderID` % 1 AS varchar) AS varchar) <> '10')",
                Sql);
        }

        [Fact]
        public override void Select_nested_collection()
        {
            base.Select_nested_collection();

            Assert.StartsWith(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`
 WHERE `c`.`City` = 'London'
ORDER BY `c`.`CustomerID`

SELECT `o`.`CustomerID`, `o`.`OrderID`
 FROM `Orders` `o`
 WHERE YEAR(`o`.`OrderDate`) = 1997
ORDER BY `o`.`OrderID`

",
                Sql);
        }

        [Fact]
        public override void Select_correlated_subquery_projection()
        {
            base.Select_correlated_subquery_projection();

            Assert.StartsWith(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`

",
                Sql);
        }

        [Fact]
        public override void Select_correlated_subquery_ordered()
        {
            base.Select_correlated_subquery_ordered();

            Assert.StartsWith(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`

",
                Sql);
        }

        [Fact]
        public override void Where_subquery_on_bool()
        {
            base.Where_subquery_on_bool();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE 'Chai' IN (
    SELECT `p2`.`ProductName`
     FROM `Products` `p2`
)",
                Sql);
        }

        [Fact]
        public override void Where_subquery_on_collection()
        {
            base.Where_subquery_on_collection();

            Assert.Equal(
                @"SELECT `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `Products` `p`
 WHERE 5 IN (
    SELECT `o`.`Quantity`
     FROM `OrderDetails` o
     WHERE `o`.`ProductID` = `p`.`ProductID`
)",
                Sql);
        }

        [Fact]
        public override void Select_many_cross_join_same_collection()
        {
            base.Select_many_cross_join_same_collection();

            Assert.Equal(
                @"SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
 FROM `Customers` `c`
CROSS JOIN `Customers` `c0`",
                Sql);
        }

        [Fact]
        public override void Join_same_collection_multiple()
        {
            base.Join_same_collection_multiple();

            Assert.Equal(
                @"SELECT `c3`.`CustomerID`, `c3`.`Address`, `c3`.`City`, `c3`.`CompanyName`, `c3`.`ContactName`, `c3`.`ContactTitle`, `c3`.`Country`, `c3`.`Fax`, `c3`.`Phone`, `c3`.`PostalCode`, `c3`.`Region`
 FROM `Customers` `o`
INNER JOIN `Customers` `c2` ON `o`.`CustomerID` = `c2`.`CustomerID`
INNER JOIN `Customers` `c3` ON `o`.`CustomerID` = `c3`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Join_same_collection_force_alias_uniquefication()
        {
            base.Join_same_collection_force_alias_uniquefication();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o`
INNER JOIN `Orders` `o0` ON `o`.`CustomerID` = `o0`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Where_chain()
        {
            string test = new DateTime(1998, 1, 1).ToString("yyyy-MM-ddTHH:mm:ss.fff");
            base.Where_chain();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE (`o`.`CustomerID` = 'QUICK') AND (`o`.`OrderDate` > '1998-01-01 00:00:00.000')",
                Sql);
        }

        [Fact]
        public override void OfType_Select()
        {
            base.OfType_Select();

            Assert.Equal(
                @"SELECT `t`.`OrderID`, `t`.`CustomerID`, `t`.`EmployeeID`, `t`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM (
    SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
     FROM `Orders` `o0`
) `t`
LEFT JOIN `Customers` `o.Customer` ON `t`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `t`.`OrderID`, `t`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void OfType_Select_OfType_Select()
        {
            base.OfType_Select_OfType_Select();

            Assert.Equal(
                @"SELECT `t1`.`OrderID`, `t1`.`CustomerID`, `t1`.`EmployeeID`, `t1`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM (
    SELECT `t0`.`OrderID`, `t0`.`CustomerID`, `t0`.`EmployeeID`, `t0`.`OrderDate`
     FROM (
        SELECT `o2`.`OrderID`, `o2`.`CustomerID`, `o2`.`EmployeeID`, `o2`.`OrderDate`
         FROM `Orders` `o2`
    ) `t0`
) `t1`
LEFT JOIN `Customers` `o.Customer` ON `t1`.`CustomerID` = `o.Customer`.`CustomerID`
ORDER BY `t1`.`OrderID`, `t1`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void OrderBy_null_coalesce_operator()
        {
            base.OrderBy_null_coalesce_operator();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY COALESCE(`c`.`Region`, 'ZZ')",
                Sql);
        }

        [Fact]
        public override void Select_null_coalesce_operator()
        {
            base.Select_null_coalesce_operator();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`CompanyName`, COALESCE(`c`.`Region`, 'ZZ') `Coalesce`
 FROM `Customers` `c`
ORDER BY `Coalesce`",
                Sql);
        }

        [Fact]
        public override void OrderBy_conditional_operator()
        {
            base.OrderBy_conditional_operator();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY CASE
    WHEN `c`.`Region` IS NULL
    THEN 'ZZ' ELSE `c`.`Region`
END",
                Sql);
        }

        [Fact]
        public override void OrderBy_comparison_operator()
        {
            base.OrderBy_comparison_operator();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY CASE
    WHEN `c`.`Region` = 'ASK'
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Contains_with_subquery()
        {
            base.Contains_with_subquery();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN (
    SELECT `o`.`CustomerID`
     FROM `Orders` `o`
)",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_array_closure()
        {
            base.Contains_with_local_array_closure();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ABCDE', 'ALFKI')

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ABCDE')",
                Sql);
        }

        [Fact]
        public override void Contains_with_subquery_and_local_array_closure()
        {
            base.Contains_with_subquery_and_local_array_closure();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c1`
     WHERE `c1`.`City` IN ('London', 'Buenos Aires') AND (`c1`.`CustomerID` = `c`.`CustomerID`))

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c1`
     WHERE `c1`.`City` IN ('London') AND (`c1`.`CustomerID` = `c`.`CustomerID`))",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_array_inline()
        {
            base.Contains_with_local_array_inline();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_list_closure()
        {
            base.Contains_with_local_list_closure();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_list_inline()
        {
            base.Contains_with_local_list_inline();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_list_inline_closure_mix()
        {
            base.Contains_with_local_list_inline_closure_mix();

            Assert.Equal(
                @":__id_0: ALFKI (Size = 450) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ABCDE', :__id_0)

:__id_0: ANATR (Size = 450) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ABCDE', :__id_0)",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_collection_false()
        {
            base.Contains_with_local_collection_false();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` NOT IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_collection_complex_predicate_and()
        {
            base.Contains_with_local_collection_complex_predicate_and();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ALFKI', 'ABCDE') AND `c`.`CustomerID` IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_collection_complex_predicate_or()
        {
            base.Contains_with_local_collection_complex_predicate_or();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ABCDE', 'ALFKI', 'ALFKI', 'ABCDE')",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins1();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ALFKI', 'ABCDE') OR `c`.`CustomerID` NOT IN ('ABCDE', 'ALFKI')",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins2();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ABCDE', 'ALFKI') AND `c`.`CustomerID` NOT IN ('ALFKI', 'ABCDE')",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_collection_sql_injection()
        {
            base.Contains_with_local_collection_sql_injection();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` IN ('ALFKI', 'ABC'')); GO; DROP TABLE Orders; GO; --', 'ALFKI', 'ABCDE')",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_collection_empty_closure()
        {
            base.Contains_with_local_collection_empty_closure();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE 0 = 1",
                Sql);
        }

        [Fact]
        public override void Contains_with_local_collection_empty_inline()
        {
            base.Contains_with_local_collection_empty_inline();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE 1 = 1",
                Sql);
        }

        [Fact]
        public override void Contains_top_level()
        {
            base.Contains_top_level();

            Assert.Equal(
                @"?: ALFKI (Size = 4000) (DbType = Object)

SELECT CASE
    WHEN ? IN (
        SELECT `c`.`CustomerID`
         FROM `Customers` `c`
    )
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END",
                Sql);
        }

        [Fact]
        public override void Substring_with_constant()
        {
            base.Substring_with_constant();

            Assert.Equal(
                @"SELECT SUBSTRING(`c`.`ContactName`, 2, 3)
 FROM `Customers` `c` LIMIT 1",
                Sql);
        }

        [Fact]
        public override void Substring_with_closure()
        {
            base.Substring_with_closure();

            Assert.Equal(
                @"?: 2

SELECT SUBSTRING(`c`.`ContactName`, ? + 1, 3)
 FROM `Customers` `c` LIMIT 1",
                Sql);
        }

        [Fact]
        public override void Substring_with_client_eval()
        {
            base.Substring_with_client_eval();

            Assert.Equal(
                @"SELECT `c`.`ContactName`
 FROM `Customers` `c` LIMIT 1",
                Sql);
        }

        [Fact]
        public override void IsNullOrEmpty_in_predicate()
        {
            base.IsNullOrEmpty_in_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`Region` IS NULL OR (`c`.`Region` = '')",
                Sql);
        }

        [Fact]
        public override void IsNullOrEmpty_in_projection()
        {
            base.IsNullOrEmpty_in_projection();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, CASE
    WHEN `c`.`Region` IS NULL OR (`c`.`Region` = '')
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void IsNullOrWhiteSpace_in_predicate()
        {
            base.IsNullOrWhiteSpace_in_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`Region` IS NULL OR (LTRIM(RTRIM(`c`.`Region`)) = '')",
                Sql);
        }

        [Fact]
        public override void TrimStart_in_predicate()
        {
            base.TrimStart_in_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE LTRIM(`c`.`ContactTitle`) = 'Owner'",
                Sql);
        }

        [Fact]
        public override void TrimStart_with_arguments_in_predicate()
        {
            base.TrimStart_with_arguments_in_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void TrimEnd_in_predicate()
        {
            base.TrimEnd_in_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE RTRIM(`c`.`ContactTitle`) = 'Owner'",
                Sql);
        }

        [Fact]
        public override void TrimEnd_with_arguments_in_predicate()
        {
            base.TrimEnd_with_arguments_in_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Trim_in_predicate()
        {
            base.Trim_in_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE LTRIM(RTRIM(`c`.`ContactTitle`)) = 'Owner'",
                Sql);
        }

        [Fact]
        public override void Trim_with_arguments_in_predicate()
        {
            base.Trim_with_arguments_in_predicate();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Projection_null_coalesce_operator()
        {
            base.Projection_null_coalesce_operator();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`CompanyName`, COALESCE(`c`.`Region`, 'ZZ') `Coalesce`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Filter_coalesce_operator()
        {
            base.Filter_coalesce_operator();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE COALESCE(`c`.`CompanyName`, `c`.`ContactName`) = 'The Big Cheese'",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Take_skip_null_coalesce_operator()
        {
            base.Take_skip_null_coalesce_operator();

            Assert.Equal(@"?: 10
?: 5

SELECT DISTINCT `t0`.*
 FROM (
    SELECT `t`.*
     FROM (
        SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
         FROM `Customers` `c`
        ORDER BY COALESCE(`c`.`Region`, 'ZZ') LIMIT ?
    ) `t`
    ORDER BY COALESCE(`t`.`Region`, 'ZZ')
    LIMIT 999999999
    OFFSET ?
) `t0`",
                Sql);
        }

        [Fact]
        public override void Select_take_null_coalesce_operator()
        {
            base.Select_take_null_coalesce_operator();

            Assert.Equal(@"?: 5

SELECT `c`.`CustomerID`, `c`.`CompanyName`, COALESCE(`c`.`Region`, 'ZZ') `Coalesce`
 FROM `Customers` `c`
ORDER BY Coalesce LIMIT ?",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Select_take_skip_null_coalesce_operator()
        {
            base.Select_take_skip_null_coalesce_operator();

            Assert.Equal(
                @"?: 10
?: 5

SELECT `t`.*
 FROM (
    SELECT `c`.`CustomerID`, `c`.`CompanyName`, COALESCE(`c`.`Region`, 'ZZ') `Coalesce`
     FROM `Customers` `c`
    ORDER BY `Coalesce` LIMIT ?
) `t`
ORDER BY `Coalesce`
LIMIT 999999999
OFFSET ?",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Select_take_skip_null_coalesce_operator2()
        {
            base.Select_take_skip_null_coalesce_operator2();

            Assert.Equal(
                @"?: 10
?: 5

SELECT `t`.*
 FROM (
    SELECT `c`.`CustomerID`, `c`.`CompanyName`, `c`.`Region`
     FROM `Customers` `c`
    ORDER BY COALESCE(`c`.`Region`, 'ZZ') LIMIT ?
) `t`
ORDER BY COALESCE(`t`.`Region`, 'ZZ')
LIMIT 999999999
OFFSET ?",
                Sql);
        }

        [Fact]
        [XuGuDbCondition(XuGuDbCondition.SupportsOffset)]
        public override void Select_take_skip_null_coalesce_operator3()
        {
            base.Select_take_skip_null_coalesce_operator3();

            Assert.Equal(
                @"?: 10
?: 5

SELECT `t`.*
 FROM (
    SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
     FROM `Customers` `c`
    ORDER BY COALESCE(`c`.`Region`, 'ZZ') LIMIT ?
) `t`
ORDER BY COALESCE(`t`.`Region`, 'ZZ')
LIMIT 999999999
OFFSET ?",
                Sql);
        }

        [Fact]
        public override void Selected_column_can_coalesce()
        {
            base.Selected_column_can_coalesce();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY COALESCE(`c`.`Region`, 'ZZ')",
                Sql);
        }

        [Fact]
        public override void Does_not_change_ordering_of_projection_with_complex_projections()
        {
            //base.Does_not_change_ordering_of_projection_with_complex_projections();
            using (var context = CreateContext())
            {
                var q = from c in context.Customers.Include(e => e.Orders).Where(c => c.ContactTitle == "Owner")
                        select new
                        {
                            Id = c.CustomerID,
                            TotalOrders = c.Orders.LongCount()
                        };

                var result = q.Where(e => e.TotalOrders > 2).ToList();

                Assert.Equal(15, result.Count);

            }

            Assert.StartsWith(
                @"SELECT `e`.`CustomerID`, (
    SELECT COUNT(*)
     FROM `Orders` `o1`
     WHERE `e`.`CustomerID` = `o1`.`CustomerID`
)
 FROM `Customers` `e`
 WHERE `e`.`ContactTitle` = 'Owner'

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void DateTime_parse_is_parameterized()
        {
            base.DateTime_parse_is_parameterized();

            Assert.Equal(
                @"?: 01/01/1998 12:00:00 (DbType = DateTime)

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE `o`.`OrderDate` > ?",
                Sql);
        }

        [Fact]
        public override void Random_next_is_not_funcletized_1()
        {
            base.Random_next_is_not_funcletized_1();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Random_next_is_not_funcletized_2()
        {
            base.Random_next_is_not_funcletized_2();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Random_next_is_not_funcletized_3()
        {
            base.Random_next_is_not_funcletized_3();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Random_next_is_not_funcletized_4()
        {
            base.Random_next_is_not_funcletized_4();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Random_next_is_not_funcletized_5()
        {
            base.Random_next_is_not_funcletized_5();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Random_next_is_not_funcletized_6()
        {
            base.Random_next_is_not_funcletized_6();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`",
                Sql);
        }

        [Fact]
        public override void Environment_newline_is_funcletized()
        {
            base.Environment_newline_is_funcletized();

            Assert.Equal(
                @"?: 
 (Size = 450) (DbType = Object)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` LIKE ('%' || ?) || '%'",
                Sql);
        }

        [Fact]
        public override void String_concat_with_navigation1()
        {
            base.String_concat_with_navigation1();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`, `o`.`CustomerID` + ' '
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.CustomerID
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void String_concat_with_navigation2()
        {
            base.String_concat_with_navigation2();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `o.Customer`.`CustomerID`, `o.Customer`.`Address`, `o.Customer`.`City`, `o.Customer`.`CompanyName`, `o.Customer`.`ContactName`, `o.Customer`.`ContactTitle`, `o.Customer`.`Country`, `o.Customer`.`Fax`, `o.Customer`.`Phone`, `o.Customer`.`PostalCode`, `o.Customer`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `o.Customer` ON `o`.`CustomerID` = `o.Customer`.CustomerID
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
        private static int ClientEvalSelectorStateless() => 42;
        protected static new bool ClientEvalPredicate(Order order) => order.OrderID > 10000;

        private void AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, object> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    new[] { query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()) },
                    new[] { query(context.Set<TItem1>(), context.Set<TItem2>()) },
                    assertOrder);
            }
        }
    }
}
