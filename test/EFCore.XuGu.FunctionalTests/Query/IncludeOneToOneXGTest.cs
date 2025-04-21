using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class IncludeOneToOneXGTest : IncludeOneToOneTestBase<IncludeOneToOneXGTest.OneToOneQueryXGFixture>
    {
        public IncludeOneToOneXGTest(OneToOneQueryXGFixture fixture)
            : base(fixture)
        {
        }

        public class OneToOneQueryXGFixture : OneToOneQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory =>
                (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }
        [ConditionalFact]
        public override void Include_address()
        {
            using var context = CreateContext();
            var people
                = context.Set<Person>()
                    .Include(p => p.Address)
                    .ToList();

            Assert.Equal(4, people.Count);
            Assert.Equal(3, people.Count(p => p.Address != null));
            Assert.Equal(4 + 3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalFact]
        public override void Include_address_EF_Property()
        {
            using var context = CreateContext();
            var people
                = context.Set<Person>()
                    .Include(p => EF.Property<Person>(p, "Address"))
                    .ToList();

            Assert.Equal(4, people.Count);
            Assert.Equal(3, people.Count(p => p.Address != null));
            Assert.Equal(4 + 3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalFact]
        public override void Include_address_shadow()
        {
            using var context = CreateContext();
            var people
                = context.Set<Person2>()
                    .Include(p => p.Address)
                    .ToList();

            Assert.Equal(3, people.Count);
            Assert.True(people.All(p => p.Address != null));
            Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalFact]
        public override void Include_address_no_tracking()
        {
            using var context = CreateContext();
            var people
                = context.Set<Person>()
                    .Include(p => p.Address)
                    .AsNoTracking()
                    .ToList();

            Assert.Equal(4, people.Count);
            Assert.Equal(3, people.Count(p => p.Address != null));
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalFact]
        public override void Include_address_no_tracking_EF_Property()
        {
            using var context = CreateContext();
            var people
                = context.Set<Person>()
                    .Include(p => EF.Property<Person>(p, "Address"))
                    .AsNoTracking()
                    .ToList();

            Assert.Equal(4, people.Count);
            Assert.Equal(3, people.Count(p => p.Address != null));
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalFact]
        public override void Include_person()
        {
            using var context = CreateContext();
            var addresses
                = context.Set<Address>()
                    .Include(a => a.Resident)
                    .ToList();

            Assert.Equal(3, addresses.Count);
            Assert.True(addresses.All(p => p.Resident != null));
            Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalFact]
        public override void Include_person_EF_Property()
        {
            using var context = CreateContext();
            var addresses
                = context.Set<Address>()
                    .Include(a => EF.Property<Address>(a, "Resident"))
                    .ToList();

            Assert.Equal(3, addresses.Count);
            Assert.True(addresses.All(p => p.Resident != null));
            Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalFact]
        public override void Include_person_shadow()
        {
            using var context = CreateContext();
            var addresses
                = context.Set<Address2>()
                    .Include(a => a.Resident)
                    .ToList();

            Assert.Equal(3, addresses.Count);
            Assert.True(addresses.All(p => p.Resident != null));
            Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalFact]
        public override void Include_person_no_tracking()
        {
            using var context = CreateContext();
            var addresses
                = context.Set<Address>()
                    .Include(a => a.Resident)
                    .AsNoTracking()
                    .ToList();

            Assert.Equal(3, addresses.Count);
            Assert.True(addresses.All(p => p.Resident != null));
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalFact]
        public override void Include_person_no_tracking_EF_Property()
        {
            using var context = CreateContext();
            var addresses
                = context.Set<Address>()
                    .Include(a => EF.Property<Address>(a, "Resident"))
                    .AsNoTracking()
                    .ToList();

            Assert.Equal(3, addresses.Count);
            Assert.True(addresses.All(p => p.Resident != null));
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalFact]
        public override void Include_address_when_person_already_tracked()
        {
            using var context = CreateContext();
            var person
                = context.Set<Person>()
                    .Single(p => p.Name == "John Snow");

            var people
                = context.Set<Person>()
                    .Include(p => p.Address)
                    .ToList();

            Assert.Equal(4, people.Count);
            Assert.Contains(person, people);
            Assert.Equal(3, people.Count(p => p.Address != null));
            Assert.Equal(4 + 3, context.ChangeTracker.Entries().Count());
        }

        [ConditionalFact]
        public override void Include_person_when_address_already_tracked()
        {
            using var context = CreateContext();
            var address
                = context.Set<Address>()
                    .Single(a => a.City == "Meereen");

            var addresses
                = context.Set<Address>()
                    .Include(a => a.Resident)
                    .ToList();

            Assert.Equal(3, addresses.Count);
            Assert.Contains(address, addresses);
            Assert.True(addresses.All(p => p.Resident != null));
            Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
        }
    }
}
