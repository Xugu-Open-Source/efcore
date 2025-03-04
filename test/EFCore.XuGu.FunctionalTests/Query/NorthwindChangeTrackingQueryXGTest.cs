using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Linq;
using Xunit;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class NorthwindChangeTrackingQueryXGTest : NorthwindChangeTrackingQueryTestBase<
        NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public NorthwindChangeTrackingQueryXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override NorthwindContext CreateNoTrackingContext()
            => new NorthwindRelationalContext(
                new DbContextOptionsBuilder(Fixture.CreateOptions())
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

        [ConditionalFact]
        public override void Entity_range_does_not_revert_when_attached_dbContext()
        {
            using var context = CreateContext();
            var customers = context.Customers.OrderBy(c => c.CustomerID).Take(2);

            var customer0 = customers.First();
            var customer1 = customers.Skip(1).First();

            var trackedEntity0 = context.ChangeTracker.Entries<Customer>().First();
            var trackedEntity1 = context.ChangeTracker.Entries<Customer>().First();

            Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
            Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
            Assert.NotEqual("425-882-8080", customer0.Phone);
            Assert.NotEqual("425-882-8080", customer1.Phone);
            Assert.NotEqual("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
            Assert.NotEqual("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);

            customer0.Phone = "425-882-8080";
            customer1.Phone = "425-882-8080";
            context.ChangeTracker.DetectChanges();

            Assert.Equal(EntityState.Modified, trackedEntity0.State);
            Assert.Equal(EntityState.Modified, trackedEntity1.State);

            context.AttachRange(customers);

            Assert.Equal(customer0.CustomerID, trackedEntity0.Property(c => c.CustomerID).CurrentValue);
            Assert.Equal(customer1.CustomerID, trackedEntity1.Property(c => c.CustomerID).CurrentValue);
            Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
            Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
            Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).CurrentValue);
            Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).CurrentValue);
            Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
            Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);
        }
        [ConditionalFact]
        public override void Entity_range_does_not_revert_when_attached_dbSet()
        {
            using var context = CreateContext();
            var customers = context.Customers.OrderBy(c => c.CustomerID).Take(2);

            var customer0 = customers.First();
            var customer1 = customers.Skip(1).First();

            var trackedEntity0 = context.ChangeTracker.Entries<Customer>().First();
            var trackedEntity1 = context.ChangeTracker.Entries<Customer>().First();

            Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
            Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
            Assert.NotEqual("425-882-8080", customer0.Phone);
            Assert.NotEqual("425-882-8080", customer1.Phone);
            Assert.NotEqual("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
            Assert.NotEqual("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);

            customer0.Phone = "425-882-8080";
            customer1.Phone = "425-882-8080";
            context.ChangeTracker.DetectChanges();

            Assert.Equal(EntityState.Modified, trackedEntity0.State);
            Assert.Equal(EntityState.Modified, trackedEntity1.State);

            context.Customers.AttachRange(customers);

            Assert.Equal(customer0.CustomerID, trackedEntity0.Property(c => c.CustomerID).CurrentValue);
            Assert.Equal(customer1.CustomerID, trackedEntity1.Property(c => c.CustomerID).CurrentValue);
            Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
            Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
            Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).CurrentValue);
            Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).CurrentValue);
            Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
            Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);
        }
    }
}
