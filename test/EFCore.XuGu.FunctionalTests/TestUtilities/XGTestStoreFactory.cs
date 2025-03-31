using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGTestStoreFactory : RelationalTestStoreFactory
    {
        public static XGTestStoreFactory Instance { get; } = new XGTestStoreFactory();
        public static XGTestStoreFactory NoBackslashEscapesInstance { get; } = new XGTestStoreFactory(noBackslashEscapes: true);

        protected bool NoBackslashEscapes { get; }
        protected string DatabaseCollation { get; }

        protected XGTestStoreFactory(
            bool noBackslashEscapes = false,
            string databaseCollation = null)
        {
            NoBackslashEscapes = noBackslashEscapes;
            DatabaseCollation = databaseCollation;
        }

        public override TestStore Create(string storeName)
            => XGTestStore.Create(storeName, noBackslashEscapes: NoBackslashEscapes, databaseCollation: DatabaseCollation);

        public override TestStore GetOrCreate(string storeName)
            => XGTestStore.GetOrCreate(storeName, noBackslashEscapes: NoBackslashEscapes, databaseCollation: DatabaseCollation);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkXG();
    }
}
