using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class GearsOfWarQueryXGFixture : GearsOfWarQueryRelationalFixture, IQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);

            new XGDbContextOptionsBuilder(optionsBuilder)
                .EnableIndexOptimizedBooleanColumns(true);

            return optionsBuilder;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Weapon>().HasIndex(e => e.IsAutomatic);
        }

        public new ISetSource GetExpectedData()
        {
            var data = (GearsOfWarData)base.GetExpectedData();

            foreach (var mission in data.Missions)
            {
                mission.Timeline = XGTestHelpers.GetExpectedValue(mission.Timeline);
            }

            return data;
        }
    }
}
