using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

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
