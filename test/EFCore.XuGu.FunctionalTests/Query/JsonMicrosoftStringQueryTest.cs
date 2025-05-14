// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    [SupportedServerVersionCondition(nameof(ServerVersionSupport.Json))]
    public class JsonMicrosoftStringQueryTest : JsonStringQueryTestBase<JsonMicrosoftStringQueryTest.JsonMicrosoftStringQueryFixture>
    {
        public JsonMicrosoftStringQueryTest(JsonMicrosoftStringQueryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public class JsonMicrosoftStringQueryFixture : JsonStringQueryFixtureBase
        {
            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            {
                return base.AddServices(serviceCollection)
                    .AddEntityFrameworkXGJsonMicrosoft();
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var options = base.AddOptions(builder);
                new XGDbContextOptionsBuilder(options)
                    .UseMicrosoftJson();

                return options;
            }
        }
    }
}
