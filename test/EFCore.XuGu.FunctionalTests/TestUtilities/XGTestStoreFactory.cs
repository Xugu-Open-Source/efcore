using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGTestStoreFactory : RelationalTestStoreFactory
    {
        public static XGTestStoreFactory Instance { get; } = new XGTestStoreFactory();
        public static XGTestStoreFactory NoBackslashEscapesInstance { get; } = new XGTestStoreFactory(true);

        protected bool NoBackslashEscapes { get; }

        protected XGTestStoreFactory(bool noBackslashEscapes = false, string databaseCollation = null)
        {
            NoBackslashEscapes = noBackslashEscapes;
        }

        public override TestStore Create(string storeName)
            => XGTestStore.Create(storeName, noBackslashEscapes: NoBackslashEscapes);

        public override TestStore GetOrCreate(string storeName)
            => XGTestStore.GetOrCreate(storeName, noBackslashEscapes: NoBackslashEscapes);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkXG();
    }
}
