using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class TPTGearsOfWarQueryXGFixture : TPTGearsOfWarQueryRelationalFixture, IQueryFixtureBase
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
                mission.Timeline = GetExpectedValue(mission.Timeline);
            }

            return data;
        }

        public static DateTimeOffset GetExpectedValue(DateTimeOffset value)
        {
            const int xgMaxMillisecondDecimalPlaces = 6;
            var decimalPlacesFactor = (decimal)Math.Pow(10, 7 - xgMaxMillisecondDecimalPlaces);

            // Change DateTimeOffset values, because XuGu does not preserve offsets and has a maximum of 6 decimal places, in contrast to
            // .NET which has 7.
            return new DateTimeOffset(
                (long)(Math.Truncate(value.UtcTicks / decimalPlacesFactor) * decimalPlacesFactor),
                TimeSpan.Zero);
        }
    }
}
