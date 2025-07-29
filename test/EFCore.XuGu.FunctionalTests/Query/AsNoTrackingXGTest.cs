using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class AsNoTrackingXGTest : AsNoTrackingTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public AsNoTrackingXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact]
        public override void Entity_not_added_to_state_manager()
        {
            using (var context = CreateContext())
            {
                var customers = context.Set<Customer>().AsNoTracking().ToList();

                Assert.Equal(91, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public override void Applied_to_multiple_body_clauses()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>().AsNoTracking()
                       from o in context.Set<Order>().AsNoTracking()
                       where c.CustomerID == o.CustomerID
                       select new
                       {
                           c,
                           o
                       })
                    .ToList();

                Assert.Equal(830, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public override void Applied_to_body_clause_with_projection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       join o in context.Set<Order>().AsNoTracking()
                           on c.CustomerID equals o.CustomerID
                       where c.CustomerID == "ALFKI"
                       select new
                       {
                           c.CustomerID,
                           c,
                           ocid = o.CustomerID,
                           o
                       })
                    .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public override void Applied_to_projection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       join o in context.Set<Order>().AsNoTracking()
                           on c.CustomerID equals o.CustomerID
                       where c.CustomerID == "ALFKI"
                       select new
                       {
                           c,
                           o
                       })
                    .AsNoTracking()
                    .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public override void Include_reference_and_collection()
        {
            using (var context = CreateContext())
            {
                var orders
                    = context.Set<Order>()
                        .Include(o => o.Customer)
                        .Include(o => o.OrderDetails)
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(830, orders.Count);
            }
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public override void Query_fast_path_when_ctor_binding()
        {
            using (var context = CreateContext())
            {
                var employees
                    = context.Set<Customer>()
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(91, employees.Count);
            }
        }

        [ConditionalFact]
        public override async Task Query_fast_path_when_ctor_binding_async()
        {
            using (var context = CreateContext())
            {
                var employees
                    = await context.Set<Customer>()
                        .AsNoTracking()
                        .ToListAsync();

                Assert.Equal(91, employees.Count);
            }
        }

        [ConditionalFact]
        public override void SelectMany_simple()
        {
            using (var context = CreateContext())
            {
                var results
                    = (from e in context.Set<Employee>()
                       from c in context.Set<Customer>()
                       select new
                       {
                           c,
                           e
                       })
                    .AsNoTracking()
                    .ToList();

                Assert.Equal(819, results.Count);
            }
        }
    }
}
