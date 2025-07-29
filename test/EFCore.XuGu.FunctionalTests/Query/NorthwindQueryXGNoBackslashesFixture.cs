using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class
        NorthwindQueryXGNoBackslashesFixture<TModelCustomizer> : NorthwindQueryXGFixture<TModelCustomizer> where TModelCustomizer : IModelCustomizer, new()
    {
        protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.NoBackslashEscapesInstance;
    }
}