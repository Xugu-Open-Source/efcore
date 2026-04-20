// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    [MonoVersionCondition(Min = "4.2.0", SkipReason = "Queries fail on Mono < 4.2.0 due to differences in the implementation of LINQ")]
    public class RowNumberPagingTest : QueryTestBase<NorthwindRowNumberPagingQueryXGFixture>, IDisposable
    {
        public RowNumberPagingTest(NorthwindRowNumberPagingQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public void Dispose()
        {
            //Assert for all tests that OFFSET or FETCH is never used
            Assert.DoesNotContain("LIMIT ", Sql);
            Assert.DoesNotContain("OFFSET ", Sql);
        }

        [Fact]
        public override void Skip()
        {
            base.Skip();

            Assert.Equal(
                @":__p_0: 5

SELECT `t`.`CustomerID`, `t`.`Address`, `t`.`City`, `t`.`CompanyName`, `t`.`ContactName`, `t`.`ContactTitle`, `t`.`Country`, `t`.`Fax`, `t`.`Phone`, `t`.`PostalCode`, `t`.`Region`
 FROM (
    SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, ROW_NUMBER() OVER(ORDER BY `c`.`CustomerID`) `__RowNumber__`
     FROM `Customers` `c`
) `t`
 WHERE `t`.`__RowNumber__` > :__p_0",
                Sql);
        }

        [Fact]
        public override void Skip_no_orderby()
        {
            base.Skip_no_orderby();

            Assert.EndsWith(
                @":__p_0: 5

SELECT `t`.`CustomerID`, `t`.`Address`, `t`.`City`, `t`.`CompanyName`, `t`.`ContactName`, `t`.`ContactTitle`, `t`.`Country`, `t`.`Fax`, `t`.`Phone`, `t`.`PostalCode`, `t`.`Region`
 FROM (
    SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, ROW_NUMBER() OVER(ORDER BY @@RowCount) `__RowNumber__`
     FROM `Customers` `c`
) `t`
 WHERE `t`.`__RowNumber__` > :__p_0",
                Sql);
        }

        [Fact]
        public override void Skip_Take()
        {
            //base.Skip_Take();

            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Skip(5).Take(10),
                assertOrder: true,
                entryCount: 10);

            Assert.Equal(
                @":__p_0: 5
:__p_1: 10

SELECT `t`.`CustomerID`, `t`.`Address`, `t`.`City`, `t`.`CompanyName`, `t`.`ContactName`, `t`.`ContactTitle`, `t`.`Country`, `t`.`Fax`, `t`.`Phone`, `t`.`PostalCode`, `t`.`Region`
 FROM (
    SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, ROW_NUMBER() OVER(ORDER BY `c`.`ContactName`) `__RowNumber__`
     FROM `Customers` `c`
) `t`
 WHERE (`t`.`__RowNumber__` > :__p_0) AND (`t`.`__RowNumber__` <= (:__p_0 + :__p_1))",
                Sql);
        }

        [Fact]
        public override void Join_Customers_Orders_Skip_Take()
        {
            base.Join_Customers_Orders_Skip_Take();

            Assert.Equal(
                @":__p_0: 10
:__p_1: 5

SELECT `t`.`ContactName`, `t`.`OrderID`
 FROM (
    SELECT `c`.`ContactName`, `o`.`OrderID`, ROW_NUMBER() OVER(ORDER BY `o`.`OrderID`) `__RowNumber__`
     FROM `Customers` `c`
    INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
) `t`
 WHERE (`t`.`__RowNumber__` > :__p_0) AND (`t`.`__RowNumber__` <= (:__p_0 + :__p_1))",
                Sql);
        }

        [Fact]
        public override void Join_Customers_Orders_Projection_With_String_Concat_Skip_Take()
        {
            base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take();

            Assert.Equal(
                @":__p_0: 10
:__p_1: 5

SELECT `t`.`c0`, `t`.`OrderID`
 FROM (
    SELECT (`c`.`ContactName` || ' ') || `c`.`ContactTitle` `c0`, `o`.`OrderID`, ROW_NUMBER() OVER(ORDER BY `o`.`OrderID`) `__RowNumber__`
     FROM `Customers` `c`
    INNER JOIN `Orders` `o` ON `c`.`CustomerID` = `o`.`CustomerID`
) `t`
 WHERE (`t`.`__RowNumber__` > :__p_0) AND (`t`.`__RowNumber__` <= (:__p_0 + :__p_1))",
                Sql);
        }

        [Fact]
        public override void Take_Skip()
        {
            base.Take_Skip();

            Assert.Equal(@":__p_0: 10
:__p_1: 5

SELECT `t0`.*
 FROM (
    SELECT `t`.*, ROW_NUMBER() OVER(ORDER BY `t`.`ContactName`) `__RowNumber__`
     FROM (
        SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
         FROM `Customers` `c`
        ORDER BY `c`.`ContactName` LIMIT :__p_0
    ) `t`
) `t0`
 WHERE `t0`.`__RowNumber__` > :__p_1",
                Sql);
        }

        [Fact]
        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();

            Assert.Equal(
                @":__p_0: 10
:__p_1: 5

SELECT DISTINCT `t0`.*
 FROM (
    SELECT `t1`.*
     FROM (
        SELECT `t`.*, ROW_NUMBER() OVER(ORDER BY `t`.`ContactName`) `__RowNumber__`
         FROM (
            SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
             FROM `Customers` `c`
            ORDER BY `c`.`ContactName` LIMIT :__p_0
        ) `t`
    ) `t1`
     WHERE `t1`.`__RowNumber__` > :__p_1
) `t0`",
                Sql);
        }

        [Fact]
        public override void Take_skip_null_coalesce_operator()
        {
            base.Take_skip_null_coalesce_operator();

            Assert.Equal(@":__p_0: 10
:__p_1: 5

SELECT DISTINCT `t0`.*
 FROM (
    SELECT `t1`.*
     FROM (
        SELECT `t`.*, ROW_NUMBER() OVER(ORDER BY COALESCE(`t`.`Region`, 'ZZ')) `__RowNumber__`
         FROM (
            SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
             FROM `Customers` `c`
            ORDER BY COALESCE(`c`.`Region`, 'ZZ') LIMIT :__p_0
        ) `t`
    ) `t1`
     WHERE `t1`.`__RowNumber__` > :__p_1
) `t0`",
                Sql);
        }

        [Fact]
        public override void Select_take_skip_null_coalesce_operator()
        {
            base.Select_take_skip_null_coalesce_operator();

            Assert.Equal(@":__p_0: 10
:__p_1: 5

SELECT `t0`.*
 FROM (
    SELECT `t`.*, ROW_NUMBER() OVER(ORDER BY `Coalesce`) `__RowNumber__`
     FROM (
        SELECT `c`.`CustomerID`, `c`.`CompanyName`, COALESCE(`c`.`Region`, 'ZZ') `Coalesce`
         FROM `Customers` `c`
        ORDER BY `Coalesce` LIMIT :__p_0
    ) `t`
) `t0`
 WHERE `t0`.`__RowNumber__` > :__p_1",
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
 WHERE `c`.`ContactName` LIKE ('%' || 'M') || '%'",
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
                @":__LocalMethod1_0: M (Size = 4000)

SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
 FROM `Customers` `c`
 WHERE `c`.`ContactName` LIKE ('%' || :__LocalMethod1_0) || '%'",
                Sql);
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
