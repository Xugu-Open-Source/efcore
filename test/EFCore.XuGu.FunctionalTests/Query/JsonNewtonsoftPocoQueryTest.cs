// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    [SupportedServerVersionCondition(nameof(ServerVersionSupport.Json))]
    public class JsonNewtonsoftPocoQueryTest : JsonPocoQueryTestBase<JsonNewtonsoftPocoQueryTest.JsonNewtonsoftPocoQueryFixture>
    {
        public JsonNewtonsoftPocoQueryTest(JsonNewtonsoftPocoQueryFixture fixture, ITestOutputHelper testOutputHelper)
            : base (fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Fact]
        public override void JsonContains_with_json_element()
        {
            using var ctx = CreateContext();
            var element = JObject.Parse(@"{""Name"": ""Joe"", ""Age"": 25}").Root;
            var count = ctx.JsonEntities.Count(e =>
                EF.Functions.JsonContains(e.Customer, element));

            Assert.Equal(1, count);
            AssertSql(
                $@"@__element_1='{{""Name"":""Joe"",""Age"":25}}' (Size = 4000)

SELECT COUNT(*)
FROM `JsonEntities` AS `j`
WHERE JSON_CONTAINS(`j`.`Customer`, {InsertJsonConvert("@__element_1")})");
        }

        public class JsonNewtonsoftPocoQueryFixture : JsonPocoQueryFixtureBase
        {
            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            {
                return base.AddServices(serviceCollection)
                    .AddEntityFrameworkXGJsonNewtonsoft();
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var options = base.AddOptions(builder);
                new XGDbContextOptionsBuilder(options)
                    .UseNewtonsoftJson();

                return options;
            }
        }
    }
}
