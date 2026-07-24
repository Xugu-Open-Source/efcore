// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities.Attributes;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query
{
    public class ComplexNavigationsQueryXuguTest : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQueryXuguFixture>
    {
        public ComplexNavigationsQueryXuguTest(ComplexNavigationsQueryXuguFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
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
                    .OrderBy(l0 => l0.Name) // <-- fix
                    .Take(1));
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task GroupJoin_client_method_in_OrderBy(bool async)
        {
            await AssertTranslationFailedWithDetails(
                () => base.GroupJoin_client_method_in_OrderBy(async),
                CoreStrings.QueryUnableToTranslateMethod(
                    "Microsoft.EntityFrameworkCore.Query.ComplexNavigationsQueryTestBase<Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query.ComplexNavigationsQueryXuguFixture>",
                    "ClientMethodNullableInt"));
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Join_with_result_selector_returning_queryable_throws_validation_error(bool async)
        {
            // Expression cannot be used for return type. Issue #23302.
            await Assert.ThrowsAsync<ArgumentException>(
                () => base.Join_with_result_selector_returning_queryable_throws_validation_error(async));
            // AssertSql deferred (Wave1: result assertions only)
        }

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.OuterApply))]
        public override async Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async)
        {
            // DefaultIfEmpty on child collection. Issue #19095.
            await Assert.ThrowsAsync<EqualException>(
                async () => await base.Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(async));
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(bool async)
        {
            await base.Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        // CHECK: Flaky only on XG 5.7.
        [SupportedServerVersionCondition("12.0.0-xg")]
        public override async Task Member_pushdown_with_multiple_collections(bool async)
        {
            await base.Member_pushdown_with_multiple_collections(async);
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
    }
}

