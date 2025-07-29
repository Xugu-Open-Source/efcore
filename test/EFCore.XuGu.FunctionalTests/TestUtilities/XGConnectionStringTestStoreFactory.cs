using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGConnectionStringTestStoreFactory : ITestStoreFactory
    {
        public static XGConnectionStringTestStoreFactory Instance { get; } = new XGConnectionStringTestStoreFactory();

        protected XGConnectionStringTestStoreFactory()
        {
        }

        public virtual TestStore Create(string storeName)
            => XGTestStore.Create(storeName, useConnectionString: true);

        public virtual TestStore GetOrCreate(string storeName)
            => XGTestStore.GetOrCreate(storeName, useConnectionString: true);

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkXG()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());

        public ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
            => new ListLoggerFactory(shouldLogCategory);
    }
}
