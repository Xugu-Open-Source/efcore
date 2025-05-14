// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Tests;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGNorthwindTestStoreFactory : XGTestStoreFactory
    {
        public const string DefaultName = "Northwind";

        public static new XGNorthwindTestStoreFactory Instance => InstanceCi;
        public static XGNorthwindTestStoreFactory InstanceCi { get; } = new XGNorthwindTestStoreFactory(databaseCollation: AppConfig.ServerVersion.DefaultUtf8CiCollation);
        public static XGNorthwindTestStoreFactory InstanceCs { get; } = new XGNorthwindTestStoreFactory(databaseCollation: AppConfig.ServerVersion.DefaultUtf8CsCollation);
        public static new XGNorthwindTestStoreFactory NoBackslashEscapesInstance { get; } = new XGNorthwindTestStoreFactory(true);

        protected XGNorthwindTestStoreFactory(bool noBackslashEscapes = false, string databaseCollation = null)
            : base(noBackslashEscapes, databaseCollation)
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => XGTestStore.GetOrCreate(storeName ?? DefaultName, "Northwind.sql", noBackslashEscapes: NoBackslashEscapes, databaseCollation: DatabaseCollation);
    }
}
