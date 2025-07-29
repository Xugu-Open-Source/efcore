using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGTestStoreFactory : RelationalTestStoreFactory
    {
        public static XGTestStoreFactory Instance { get; } = new XGTestStoreFactory();
        public static XGTestStoreFactory NoBackslashEscapesInstance { get; } = new XGTestStoreFactory(true);

        private readonly bool _noBackslashEscapes;

        protected XGTestStoreFactory(bool noBackslashEscapes = false)
        {
            _noBackslashEscapes = noBackslashEscapes;
        }

        public override TestStore Create(string storeName)
            => XGTestStore.Create(storeName);

        public override TestStore GetOrCreate(string storeName)
            => XGTestStore.GetOrCreate(storeName);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkXG();
    }
}
