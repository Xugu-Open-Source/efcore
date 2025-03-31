using System;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Tests;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class TransactionXGTest : TransactionTestBase<TransactionXGTest.TransactionXGFixture>
    {
        public TransactionXGTest(TransactionXGFixture fixture)
            : base(fixture)
        {
        }

        protected override bool SnapshotSupported => false;
        protected override bool AmbientTransactionsSupported => true;
        protected override bool DirtyReadsOccur => false;

        protected override DbContext CreateContextWithConnectionString()
        {
            var options = Fixture.AddOptions(
                    new DbContextOptionsBuilder()
                        .UseXG(
                            TestStore.ConnectionString,
                            AppConfig.ServerVersion,
                            b => XGTestStore.AddOptions(b).ExecutionStrategy(c => new XGExecutionStrategy(c))))
                .UseInternalServiceProvider(Fixture.ServiceProvider);

            return new DbContext(options.Options);
        }

        public class TransactionXGFixture : TransactionFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            public override void Reseed()
            {
                using var context = CreateContext();
                context.Set<TransactionCustomer>().RemoveRange(context.Set<TransactionCustomer>());
                context.Set<TransactionOrder>().RemoveRange(context.Set<TransactionOrder>());
                context.SaveChanges();

                base.Seed(context);
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                new XGDbContextOptionsBuilder(
                        base.AddOptions(builder))
                    .ExecutionStrategy(c => new XGExecutionStrategy(c));
                return builder;
            }

            // public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            // {
            //     new XGDbContextOptionsBuilder(base.AddOptions(builder))
            //         .MaxBatchSize(1);
            //     return builder;
            // }
        }
    }
}
