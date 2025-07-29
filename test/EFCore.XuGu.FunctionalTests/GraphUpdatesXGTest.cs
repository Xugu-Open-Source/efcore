using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public abstract class GraphUpdatesXGTest
    {
        public abstract class GraphUpdatesXGTestBase<TFixture> : GraphUpdatesTestBase<TFixture>
            where TFixture : GraphUpdatesXGTestBase<TFixture>.GraphUpdatesXGFixtureBase, new()
        {
            protected GraphUpdatesXGTestBase(TFixture fixture)
                : base(fixture)
            {
            }

            protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
                => facade.UseTransaction(transaction.GetDbTransaction());

            public abstract class GraphUpdatesXGFixtureBase : GraphUpdatesFixtureBase
            {
                public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
                protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
                //protected virtual bool AutoDetectChanges => false;

                //public override DbContext CreateContext()
                //{
                //    var context = base.CreateContext();
                //    context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;

                //    return context;
                //}
            }
        }

        public class SnapshotNotifications
            : GraphUpdatesXGTestBase<SnapshotNotifications.SnapshotNotificationsFixture>
        {
            public SnapshotNotifications(SnapshotNotificationsFixture fixture, ITestOutputHelper testOutputHelper)
                : base(fixture)
            {
                //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
            }

            public class SnapshotNotificationsFixture : GraphUpdatesXGFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesSnapshotTest";
                //protected override bool AutoDetectChanges => true;

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class ChangedNotifications
            : GraphUpdatesXGTestBase<ChangedNotifications.ChangedNotificationsFixture>
        {
            public ChangedNotifications(ChangedNotificationsFixture fixture)
                : base(fixture)
            {
            }

            public class ChangedNotificationsFixture : GraphUpdatesXGFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesChangedTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class ChangedChangingNotifications
            : GraphUpdatesXGTestBase<ChangedChangingNotifications.ChangedChangingNotificationsFixture>
        {
            public ChangedChangingNotifications(ChangedChangingNotificationsFixture fixture)
                : base(fixture)
            {
            }

            public class ChangedChangingNotificationsFixture : GraphUpdatesXGFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesFullTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class FullWithOriginalsNotifications
            : GraphUpdatesXGTestBase<FullWithOriginalsNotifications.FullWithOriginalsNotificationsFixture>
        {
            public FullWithOriginalsNotifications(FullWithOriginalsNotificationsFixture fixture)
                : base(fixture)
            {
            }

            public class FullWithOriginalsNotificationsFixture : GraphUpdatesXGFixtureBase
            {
                protected override string StoreName { get; } = "GraphUpdatesOriginalsTest";

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }
    }
}
