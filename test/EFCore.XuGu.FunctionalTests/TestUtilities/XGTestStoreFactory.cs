// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.Tests;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGTestStoreFactory : RelationalTestStoreFactory
    {
        public static XGTestStoreFactory Instance => InstanceCi;
        public static XGTestStoreFactory InstanceCi { get; } = new XGTestStoreFactory(databaseCollation: AppConfig.ServerVersion.DefaultUtf8CiCollation);
        public static XGTestStoreFactory InstanceCs { get; } = new XGTestStoreFactory(databaseCollation: AppConfig.ServerVersion.DefaultUtf8CsCollation);

        public static XGTestStoreFactory NoBackslashEscapesInstance { get; } = new XGTestStoreFactory(noBackslashEscapes: true);
        public static XGTestStoreFactory GuidBinary16Instance { get; } = new XGTestStoreFactory();

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
            => serviceCollection
                .AddEntityFrameworkXG()
                .AddEntityFrameworkXGNetTopologySuite();
    }
}
