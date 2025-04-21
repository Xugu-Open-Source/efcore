using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public abstract class ProxyGraphUpdatesXGTest
    {
        public abstract class ProxyGraphUpdatesXGTestBase<TFixture> : ProxyGraphUpdatesTestBase<TFixture>
            where TFixture : ProxyGraphUpdatesXGTestBase<TFixture>.ProxyGraphUpdatesXGFixtureBase, new()
        {
            protected ProxyGraphUpdatesXGTestBase(TFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public abstract class ProxyGraphUpdatesXGFixtureBase : ProxyGraphUpdatesFixtureBase
            {
                public TestSqlLoggerFactory TestSqlLoggerFactory
                    => (TestSqlLoggerFactory)ListLoggerFactory;

                protected override ITestStoreFactory TestStoreFactory
                    => XGTestStoreFactory.Instance;
            }

            [ConditionalFact]
            public override void Save_two_entity_cycle_with_lazy_loading()
    => ExecuteWithStrategyInTransaction(
        context =>
        {
            context.AddRange(
                context.CreateProxy<Car>(
                    car =>
                    {
                        car.Owner = context.CreateProxy<Person>();
                        car.Id = Guid.NewGuid();
                    }),
                context.CreateProxy<Car>(
                    car =>
                    {
                        car.Owner = context.CreateProxy<Person>();
                        car.Id = Guid.NewGuid();
                    }));

            context.SaveChanges();
        },
        context =>
        {
            var cars = context.Set<Car>().ToList();

            var owner0 = cars[0].Owner;
            var owner1 = cars[1].Owner;

            (cars[1].Owner, cars[0].Owner) = (cars[0].Owner, cars[1].Owner);

            cars[0].Owner.Vehicle = cars[0];
            cars[1].Owner.Vehicle = cars[1];

            if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                context.SaveChanges();
                Assert.Same(owner0, cars[1].Owner);
                Assert.Same(owner1, cars[0].Owner);
                Assert.Same(cars[0], cars[0].Owner.Vehicle);
                Assert.Same(cars[1], cars[1].Owner.Vehicle);
            }
            else
            {
                var message = Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message;
                Assert.StartsWith(CoreStrings.CircularDependency("").Substring(0, 30), message);
            }
        });

            [ConditionalFact]
            public override void Can_use_record_proxies_with_base_types_to_load_reference()
                => ExecuteWithStrategyInTransaction(
                    context =>
                    {
                        context.AddRange(
                            context.CreateProxy<RecordCar>(
                                car =>
                                {
                                    car.Owner = context.CreateProxy<RecordPerson>(new RecordPerson { Id = 1 });
                                }));

                        context.SaveChanges();
                    },
                    context =>
                    {
                        var car = context.Set<RecordCar>().Single();
                        if (!DoesLazyLoading)
                        {
                            context.Entry(car).Reference(e => e.Owner).Load();
                        }

                        Assert.Equal(car.Owner.Id, car.OwnerId);
                        Assert.Same(car, car.Owner.Vehicles.Single());
                    });

            [ConditionalFact]
            public override void Can_use_record_proxies_with_base_types_to_load_collection()
                => ExecuteWithStrategyInTransaction(
                    context =>
                    {
                        context.AddRange(
                            context.CreateProxy<RecordCar>(
                                car =>
                                {
                                    car.Owner = context.CreateProxy<RecordPerson>(new RecordPerson { Id = 1 });
                                }));

                        context.SaveChanges();
                    },
                    context =>
                    {
                        var owner = context.Set<RecordPerson>().Single();
                        if (!DoesLazyLoading)
                        {
                            context.Entry(owner).Collection(e => e.Vehicles).Load();
                        }

                        Assert.Equal(owner.Id, owner.Vehicles.Single().Id);
                        Assert.Same(owner, owner.Vehicles.Single().Owner);
                    });

            [ConditionalFact]
            public override void Avoid_nulling_shared_FK_property_when_deleting()
            {
                //Fixture.ResetDatabase();
                ExecuteWithStrategyInTransaction(
                    context =>
                    {
                        var root = context
                            .Set<SharedFkRoot>()
                            .Include(e => e.Parents)
                            .Include(e => e.Dependants)
                            .Single();

                        Assert.Equal(3, context.ChangeTracker.Entries().Count());

                        var dependent = root.Dependants.Single();
                        var parent = root.Parents.Single();

                        Assert.Same(root, dependent.Root);
                        Assert.Same(parent, dependent.Parent);
                        Assert.Same(root, parent.Root);
                        Assert.Same(dependent, parent.Dependant);

                        Assert.Equal(root.Id, dependent.RootId);
                        Assert.Equal(root.Id, parent.RootId);
                        Assert.Equal(dependent.Id, parent.DependantId);

                        context.Remove(dependent);

                        Assert.Equal(3, context.ChangeTracker.Entries().Count());

                        Assert.Equal(EntityState.Unchanged, context.Entry(root).State);
                        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
                        Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);

                        Assert.Same(root, dependent.Root);
                        Assert.Same(parent, dependent.Parent);
                        Assert.Same(root, parent.Root);
                        Assert.Null(parent.Dependant);

                        Assert.Equal(root.Id, dependent.RootId);
                        Assert.Equal(root.Id, parent.RootId);
                        Assert.Null(parent.DependantId);

                        context.SaveChanges();

                        Assert.Equal(2, context.ChangeTracker.Entries().Count());

                        Assert.Equal(EntityState.Unchanged, context.Entry(root).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
                        Assert.Equal(EntityState.Detached, context.Entry(dependent).State);

                        Assert.Same(root, dependent.Root);
                        Assert.Same(parent, dependent.Parent);
                        Assert.Same(root, parent.Root);
                        Assert.Null(parent.Dependant);

                        Assert.Equal(root.Id, dependent.RootId);
                        Assert.Equal(root.Id, parent.RootId);
                        Assert.Null(parent.DependantId);
                    },
                    context =>
                    {
                        var root = context
                            .Set<SharedFkRoot>()
                            .Include(e => e.Parents)
                            .Include(e => e.Dependants)
                            .Single();

                        Assert.Equal(2, context.ChangeTracker.Entries().Count());

                        Assert.Empty(root.Dependants);
                        var parent = root.Parents.Single();

                        Assert.Same(root, parent.Root);
                        Assert.Null(parent.Dependant);

                        Assert.Equal(root.Id, parent.RootId);
                        Assert.Null(parent.DependantId);
                    });
            }

            [ConditionalTheory]
            [InlineData(false)]
            [InlineData(true)]
            public override void Avoid_nulling_shared_FK_property_when_nulling_navigation(bool nullPrincipal)
                => ExecuteWithStrategyInTransaction(
                    context =>
                    {
                        var root = context
                            .Set<SharedFkRoot>()
                            .Include(e => e.Parents)
                            .Include(e => e.Dependants)
                            .Single();

                        Assert.Equal(3, context.ChangeTracker.Entries().Count());

                        var dependent = root.Dependants.Single();
                        var parent = root.Parents.Single();

                        Assert.Same(root, dependent.Root);
                        Assert.Same(parent, dependent.Parent);
                        Assert.Same(root, parent.Root);
                        Assert.Same(dependent, parent.Dependant);

                        Assert.Equal(root.Id, dependent.RootId);
                        Assert.Equal(root.Id, parent.RootId);
                        Assert.Equal(dependent.Id, parent.DependantId);

                        if (nullPrincipal)
                        {
                            dependent.Parent = null;
                        }
                        else
                        {
                            parent.Dependant = null;
                        }

                        Assert.Equal(3, context.ChangeTracker.Entries().Count());

                        Assert.Equal(EntityState.Unchanged, context.Entry(root).State);
                        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);

                        Assert.Same(root, dependent.Root);
                        Assert.Null(dependent.Parent);
                        Assert.Same(root, parent.Root);
                        Assert.Null(parent.Dependant);

                        Assert.Equal(root.Id, dependent.RootId);
                        Assert.Equal(root.Id, parent.RootId);
                        Assert.Null(parent.DependantId);

                        context.SaveChanges();

                        Assert.Equal(3, context.ChangeTracker.Entries().Count());

                        Assert.Same(root, dependent.Root);
                        Assert.Null(dependent.Parent);
                        Assert.Same(root, parent.Root);
                        Assert.Null(parent.Dependant);

                        Assert.Equal(root.Id, dependent.RootId);
                        Assert.Equal(root.Id, parent.RootId);
                        Assert.Null(parent.DependantId);
                    },
                    context =>
                    {
                        var root = context
                            .Set<SharedFkRoot>()
                            .Include(e => e.Parents)
                            .Include(e => e.Dependants)
                            .Single();

                        Assert.Equal(3, context.ChangeTracker.Entries().Count());

                        var dependent = root.Dependants.Single();
                        var parent = root.Parents.Single();

                        Assert.Same(root, dependent.Root);
                        Assert.Null(dependent.Parent);
                        Assert.Same(root, parent.Root);
                        Assert.Null(parent.Dependant);

                        Assert.Equal(root.Id, dependent.RootId);
                        Assert.Equal(root.Id, parent.RootId);
                        Assert.Null(parent.DependantId);
                    });

            [ConditionalFact]
            public override void No_fixup_to_Deleted_entities()
            {
                using var context = CreateContext();

                var root = LoadRoot(context);
                if (!DoesLazyLoading)
                {
                    context.Entry(root).Collection(e => e.OptionalChildren).Load();
                }

                var existing = root.OptionalChildren.OrderBy(e => e.Id).First();

                existing.Parent = null;
                existing.ParentId = null;
                ((ICollection<Optional1>)root.OptionalChildren).Remove(existing);

                context.Entry(existing).State = EntityState.Deleted;

                var queried = context.Set<Optional1>().ToList();

                Assert.Null(existing.Parent);
                Assert.Null(existing.ParentId);
                Assert.Single(root.OptionalChildren);
                Assert.DoesNotContain(existing, root.OptionalChildren);

                Assert.Equal(2, queried.Count);
                Assert.Contains(existing, queried);
            }

            [ConditionalFact]
            public override void Sometimes_not_calling_DetectChanges_when_required_does_not_throw_for_null_ref()
                => ExecuteWithStrategyInTransaction(
                    context =>
                    {
                        var dependent = context.Set<BadOrder>().Single();

                        dependent.BadCustomerId = null;

                        var principal = context.Set<BadCustomer>().Single();

                        principal.Status++;

                        Assert.Null(dependent.BadCustomerId);
                        Assert.Null(dependent.BadCustomer);
                        Assert.Empty(principal.BadOrders);

                        context.SaveChanges();

                        Assert.Null(dependent.BadCustomerId);
                        Assert.Null(dependent.BadCustomer);
                        Assert.Empty(principal.BadOrders);
                    },
                    context =>
                    {
                        var dependent = context.Set<BadOrder>().Single();
                        var principal = context.Set<BadCustomer>().Single();

                        Assert.Null(dependent.BadCustomerId);
                        Assert.Null(dependent.BadCustomer);
                        Assert.Empty(principal.BadOrders);
                    });
        }


        public class LazyLoading : ProxyGraphUpdatesXGTestBase<LazyLoading.ProxyGraphUpdatesWithLazyLoadingXGFixture>
        {
            public LazyLoading(ProxyGraphUpdatesWithLazyLoadingXGFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading
                => true;

            protected override bool DoesChangeTracking
                => false;

            public class ProxyGraphUpdatesWithLazyLoadingXGFixture : ProxyGraphUpdatesXGFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphLazyLoadingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseLazyLoadingProxies());

                protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                    => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
            }
        }

        public class ChangeTracking : ProxyGraphUpdatesXGTestBase<ChangeTracking.ProxyGraphUpdatesWithChangeTrackingXGFixture>
        {
            public ChangeTracking(ProxyGraphUpdatesWithChangeTrackingXGFixture fixture)
                : base(fixture)
            {
            }

            // Needs lazy loading
            public override void Save_two_entity_cycle_with_lazy_loading()
            {
            }

            protected override bool DoesLazyLoading
                => false;

            protected override bool DoesChangeTracking
                => true;

            public class ProxyGraphUpdatesWithChangeTrackingXGFixture : ProxyGraphUpdatesXGFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphChangeTrackingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseChangeTrackingProxies());

                protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                    => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
            }
        }

        public class ChangeTrackingAndLazyLoading : ProxyGraphUpdatesXGTestBase<
            ChangeTrackingAndLazyLoading.ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingXGFixture>
        {
            public ChangeTrackingAndLazyLoading(ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingXGFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading
                => true;

            protected override bool DoesChangeTracking
                => true;

            public class ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingXGFixture : ProxyGraphUpdatesXGFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphChangeTrackingAndLazyLoadingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseLazyLoadingProxies().UseChangeTrackingProxies());

                protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                    => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
            }
        }
    }
}
