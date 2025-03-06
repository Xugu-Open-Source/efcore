// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Xugu.Internal;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class XuguDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly IXuguConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguDatabaseCreator(
            RelationalDatabaseCreatorDependencies dependencies,
            IXuguConnection connection,
            IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(dependencies)
        {
            _connection = connection;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void Create()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateCreateOperations(), masterConnection);
            }

            Exists(retryOnNotExists: true);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task CreateAsync(CancellationToken cancellationToken = default)
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken)
                    .ConfigureAwait(false);
            }

            await ExistsAsync(retryOnNotExists: true, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool HasTables()
            => Dependencies.ExecutionStrategy.Execute(
                _connection,
                connection => (int)CreateHasTablesCommand()
                        .ExecuteScalar(
                            new RelationalCommandParameterObject(
                                connection,
                                null,
                                null,
                                Dependencies.CurrentContext.Context,
                                Dependencies.CommandLogger, CommandSource.Migrations))!
                    != 0,
                null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
            => (int)(await Dependencies.ExecutionStrategy.ExecuteAsync(
                    _connection,
                    (connection, ct) => CreateHasTablesCommand()
                        .ExecuteScalarAsync(
                            new RelationalCommandParameterObject(
                                connection,
                                null,
                                null,
                                Dependencies.CurrentContext.Context,
                                Dependencies.CommandLogger, CommandSource.Migrations),
                            cancellationToken: ct),
                    null,
                    cancellationToken).ConfigureAwait(false))!
                != 0;

        private IRelationalCommand CreateHasTablesCommand()
            => _rawSqlCommandBuilder
                .Build(
                    @"SELECT COUNT(*)
          from all_tables t left join 
(select db_id ,schema_id from all_schemas where  db_id = (select db_id from all_databases where db_name = DATABASE() limit 1) and schema_name = CURRENT_SCHEMA()) d 
on t.db_id = d.db_id and  t.schema_id = d.schema_id where d.db_id is not null");

        private IReadOnlyList<MigrationCommand> CreateCreateOperations()
        {
            var builder = new XGConnectionStringBuilder(_connection.DbConnection.ConnectionString);
            object name;
            builder.TryGetValue("DB", out name);
            return Dependencies.MigrationsSqlGenerator.Generate(
                new[] { new XuguCreateDatabaseOperation { Name = (string)name } });
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool Exists()
            => Exists(retryOnNotExists: false);

        private bool Exists(bool retryOnNotExists)
            => Dependencies.ExecutionStrategy.Execute(
                DateTime.UtcNow + RetryTimeout, giveUp =>
                    {
                        while (true)
                        {
                            var opened = false;
                            try
                            {
                                using var _ = new TransactionScope(TransactionScopeOption.Suppress);
                                _connection.Open(errorsExpected: true);
                                opened = true;

                                _rawSqlCommandBuilder
                                    .Build("SELECT 1")
                                    .ExecuteNonQuery(
                                        new RelationalCommandParameterObject(
                                            _connection,
                                            null,
                                            null,
                                            Dependencies.CurrentContext.Context,
                                            Dependencies.CommandLogger, CommandSource.Migrations));

                                return true;
                            }
                            catch (Exception)
                            {
                                if (!retryOnNotExists)
                                {
                                    return false;
                                }

                                if (DateTime.UtcNow > giveUp)
                                {
                                    throw;
                                }

                                Thread.Sleep(RetryDelay);
                            }
                            finally
                            {
                                if (opened)
                                {
                                    _connection.Close();
                                }
                            }
                        }
                    },
                null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
            => ExistsAsync(retryOnNotExists: false, cancellationToken: cancellationToken);

        private Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
            => Dependencies.ExecutionStrategy.ExecuteAsync(
                DateTime.UtcNow + RetryTimeout, async (giveUp, ct) =>
                    {
                        while (true)
                        {
                            var opened = false;

                            try
                            {
                                using var _ = new TransactionScope(
                                    TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled);
                                await _connection.OpenAsync(ct, errorsExpected: true).ConfigureAwait(false);
                                opened = true;

                                await _rawSqlCommandBuilder
                                    .Build("SELECT 1")
                                    .ExecuteNonQueryAsync(
                                        new RelationalCommandParameterObject(
                                            _connection,
                                            null,
                                            null,
                                            Dependencies.CurrentContext.Context,
                                            Dependencies.CommandLogger, CommandSource.Migrations),
                                        ct)
                                    .ConfigureAwait(false);

                                return true;
                            }
                            catch (Exception)
                            {
                                if (!retryOnNotExists)
                                {
                                    return false;
                                }

                                if (DateTime.UtcNow > giveUp)
                                {
                                    throw;
                                }

                                await Task.Delay(RetryDelay, ct);
                            }
                            finally
                            {
                                if (opened)
                                {
                                    await _connection.CloseAsync();
                                }
                            }
                        }
                    }, null, cancellationToken);


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override void Delete()
        {
            using var masterConnection = _connection.CreateMasterConnection();
            Dependencies.MigrationCommandExecutor
                .ExecuteNonQuery(CreateDropCommands(), masterConnection);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override async Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            using var masterConnection = _connection.CreateMasterConnection();
            await Dependencies.MigrationCommandExecutor
                .ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken)
                .ConfigureAwait(false);
        }

        private IReadOnlyList<MigrationCommand> CreateDropCommands()
        {
            var databaseName = _connection.DbConnection.Database;
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException(XuguStrings.NoInitialCatalog);
            }

            var operations = new MigrationOperation[] { new XuguDropDatabaseOperation { Name = databaseName } };

            return Dependencies.MigrationsSqlGenerator.Generate(operations);
        }

    }
}
