using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EntityFrameworkCore.XuGu.Infrastructure;
using EntityFrameworkCore.XuGu.Storage;
using EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;
using Xunit.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System.Runtime.CompilerServices;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindSetOperationsQueryXGTest : NorthwindSetOperationsQueryRelationalTestBase<
        NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindSetOperationsQueryXGTest(
            NorthwindQueryXGFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Intersect(bool async)
        {
            await base.Intersect(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
FROM `Customers` AS `c`
WHERE `c`.`City` = 'London'
INTERSECT
SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
FROM `Customers` AS `c0`
WHERE `c0`.`ContactName` LIKE '%Thomas%'");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Intersect_nested(bool async)
        {
            await base.Intersect_nested(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
FROM `Customers` AS `c`
WHERE `c`.`City` = 'México D.F.'
INTERSECT
SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
FROM `Customers` AS `c0`
WHERE `c0`.`ContactTitle` = 'Owner'
INTERSECT
SELECT `c1`.`CustomerID`, `c1`.`Address`, `c1`.`City`, `c1`.`CompanyName`, `c1`.`ContactName`, `c1`.`ContactTitle`, `c1`.`Country`, `c1`.`Fax`, `c1`.`Phone`, `c1`.`PostalCode`, `c1`.`Region`
FROM `Customers` AS `c1`
WHERE `c1`.`Fax` IS NOT NULL");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Intersect_non_entity(bool async)
        {
            await base.Intersect_non_entity(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`
FROM `Customers` AS `c`
WHERE `c`.`City` = 'México D.F.'
INTERSECT
SELECT `c0`.`CustomerID`
FROM `Customers` AS `c0`
WHERE `c0`.`ContactTitle` = 'Owner'");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptInterceptPrecedence))]
        public override async Task Union_Intersect(bool async)
        {
            await base.Union_Intersect(async);

            AssertSql(
                @"(
    SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
    FROM `Customers` AS `c`
    WHERE `c`.`City` = 'Berlin'
    UNION
    SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
    FROM `Customers` AS `c0`
    WHERE `c0`.`City` = 'London'
)
INTERSECT
SELECT `c1`.`CustomerID`, `c1`.`Address`, `c1`.`City`, `c1`.`CompanyName`, `c1`.`ContactName`, `c1`.`ContactTitle`, `c1`.`Country`, `c1`.`Fax`, `c1`.`Phone`, `c1`.`PostalCode`, `c1`.`Region`
FROM `Customers` AS `c1`
WHERE `c1`.`ContactName` LIKE '%Thomas%'");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Except(bool async)
        {
            await base.Except(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
FROM `Customers` AS `c`
WHERE `c`.`City` = 'London'
EXCEPT
SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
FROM `Customers` AS `c0`
WHERE `c0`.`ContactName` LIKE '%Thomas%'");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Except_simple_followed_by_projecting_constant(bool async)
        {
            await base.Except_simple_followed_by_projecting_constant(async);

            AssertSql(
                @"SELECT 1
FROM (
    SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
    FROM `Customers` AS `c`
    EXCEPT
    SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
    FROM `Customers` AS `c0`
) AS `t`");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Except_nested(bool async)
        {
            await base.Except_nested(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
FROM `Customers` AS `c`
WHERE `c`.`ContactTitle` = 'Owner'
EXCEPT
SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
FROM `Customers` AS `c0`
WHERE `c0`.`City` = 'México D.F.'
EXCEPT
SELECT `c1`.`CustomerID`, `c1`.`Address`, `c1`.`City`, `c1`.`CompanyName`, `c1`.`ContactName`, `c1`.`ContactTitle`, `c1`.`Country`, `c1`.`Fax`, `c1`.`Phone`, `c1`.`PostalCode`, `c1`.`Region`
FROM `Customers` AS `c1`
WHERE `c1`.`City` = 'Seattle'");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Except_non_entity(bool async)
        {
            await base.Except_non_entity(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`
FROM `Customers` AS `c`
WHERE `c`.`ContactTitle` = 'Owner'
EXCEPT
SELECT `c0`.`CustomerID`
FROM `Customers` AS `c0`
WHERE `c0`.`City` = 'México D.F.'");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Select_Except_reference_projection(bool async)
        {
            await base.Select_Except_reference_projection(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
FROM `Orders` AS `o`
LEFT JOIN `Customers` AS `c` ON `o`.`CustomerID` = `c`.`CustomerID`
EXCEPT
SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
FROM `Orders` AS `o0`
LEFT JOIN `Customers` AS `c0` ON `o0`.`CustomerID` = `c0`.`CustomerID`
WHERE `o0`.`CustomerID` = 'ALFKI'");
        }

        [ConditionalTheory(Skip = "TODO: XG does not seem to allow an ORDER BY or LIMIT clause directly in a SELECT statement that is part of a UNION.")]
        public override Task Union_Take_Union_Take(bool async)
        {
            // TODO: XG does not seem to allow an ORDER BY or LIMIT clause directly in a SELECT statement that is part of a UNION.
            //       To make this work, the SELECT statement containing the ORDER BY and/or LIMIT clause needs to be wrapped by another
            //       SELECT statement.
            return base.Union_Take_Union_Take(async);
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.ExceptIntercept))]
        public override async Task Union_Select_scalar(bool async)
        {
            await base.Union_Select_scalar(async);

            AssertSql(
                @"SELECT 1
FROM (
    SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`
    FROM `Customers` AS `c`
    EXCEPT
    SELECT `c0`.`CustomerID`, `c0`.`Address`, `c0`.`City`, `c0`.`CompanyName`, `c0`.`ContactName`, `c0`.`ContactTitle`, `c0`.`Country`, `c0`.`Fax`, `c0`.`Phone`, `c0`.`PostalCode`, `c0`.`Region`
    FROM `Customers` AS `c0`
) AS `t`");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
        private static IEnumerable<object[]> GetSetOperandTestCases()
        => from async in new[] { true, false }
           from leftType in _supportedOperandExpressionType
           from rightType in _supportedOperandExpressionType
           select new object[] { async, leftType, rightType };

        // ReSharper disable once StaticMemberInGenericType
        private static readonly string[] _supportedOperandExpressionType =
        {
            "Column", "Function", "Constant", "Unary", "Binary", "ScalarSubquery"
        };

        [ConditionalTheory]
#pragma warning disable xUnit1016 // MemberData must reference a public member
        [MemberData(nameof(GetSetOperandTestCases))]
#pragma warning restore xUnit1016 // MemberData must reference a public member

        public override async Task Union_over_different_projection_types(bool async, string leftType, string rightType)
        {
            //await base.Union_over_different_projection_types(async, leftType, rightType);

            var (left, right) = (ExpressionGenerator(leftType), ExpressionGenerator(rightType));
            await AssertQuery(async, ss => left(ss.Set<Order>()).Union(right(ss.Set<Order>())));

            static Func<IQueryable<Order>, IQueryable<object>> ExpressionGenerator(string expressionType)
            {
                switch (expressionType)
                {
                    case "Column":
                        return os => os.Select(o => (object)o.OrderID);
                    case "Function":
                        return os => os
                            .GroupBy(o => o.OrderID)
                            .Select(g => (object)g.Count());
                    case "Constant":
                        return os => os.Select(o => (object)8);
                    case "Unary":
                        return os => os.Select(o => (object)-o.OrderID);
                    case "Binary":
                        return os => os.Select(o => (object)(o.OrderID + 1));
                    case "ScalarSubquery":
                        return os => os.Select(o => (object)o.OrderDetails.Count());
                    default:
                        throw new InvalidOperationException();
                }
            }

            var leftSql = GenerateSql(leftType);
            var rightSql = GenerateSql(rightType);

            switch (leftType)
            {
                case "Column":
                    leftSql = leftSql.Replace("{Alias}", "");
                    break;

                case "Binary":
                case "Constant":
                case "Function":
                case "ScalarSubquery":
                case "Unary":
                    leftSql = leftSql.Replace("{Alias}", " AS `c`");
                    break;

                default:
                    throw new ArgumentException("Unexpected type: " + leftType);
            }

            switch (rightType)
            {
                case "Column":
                    rightSql = rightSql.Replace("{Alias}", leftType == "Column" ? "" : " AS `c`");
                    break;

                case "Binary":
                case "Constant":
                case "Function":
                case "ScalarSubquery":
                case "Unary":
                    rightSql = rightSql.Replace("{Alias}", leftType == "Column" ? " AS `OrderID`" : " AS `c`");
                    break;
                default:
                    throw new ArgumentException("Unexpected type: " + rightType);
            }

            // Fix up right-side SQL as table aliases shift
            rightSql = leftType == "ScalarSubquery"
                ? rightSql.Replace("`o`", "`o1`").Replace("`o0`", "`o2`")
                : rightSql.Replace("`o0`", "`o1`").Replace("`o`", "`o0`");

            AssertSql(leftSql + Environment.NewLine + "UNION" + Environment.NewLine + rightSql);

            static string GenerateSql(string expressionType)
            {
                switch (expressionType)
                {
                    case "Column":
                        return @"SELECT `o`.`OrderID`{Alias}
FROM `Orders` AS `o`";
                    case "Function":
                        return @"SELECT COUNT(*){Alias}
FROM `Orders` AS `o`
GROUP BY `o`.`OrderID`";
                    case "Constant":
                        return @"SELECT 8{Alias}
FROM `Orders` AS `o`";
                    case "Unary":
                        return @"SELECT -`o`.`OrderID`{Alias}
FROM `Orders` AS `o`";
                    case "Binary":
                        return @"SELECT `o`.`OrderID` + 1{Alias}
FROM `Orders` AS `o`";
                    case "ScalarSubquery":
                        return @"SELECT (
    SELECT COUNT(*)
    FROM `Order Details` AS `o`
    WHERE `o0`.`OrderID` = `o`.`OrderID`){Alias}
FROM `Orders` AS `o0`";
                    default:
                        throw new ArgumentException("Unexpected type: " + expressionType);
                }
            }
        }
    }
}
