// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindGroupByQueryXGTest : NorthwindGroupByQueryRelationalTestBase<
        NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindGroupByQueryXGTest(
            NorthwindQueryXGFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        public override Task Self_join_GroupBy_Aggregate(bool async)
            => AssertQuery(
                async,
                ss => (from o1 in ss.Set<Order>().Where(o => o.OrderID < 10400)
                       join o2 in ss.Set<Order>() on o1.OrderID equals o2.OrderID
                       group o2 by o1.CustomerID)
                    .Select(g => new { g.Key, Count = (float)g.Average(o => o.OrderID) }),
                e => e.Key);

        public override Task Select_anonymous_GroupBy_Aggregate(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10300)
                    .Select(
                        o => new
                        {
                            A = o.CustomerID,
                            B = o.OrderDate,
                            C = o.OrderID
                        })
                    .GroupBy(e => e.A)
                    .Select(
                        g => new
                        {
                            Min = g.Min(o => o.B),
                            Max = g.Max(o => o.B),
                            Sum = g.Sum(o => o.C),
                            Avg = Math.Ceiling(g.Average(o => o.C))
                        }));

        public override Task OrderBy_Skip_GroupBy_Aggregate(bool async)
            => AssertQueryScalar(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID)
                    .Skip(80)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => Math.Ceiling(g.Average(o => o.OrderID))));

        public override Task Join_GroupBy_Aggregate(bool async)
            => AssertQuery(
                async,
                ss =>
                    (from o in ss.Set<Order>()
                     join c in ss.Set<Customer>() on o.CustomerID equals c.CustomerID
                     group o by c.CustomerID)
                    .Select(g => new { g.Key, Count = Math.Ceiling(g.Average(o => o.OrderID)) }),
                e => e.Key);

        public override Task Join_complex_GroupBy_Aggregate(bool async)
            => AssertQuery(
                async,
                ss =>
                    (from o in ss.Set<Order>().Where(o => o.OrderID < 10400).OrderBy(o => o.OrderDate).Take(100)
                     join c in ss.Set<Customer>().Where(c => c.CustomerID != "DRACD" && c.CustomerID != "FOLKO")
                             .OrderBy(c => c.City).Skip(10).Take(50)
                         on o.CustomerID equals c.CustomerID
                     group o by c.CustomerID)
                    .Select(
                        g => new { g.Key, Count = Math.Ceiling(g.Average(o => o.OrderID)) }),
                e => e.Key);

        public override Task GroupJoin_GroupBy_Aggregate_3(bool async)
            => AssertQuery(
                async,
                ss =>
                    (from o in ss.Set<Order>()
                     join c in ss.Set<Customer>()
                         on o.CustomerID equals c.CustomerID into grouping
                     from c in grouping.DefaultIfEmpty()
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g => new { g.Key, Average = Math.Ceiling(g.Average(o => o.OrderID)) }),
                e => e.Key);

        public override Task GroupJoin_GroupBy_Aggregate(bool async)
            => AssertQuery(
                async,
                ss =>
                    (from c in ss.Set<Customer>()
                     join o in ss.Set<Order>()
                         on c.CustomerID equals o.CustomerID into grouping
                     from o in grouping.DefaultIfEmpty()
                     where o != null
                     select o)
                    .GroupBy(o => o.CustomerID)
                    .Select(
                        g => new { g.Key, Average = Math.Ceiling(g.Average(o => o.OrderID)) }),
                e => e.Key);

        public override Task GroupBy_with_result_selector(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, (k, g) =>
                        new
                        {
                            // ReSharper disable once PossibleMultipleEnumeration
                            Sum = g.Sum(o => o.OrderID),
                            // ReSharper disable once PossibleMultipleEnumeration
                            Min = g.Min(o => o.OrderID),
                            // ReSharper disable once PossibleMultipleEnumeration
                            Max = g.Max(o => o.OrderID),
                            // ReSharper disable once PossibleMultipleEnumeration
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => e.Min + " " + e.Max);

        public override Task GroupBy_Property_Select_Sum_Min_Max_Avg(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => (e.Min, e.Max));

        public override Task GroupBy_Property_Select_Sum_Min_Key_Max_Avg(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => EF.Property<string>(o, "CustomerID")).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            g.Key,
                            Max = g.Max(o => o.OrderID),
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => e.Min + " " + e.Max);

        public override Task GroupBy_Property_Select_Key_Sum_Min_Max_Avg(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => e.Min + " " + e.Max);

        public override Task GroupBy_Property_Select_Key_Average(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(
                    g =>
                        new { g.Key, Average = Math.Ceiling(g.Average(o => o.OrderID)) }),
                e => e.Key);

        public override Task GroupBy_Property_Select_Average(bool async)
            => AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(o => o.CustomerID).Select(g => Math.Ceiling(g.Average(o => o.OrderID))));

        public override Task GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.EmployeeID),
                            Max = g.Max(o => o.EmployeeID),
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => e.Sum + " " + e.Avg);

        public override Task GroupBy_Property_anonymous_element_selector_Average(bool async)
            => AssertQueryScalar(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => o.CustomerID, o => new { o.OrderID, o.EmployeeID }).Select(g => Math.Ceiling(g.Average(o => o.OrderID))));

        public override Task GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            g.Key.CustomerID,
                            Max = g.Max(o => o.OrderID),
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => e.Min + " " + e.Max);

        public override Task GroupBy_Composite_Select_Sum_Min_Max_Avg(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => e.Min + " " + e.Max);

        public override Task GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            g.Key,
                            Max = g.Max(o => o.OrderID),
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => e.Min + " " + e.Max);

        public override Task GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            g.Key.CustomerID,
                            g.Key.EmployeeID,
                            Max = g.Max(o => o.OrderID),
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => e.Min + " " + e.Max);

        public override Task GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new
                        {
                            g.Key,
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => e.Min + " " + e.Max);

        public override Task GroupBy_Composite_Select_Key_Average(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new { g.Key, Average = Math.Ceiling(g.Average(o => o.OrderID)) }),
                e => e.Key.CustomerID + " " + e.Key.EmployeeID);

        public override Task GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().GroupBy(
                    o => new { o.CustomerID, o.EmployeeID }).Select(
                    g =>
                        new CompositeDto
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            CustomerId = g.Key.CustomerID,
                            EmployeeId = g.Key.EmployeeID,
                            Max = g.Max(o => o.OrderID),
                            Avg = Math.Ceiling(g.Average(o => o.OrderID))
                        }),
                e => e.CustomerId + " " + e.EmployeeId);

        public override async Task GroupBy_Composite_Select_Average(bool async)
        {
            await AssertQueryScalar(
            async,
            ss => ss.Set<Order>().GroupBy(
                o => new { o.CustomerID, o.EmployeeID }).Select(g => Math.Ceiling(g.Average(o => o.OrderID))));
        }

        public override async Task GroupBy_anonymous_Select_Average(bool async)
        {
            await AssertQueryScalar(
            async,
            ss => ss.Set<Order>().GroupBy(
                o => new { o.CustomerID }).Select(g => g.Average(o => (float)o.OrderID)));

            AssertSql(
                """
SELECT AVG(CAST(`o`.`OrderID` AS double))
FROM `Orders` AS `o`
GROUP BY `o`.`CustomerID`
""");
        }

        public override async Task GroupBy_anonymous_Select_Sum_Min_Max_Avg(bool async)
        {
            await AssertQuery(
            async,
            ss => ss.Set<Order>().GroupBy(
                o => new { o.CustomerID }).Select(
                g =>
                    new
                    {
                        Sum = g.Sum(o => o.OrderID),
                        Min = g.Min(o => o.OrderID),
                        Max = g.Max(o => o.OrderID),
                        Avg = Math.Ceiling(g.Average(o => o.OrderID))
                    }),
            e => e.Min + " " + e.Max);
        }

        public override async Task AsEnumerable_in_subquery_for_GroupBy(bool async)
        {
            await base.AsEnumerable_in_subquery_for_GroupBy(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`, `c`.`Address`, `c`.`City`, `c`.`CompanyName`, `c`.`ContactName`, `c`.`ContactTitle`, `c`.`Country`, `c`.`Fax`, `c`.`Phone`, `c`.`PostalCode`, `c`.`Region`, `t2`.`OrderID`, `t2`.`CustomerID`, `t2`.`EmployeeID`, `t2`.`OrderDate`, `t2`.`CustomerID0`
FROM `Customers` AS `c`
LEFT JOIN LATERAL (
    SELECT `t0`.`OrderID`, `t0`.`CustomerID`, `t0`.`EmployeeID`, `t0`.`OrderDate`, `t`.`CustomerID` AS `CustomerID0`
    FROM (
        SELECT `o`.`CustomerID`
        FROM `Orders` AS `o`
        WHERE `o`.`CustomerID` = `c`.`CustomerID`
        GROUP BY `o`.`CustomerID`
    ) AS `t`
    LEFT JOIN (
        SELECT `t1`.`OrderID`, `t1`.`CustomerID`, `t1`.`EmployeeID`, `t1`.`OrderDate`
        FROM (
            SELECT `o0`.`OrderID`, `o0`.`CustomerID`, `o0`.`EmployeeID`, `o0`.`OrderDate`, ROW_NUMBER() OVER(PARTITION BY `o0`.`CustomerID` ORDER BY `o0`.`OrderDate` DESC) AS `row`
            FROM `Orders` AS `o0`
            WHERE `o0`.`CustomerID` = `c`.`CustomerID`
        ) AS `t1`
        WHERE `t1`.`row` <= 1
    ) AS `t0` ON `t`.`CustomerID` = `t0`.`CustomerID`
) AS `t2` ON TRUE
WHERE `c`.`CustomerID` LIKE 'F%'
ORDER BY `c`.`CustomerID`, `t2`.`CustomerID0`");
        }

        public override async Task Complex_query_with_groupBy_in_subquery1(bool async)
        {
            await base.Complex_query_with_groupBy_in_subquery1(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`, `t`.`Sum`, `t`.`CustomerID`
FROM `Customers` AS `c`
LEFT JOIN LATERAL (
    SELECT COALESCE(SUM(`o`.`OrderID`), 0) AS `Sum`, `o`.`CustomerID`
    FROM `Orders` AS `o`
    WHERE `c`.`CustomerID` = `o`.`CustomerID`
    GROUP BY `o`.`CustomerID`
) AS `t` ON TRUE
ORDER BY `c`.`CustomerID`");
        }

        public override async Task Complex_query_with_groupBy_in_subquery2(bool async)
        {
            await base.Complex_query_with_groupBy_in_subquery2(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`, `t`.`Max`, `t`.`Sum`, `t`.`CustomerID`
FROM `Customers` AS `c`
LEFT JOIN LATERAL (
    SELECT MAX(CHAR_LENGTH(`o`.`CustomerID`)) AS `Max`, COALESCE(SUM(`o`.`OrderID`), 0) AS `Sum`, `o`.`CustomerID`
    FROM `Orders` AS `o`
    WHERE `c`.`CustomerID` = `o`.`CustomerID`
    GROUP BY `o`.`CustomerID`
) AS `t` ON TRUE
ORDER BY `c`.`CustomerID`");
        }

        public override async Task Complex_query_with_groupBy_in_subquery3(bool async)
        {
            await base.Complex_query_with_groupBy_in_subquery3(async);

            AssertSql(
                @"SELECT `c`.`CustomerID`, `t`.`Max`, `t`.`Sum`, `t`.`CustomerID`
FROM `Customers` AS `c`
LEFT JOIN LATERAL (
    SELECT MAX(CHAR_LENGTH(`o`.`CustomerID`)) AS `Max`, COALESCE(SUM(`o`.`OrderID`), 0) AS `Sum`, `o`.`CustomerID`
    FROM `Orders` AS `o`
    GROUP BY `o`.`CustomerID`
) AS `t` ON TRUE
ORDER BY `c`.`CustomerID`");
        }

        public override async Task Select_nested_collection_with_groupby(bool async)
        {
            await base.Select_nested_collection_with_groupby(async);

            AssertSql(
                @"SELECT EXISTS (
    SELECT 1
    FROM `Orders` AS `o`
    WHERE `c`.`CustomerID` = `o`.`CustomerID`), `c`.`CustomerID`, `t`.`OrderID`
FROM `Customers` AS `c`
LEFT JOIN LATERAL (
    SELECT `o0`.`OrderID`
    FROM `Orders` AS `o0`
    WHERE `c`.`CustomerID` = `o0`.`CustomerID`
    GROUP BY `o0`.`OrderID`
) AS `t` ON TRUE
WHERE `c`.`CustomerID` LIKE 'F%'
ORDER BY `c`.`CustomerID`");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.OuterReferenceInMultiLevelSubquery))]
        public override async Task GroupBy_group_Distinct_Select_Distinct_aggregate(bool async)
        {
            await base.GroupBy_group_Distinct_Select_Distinct_aggregate(async);

            AssertSql(
                @"SELECT `o`.`CustomerID` AS `Key`, MAX(DISTINCT (`o`.`OrderDate`)) AS `Max`
FROM `Orders` AS `o`
GROUP BY `o`.`CustomerID`");
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.OuterReferenceInMultiLevelSubquery))]
        public override Task GroupBy_Count_in_projection(bool async)
        {
            return base.GroupBy_Count_in_projection(async);
        }

        [SupportedServerVersionCondition("12.0.0-xg")]
        public override Task GroupBy_group_Where_Select_Distinct_aggregate(bool async)
        {
            // See https://github.com/mysql-net/XuguClient/issues/898.
            return base.GroupBy_group_Where_Select_Distinct_aggregate(async);
        }

        [SupportedServerVersionCondition("12.0.0-xg")]
        public override Task GroupBy_constant_with_where_on_grouping_with_aggregate_operators(bool async)
        {
            // See https://github.com/mysql-net/XuguClient/issues/980.
            return base.GroupBy_constant_with_where_on_grouping_with_aggregate_operators(async);
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
