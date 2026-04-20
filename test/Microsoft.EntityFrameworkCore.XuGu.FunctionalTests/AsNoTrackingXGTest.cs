// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    [MonoVersionCondition(Min = "4.2.0", SkipReason = "Queries fail on Mono < 4.2.0 due to differences in the implementation of LINQ")]
    public class AsNoTrackingXGTest : AsNoTrackingTestBase<NorthwindQueryXGFixture>
    {
        public AsNoTrackingXGTest(NorthwindQueryXGFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public override void Entity_not_added_to_state_manager()
        {
            using (var context = CreateContext())
            {
                var customers = context.Set<Customer>().AsNoTracking().ToList();

                Assert.Equal(91, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public override void Applied_to_body_clause()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       join o in context.Set<Order>().AsNoTracking()
                           on c.CustomerID equals o.CustomerID
                       where c.CustomerID == "ALFKI"
                       select o)
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public override void Applied_to_multiple_body_clauses()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>().AsNoTracking()
                       from o in context.Set<Order>().AsNoTracking()
                       where c.CustomerID == o.CustomerID
                       select new { c, o })
                        .ToList();

                Assert.Equal(830, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public override void Applied_to_body_clause_with_projection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       join o in context.Set<Order>().AsNoTracking()
                           on c.CustomerID equals o.CustomerID
                       where c.CustomerID == "ALFKI"
                       select new { c.CustomerID, c, ocid = o.CustomerID, o })
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public override void Applied_to_projection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       join o in context.Set<Order>().AsNoTracking()
                           on c.CustomerID equals o.CustomerID
                       where c.CustomerID == "ALFKI"
                       select new { c, o })
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public override void Can_get_current_values()
        {
            using (var db = CreateContext())
            {
                var customer = db.Customers.First();

                customer.CompanyName = "foo";

                var dbCustomer = db.Customers.AsNoTracking().First();

                Assert.NotEqual(customer.CompanyName, dbCustomer.CompanyName);
            }
        }

        [Fact]
        public override void Include_reference_and_collection()
        {
            using (var context = CreateContext())
            {
                var orders
                    = context.Set<Order>()
                        .Include(o => o.OrderDetails)
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(830, orders.Count);
            }
        }

        [Fact]
        public override void Where_simple_shadow()
        {
            using (var context = CreateContext())
            {
                var employees
                    = context.Set<Employee>()
                        .Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(6, employees.Count);
            }
        }

        [Fact]
        public override void SelectMany_simple()
        {
            using (var context = CreateContext())
            {
                var results
                    = (from e in context.Set<Employee>()
                       from c in context.Set<Customer>()
                       select new { c, e })
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(819, results.Count);
            }
        }
    }
}
