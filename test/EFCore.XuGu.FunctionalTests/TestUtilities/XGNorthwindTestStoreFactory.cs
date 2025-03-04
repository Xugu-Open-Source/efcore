using Microsoft.EntityFrameworkCore.TestUtilities;

namespace EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGNorthwindTestStoreFactory : XGTestStoreFactory
    {
        public static new XGNorthwindTestStoreFactory Instance => InstanceCi;
        public static XGNorthwindTestStoreFactory InstanceCi { get; } = new XGNorthwindTestStoreFactory(databaseCollation: XGTestStore.ModernCiCollation);
        public static XGNorthwindTestStoreFactory InstanceCs { get; } = new XGNorthwindTestStoreFactory(databaseCollation: XGTestStore.ModernCsCollation);
        public static new XGNorthwindTestStoreFactory NoBackslashEscapesInstance { get; } = new XGNorthwindTestStoreFactory(true);

        protected XGNorthwindTestStoreFactory(bool noBackslashEscapes = false, string databaseCollation = null)
            : base(noBackslashEscapes, databaseCollation)
        {
        }

        public override TestStore GetOrCreate(string storeName)
            => XGTestStore.GetOrCreate(storeName, "Northwind.sql", noBackslashEscapes: NoBackslashEscapes);
    }
}
