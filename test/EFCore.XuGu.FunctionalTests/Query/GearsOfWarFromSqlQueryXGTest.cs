using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class GearsOfWarFromSqlQueryXGTest : GearsOfWarFromSqlQueryTestBase<GearsOfWarQueryXGFixture>
    {
        public GearsOfWarFromSqlQueryXGTest(GearsOfWarQueryXGFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public override void From_sql_queryable_simple_columns_out_of_order()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Weapon>()
                    .FromSql(@"SELECT `Id`, `Name`, `IsAutomatic`, `AmmunitionType`, `OwnerFullName`, `SynergyWithId` FROM `Weapons` ORDER BY `Name`")
                    .ToArray();

                Assert.Equal(10, actual.Length);

                var first = actual.First();

                Assert.Equal(AmmunitionType.Shell, first.AmmunitionType);
                Assert.Equal("Baird's Gnasher", first.Name);
            }
        }
    }
}
