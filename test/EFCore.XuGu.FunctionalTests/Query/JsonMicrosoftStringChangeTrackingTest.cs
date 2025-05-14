// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Json.Microsoft.Extensions.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    [SupportedServerVersionCondition(nameof(ServerVersionSupport.Json))]
    public class JsonMicrosoftStringChangeTrackingTest : JsonStringChangeTrackingTestBase<JsonMicrosoftStringChangeTrackingTest.JsonMicrosoftStringChangeTrackingFixture>
    {
        public JsonMicrosoftStringChangeTrackingTest(JsonMicrosoftStringChangeTrackingFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public class JsonMicrosoftStringChangeTrackingFixture : JsonStringChangeTrackingFixtureBase
        {
            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            {
                return base.AddServices(serviceCollection)
                    .AddEntityFrameworkXGJsonMicrosoft();
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder, XGJsonChangeTrackingOptions? changeTrackingOptions)
            {
                builder = base.AddOptions(builder, changeTrackingOptions);

                if (changeTrackingOptions != null)
                {
                    new XGDbContextOptionsBuilder(builder)
                        .UseMicrosoftJson(changeTrackingOptions.Value);
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
