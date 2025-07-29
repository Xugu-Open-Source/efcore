// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using XuguClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly IXGRelationalConnection _connection;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;
        private readonly string _databaseName = "SYSTEM";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XGDatabaseCreator(
            [NotNull] RelationalDatabaseCreatorDependencies dependencies,
            [NotNull] IXGRelationalConnection connection,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(dependencies)
        {
            if (connection.DbConnection.ConnectionString != null)
            {
                Match match = Regex.Match(connection.DbConnection.ConnectionString, @".*DB=([^;]+)");

                if (match.Success)
                {
                    _databaseName = match.Groups[1].Value;
                }
            }
            _connection = connection;
            ((XGConnection)_connection.DbConnection).ChangeDatabase(_databaseName, false);
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        public virtual TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);

        public virtual TimeSpan RetryTimeout { get; set; } = TimeSpan.FromMinutes(1);

        public override void Create()
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateCreateOperations(), masterConnection);

                ClearPool();
            }

            Exists(retryOnNotExists: true);
        }

        public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken).ConfigureAwait(false);

                ClearPool();
            }

            await ExistsAsync(retryOnNotExists: true, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override bool HasTables()
            => Dependencies.ExecutionStrategyFactory.Create().Execute(_connection, connection => Convert.ToInt64(CreateHasTablesCommand().ExecuteScalar(connection)) != 0);

        protected override Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(_connection,
                async (connection, ct) => Convert.ToInt64(await CreateHasTablesCommand().ExecuteScalarAsync(connection, cancellationToken: ct).ConfigureAwait(false)) != 0, cancellationToken);

        private IRelationalCommand CreateHasTablesCommand()
            => _rawSqlCommandBuilder
                .Build(@"
                    SELECT CASE WHEN COUNT(*) = 0 THEN FALSE ELSE TRUE END
FROM all_tables;");

        private IReadOnlyList<MigrationCommand> CreateCreateOperations()
        {
            Match match = Regex.Match(_connection.DbConnection.ConnectionString, @".*CHAR_SET=([^;]+)");
            string charSet = null;
            if (match.Success)
            {
                charSet = match.Groups[1].Value;
            }
            return Dependencies.MigrationsSqlGenerator.Generate((new[] { new XGCreateDatabaseOperation { Name = _databaseName,CharSet= charSet} }));
        }

        public override bool Exists()
            => Exists(retryOnNotExists: false);

        private bool Exists(bool retryOnNotExists)
            => Dependencies.ExecutionStrategyFactory.Create().Execute(DateTime.UtcNow + RetryTimeout, giveUp =>
            {
                while (true)
                {
                    try
                    {
                        try
                        {
                            using (var masterConnection = _connection.CreateMasterConnection())
                            {
                                masterConnection.Open();
                                using (var cmd = masterConnection.DbConnection.CreateCommand())
                                {
                                    cmd.CommandText = $"SELECT COUNT(*) FROM ALL_DATABASES WHERE DB_NAME = '{_databaseName}';";
                                    long i = (long)cmd.ExecuteScalar();
                                    if (i > 0)
                                    {
                                        cmd.CommandText = $"USE `{_databaseName}`";
                                        cmd.ExecuteNonQuery();
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (!e.Message.Contains("1045")) // Access denied because credentials were lost
                            {
                                throw;
                            }

                            _connection.Open(errorsExpected: true);

                            _connection.Close();
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        if (!retryOnNotExists && IsDoesNotExist(e))
                            return false;

                        if (DateTime.UtcNow > giveUp || !RetryOnExistsFailure(e))
                            throw;

                        Thread.Sleep(RetryDelay);
                    }
                }
            });

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            => ExistsAsync(retryOnNotExists: false, cancellationToken: cancellationToken);

        private Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
            => Dependencies.ExecutionStrategyFactory.Create().ExecuteAsync(DateTime.UtcNow + RetryTimeout, async (giveUp, ct) =>
            {
                while (true)
                {
                    try
                    {
                        try
                        {
                            using (var masterConnection = _connection.CreateMasterConnection())
                            {
                                await masterConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
                                using (var cmd = masterConnection.DbConnection.CreateCommand())
                                {
                                    cmd.CommandText = $"SELECT COUNT(*) FROM ALL_DATABASES WHERE DB_NAME = '{_databaseName}';";
                                    long i = (long)cmd.ExecuteScalar();
                                    if (i > 0)
                                    {
                                        cmd.CommandText = $"USE `{_databaseName}`";
                                        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            if (!e.Message.Contains("1045")) // Access denied because credentials were lost
                            {
                                throw;
                            }

                            await _connection.OpenAsync(ct, errorsExpected: true);

                            _connection.Close();
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        if (!retryOnNotExists && IsDoesNotExist(e))
                            return false;

                        if (DateTime.UtcNow > giveUp || !RetryOnExistsFailure(e))
                            throw;

                        await Task.Delay(RetryDelay, ct).ConfigureAwait(false);
                    }
                }
            }, cancellationToken);

        private static bool IsDoesNotExist(Exception exception) => exception.Message.Contains("E2016");

        private bool RetryOnExistsFailure(Exception exception)
        {
            if (exception.Message.Contains("E34305"))
            {
                ClearPool();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Delete()
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                Dependencies.MigrationCommandExecutor
                    .ExecuteNonQuery(CreateDropCommands(), masterConnection);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            ClearAllPools();

            using (var masterConnection = _connection.CreateMasterConnection())
            {
                await Dependencies.MigrationCommandExecutor.ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken).ConfigureAwait(false);
            }
        }

        private IReadOnlyList<MigrationCommand> CreateDropCommands()
        {
            var operations = new MigrationOperation[]
            {
                // TODO Check DbConnection.Database always gives us what we want
                // Issue #775
                new XGDropDatabaseOperation { Name = _databaseName }
            };

            var masterCommands = Dependencies.MigrationsSqlGenerator.Generate(operations);
            return masterCommands;
        }

        // Clear connection pools in case there are active connections that are pooled
        private static void ClearAllPools() => XGConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        private void ClearPool() => XGConnection.ClearPool(_connection.DbConnection as XGConnection);

        public override bool EnsureCreated()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
            {
                if (!Exists())
                {
                    Create();
                    CreateTables();
                    return true;
                }
                else
                {
                    if (!HasTables())
                    {
                        CreateTables();
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
