using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGRelationalTransactionFactory : RelationalTransactionFactory
    {
        public XGRelationalTransactionFactory([NotNull] RelationalTransactionFactoryDependencies dependencies)
            : base(dependencies)
        {
        }

        public override RelationalTransaction Create(
            IRelationalConnection connection,
            DbTransaction transaction,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
            => new XGRelationalTransaction(connection, transaction, logger, transactionOwned);
    }
}
