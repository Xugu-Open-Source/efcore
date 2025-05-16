using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class InheritanceQueryXGTest : InheritanceRelationalQueryTestBase<InheritanceQueryXGFixture>
    {
        public InheritanceQueryXGTest(InheritanceQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        //[ConditionalTheory(Skip = "")]
        //[MemberData(nameof(IsAsyncData))]
        public override Task Byte_enum_value_constant_used_in_projection(bool async)
        {
            return base.Byte_enum_value_constant_used_in_projection(async);
        }

        public override void Setting_foreign_key_to_a_different_type_throws()
        {
            base.Setting_foreign_key_to_a_different_type_throws();
        }
    }
}
