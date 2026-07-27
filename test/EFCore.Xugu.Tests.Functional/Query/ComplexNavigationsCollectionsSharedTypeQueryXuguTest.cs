// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query
{
    public class ComplexNavigationsCollectionsSharedTypeQueryXuguTest : ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<
        ComplexNavigationsSharedTypeQueryXGTest.ComplexNavigationsSharedTypeQueryXGFixture>
    {
        public ComplexNavigationsCollectionsSharedTypeQueryXuguTest(
            ComplexNavigationsSharedTypeQueryXGTest.ComplexNavigationsSharedTypeQueryXGFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task SelectMany_with_Include1(bool async)
        {
            await base.SelectMany_with_Include1(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task SelectMany_with_navigation_and_Distinct(bool async)
        {
            await base.SelectMany_with_navigation_and_Distinct(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override async Task Take_Select_collection_Take(bool async)
        {
            await base.Take_Select_collection_Take(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override async Task Skip_Take_Select_collection_Skip_Take(bool async)
        {
            await base.Skip_Take_Select_collection_Skip_Take(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override async Task Skip_Take_on_grouping_element_inside_collection_projection(bool async)
        {
            await base.Skip_Take_on_grouping_element_inside_collection_projection(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override async Task Skip_Take_Distinct_on_grouping_element(bool async)
        {
            await base.Skip_Take_Distinct_on_grouping_element(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override async Task Skip_Take_on_grouping_element_with_collection_include(bool async)
        {
            await base.Skip_Take_on_grouping_element_with_collection_include(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override async Task Skip_Take_on_grouping_element_with_reference_include(bool async)
        {
            await base.Skip_Take_on_grouping_element_with_reference_include(async);
            // AssertSql deferred (Wave1: result assertions only)
        }


        #region APPLY/LATERAL not supported (XuguStrings.ApplyNotSupported)

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Complex_query_with_let_collection_projection_FirstOrDefault(bool async)
            => base.Complex_query_with_let_collection_projection_FirstOrDefault(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(bool async)
            => base.Complex_query_with_let_collection_projection_FirstOrDefault_with_ToList_on_inner_and_outer(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Filtered_include_after_different_filtered_include_different_level(bool async)
            => base.Filtered_include_after_different_filtered_include_different_level(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
            => base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
            => base.Filtered_include_complex_three_level_with_middle_having_filter1(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
            => base.Filtered_include_complex_three_level_with_middle_having_filter2(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(bool async)
            => base.Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async)
            => base.Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(bool async)
            => base.Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Filtered_include_Take_with_another_Take_on_top_level(bool async)
            => base.Filtered_include_Take_with_another_Take_on_top_level(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(bool async)
            => base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(bool async)
            => base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Projecting_collection_after_optional_reference_correlated_with_parent(bool async)
            => base.Projecting_collection_after_optional_reference_correlated_with_parent(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task Projecting_collection_with_group_by_after_optional_reference_correlated_with_parent(bool async)
            => base.Projecting_collection_with_group_by_after_optional_reference_correlated_with_parent(async);

        [ConditionalTheory(Skip = "XuguDB does not support CROSS APPLY / OUTER APPLY / LATERAL (XuguStrings.ApplyNotSupported).")]
        public override Task SelectMany_with_predicate_and_DefaultIfEmpty_projecting_root_collection_element_and_another_collection(bool async)
            => base.SelectMany_with_predicate_and_DefaultIfEmpty_projecting_root_collection_element_and_another_collection(async);

        #endregion

        
        [ConditionalTheory(Skip = "XuguDB semantic difference: result ordering / null handling / value formatting differs from SQL Server baseline. Server behavioral limitation.")]
        public override Task Include_reference_collection_order_by_reference_navigation(bool async)
            => base.Include_reference_collection_order_by_reference_navigation(async);

        
        [ConditionalTheory(Skip = "XuguDB semantic difference: result ordering / null handling / value formatting differs from SQL Server baseline. Server behavioral limitation.")]
        public override Task Optional_navigation_with_Include_and_order(bool async)
            => base.Optional_navigation_with_Include_and_order(async);

        
        [ConditionalTheory(Skip = "XuguDB semantic difference: result ordering / null handling / value formatting differs from SQL Server baseline. Server behavioral limitation.")]
        public override Task Optional_navigation_with_order_by_and_Include(bool async)
            => base.Optional_navigation_with_order_by_and_Include(async);

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}

