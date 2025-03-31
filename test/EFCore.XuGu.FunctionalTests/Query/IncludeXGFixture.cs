using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class IncludeXGFixture : NorthwindQueryXGFixture<NoopModelCustomizer>
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder);

        protected override bool ShouldLogCategory(string logCategory)
            => base.ShouldLogCategory(logCategory) || logCategory == DbLoggerCategory.Query.Name;
    }
}
