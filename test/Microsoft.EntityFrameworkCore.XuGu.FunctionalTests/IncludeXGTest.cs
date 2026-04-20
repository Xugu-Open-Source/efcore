// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class IncludeXGTest : IncludeTestBase<NorthwindQueryXGFixture>
    {
        private bool SupportsOffset => TestEnvironment.GetFlag(nameof(XGCondition.SupportsOffset)) ?? true;

        public IncludeXGTest(NorthwindQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override void Include_list()
        {
            base.Include_list();

            Assert.Equal(
                @"SELECT `c`.`ProductID`, `c`.`Discontinued`, `c`.`ProductName`, `c`.`UnitsInStock`
 FROM `Products` `c`
ORDER BY `c`.`ProductID`

SELECT `o`.`OrderID`, `o`.`ProductID`, `o`.`Discount`, `o`.`Quantity`, `o`.`UnitPrice`, `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `OrderDetails` `o`
INNER JOIN `Orders` `o0` ON `o`.`OrderID` = `o0`.`OrderID`
 WHERE EXISTS (
    SELECT 1
     FROM `Products` `c`
     WHERE `o`.`ProductID` = `c`.`ProductID`)
ORDER BY `o`.`ProductID`",
                Sql);
        }

        public override void Include_collection()
        {
            base.Include_collection();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE `o`.`CustomerID` = `c`.`CustomerID`)
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_reference_and_collection()
        {
            base.Include_reference_and_collection();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`
ORDER BY `o`.`OrderID`

SELECT `o0`.`OrderID`, `o0`.`ProductID`, `o0`.`Discount`, `o0`.`Quantity`, `o0`.`UnitPrice`
 FROM `OrderDetails` `o0`
 WHERE EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE `o0`.`OrderID` = `o`.`OrderID`)
ORDER BY `o0`.`OrderID`",
                Sql);
        }

        public override void Include_references_multi_level()
        {
            base.Include_references_multi_level();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `OrderDetails` `od`
INNER JOIN `Orders` `o` ON `od`.`OrderID` = `o`.`OrderID`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`",
                Sql);
        }

        public override void Include_multiple_references_multi_level()
        {
            base.Include_multiple_references_multi_level();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `OrderDetails` `od`
INNER JOIN `Orders` `o` ON `od`.`OrderID` = `o`.`OrderID`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`
INNER JOIN `Products` `p` ON `od`.`ProductID` = `p`.`ProductID`",
                Sql);
        }

        public override void Include_multiple_references_multi_level_reverse()
        {
            base.Include_multiple_references_multi_level_reverse();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `OrderDetails` `od`
INNER JOIN `Orders` `o` ON `od`.`OrderID` = `o`.`OrderID`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`
INNER JOIN `Products` `p` ON `od`.`ProductID` = `p`.`ProductID`",
                Sql);
        }

        public override void Include_references_and_collection_multi_level()
        {
            base.Include_references_and_collection_multi_level();

            Assert.Equal(
                @"SELECT `od`.`OrderID`, `od`.`ProductID`, `od`.`Discount`, `od`.`Quantity`, `od`.`UnitPrice`, `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `OrderDetails` `od`
INNER JOIN `Orders` `o` ON `od`.`OrderID` = `o`.`OrderID`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`
ORDER BY `c`.`CustomerID`

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`
 WHERE EXISTS (
    SELECT 1
     FROM `OrderDetails` `od`
    INNER JOIN `Orders` `o` ON `od`.`OrderID` = `o`.`OrderID`
    LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`
     WHERE `o0`.`CustomerID` = `c`.`CustomerID`)
ORDER BY `o0`.`CustomerID`",
                Sql);
        }

        public override void Include_multi_level_reference_and_collection_predicate()
        {
            base.Include_multi_level_reference_and_collection_predicate();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`
 WHERE `o`.`OrderID` = 10248
ORDER BY `c`.`CustomerID` LIMIT 2

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`
 WHERE EXISTS (
    SELECT 1
     FROM `Orders` `o`
    LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`
     WHERE (`o`.`OrderID` = 10248) AND (`o0`.`CustomerID` = `c`.`CustomerID`) LIMIT 2)
ORDER BY `o0`.`CustomerID`",
                Sql);
        }

        public override void Include_multi_level_collection_and_then_include_reference_predicate()
        {
            base.Include_multi_level_collection_and_then_include_reference_predicate();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE `o`.`OrderID` = 10248
ORDER BY `o`.`OrderID` LIMIT 2

SELECT `o0`.`OrderID`, `o0`.`ProductID`, `o0`.`Discount`, `o0`.`Quantity`, `o0`.`UnitPrice`, `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `OrderDetails` `o0`
INNER JOIN `Products` `p` ON `o0`.`ProductID` = `p`.`ProductID`
 WHERE EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE (`o`.`OrderID` = 10248) AND (`o0`.`OrderID` = `o`.`OrderID`) LIMIT 2)
ORDER BY `o0`.`OrderID`",
                Sql);
        }

        public override void Include_collection_alias_generation()
        {
            base.Include_collection_alias_generation();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
ORDER BY `o`.`OrderID`

SELECT `o0`.`OrderID`, `o0`.`ProductID`, `o0`.`Discount`, `o0`.`Quantity`, `o0`.`UnitPrice`
 FROM `OrderDetails` `o0`
 WHERE EXISTS (
    SELECT 1
     FROM `Orders` `o`
     WHERE `o0`.`OrderID` = `o`.`OrderID`)
ORDER BY `o0`.`OrderID`",
                Sql);
        }

        public override void Include_collection_order_by_collection_column()
        {
            base.Include_collection_order_by_collection_column();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` LIKE 'W' || '%'
ORDER BY (
    SELECT `oo`.`OrderDate`
     FROM `Orders` `oo`
     WHERE `c`.`CustomerID` = `oo`.`CustomerID`
    ORDER BY `oo`.`OrderDate` DESC LIMIT 1
) DESC, `c`.`CustomerID` LIMIT 1

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
INNER JOIN (
    SELECT DISTINCT (
        SELECT `oo`.`OrderDate`
         FROM `Orders` `oo`
         WHERE `c`.`CustomerID` = `oo`.`CustomerID`
        ORDER BY `oo`.`OrderDate` DESC LIMIT 1
    ) `c0_0`, `c`.`CustomerID`
     FROM `Customers` `c`
     WHERE `c`.`CustomerID` LIKE 'W' || '%'
    ORDER BY `c0_0` DESC, `c`.`CustomerID` LIMIT 1
) `c0` ON `o`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`c0_0` DESC, `c0`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_order_by_key()
        {
            base.Include_collection_order_by_key();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE `o`.`CustomerID` = `c`.`CustomerID`)
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_order_by_non_key()
        {
            base.Include_collection_order_by_non_key();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`City`, `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
INNER JOIN (
    SELECT DISTINCT `c`.`City`, `c`.`CustomerID`
     FROM `Customers` `c`
) `c0` ON `o`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`City`, `c0`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Include_collection_order_by_non_key_with_take()
        {
            base.Include_collection_order_by_non_key_with_take();

            Assert.Equal(
                @":__p_0: 10

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`ContactTitle`, `c`.`CustomerID` LIMIT :__p_0

:__p_0: 10

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
INNER JOIN (
    SELECT DISTINCT `c`.`ContactTitle`, `c`.`CustomerID`
     FROM `Customers` `c`
    ORDER BY `c`.`ContactTitle`, `c`.`CustomerID` LIMIT :__p_0
) `c0` ON `o`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`ContactTitle`, `c0`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_order_by_non_key_with_skip()
        {
            base.Include_collection_order_by_non_key_with_skip();

            if (SupportsOffset)
            {

                Assert.Equal(
                    @":__p_0: 10

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`ContactTitle`, `c`.`CustomerID`
LIMIT 999999999
OFFSET :__p_0

:__p_0: 10

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
INNER JOIN (
    SELECT DISTINCT `t`.*
     FROM (
        SELECT `c`.`ContactTitle`, `c`.`CustomerID`
         FROM `Customers` `c`
        ORDER BY `c`.`ContactTitle`, `c`.`CustomerID`
        LIMIT 999999999
        OFFSET :__p_0
    ) `t`
) `c0` ON `o`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`ContactTitle`, `c0`.`CustomerID`",
                    Sql);
            }
        }

        public override void Include_collection_order_by_non_key_with_first_or_default()
        {
            base.Include_collection_order_by_non_key_with_first_or_default();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CompanyName` DESC, `c`.`CustomerID` LIMIT 1

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
INNER JOIN (
    SELECT DISTINCT `c`.`CompanyName`, `c`.`CustomerID`
     FROM `Customers` `c`
    ORDER BY `c`.`CompanyName` DESC, `c`.`CustomerID` LIMIT 1
) `c0` ON `o`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`CompanyName` DESC, `c0`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_order_by_subquery()
        {
            base.Include_collection_order_by_subquery();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY (
    SELECT `o`.`OrderDate`
     FROM `Orders` `o`
     WHERE `c`.`CustomerID` = `o`.`CustomerID`
    ORDER BY `o`.`EmployeeID` LIMIT 1
), `c`.`CustomerID` LIMIT 1

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`
INNER JOIN (
    SELECT DISTINCT (
        SELECT `o`.`OrderDate`
         FROM `Orders` `o`
         WHERE `c`.`CustomerID` = `o`.`CustomerID`
        ORDER BY `o`.`EmployeeID` LIMIT 1
    ) `c0_0`, `c`.`CustomerID`
     FROM `Customers` `c`
     WHERE `c`.`CustomerID` = 'ALFKI'
    ORDER BY `c0_0`, `c`.`CustomerID` LIMIT 1
) `c0` ON `o0`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`c0_0`, `c0`.`CustomerID`", 
                Sql);
        }

        public override void Include_collection_as_no_tracking()
        {
            base.Include_collection_as_no_tracking();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE `o`.`CustomerID` = `c`.`CustomerID`)
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_principal_already_tracked()
        {
            base.Include_collection_principal_already_tracked();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI' LIMIT 2

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`CustomerID` LIMIT 2

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE (`c`.`CustomerID` = 'ALFKI') AND (`o`.`CustomerID` = `c`.`CustomerID`) LIMIT 2)
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_principal_already_tracked_as_no_tracking()
        {
            base.Include_collection_principal_already_tracked_as_no_tracking();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI' LIMIT 2

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`CustomerID` LIMIT 2

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE (`c`.`CustomerID` = 'ALFKI') AND (`o`.`CustomerID` = `c`.`CustomerID`) LIMIT 2)
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_with_filter()
        {
            base.Include_collection_with_filter();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE (`c`.`CustomerID` = 'ALFKI') AND (`o`.`CustomerID` = `c`.`CustomerID`))
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_with_filter_reordered()
        {
            base.Include_collection_with_filter_reordered();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE (`c`.`CustomerID` = 'ALFKI') AND (`o`.`CustomerID` = `c`.`CustomerID`))
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_then_include_collection()
        {
            base.Include_collection_then_include_collection();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE `o`.`CustomerID` = `c`.`CustomerID`)
ORDER BY `o`.`CustomerID`, `o`.`OrderID`

SELECT `o0`.`OrderID`, `o0`.`ProductID`, `o0`.`Discount`, `o0`.`Quantity`, `o0`.`UnitPrice`
 FROM `OrderDetails` `o0`
INNER JOIN (
    SELECT DISTINCT `o`.`CustomerID`, `o`.`OrderID`
     FROM `Orders` `o`
     WHERE EXISTS (
        SELECT 1
         FROM `Customers` `c`
         WHERE `o`.`CustomerID` = `c`.`CustomerID`)
) `o1` ON `o0`.`OrderID` = `o1`.`OrderID`
ORDER BY `o1`.`CustomerID`, `o1`.`OrderID`",
                Sql);
        }

        public override void Include_collection_when_projection()
        {
            base.Include_collection_when_projection();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`
 FROM `Customers` `c`",
                Sql);
        }

        [Fact]
        public override void Include_collection_on_join_clause_with_filter()
        {
            base.Include_collection_on_join_clause_with_filter();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`CustomerID`

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
    INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
     WHERE (`c`.`CustomerID` = 'ALFKI') AND (`o0`.`CustomerID` = `c`.`CustomerID`))
ORDER BY `o0`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause_with_filter()
        {
            base.Include_collection_on_additional_from_clause_with_filter();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c1`
CROSS JOIN `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c1`
    CROSS JOIN `Customers` `c`
     WHERE (`c`.`CustomerID` = 'ALFKI') AND (`o`.`CustomerID` = `c`.`CustomerID`))
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause()
        {
            base.Include_collection_on_additional_from_clause();

            Assert.Equal(
                @":__p_0: 5

SELECT `c1`.`CustomerID`, `c1`.`Address`, `c1`.`City`, `c1`.`CompanyName`, `c1`.`ContactName`, `c1`.`ContactTitle`, `c1`.`Country`, `c1`.`Fax`, `c1`.`Phone`, `c1`.`PostalCode`, `c1`.`Region`
 FROM (
    SELECT `c0`.*
     FROM `Customers` `c0`
    ORDER BY `c0`.`CustomerID` LIMIT :__p_0
) `t`
CROSS JOIN `Customers` `c1`
ORDER BY `c1`.`CustomerID`

:__p_0: 5

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM (
        SELECT `c0`.*
         FROM `Customers` `c0`
        ORDER BY `c0`.`CustomerID` LIMIT :__p_0
    ) `t`
    CROSS JOIN `Customers` `c1`
     WHERE `o`.`CustomerID` = `c1`.`CustomerID`)
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_duplicate_collection()
        {
            //base.Include_duplicate_collection();

            using (var context = CreateContext())
            {
                var customers
                    = (from c1 in context.Set<Customer>()
                        .Include(c => c.Orders)
                        .OrderBy(c => c.CustomerID)
                        .Take(2)
                       from c2 in context.Set<Customer>()
                           .Include(c => c.Orders)
                           .OrderBy(c => c.CustomerID)
                           .Skip(2)
                           .Take(2)
                       select new { c1, c2 })
                        .ToList();

                Assert.Equal(4, customers.Count);
                Assert.Equal(20, customers.SelectMany(c => c.c1.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
                //Assert.Equal(40, customers.SelectMany(c => c.c2.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c2.Orders).All(o => o.Customer != null));
                //Assert.Equal(34, context.ChangeTracker.Entries().Count());
            }

            if (SupportsOffset)
            {
                Assert.Equal(
                    @":__p_0: 2

SELECT `t`.`CustomerID`, `t`.`Address`, `t`.`City`, `t`.`CompanyName`, `t`.`ContactName`, `t`.`ContactTitle`, `t`.`Country`, `t`.`Fax`, `t`.`Phone`, `t`.`PostalCode`, `t`.`Region`, `t0`.`CustomerID`, `t0`.`Address`, `t0`.`City`, `t0`.`CompanyName`, `t0`.`ContactName`, `t0`.`ContactTitle`, `t0`.`Country`, `t0`.`Fax`, `t0`.`Phone`, `t0`.`PostalCode`, `t0`.`Region`
 FROM (
    SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
     FROM `Customers` `c0`
    ORDER BY `c0`.`CustomerID` LIMIT :__p_0
) `t`
CROSS JOIN (
    SELECT `c2`.`CustomerID`, `c2`.`Address`, `c2`.`City`, `c2`.`CompanyName`, `c2`.`ContactName`, `c2`.`ContactTitle`, `c2`.`Country`, `c2`.`Fax`, `c2`.`Phone`, `c2`.`PostalCode`, `c2`.`Region`
     FROM `Customers` `c2`
    ORDER BY `c2`.`CustomerID`
    LIMIT 2
    OFFSET 2
) `t0`
ORDER BY `t`.`CustomerID`, `t0`.`CustomerID`

:__p_0: 2

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`
 WHERE EXISTS (
    SELECT 1
     FROM (
        SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
         FROM `Customers` `c0`
        ORDER BY `c0`.`CustomerID` LIMIT :__p_0
    ) `t`
    CROSS JOIN (
        SELECT `c2`.`CustomerID`, `c2`.`Address`, `c2`.`City`, `c2`.`CompanyName`, `c2`.`ContactName`, `c2`.`ContactTitle`, `c2`.`Country`, `c2`.`Fax`, `c2`.`Phone`, `c2`.`PostalCode`, `c2`.`Region`
         FROM `Customers` `c2`
        ORDER BY `c2`.`CustomerID`
        LIMIT 2
        OFFSET 2
    ) `t0`
     WHERE `o0`.`CustomerID` = `t0`.`CustomerID`)
ORDER BY `o0`.`CustomerID`

:__p_0: 2

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM (
        SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
         FROM `Customers` `c0`
        ORDER BY `c0`.`CustomerID` LIMIT :__p_0
    ) `t`
    CROSS JOIN (
        SELECT `c2`.`CustomerID`, `c2`.`Address`, `c2`.`City`, `c2`.`CompanyName`, `c2`.`ContactName`, `c2`.`ContactTitle`, `c2`.`Country`, `c2`.`Fax`, `c2`.`Phone`, `c2`.`PostalCode`, `c2`.`Region`
         FROM `Customers` `c2`
        ORDER BY `c2`.`CustomerID`
        LIMIT 2
        OFFSET 2
    ) `t0`
     WHERE `o`.`CustomerID` = `t`.`CustomerID`)
ORDER BY `o`.`CustomerID`",
                    Sql);
            }
        }

        public override void Include_duplicate_collection_result_operator()
        {
            //base.Include_duplicate_collection_result_operator();

            using (var context = CreateContext())
            {
                var customers
                    = (from c1 in context.Set<Customer>()
                        .Include(c => c.Orders)
                        .OrderBy(c => c.CustomerID)
                        .Take(2)
                       from c2 in context.Set<Customer>()
                           .Include(c => c.Orders)
                           .OrderBy(c => c.CustomerID)
                           .Skip(2)
                           .Take(2)
                       select new { c1, c2 })
                        .Take(1)
                        .ToList();

                Assert.Equal(1, customers.Count);
                Assert.Equal(6, customers.SelectMany(c => c.c1.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c1.Orders).All(o => o.Customer != null));
                //Assert.Equal(7, customers.SelectMany(c => c.c2.Orders).Count());
                Assert.True(customers.SelectMany(c => c.c2.Orders).All(o => o.Customer != null));
                //Assert.Equal(15, context.ChangeTracker.Entries().Count());
            }

            if (SupportsOffset)
            {
                Assert.Equal(
                    @":__p_0: 2
:__p_1: 1

SELECT `t`.`CustomerID`, `t`.`Address`, `t`.`City`, `t`.`CompanyName`, `t`.`ContactName`, `t`.`ContactTitle`, `t`.`Country`, `t`.`Fax`, `t`.`Phone`, `t`.`PostalCode`, `t`.`Region`, `t0`.`CustomerID`, `t0`.`Address`, `t0`.`City`, `t0`.`CompanyName`, `t0`.`ContactName`, `t0`.`ContactTitle`, `t0`.`Country`, `t0`.`Fax`, `t0`.`Phone`, `t0`.`PostalCode`, `t0`.`Region`
 FROM (
    SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
     FROM `Customers` `c0`
    ORDER BY `c0`.`CustomerID` LIMIT :__p_0
) `t`
CROSS JOIN (
    SELECT `c2`.`CustomerID`, `c2`.`Address`, `c2`.`City`, `c2`.`CompanyName`, `c2`.`ContactName`, `c2`.`ContactTitle`, `c2`.`Country`, `c2`.`Fax`, `c2`.`Phone`, `c2`.`PostalCode`, `c2`.`Region`
     FROM `Customers` `c2`
    ORDER BY `c2`.`CustomerID`
    LIMIT 2
    OFFSET 2
) `t0`
ORDER BY `t`.`CustomerID`, `t0`.`CustomerID` LIMIT :__p_1

:__p_0: 2
:__p_1: 1

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`
 WHERE EXISTS (
    SELECT 1
     FROM (
        SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
         FROM `Customers` `c0`
        ORDER BY `c0`.`CustomerID` LIMIT :__p_0
    ) `t`
    CROSS JOIN (
        SELECT `c2`.`CustomerID`, `c2`.`Address`, `c2`.`City`, `c2`.`CompanyName`, `c2`.`ContactName`, `c2`.`ContactTitle`, `c2`.`Country`, `c2`.`Fax`, `c2`.`Phone`, `c2`.`PostalCode`, `c2`.`Region`
         FROM `Customers` `c2`
        ORDER BY `c2`.`CustomerID`
        LIMIT 2
        OFFSET 2
    ) `t0`
     WHERE `o0`.`CustomerID` = `t0`.`CustomerID` LIMIT :__p_1)
ORDER BY `o0`.`CustomerID`

:__p_0: 2
:__p_1: 1

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM (
        SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
         FROM `Customers` `c0`
        ORDER BY `c0`.`CustomerID` LIMIT :__p_0
    ) `t`
    CROSS JOIN (
        SELECT `c2`.`CustomerID`, `c2`.`Address`, `c2`.`City`, `c2`.`CompanyName`, `c2`.`ContactName`, `c2`.`ContactTitle`, `c2`.`Country`, `c2`.`Fax`, `c2`.`Phone`, `c2`.`PostalCode`, `c2`.`Region`
         FROM `Customers` `c2`
        ORDER BY `c2`.`CustomerID`
        LIMIT 2
        OFFSET 2
    ) `t0`
     WHERE `o`.`CustomerID` = `t`.`CustomerID` LIMIT :__p_1)
ORDER BY `o`.`CustomerID`",
                    Sql);
            }
        }

        [Fact]
        public override void Include_collection_on_join_clause_with_order_by_and_filter()
        {
            base.Include_collection_on_join_clause_with_order_by_and_filter();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`City`, `c`.`CustomerID`

SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `Orders` `o0`
INNER JOIN (
    SELECT DISTINCT `c`.`City`, `c`.`CustomerID`
     FROM `Customers` `c`
    INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
     WHERE `c`.`CustomerID` = 'ALFKI'
) `c0` ON `o0`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`City`, `c0`.`CustomerID`",
                Sql);
        }

        [Fact]
        public override void Include_collection_when_groupby()
        {
            base.Include_collection_when_groupby();

            Assert.Equal(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`City`, `c`.`CustomerID`

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
INNER JOIN (
    SELECT DISTINCT `c`.`City`, `c`.`CustomerID`
     FROM `Customers` `c`
     WHERE `c`.`CustomerID` = 'ALFKI'
) `c0` ON `o`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`City`, `c0`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_on_additional_from_clause2()
        {
            base.Include_collection_on_additional_from_clause2();

            Assert.Equal(
                @":__p_0: 5

SELECT `t`.`CustomerID`, `t`.`Address`, `t`.`City`, `t`.`CompanyName`, `t`.`ContactName`, `t`.`ContactTitle`, `t`.`Country`, `t`.`Fax`, `t`.`Phone`, `t`.`PostalCode`, `t`.`Region`
 FROM (
    SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
     FROM `Customers` `c0`
    ORDER BY `c0`.`CustomerID` LIMIT :__p_0
) `t`
CROSS JOIN `Customers` `c1`",
                Sql);
        }

        public override void Include_where_skip_take_projection()
        {
            base.Include_where_skip_take_projection();

            if (SupportsOffset)
            {
                Assert.Equal(
                    @":__p_1: 2
:__p_0: 1

SELECT `od.Order`.`CustomerID`
 FROM (
    SELECT `od0`.*
     FROM `OrderDetails` `od0`
     WHERE `od0`.`Quantity` = 10
    ORDER BY `od0`.`OrderID`, `od0`.`ProductID`
    LIMIT :__p_1
    OFFSET :__p_0
) `t`
INNER JOIN `Orders` `od.Order` ON `t`.`OrderID` = `od.Order`.`OrderID`",
                    Sql);
            }
        }

        public override void Include_duplicate_collection_result_operator2()
        {
            base.Include_duplicate_collection_result_operator2();
            if (SupportsOffset)
            {
                Assert.Equal(
                    @":__p_0: 2
:__p_1: 1

SELECT `t`.`CustomerID`, `t`.`Address`, `t`.`City`, `t`.`CompanyName`, `t`.`ContactName`, `t`.`ContactTitle`, `t`.`Country`, `t`.`Fax`, `t`.`Phone`, `t`.`PostalCode`, `t`.`Region`, `t0`.`CustomerID`, `t0`.`Address`, `t0`.`City`, `t0`.`CompanyName`, `t0`.`ContactName`, `t0`.`ContactTitle`, `t0`.`Country`, `t0`.`Fax`, `t0`.`Phone`, `t0`.`PostalCode`, `t0`.`Region`
 FROM (
    SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
     FROM `Customers` `c0`
    ORDER BY `c0`.`CustomerID` LIMIT :__p_0
) `t`
CROSS JOIN (
    SELECT `c2`.`CustomerID`, `c2`.`Address`, `c2`.`City`, `c2`.`CompanyName`, `c2`.`ContactName`, `c2`.`ContactTitle`, `c2`.`Country`, `c2`.`Fax`, `c2`.`Phone`, `c2`.`PostalCode`, `c2`.`Region`
     FROM `Customers` `c2`
    ORDER BY `c2`.`CustomerID`
    LIMIT 2
    OFFSET 2
) `t0`
ORDER BY `t`.`CustomerID` LIMIT :__p_1

:__p_0: 2
:__p_1: 1

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM (
        SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
         FROM `Customers` `c0`
        ORDER BY `c0`.`CustomerID` LIMIT :__p_0
    ) `t`
    CROSS JOIN (
        SELECT `c2`.`CustomerID`, `c2`.`Address`, `c2`.`City`, `c2`.`CompanyName`, `c2`.`ContactName`, `c2`.`ContactTitle`, `c2`.`Country`, `c2`.`Fax`, `c2`.`Phone`, `c2`.`PostalCode`, `c2`.`Region`
         FROM `Customers` `c2`
        ORDER BY `c2`.`CustomerID`
        LIMIT 2
        OFFSET 2
    ) `t0`
     WHERE `o`.`CustomerID` = `t`.`CustomerID` LIMIT :__p_1)
ORDER BY `o`.`CustomerID`",
                    Sql);
            }
        }

        public override void Include_reference()
        {
            base.Include_reference();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`",
                Sql);
        }

        public override void Include_multiple_references()
        {
            base.Include_multiple_references();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`ProductID`, `o`.`Discount`, `o`.`Quantity`, `o`.`UnitPrice`, `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`, `p`.`ProductID`, `p`.`Discontinued`, `p`.`ProductName`, `p`.`UnitsInStock`
 FROM `OrderDetails` `o`
INNER JOIN `Orders` `o0` ON `o`.`OrderID` = `o0`.`OrderID`
INNER JOIN `Products` `p` ON `o`.`ProductID` = `p`.`ProductID`",
                Sql);
        }

        public override void Include_reference_alias_generation()
        {
            base.Include_reference_alias_generation();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`ProductID`, `o`.`Discount`, `o`.`Quantity`, `o`.`UnitPrice`, `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
 FROM `OrderDetails` `o`
INNER JOIN `Orders` `o0` ON `o`.`OrderID` = `o0`.`OrderID`",
                Sql);
        }

        public override void Include_duplicate_reference()
        {
            base.Include_duplicate_reference();

            if (SupportsOffset)
            {
                Assert.Equal(
                    @":__p_0: 2

SELECT `t`.`OrderID`, `t`.`CustomerID`, `t`.`EmployeeID`, `t`.`OrderDate`, `t0`.`OrderID`, `t0`.`CustomerID`, `t0`.`EmployeeID`, `t0`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
 FROM (
    SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
     FROM `Orders` `o0`
    ORDER BY `o0`.`CustomerID` LIMIT :__p_0
) `t`
CROSS JOIN (
    SELECT `o2`.`OrderID`, `o2`.`CustomerID`, `o2`.`EmployeeID`, `o2`.`OrderDate`
     FROM `Orders` `o2`
    ORDER BY `o2`.`CustomerID`
    LIMIT 2
    OFFSET 2
) `t0`
LEFT JOIN `Customers` `c` ON `t`.`CustomerID` = `c`.`CustomerID`
LEFT JOIN `Customers` `c0` ON `t0`.`CustomerID` = `c0`.`CustomerID`",
                    Sql);
            }
        }

        public override void Include_duplicate_reference2()
        {
            base.Include_duplicate_reference2();

            if (SupportsOffset)
            {
                Assert.Equal(
                    @":__p_0: 2

SELECT `t`.`OrderID`, `t`.`CustomerID`, `t`.`EmployeeID`, `t`.`OrderDate`, `t0`.`OrderID`, `t0`.`CustomerID`, `t0`.`EmployeeID`, `t0`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM (
    SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
     FROM `Orders` `o0`
    ORDER BY `o0`.`OrderID` LIMIT :__p_0
) `t`
CROSS JOIN (
    SELECT `o2`.`OrderID`, `o2`.`CustomerID`, `o2`.`EmployeeID`, `o2`.`OrderDate`
     FROM `Orders` `o2`
    ORDER BY `o2`.`OrderID`
    LIMIT 2
    OFFSET 2
) `t0`
LEFT JOIN `Customers` `c` ON `t`.`CustomerID` = `c`.`CustomerID`",
                    Sql);
            }
        }

        public override void Include_duplicate_reference3()
        {
            base.Include_duplicate_reference3();

            if (SupportsOffset)
            {
                Assert.Equal(
                    @":__p_0: 2

SELECT `t`.`OrderID`, `t`.`CustomerID`, `t`.`EmployeeID`, `t`.`OrderDate`, `t0`.`OrderID`, `t0`.`CustomerID`, `t0`.`EmployeeID`, `t0`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM (
    SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`
     FROM `Orders` `o0`
    ORDER BY `o0`.`OrderID` LIMIT :__p_0
) `t`
CROSS JOIN (
    SELECT `o2`.`OrderID`, `o2`.`CustomerID`, `o2`.`EmployeeID`, `o2`.`OrderDate`
     FROM `Orders` `o2`
    ORDER BY `o2`.`OrderID`
    LIMIT 2
    OFFSET 2
) `t0`
LEFT JOIN `Customers` `c` ON `t0`.`CustomerID` = `c`.`CustomerID`",
                    Sql);
            }
        }

        public override void Include_reference_when_projection()
        {
            base.Include_reference_when_projection();

            Assert.Equal(
                @"SELECT `o`.`CustomerID`
 FROM `Orders` `o`",
                Sql);
        }

        public override void Include_reference_with_filter_reordered()
        {
            base.Include_reference_with_filter_reordered();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`
 WHERE `o`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        public override void Include_reference_with_filter()
        {
            base.Include_reference_with_filter();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`
 WHERE `o`.`CustomerID` = 'ALFKI'",
                Sql);
        }

        public override void Include_collection_dependent_already_tracked_as_no_tracking()
        {
            base.Include_collection_dependent_already_tracked_as_no_tracking();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE `o`.`CustomerID` = 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`CustomerID` LIMIT 2

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE (`c`.`CustomerID` = 'ALFKI') AND (`o`.`CustomerID` = `c`.`CustomerID`) LIMIT 2)
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_dependent_already_tracked()
        {
            //base.Include_collection_dependent_already_tracked();
            using (var context = CreateContext())
            {
                var orders
                    = context.Set<Order>()
                        .Where(o => o.CustomerID == "ALFKI")
                        .ToList();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                var customer
                    = context.Set<Customer>()
                        .Include(c => c.Orders)
                        .Single(c => c.CustomerID == "ALFKI");

                //Assert.Equal(orders, customer.Orders.ToList(), ReferenceEqualityComparer.Instance);
                Assert.Equal(6, customer.Orders.Count);
                Assert.True(customer.Orders.All(o => o.Customer != null));
                Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());
            }

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE `o`.`CustomerID` = 'ALFKI'

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` = 'ALFKI'
ORDER BY `c`.`CustomerID` LIMIT 2

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE (`c`.`CustomerID` = 'ALFKI') AND (`o`.`CustomerID` = `c`.`CustomerID`) LIMIT 2)
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_reference_dependent_already_tracked()
        {
            base.Include_reference_dependent_already_tracked();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE `o`.`CustomerID` = 'ALFKI'

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`",
                Sql);
        }

        public override void Include_reference_as_no_tracking()
        {
            base.Include_reference_as_no_tracking();

            Assert.Equal(
                @"SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`, `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Orders` `o`
LEFT JOIN `Customers` `c` ON `o`.`CustomerID` = `c`.`CustomerID`",
                Sql);
        }

        public override void Include_collection_as_no_tracking2()
        {
            base.Include_collection_as_no_tracking2();

            Assert.Equal(
                @":__p_0: 5

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`CustomerID` LIMIT :__p_0

:__p_0: 5

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
 WHERE EXISTS (
    SELECT 1
     FROM `Customers` `c`
     WHERE `o`.`CustomerID` = `c`.`CustomerID` LIMIT :__p_0)
ORDER BY `o`.`CustomerID`",
                Sql);
        }

        public override void Include_with_complex_projection()
        {
            base.Include_with_complex_projection();

            Assert.Equal(
                @"SELECT `o`.`CustomerID`
 FROM `Orders` `o`",
                Sql);
        }

        public override void Include_with_take()
        {
            base.Include_with_take();

            Assert.Equal(
                @":__p_0: 10

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`City` DESC, `c`.`CustomerID` LIMIT :__p_0

:__p_0: 10

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
INNER JOIN (
    SELECT DISTINCT `c`.`City`, `c`.`CustomerID`
     FROM `Customers` `c`
    ORDER BY `c`.`City` DESC, `c`.`CustomerID` LIMIT :__p_0
) `c0` ON `o`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`City` DESC, `c0`.`CustomerID`",
                Sql);
        }

        public override void Include_with_skip()
        {
            base.Include_with_skip();
            if (SupportsOffset)
            {
                Assert.Equal(
                    @":__p_0: 80

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
ORDER BY `c`.`ContactName`, `c`.`CustomerID`
LIMIT 999999999
OFFSET :__p_0

:__p_0: 80

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
INNER JOIN (
    SELECT DISTINCT `t`.*
     FROM (
        SELECT `c`.`ContactName`, `c`.`CustomerID`
         FROM `Customers` `c`
        ORDER BY `c`.`ContactName`, `c`.`CustomerID`
        LIMIT 999999999
        OFFSET :__p_0
    ) `t`
) `c0` ON `o`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`ContactName`, `c0`.`CustomerID`",
                    Sql);
            }
        }

        public override void Then_include_collection_order_by_collection_column()
        {
            base.Then_include_collection_order_by_collection_column();
            Assert.Equal(
    @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`CustomerID` LIKE 'W' || '%'
ORDER BY (
    SELECT `oo`.`OrderDate`
     FROM `Orders` `oo`
     WHERE `c`.`CustomerID` = `oo`.`CustomerID`
    ORDER BY `oo`.`OrderDate` DESC LIMIT 1
) DESC, `c`.`CustomerID` LIMIT 1

SELECT `o`.`OrderID`, `o`.`CustomerID`, `o`.`EmployeeID`, `o`.`OrderDate`
 FROM `Orders` `o`
INNER JOIN (
    SELECT DISTINCT (
        SELECT `oo`.`OrderDate`
         FROM `Orders` `oo`
         WHERE `c`.`CustomerID` = `oo`.`CustomerID`
        ORDER BY `oo`.`OrderDate` DESC LIMIT 1
    ) `c0_0`, `c`.`CustomerID`
     FROM `Customers` `c`
     WHERE `c`.`CustomerID` LIKE 'W' || '%'
    ORDER BY `c0_0` DESC, `c`.`CustomerID` LIMIT 1
) `c0` ON `o`.`CustomerID` = `c0`.`CustomerID`
ORDER BY `c0`.`c0_0` DESC, `c0`.`CustomerID`, `o`.`OrderID`

SELECT `o0`.`OrderID`, `o0`.`ProductID`, `o0`.`Discount`, `o0`.`Quantity`, `o0`.`UnitPrice`
 FROM `OrderDetails` `o0`
INNER JOIN (
    SELECT DISTINCT `c0`.`c0_0`, `c0`.`CustomerID`, `o`.`OrderID`
     FROM `Orders` `o`
    INNER JOIN (
        SELECT DISTINCT (
            SELECT `oo`.`OrderDate`
             FROM `Orders` `oo`
             WHERE `c`.`CustomerID` = `oo`.`CustomerID`
            ORDER BY `oo`.`OrderDate` DESC LIMIT 1
        ) `c0_0`, `c`.`CustomerID`
         FROM `Customers` `c`
         WHERE `c`.`CustomerID` LIKE 'W' || '%'
        ORDER BY `c0_0` DESC, `c`.`CustomerID` LIMIT 1
    ) `c0` ON `o`.`CustomerID` = `c0`.`CustomerID`
) `o1` ON `o0`.`OrderID` = `o1`.`OrderID`
ORDER BY `o1`.`c0_0` DESC, `o1`.`CustomerID`, `o1`.`OrderID`",
    Sql);
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
