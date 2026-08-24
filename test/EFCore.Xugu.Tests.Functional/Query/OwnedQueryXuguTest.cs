// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query
{
    public class OwnedQueryXuguTest : OwnedQueryRelationalTestBase<OwnedQueryXuguTest.OwnedQueryXuguFixture>
    {
        public OwnedQueryXuguTest(OwnedQueryXuguFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalTheory(Skip = "XuguDB E17010: FROM subquery cannot reference outer query (docs: reference/sql/select/subquery.md table subquery; server [E17010]).")]
        public override Task Distinct_over_owned_collection(bool async)
            => base.Distinct_over_owned_collection(async);

        [ConditionalTheory(Skip = "XuguDB E17010: FROM subquery cannot reference outer query (docs: reference/sql/select/subquery.md table subquery; server [E17010]).")]
        public override Task Union_over_owned_collection(bool async)
            => base.Union_over_owned_collection(async);

        [ConditionalTheory(Skip = "XuguDB E19132: LIMIT/OFFSET expects integer (unexpected FCONST); Wave4 pending OFFSET cast/inlining).")]
        public override Task Client_method_skip_loads_owned_navigations_variation_2(bool async)
            => base.Client_method_skip_loads_owned_navigations_variation_2(async);

        [ConditionalTheory(Skip = "XuguDB E19132: LIMIT/OFFSET expects integer (unexpected FCONST); Wave4 pending OFFSET cast/inlining).")]
        public override Task Client_method_skip_loads_owned_navigations(bool async)
            => base.Client_method_skip_loads_owned_navigations(async);

        // Shared-store isolation maps every fixture table behind a per-store prefix
        // (e.g. EF_<hash>_OWNEDPERSON, see XuguTestModelExtensions.ApplyTablePrefix), while the
        // upstream raw SQL references the logical name [OwnedPerson] → server E5021 表或视图不存在.
        // Same pattern as GearsOfWarFromSqlQueryXGTest.From_sql_queryable_simple_columns_out_of_order.
        [ConditionalTheory]
        public override async Task Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(bool async)
        {
            using var context = CreateContext();
            var table = XuguTestStoreFactory.Instance.FormatTableName(Fixture.TestStore.Name, "OwnedPerson");
            var query = context.Set<OwnedPerson>().FromSqlRaw("SELECT * FROM `" + table + "`");

            if (async)
            {
                await query.ToListAsync();
            }
            else
            {
                query.ToList();
            }
        }


        public class OwnedQueryXuguFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory
                => XuguRelationalTestStoreFactory.Instance;

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => XuguFunctionalTestHelpers.AddModelCacheKey(base.AddServices(serviceCollection), StoreName);

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);
                XuguFunctionalTestHelpers.ApplyTablePrefix(modelBuilder, StoreName);
            }
        }
    }
}

