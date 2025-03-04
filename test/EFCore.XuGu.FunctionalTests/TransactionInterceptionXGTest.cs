using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Xunit;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public abstract class TransactionInterceptionXGTestBase : TransactionInterceptionTestBase
    {
        protected TransactionInterceptionXGTestBase(InterceptionXGFixtureBase fixture)
            : base(fixture)
        {
        }

        public abstract class InterceptionXGFixtureBase : InterceptionFixtureBase
        {
            protected override string StoreName => "TransactionInterception";
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkXG(), injectedInterceptors);
        }

        // Made internal to skip all tests.
        internal class TransactionInterceptionXGTest
            : TransactionInterceptionXGTestBase, IClassFixture<TransactionInterceptionXGTest.InterceptionXGFixture>
        {
            public TransactionInterceptionXGTest(InterceptionXGFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionXGFixture : InterceptionXGFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => false;
            }
        }

        // Made internal to skip all tests.
        internal class TransactionInterceptionWithDiagnosticsXGTest
            : TransactionInterceptionXGTestBase, IClassFixture<TransactionInterceptionWithDiagnosticsXGTest.InterceptionXGFixture>
        {
            public TransactionInterceptionWithDiagnosticsXGTest(InterceptionXGFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionXGFixture : InterceptionXGFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => true;
            }
        }
    }
}
