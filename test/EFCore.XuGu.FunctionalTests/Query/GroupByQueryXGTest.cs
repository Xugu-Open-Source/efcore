using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class GroupByQueryXGTest : GroupByQueryTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        // ReSharper disable once UnusedParameter.Local
        public GroupByQueryXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task OrderBy_Skip_GroupBy_Aggregate(bool isAsync)
        {
            await base.OrderBy_Skip_GroupBy_Aggregate(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task Join_GroupBy_Aggregate(bool isAsync)
        {
            await base.Join_GroupBy_Aggregate(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override void Join_GroupBy_entity_ToList()
        {
            base.Join_GroupBy_entity_ToList();
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Composite_Select_Average(bool isAsync)
        {
            await base.GroupBy_Composite_Select_Average(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Property_Select_Average(bool isAsync)
        {
            await base.GroupBy_Property_Select_Average(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Property_Select_Sum_Min_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Property_Select_Sum_Min_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Property_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Property_Select_Sum_Min_Key_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupJoin_GroupBy_Aggregate(bool isAsync)
        {
            await base.GroupJoin_GroupBy_Aggregate(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Property_anonymous_element_selector_Average(bool isAsync)
        {
            await base.GroupBy_Property_anonymous_element_selector_Average(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupJoin_complex_GroupBy_Aggregate(bool isAsync)
        {
            await base.GroupJoin_complex_GroupBy_Aggregate(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Composite_Select_Sum_Min_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Composite_Select_Sum_Min_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_anonymous_Select_Average(bool isAsync)
        {
            await base.GroupBy_anonymous_Select_Average(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task Self_join_GroupBy_Aggregate(bool isAsync)
        {
            await base.Self_join_GroupBy_Aggregate(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task Join_complex_GroupBy_Aggregate(bool isAsync)
        {
            await base.Join_complex_GroupBy_Aggregate(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_anonymous_Select_Sum_Min_Max_Avg(bool isAsync)
        {
            await base.GroupBy_anonymous_Select_Sum_Min_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_param_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            await base.GroupBy_param_Select_Sum_Min_Key_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task Select_anonymous_GroupBy_Aggregate(bool isAsync)
        {
            await base.Select_anonymous_GroupBy_Aggregate(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_with_result_selector(bool isAsync)
        {
            await base.GroupBy_with_result_selector(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Constant_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Constant_Select_Sum_Min_Key_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Property_Select_Key_Sum_Min_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Property_Select_Key_Sum_Min_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupJoin_GroupBy_Aggregate_5(bool isAsync)
        {
            await base.GroupJoin_GroupBy_Aggregate_5(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Property_scalar_element_selector_Average(bool isAsync)
        {
            await base.GroupBy_Property_scalar_element_selector_Average(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Composite_Select_Key_Average(bool isAsync)
        {
            await base.GroupBy_Composite_Select_Key_Average(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupJoin_GroupBy_Aggregate_3(bool isAsync)
        {
            await base.GroupJoin_GroupBy_Aggregate_3(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg(bool isAsync)
        {
            await base.GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Property_Select_Key_Average(bool isAsync)
        {
            await base.GroupBy_Property_Select_Key_Average(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(isAsync);
        }

        [ConditionalFact(Skip = "issue #571")]
        public override async Task GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(bool isAsync)
        {
            await base.GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(isAsync);
        }
    }
}
