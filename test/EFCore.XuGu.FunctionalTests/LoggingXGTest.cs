using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Tests;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    // TODO: Reenable once this issue has been fixed in EF Core upstream.
    // Skip because LoggingTestBase uses the wrong order:
    // Wrong:   DefaultOptions + "NoTracking"
    // Correct: "NoTracking" + DefaultOptions
    // The order in LoggingRelationalTestBase<,> is correct though.
    internal class LoggingXGTest : LoggingRelationalTestBase<XGDbContextOptionsBuilder, XGOptionsExtension>
    {
        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            IServiceCollection services,
            Action<RelationalDbContextOptionsBuilder<XGDbContextOptionsBuilder, XGOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder()
                .UseInternalServiceProvider(services.AddEntityFrameworkXG().BuildServiceProvider(validateScopes: true))
                .UseXG("Database=DummyDatabase", AppConfig.ServerVersion, relationalAction);

        protected override TestLogger CreateTestLogger()
            => new TestLogger<XGLoggingDefinitions>();

        protected override string ProviderName => "Microsoft.EntityFrameworkCore.XuGu";

        protected override string ProviderVersion => typeof(XGOptionsExtension).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        protected override string DefaultOptions => $"ServerVersion {AppConfig.ServerVersion} ";
    }
}
