using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using EntityFrameworkCore.XuGu.Storage.Internal;
using Xunit;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public abstract class SaveChangesInterceptionXGTestBase : SaveChangesInterceptionTestBase
    {
        protected SaveChangesInterceptionXGTestBase(InterceptionXGFixtureBase fixture)
            : base(fixture)
        {
        }
        [ConditionalTheory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public override async Task Intercept_SaveChanges_passively(bool async, bool inject, bool noAcceptChanges)
        {
            var id = new Random().Next();
            var (context, interceptor) = CreateContext<PassiveSaveChangesInterceptor>(inject);

            using var _ = context;

            var savingEventCalled = false;
            var resultFromEvent = 0;
            Exception exceptionFromEvent = null;

            context.SavingChanges += (sender, args) =>
            {
                Assert.Same(context, sender);
                savingEventCalled = true;
            };

            context.SavedChanges += (sender, args) =>
            {
                Assert.Same(context, sender);
                resultFromEvent = args.EntitiesSavedCount;
            };

            context.SaveChangesFailed += (sender, args) =>
            {
                Assert.Same(context, sender);
                exceptionFromEvent = args.Exception;
            };

            context.Add(new Singularity { Id = id, Type = "Red Dwarf" });

            using var transaction = context.Database.BeginTransaction();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

            var savedCount = noAcceptChanges
                ? async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges()
                : async
                    ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                    : context.SaveChanges(acceptAllChangesOnSuccess: false);

            Assert.Equal(1, savedCount);

            Assert.True(savingEventCalled);
            Assert.Equal(savedCount, resultFromEvent);
            Assert.Null(exceptionFromEvent);

            AssertNormalOutcome(context, interceptor, async);

            listener.AssertEventsInOrder(
                CoreEventId.SaveChangesStarting.Name,
                CoreEventId.SaveChangesCompleted.Name);

            Assert.Equal(1, context.Set<Singularity>().AsNoTracking().Count(e => e.Id == id));
        }

        //protected class PassiveSaveChangesInterceptor : SaveChangesInterceptorBase
        //{
        //}

        [ConditionalTheory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public override async Task Intercept_SaveChanges_to_suppress_save(bool async, bool inject, bool noAcceptChanges)
        {
            var id = new Random().Next();
            var (context, interceptor) = CreateContext<SuppressingSaveChangesInterceptor>(inject);

            using var _ = context;

            var savingEventCalled = false;
            var resultFromEvent = 0;
            Exception exceptionFromEvent = null;

            context.SavingChanges += (sender, args) =>
            {
                Assert.Same(context, sender);
                savingEventCalled = true;
            };

            context.SavedChanges += (sender, args) =>
            {
                Assert.Same(context, sender);
                resultFromEvent = args.EntitiesSavedCount;
            };

            context.SaveChangesFailed += (sender, args) =>
            {
                Assert.Same(context, sender);
                exceptionFromEvent = args.Exception;
            };

            context.Add(new Singularity { Id = id, Type = "Red Dwarf" });

            using var transaction = context.Database.BeginTransaction();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

            var savedCount = noAcceptChanges
                ? async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges()
                : async
                    ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                    : context.SaveChanges(acceptAllChangesOnSuccess: false);

            Assert.Equal(-1, savedCount);

            Assert.True(savingEventCalled);
            Assert.Equal(savedCount, resultFromEvent);
            Assert.Null(exceptionFromEvent);

            AssertNormalOutcome(context, interceptor, async);

            listener.AssertEventsInOrder(
                CoreEventId.SaveChangesStarting.Name,
                CoreEventId.SaveChangesCompleted.Name);

            Assert.Equal(0, context.Set<Singularity>().AsNoTracking().Count(e => e.Id == id));
        }
        [ConditionalTheory]
        [InlineData(false, false, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(true, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(true, false, true, false)]
        [InlineData(false, true, true, false)]
        [InlineData(true, true, true, false)]
        [InlineData(false, false, false, true)]
        [InlineData(true, false, false, true)]
        [InlineData(false, true, false, true)]
        [InlineData(true, true, false, true)]
        [InlineData(false, false, true, true)]
        [InlineData(true, false, true, true)]
        [InlineData(false, true, true, true)]
        [InlineData(true, true, true, true)]
        public override async Task Intercept_SaveChanges_failed(bool async, bool inject, bool noAcceptChanges, bool concurrencyError)
        {
            if (concurrencyError
                && !SupportsOptimisticConcurrency)
            {
                return;
            }
            var id = new Random().Next();
            var (context, interceptor) = CreateContext<PassiveSaveChangesInterceptor>(inject);

            using var _ = context;

            using var transaction = context.Database.BeginTransaction();

            if (!concurrencyError)
            {
                context.Add(new Singularity { Id = id, Type = "Red Dwarf" });
                var ___ = async ? await context.SaveChangesAsync() : context.SaveChanges();
                context.ChangeTracker.Clear();
            }

            var savingEventCalled = false;
            var resultFromEvent = -1;
            Exception exceptionFromEvent = null;

            context.SavingChanges += (sender, args) =>
            {
                Assert.Same(context, sender);
                savingEventCalled = true;
            };

            context.SavedChanges += (sender, args) =>
            {
                Assert.Same(context, sender);
                resultFromEvent = args.EntitiesSavedCount;
            };

            context.SaveChangesFailed += (sender, args) =>
            {
                Assert.Same(context, sender);
                exceptionFromEvent = args.Exception;
            };

            context.Entry(new Singularity { Id = id, Type = "Red Dwarf" }).State
                = concurrencyError ? EntityState.Modified : EntityState.Added;

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

            Exception thrown = null;

            try
            {
                var __ = noAcceptChanges
                    ? async
                        ? await context.SaveChangesAsync()
                        : context.SaveChanges()
                    : async
                        ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                        : context.SaveChanges(acceptAllChangesOnSuccess: false);
            }
            catch (Exception e)
            {
                thrown = e;
            }

            Assert.Equal(async, interceptor.AsyncCalled);
            Assert.NotEqual(async, interceptor.SyncCalled);
            Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
            Assert.True(interceptor.FailedCalled);
            Assert.Same(context, interceptor.Context);
            Assert.Same(thrown, interceptor.Exception);

            Assert.True(savingEventCalled);
            Assert.Equal(-1, resultFromEvent);
            Assert.Same(thrown, exceptionFromEvent);

            if (concurrencyError)
            {
                listener.AssertEventsInOrder(
                    CoreEventId.SaveChangesStarting.Name,
                    CoreEventId.OptimisticConcurrencyException.Name);
            }
            else
            {
                listener.AssertEventsInOrder(
                    CoreEventId.SaveChangesStarting.Name,
                    CoreEventId.SaveChangesFailed.Name);
            }
        }

        [ConditionalTheory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public override async Task Intercept_connection_with_multiple_interceptors(bool async, bool inject, bool noAcceptChanges)
        {
            var interceptor1 = new PassiveSaveChangesInterceptor();
            var interceptor2 = new ResultMutatingSaveChangesInterceptor();
            var interceptor3 = new ResultMutatingSaveChangesInterceptor();
            var interceptor4 = new PassiveSaveChangesInterceptor();
            var id = new Random().Next();
            using var context = CreateContext(
                new IInterceptor[] { new PassiveSaveChangesInterceptor(), interceptor1, interceptor2 },
                new IInterceptor[] { interceptor3, interceptor4, new PassiveSaveChangesInterceptor() });

            context.Add(new Singularity { Id = id, Type = "Red Dwarf" });

            using var transaction = context.Database.BeginTransaction();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

            var savedCount = noAcceptChanges
                ? async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges()
                : async
                    ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                    : context.SaveChanges(acceptAllChangesOnSuccess: false);

            Assert.Equal(777, savedCount);

            AssertNormalOutcome(context, interceptor1, async);
            AssertNormalOutcome(context, interceptor2, async);
            AssertNormalOutcome(context, interceptor3, async);
            AssertNormalOutcome(context, interceptor4, async);

            listener.AssertEventsInOrder(
                CoreEventId.SaveChangesStarting.Name,
                CoreEventId.SaveChangesCompleted.Name);

            Assert.Equal(1, context.Set<Singularity>().AsNoTracking().Count(e => e.Id == id));
        }
        [ConditionalTheory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public override async Task Intercept_SaveChanges_to_change_result(bool async, bool inject, bool noAcceptChanges)
        {
            var id = new Random().Next();
            var (context, interceptor) = CreateContext<ResultMutatingSaveChangesInterceptor>(inject);

            using var _ = context;

            var savingEventCalled = false;
            var resultFromEvent = 0;
            Exception exceptionFromEvent = null;

            context.SavingChanges += (sender, args) =>
            {
                Assert.Same(context, sender);
                savingEventCalled = true;
            };

            context.SavedChanges += (sender, args) =>
            {
                Assert.Same(context, sender);
                resultFromEvent = args.EntitiesSavedCount;
            };

            context.SaveChangesFailed += (sender, args) =>
            {
                Assert.Same(context, sender);
                exceptionFromEvent = args.Exception;
            };

            context.Add(new Singularity { Id = id, Type = "Red Dwarf" });

            using var transaction = context.Database.BeginTransaction();

            using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

            var savedCount = noAcceptChanges
                ? async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges()
                : async
                    ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                    : context.SaveChanges(acceptAllChangesOnSuccess: false);

            Assert.Equal(777, savedCount);

            Assert.True(savingEventCalled);
            Assert.Equal(savedCount, resultFromEvent);
            Assert.Null(exceptionFromEvent);

            AssertNormalOutcome(context, interceptor, async);

            listener.AssertEventsInOrder(
                CoreEventId.SaveChangesStarting.Name,
                CoreEventId.SaveChangesCompleted.Name);

            Assert.Equal(1, context.Set<Singularity>().AsNoTracking().Count(e => e.Id == id));
        }
        private static void AssertNormalOutcome(DbContext context, SaveChangesInterceptorBase interceptor, bool async)
        {
            Assert.Equal(async, interceptor.AsyncCalled);
            Assert.NotEqual(async, interceptor.SyncCalled);
            Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
            Assert.False(interceptor.FailedCalled);
            Assert.Same(context, interceptor.Context);
        }
        public abstract class InterceptionXGFixtureBase : InterceptionFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => XGTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkXG(), injectedInterceptors);
        }

        public class SaveChangesInterceptionXGTest
            : SaveChangesInterceptionXGTestBase, IClassFixture<SaveChangesInterceptionXGTest.InterceptionXGFixture>
        {
            public SaveChangesInterceptionXGTest(InterceptionXGFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionXGFixture : InterceptionXGFixtureBase
            {
                protected override string StoreName
                    => "SaveChangesInterception";

                protected override bool ShouldSubscribeToDiagnosticListener
                    => false;

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                {
                    new XGDbContextOptionsBuilder(base.AddOptions(builder))
                        .ExecutionStrategy(d => new XGExecutionStrategy(d));
                    return builder;
                }
            }
        }

        public class SaveChangesInterceptionWithDiagnosticsXGTest
            : SaveChangesInterceptionXGTestBase,
                IClassFixture<SaveChangesInterceptionWithDiagnosticsXGTest.InterceptionXGFixture>
        {
            public SaveChangesInterceptionWithDiagnosticsXGTest(InterceptionXGFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionXGFixture : InterceptionXGFixtureBase
            {
                protected override string StoreName => "SaveChangesInterceptionWithDiagnostics";

                protected override bool ShouldSubscribeToDiagnosticListener
                    => true;

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                {
                    new XGDbContextOptionsBuilder(base.AddOptions(builder))
                        .ExecutionStrategy(d => new XGExecutionStrategy(d));
                    return builder;
                }
            }
        }
    }
}
