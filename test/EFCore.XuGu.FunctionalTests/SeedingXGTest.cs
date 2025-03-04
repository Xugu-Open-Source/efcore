using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using EntityFrameworkCore.XuGu.Tests;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class SeedingXGTest : SeedingTestBase
    {
        protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        {
            var context = new SeedingXGContext(testId);

            context.Database.EnsureClean();

            return context;
        }

        protected class SeedingXGContext : SeedingContext
        {
            public SeedingXGContext(string testId)
                : base(testId)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseXG(XGTestStore.CreateConnectionString($"Seeds{TestId}", false), AppConfig.ServerVersion);
        }
    }
}
