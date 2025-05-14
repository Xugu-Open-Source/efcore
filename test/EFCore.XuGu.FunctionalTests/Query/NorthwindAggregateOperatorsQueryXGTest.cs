// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindAggregateOperatorsQueryXGTest : NorthwindAggregateOperatorsQueryRelationalTestBase<
        NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindAggregateOperatorsQueryXGTest(
            NorthwindQueryXGFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override Task Sum_over_nested_subquery_is_client_eval(bool async)
            => AssertSum(
                async,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Sum(od => od.ProductID)));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Contains_with_local_array_closure(bool async)
        {
            var ids = new[] { "ABCDE", "ALFKI" };

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));

            ids = new[] { "ABCDE" };

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
                assertEmpty: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Contains_with_subquery_and_local_array_closure(bool async)
        {
            var ids = new[] { "London", "Buenos Aires" };

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => ss.Set<Customer>().Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)));

            ids = new[] { "London" };

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => ss.Set<Customer>().Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Contains_with_local_uint_array_closure(bool async)
        {
            var ids = new uint[] { 0, 1 };

            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)));

            ids = new uint[] { 0 };

            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)),
                assertEmpty: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override Task Contains_with_local_array_inline(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => new[] { "ABCDE", "ALFKI" }.Contains(c.CustomerID)));

        public override Task Average_over_max_subquery_is_client_eval(bool async)
            => AssertAverage(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Max(od => od.ProductID)),
                asserter: (a, b) => Assert.Equal((int)(a), (int)b)); // added flouting point precision tolerance

        public override Task Average_over_nested_subquery_is_client_eval(bool async)
            => AssertAverage(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Average(od => od.ProductID)),
                asserter: (a, b) => Assert.Equal(Math.Floor(a), Math.Floor(b))); // added flouting point precision tolerance

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override Task Sum_with_division_on_decimal(bool async)
        => AssertSum(
            async,
            ss => ss.Set<OrderDetail>(),
            selector: od => od.Quantity / 2.09m,
            asserter: (e, a) => Assert.InRange(e - a, -0.1m, 25m));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override Task Sum_with_division_on_decimal_no_significant_digits(bool async)
            => AssertSum(
                async,
                ss => ss.Set<OrderDetail>(),
                selector: od => od.Quantity / 2m,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 25m));

        // TODO: Implement TranslatePrimitiveCollection.
        public override async Task Contains_with_local_anonymous_type_array_closure(bool async)
        {
            // Aggregates. Issue #15937.
            // await AssertTranslationFailed(() => base.Contains_with_local_anonymous_type_array_closure(async));

            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Contains_with_local_anonymous_type_array_closure(async));

            AssertSql();
        }

        // TODO: Implement TranslatePrimitiveCollection.
        public override async Task Contains_with_local_tuple_array_closure(bool async)
        {
            // await AssertTranslationFailed(() => base.Contains_with_local_tuple_array_closure(async));

            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Contains_with_local_tuple_array_closure(async));
        }

        public override async Task Contains_with_local_enumerable_inline(bool async)
        {
            // Issue #31776
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                    await base.Contains_with_local_enumerable_inline(async));

            AssertSql();
        }

        protected override bool CanExecuteQueryString
            => true;

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
