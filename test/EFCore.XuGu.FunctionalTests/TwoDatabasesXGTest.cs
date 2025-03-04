using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using EntityFrameworkCore.XuGu.Tests;
using Xunit;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class TwoDatabasesXGTest : TwoDatabasesTestBase, IClassFixture<XGFixture>
    {
        public TwoDatabasesXGTest(XGFixture fixture)
            : base(fixture)
        {
        }

        protected new XGFixture Fixture
            => (XGFixture)base.Fixture;

        protected override DbContextOptionsBuilder CreateTestOptions(
            DbContextOptionsBuilder optionsBuilder, bool withConnectionString = false)
            => withConnectionString
                ? optionsBuilder.UseXG(DummyConnectionString, AppConfig.ServerVersion)
                : optionsBuilder.UseXG(AppConfig.ServerVersion);

        protected override TwoDatabasesWithDataContext CreateBackingContext(string databaseName)
            => new TwoDatabasesWithDataContext(Fixture.CreateOptions(XGTestStore.Create(databaseName)));

        protected override string DummyConnectionString { get; } = "Server=localhost;Database=DoesNotExist;AllowUserVariables=True;Use Affected Rows=False";
    }
}
