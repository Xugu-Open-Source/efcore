using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class IncludeXGTest : IncludeTestBase<IncludeXGFixture>
    {
        public IncludeXGTest(IncludeXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Include_when_result_operator(bool useString)
        {
            using (var context = CreateContext())
            {
                var any
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .ToList()
                            .Any()
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .ToList()
                            .Any();

                Assert.True(any);
            }
        }
    }
}
