using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class CaseSensitiveNorthwindQueryXGFixture<TModelCustomizer> : NorthwindQueryRelationalFixture<TModelCustomizer>
        where TModelCustomizer : IModelCustomizer, new()
    {
        protected override string StoreName => "NorthwindCs";
        protected override ITestStoreFactory TestStoreFactory => XGNorthwindTestStoreFactory.InstanceCs;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);
            new XGDbContextOptionsBuilder(optionsBuilder).EnableStringComparisonTranslations();
            return optionsBuilder;
        }
    }
}
