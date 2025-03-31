using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public static class XGDatabaseFacadeTestExtensions
    {
        public static void EnsureClean(this DatabaseFacade databaseFacade)
            => new XGDatabaseCleaner(
                    databaseFacade.GetService<IXGOptions>(),
                    databaseFacade.GetService<IRelationalTypeMappingSource>())
                .Clean(databaseFacade);
    }
}
