// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using System.Text.RegularExpressions;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGRelationalConnection : RelationalConnection, IXGRelationalConnection
    {
        private const string NoBackslashEscapes = "NO_BACKSLASH_ESCAPES";

        private readonly XGOptionsExtension _xgOptionsExtension;

        // ReSharper disable once VirtualMemberCallInConstructor
        public XGRelationalConnection(RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
            _xgOptionsExtension = Dependencies.ContextOptions.FindExtension<XGOptionsExtension>() ?? new XGOptionsExtension();
        }

        // TODO: Remove, because we don't use it anywhere.
        private bool IsMasterConnection { get; set; }


        protected override DbConnection CreateDbConnection()
            => new XGConnection(AddConnectionStringOptions(new XGConnectionStringBuilder(ConnectionString!)).ConnectionString);

        public virtual IXGRelationalConnection CreateMasterConnection()
        {
            // Add master connection specific options.
            string pattern = @"DB=[^;]+";
            var csb = new XGConnectionStringBuilder(ConnectionString)
            {
                Database = string.Empty,
                ConnectionString = Regex.Replace(ConnectionString, pattern, $"DB=SYSTEM")
            };

            csb = AddConnectionStringOptions(csb);

            var connectionString = csb.ConnectionString;
            var relationalOptions = RelationalOptionsExtension.Extract(Dependencies.ContextOptions);

            // Apply modified connection string.
            relationalOptions = relationalOptions.Connection is null
                ? relationalOptions.WithConnectionString(connectionString)
                : relationalOptions.WithConnection(DbConnection.CloneWith(connectionString));

            var optionsBuilder = new DbContextOptionsBuilder();
            var optionsBuilderInfrastructure = (IDbContextOptionsBuilderInfrastructure)optionsBuilder;

            optionsBuilderInfrastructure.AddOrUpdateExtension(relationalOptions);

            return new XGRelationalConnection(Dependencies with { ContextOptions = optionsBuilder.Options })
            {
                IsMasterConnection = true
            };
        }

        [AllowNull]
        public new virtual XGConnection DbConnection
        {
            get => (XGConnection)base.DbConnection;
            set => base.DbConnection = value;
        }

        protected virtual XGConnectionStringBuilder AddConnectionStringOptions(XGConnectionStringBuilder builder)
        {

            return builder;
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
                if (_xgOptionsExtension.UpdateSqlModeOnOpen && _xgOptionsExtension.NoBackslashEscapes)
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
                if (_xgOptionsExtension.UpdateSqlModeOnOpen && _xgOptionsExtension.NoBackslashEscapes)
                {
                    await AddSqlModeAsync(NoBackslashEscapes)
                        .ConfigureAwait(false);
                }
            }

            return result;
        }

        public virtual void AddSqlMode(string mode)
            => Dependencies.CurrentContext.Context?.Database.ExecuteSqlInterpolated($@"SET SESSION sql_mode = CONCAT(@@sql_mode, ',', {mode});");

        public virtual Task AddSqlModeAsync(string mode, CancellationToken cancellationToken = default)
            => Dependencies.CurrentContext.Context?.Database.ExecuteSqlInterpolatedAsync($@"SET SESSION sql_mode = CONCAT(@@sql_mode, ',', {mode});", cancellationToken);

        public virtual void RemoveSqlMode(string mode)
            => Dependencies.CurrentContext.Context?.Database.ExecuteSqlInterpolated($@"SET SESSION sql_mode = REPLACE(@@sql_mode, {mode}, '');");

        public virtual void RemoveSqlModeAsync(string mode, CancellationToken cancellationToken = default)
            => Dependencies.CurrentContext.Context?.Database.ExecuteSqlInterpolatedAsync($@"SET SESSION sql_mode = REPLACE(@@sql_mode, {mode}, '');", cancellationToken);
    }
}
