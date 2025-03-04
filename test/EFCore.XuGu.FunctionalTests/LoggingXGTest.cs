using System;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using EntityFrameworkCore.XuGu.Tests;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class LoggingXGTest : LoggingRelationalTestBase<XGDbContextOptionsBuilder, XGOptionsExtension>
    {
        protected override DbContextOptionsBuilder CreateOptionsBuilder(
            IServiceCollection services,
            Action<RelationalDbContextOptionsBuilder<XGDbContextOptionsBuilder, XGOptionsExtension>> relationalAction)
            => new DbContextOptionsBuilder()
                .UseInternalServiceProvider(services.AddEntityFrameworkXG().BuildServiceProvider())
                .UseXG("Database=DummyDatabase", AppConfig.ServerVersion, relationalAction);

        protected override string ProviderName => "EntityFrameworkCore.XuGu";
        protected override string DefaultOptions => $"ServerVersion {AppConfig.ServerVersion}";
    }
}
