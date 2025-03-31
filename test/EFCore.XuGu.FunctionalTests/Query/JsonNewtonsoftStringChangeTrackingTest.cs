using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Extensions.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    [SupportedServerVersionCondition(nameof(ServerVersionSupport.Json))]
    public class JsonNewtonsoftStringChangeTrackingTest : JsonStringChangeTrackingTestBase<JsonNewtonsoftStringChangeTrackingTest.JsonNewtonsoftStringChangeTrackingFixture>
    {
        public JsonNewtonsoftStringChangeTrackingTest(JsonNewtonsoftStringChangeTrackingFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public class JsonNewtonsoftStringChangeTrackingFixture : JsonStringChangeTrackingFixtureBase
        {
            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            {
                return base.AddServices(serviceCollection)
                    .AddEntityFrameworkXGJsonNewtonsoft();
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder, XGJsonChangeTrackingOptions? changeTrackingOptions)
            {
                builder = base.AddOptions(builder, changeTrackingOptions);

                if (changeTrackingOptions != null)
                {
                    new XGDbContextOptionsBuilder(builder)
                        .UseNewtonsoftJson(changeTrackingOptions.Value);
                }

                return builder;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                var currentContext = (JsonStringChangeTrackingContext)context;
                if (currentContext.ChangeTrackingOptions != null &&
                    !currentContext.ChangeTrackingOptions.Value.AreChangeTrackingOptionsGlobal)
                {
                    modelBuilder.Entity<JsonEntity>(
                        entity =>
                        {
                            entity.Property(e => e.Customer)
                                .UseJsonChangeTrackingOptions(currentContext.ChangeTrackingOptions.Value.ChangeTrackingOptions);
                        });
                }
            }
        }
    }
}
