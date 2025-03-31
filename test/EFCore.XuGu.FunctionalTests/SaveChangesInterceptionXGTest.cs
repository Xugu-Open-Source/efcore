using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public abstract class SaveChangesInterceptionXGTestBase : SaveChangesInterceptionTestBase
    {
        protected SaveChangesInterceptionXGTestBase(InterceptionXGFixtureBase fixture)
            : base(fixture)
        {
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
