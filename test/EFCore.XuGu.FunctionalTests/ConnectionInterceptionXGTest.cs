using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using EntityFrameworkCore.XuGu.Tests;
using Xunit;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public abstract class ConnectionInterceptionXGTestBase : ConnectionInterceptionTestBase
    {
        protected ConnectionInterceptionXGTestBase(InterceptionXGFixtureBase fixture)
            : base(fixture)
        {
        }

        public abstract class InterceptionXGFixtureBase : InterceptionFixtureBase
        {
            protected override string StoreName => "ConnectionInterception";
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkXG(), injectedInterceptors);
        }

        protected override BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder)
            => new BadUniverseContext(optionsBuilder.UseXG(new FakeDbConnection(), AppConfig.ServerVersion).Options);

        public class FakeDbConnection : DbConnection
        {
            public override string ConnectionString { get; set; }
            public override string Database => "Database";
            public override string DataSource => "DataSource";
            public override string ServerVersion => throw new NotImplementedException();
            public override ConnectionState State => ConnectionState.Closed;
            public override void ChangeDatabase(string databaseName) => throw new NotImplementedException();
            public override void Close() => throw new NotImplementedException();
            public override void Open() => throw new NotImplementedException();
            protected override DbTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel) => throw new NotImplementedException();
            protected override DbCommand CreateDbCommand() => throw new NotImplementedException();
        }

        public class ConnectionInterceptionXGTest
            : ConnectionInterceptionXGTestBase, IClassFixture<ConnectionInterceptionXGTest.InterceptionXGFixture>
        {
            public ConnectionInterceptionXGTest(InterceptionXGFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionXGFixture : InterceptionXGFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => false;
            }
        }

        public class ConnectionInterceptionWithDiagnosticsXGTest
            : ConnectionInterceptionXGTestBase, IClassFixture<ConnectionInterceptionWithDiagnosticsXGTest.InterceptionXGFixture>
        {
            public ConnectionInterceptionWithDiagnosticsXGTest(InterceptionXGFixture fixture)
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
