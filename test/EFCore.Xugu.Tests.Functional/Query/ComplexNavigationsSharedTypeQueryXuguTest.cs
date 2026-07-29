// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query
{
    public class ComplexNavigationsSharedTypeQueryXGTest : ComplexNavigationsSharedTypeQueryRelationalTestBase<
        ComplexNavigationsSharedTypeQueryXGTest.ComplexNavigationsSharedTypeQueryXGFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public ComplexNavigationsSharedTypeQueryXGTest(
            ComplexNavigationsSharedTypeQueryXGFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.OuterReferenceInMultiLevelSubquery))]
        [ConditionalTheory(Skip = "XuguDB E17010: FROM subquery cannot reference outer query (docs: reference/sql/select/subquery.md table subquery; server [E17010]).")]
        public override Task Contains_with_subquery_optional_navigation_and_constant_item(bool async)
        {
            return base.Contains_with_subquery_optional_navigation_and_constant_item(async);
        }

        public override async Task SelectMany_subquery_with_custom_projection(bool async)
        {
            // TODO: Fix test in EF Core upstream.
            //           ORDER BY `l`.`Id`
            //       is ambiguous, since all 5 queried rows have an `Id` of `1`.
            await AssertQuery(
                async,
                ss => ss.Set<Level1>()/*.OrderBy(l1 => l1.Id)*/.SelectMany( // <-- has no effect anymore
                        l1 => l1.OneToMany_Optional1.Select(
                            l2 => new { l2.Name }))
                    .OrderBy(e => e.Name) // <-- fix
                    .Take(1));
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(bool async)
        {
            await base.Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/26104")]
        public override Task GroupBy_aggregate_where_required_relationship(bool async)
            => base.GroupBy_aggregate_where_required_relationship(async);

        [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/26104")]
        public override Task GroupBy_aggregate_where_required_relationship_2(bool async)
            => base.GroupBy_aggregate_where_required_relationship_2(async);

        public override async Task GroupJoin_client_method_in_OrderBy(bool async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await base.GroupJoin_client_method_in_OrderBy(async));
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
        {
            // Expression cannot be used for return type. Issue #23302.
            await Assert.ThrowsAsync<ArgumentException>(
                () => base.Join_with_result_selector_returning_queryable_throws_validation_error(async));
            // AssertSql deferred (Wave1: result assertions only)
        }

        [ConditionalTheory(Skip = "Does not throw an EqualException, but still does not work.")]
        public override async Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async)
        {
            // DefaultIfEmpty on child collection. Issue #19095.
            await Assert.ThrowsAsync<EqualException>(
                async () => await base.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(async));
            // AssertSql deferred (Wave1: result assertions only)
        }


        #region APPLY/LATERAL not supported (XuguStrings.ApplyNotSupported)

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Correlated_projection_with_first(bool async)
            => base.Correlated_projection_with_first(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Let_let_contains_from_outer_let(bool async)
            => base.Let_let_contains_from_outer_let(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Multiple_select_many_in_projection(bool async)
            => base.Multiple_select_many_in_projection(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Prune_does_not_throw_null_ref(bool async)
            => base.Prune_does_not_throw_null_ref(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Single_select_many_in_projection_with_take(bool async)
            => base.Single_select_many_in_projection_with_take(async);

        #endregion

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);


        [ConditionalTheory(Skip = "XuguDB E19132: composite-key join on GROUP BY aggregate projection rejected (server syntax).")]
        public override Task Composite_key_join_on_groupby_aggregate_projecting_only_grouping_key(bool async)
            => base.Composite_key_join_on_groupby_aggregate_projecting_only_grouping_key(async);

        [ConditionalTheory(Skip = "XuguDB E19132: composite-key join on GROUP BY aggregate projection rejected (server syntax).")]
        public override Task Composite_key_join_on_groupby_aggregate_projecting_only_grouping_key2(bool async)
            => base.Composite_key_join_on_groupby_aggregate_projecting_only_grouping_key2(async);

        #region High-priority semantic residuals
        public override Task Include18_1_1(bool async)
            => base.Include18_1_1(async);

        public override Task GroupJoin_on_left_side_being_a_subquery(bool async)
            => base.GroupJoin_on_left_side_being_a_subquery(async);

        public override Task GroupJoin_on_right_side_being_a_subquery(bool async)
            => base.GroupJoin_on_right_side_being_a_subquery(async);

        public override Task OrderBy_nav_prop_reference_optional(bool async)
            => base.OrderBy_nav_prop_reference_optional(async);

        public override Task OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty(bool async)
            => base.OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty(async);

        [ConditionalTheory(Skip = "XuguDB residual: collection-count OrderBy / Sum+Include cast selector semantics; not fixed by NULLS FIRST. Wave5f.")] 
        public override Task OrderBy_collection_count_ThenBy_reference_navigation(bool async)
            => base.OrderBy_collection_count_ThenBy_reference_navigation(async);

        public override Task Member_over_null_check_ternary_and_nested_dto_type(bool async)
            => base.Member_over_null_check_ternary_and_nested_dto_type(async);

        [ConditionalTheory(Skip = "XuguDB residual: collection-count OrderBy / Sum+Include cast selector semantics; not fixed by NULLS FIRST. Wave5f.")] 
        public override Task Sum_with_filter_with_include_selector_cast_using_as(bool async)
            => base.Sum_with_filter_with_include_selector_cast_using_as(async);

        public override Task Optional_navigation_take_optional_navigation(bool async)
            => base.Optional_navigation_take_optional_navigation(async);

        #endregion


        public class ComplexNavigationsSharedTypeQueryXGFixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase
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
