using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Xunit;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public abstract class CommandInterceptionXGTestBase : CommandInterceptionTestBase
    {
        protected CommandInterceptionXGTestBase(InterceptionXGFixtureBase fixture)
            : base(fixture)
        {
        }

        public abstract class InterceptionXGFixtureBase : InterceptionFixtureBase
        {
            protected override string StoreName => "CommandInterception";
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkXG(), injectedInterceptors);
        }

        // Made internal to skip all tests.
        internal class CommandInterceptionXGTest
            : CommandInterceptionXGTestBase, IClassFixture<CommandInterceptionXGTest.InterceptionXGFixture>
        {
            public CommandInterceptionXGTest(InterceptionXGFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionXGFixture : InterceptionXGFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => false;
            }
        }

        // Made internal to skip all tests.
        internal class CommandInterceptionWithDiagnosticsXGTest
            : CommandInterceptionXGTestBase, IClassFixture<CommandInterceptionWithDiagnosticsXGTest.InterceptionXGFixture>
        {
            public CommandInterceptionWithDiagnosticsXGTest(InterceptionXGFixture fixture)
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
