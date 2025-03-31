using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests;

public abstract class QueryExpressionInterceptionXGTestBase : QueryExpressionInterceptionTestBase
{
    protected QueryExpressionInterceptionXGTestBase(InterceptionXGFixtureBase fixture)
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

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new XGDbContextOptionsBuilder(base.AddOptions(builder))
                .ExecutionStrategy(d => new XGExecutionStrategy(d));
            return builder;
        }
    }

    public class QueryExpressionInterceptionXGTest
        : QueryExpressionInterceptionXGTestBase, IClassFixture<QueryExpressionInterceptionXGTest.InterceptionXGFixture>
    {
        public QueryExpressionInterceptionXGTest(InterceptionXGFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionXGFixture : InterceptionXGFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterception";

            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class QueryExpressionInterceptionWithDiagnosticsXGTest
        : QueryExpressionInterceptionXGTestBase,
            IClassFixture<QueryExpressionInterceptionWithDiagnosticsXGTest.InterceptionXGFixture>
    {
        public QueryExpressionInterceptionWithDiagnosticsXGTest(InterceptionXGFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionXGFixture : InterceptionXGFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterceptionWithDiagnostics";

            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
