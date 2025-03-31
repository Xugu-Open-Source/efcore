using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Tests;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class SeedingXGTest : SeedingTestBase
    {
        protected override TestStore TestStore => XGTestStore.Create("SeedingTest");

        protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
            => new SeedingXGContext(testId);

        protected class SeedingXGContext : SeedingContext
        {
            public SeedingXGContext(string testId)
                : base(testId)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseXG(XGTestStore.CreateConnectionString($"Seeds{TestId}"), AppConfig.ServerVersion);
        }
    }
}
