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

public class MaterializationInterceptionXGTest : MaterializationInterceptionTestBase,
    IClassFixture<MaterializationInterceptionXGTest.MaterializationInterceptionXGFixture>
{
    public MaterializationInterceptionXGTest(MaterializationInterceptionXGFixture fixture)
        : base(fixture)
    {
    }

    public class MaterializationInterceptionXGFixture : SingletonInterceptorsFixtureBase
    {
        protected override string StoreName
            => "MaterializationInterception";

        protected override ITestStoreFactory TestStoreFactory
            => XGTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkXG(), injectedInterceptors);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new XGDbContextOptionsBuilder(base.AddOptions(builder))
                .ExecutionStrategy(d => new XGExecutionStrategy(d));
            return builder;
        }
    }
}
