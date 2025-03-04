// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using EntityFrameworkCore.XuGu.Infrastructure;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Internal;
using XuguClient;
using System.Text.RegularExpressions;

namespace EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGRelationalConnection : RelationalConnection, IXGRelationalConnection
    {
        private const string NoBackslashEscapes = "NO_BACKSLASH_ESCAPES";

        private readonly XGOptionsExtension _XGOptionsExtension;

        // ReSharper disable once VirtualMemberCallInConstructor
        public XGRelationalConnection(
            [NotNull] RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
            _XGOptionsExtension = Dependencies.ContextOptions.FindExtension<XGOptionsExtension>() ?? new XGOptionsExtension();
        }

        private bool IsMasterConnection { get; set; }

        protected override DbConnection CreateDbConnection()
            => new XGConnection(new XGConnectionStringBuilder(ConnectionString).ConnectionString);

        public virtual IXGRelationalConnection CreateMasterConnection()
        {
            var relationalOptions = RelationalOptionsExtension.Extract(Dependencies.ContextOptions);
            var connection = (XGConnection)relationalOptions.Connection;
            var connectionString = connection?.ConnectionString ?? relationalOptions.ConnectionString;

            // Add master connection specific options.
            string pattern = @"DB=[^;]+";
            var csb = new XGConnectionStringBuilder(connectionString)
            {
                Database = string.Empty,
                ConnectionString = Regex.Replace(connectionString, pattern, $"DB=SYSTEM")
            };

            // Apply modified connection string.
            relationalOptions = connection is null
                ? relationalOptions.WithConnectionString(csb.ConnectionString)
                : relationalOptions.WithConnection(connection.CloneWith(csb.ConnectionString));

            var optionsBuilder = new DbContextOptionsBuilder();
            var optionsBuilderInfrastructure = (IDbContextOptionsBuilderInfrastructure)optionsBuilder;

            optionsBuilderInfrastructure.AddOrUpdateExtension(relationalOptions);

            return new XGRelationalConnection(Dependencies.With(optionsBuilder.Options))
            {
                IsMasterConnection = true
            };
        }

        protected override bool SupportsAmbientTransactions => true;

        // CHECK: Is this obsolete or has it been moved somewhere else?
        // public override bool IsMultipleActiveResultSetsEnabled => false;

        public override void EnlistTransaction(Transaction transaction)
        {
            try
            {
                base.EnlistTransaction(transaction);
            }
            catch (Exception e)
            {
                if (e.Message == "Already enlisted in a Transaction.")
                {
                    // Return expected exception type.
                    throw new InvalidOperationException(e.Message, e);
                }

                throw;
            }
        }

        public override bool Open(bool errorsExpected = false)
        {
            var result = base.Open(errorsExpected);

            if (result)
            {
                if (_XGOptionsExtension.UpdateSqlModeOnOpen && _XGOptionsExtension.NoBackslashEscapes)
                {
                    AddSqlMode(NoBackslashEscapes);
                }
            }

            return result;
        }

        public override async Task<bool> OpenAsync(CancellationToken cancellationToken, bool errorsExpected = false)
        {
            var result = await base.OpenAsync(cancellationToken, errorsExpected)
                .ConfigureAwait(false);

            if (result)
            {
                if (_XGOptionsExtension.UpdateSqlModeOnOpen && _XGOptionsExtension.NoBackslashEscapes)
                {
                    await AddSqlModeAsync(NoBackslashEscapes)
                        .ConfigureAwait(false);
                }
            }

            return result;
        }

        public virtual void AddSqlMode(string mode)
            => Dependencies.CurrentContext.Context?.Database.ExecuteSqlInterpolated($@"SET SESSION sql_mode = CONCAT(@@sql_mode, ',', {mode});");

        public virtual Task AddSqlModeAsync(string mode)
            => Dependencies.CurrentContext.Context?.Database.ExecuteSqlInterpolatedAsync($@"SET SESSION sql_mode = CONCAT(@@sql_mode, ',', {mode});");

        public virtual void RemoveSqlMode(string mode)
            => Dependencies.CurrentContext.Context?.Database.ExecuteSqlInterpolated($@"SET SESSION sql_mode = REPLACE(@@sql_mode, {mode}, '');");

        public virtual void RemoveSqlModeAsync(string mode)
            => Dependencies.CurrentContext.Context?.Database.ExecuteSqlInterpolatedAsync($@"SET SESSION sql_mode = REPLACE(@@sql_mode, {mode}, '');");
    }
}
