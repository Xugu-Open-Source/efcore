// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGConnectionStringTestStoreFactory : RelationalTestStoreFactory
    {
        public static XGConnectionStringTestStoreFactory Instance { get; } = new XGConnectionStringTestStoreFactory();

        protected XGConnectionStringTestStoreFactory()
        {
        }

        public override TestStore Create(string storeName)
            => XGTestStore.Create(storeName, useConnectionString: true);

        public override TestStore GetOrCreate(string storeName)
            => XGTestStore.GetOrCreate(storeName, useConnectionString: true);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkXG();
    }
}
