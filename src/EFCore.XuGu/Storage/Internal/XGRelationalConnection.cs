// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGRelationalConnection : RelationalConnection, IXGRelationalConnection
    {
        public XGRelationalConnection([NotNull] RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override DbConnection CreateDbConnection() => new XGConnection(ConnectionString);

        public virtual IXGRelationalConnection CreateMasterConnection()
        {
            string pattern = @"DB=[^;]+";
            var csb = new XGConnectionStringBuilder(ConnectionString)
            {
                Database = string.Empty,
                ConnectionString = Regex.Replace(ConnectionString, pattern, $"DB=SYSTEM")
            };

            var contextOptions = new DbContextOptionsBuilder()
                .UseXG(csb.ConnectionString)
                .Options;

            return new XGRelationalConnection(Dependencies.With(contextOptions));
        }

        public virtual IXGRelationalConnection CreateMasterConnection(string database)
        {
            string pattern = @"DB=[^;]+";
            var csb = new XGConnectionStringBuilder(ConnectionString)
            {
                Database = database,
                ConnectionString = Regex.Replace(ConnectionString, pattern, $"DB=SYSTEM")
            };

            var contextOptions = new DbContextOptionsBuilder()
                .UseXG(csb.ConnectionString)
                .Options;

            return new XGRelationalConnection(Dependencies.With(contextOptions));
        }

        protected override bool SupportsAmbientTransactions => false;
        public override bool IsMultipleActiveResultSetsEnabled => false;

        public override async Task<IDbContextTransaction> BeginTransactionAsync(
            System.Data.IsolationLevel isolationLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await OpenAsync(cancellationToken).ConfigureAwait(false);

            if (CurrentTransaction != null)
            {
                throw new InvalidOperationException(RelationalStrings.TransactionAlreadyStarted);
            }

            if (Transaction.Current != null)
            {
                throw new InvalidOperationException(RelationalStrings.ConflictingAmbientTransaction);
            }

            if (EnlistedTransaction != null)
            {
                throw new InvalidOperationException(RelationalStrings.ConflictingEnlistedTransaction);
            }

            return await BeginTransactionWithNoPreconditionsAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IDbContextTransaction> BeginTransactionWithNoPreconditionsAsync(
            System.Data.IsolationLevel isolationLevel, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dbTransaction = await ((XGConnection)DbConnection).BeginTransactionAsync(isolationLevel, cancellationToken)
                .ConfigureAwait(false);

            CurrentTransaction = Dependencies.RelationalTransactionFactory.Create(
                this,
                dbTransaction,
                Dependencies.TransactionLogger,
                transactionOwned: true);

            Dependencies.TransactionLogger.TransactionStarted(
                this,
                dbTransaction,
                CurrentTransaction.TransactionId,
                DateTimeOffset.UtcNow);

            return CurrentTransaction;
        }

        public virtual async Task CommitTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CurrentTransaction == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
            }

            await ((XGRelationalTransaction)CurrentTransaction).CommitAsync(cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task RollbackTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CurrentTransaction == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoActiveTransaction);
            }

            await ((XGRelationalTransaction)CurrentTransaction).RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
