// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Utilities;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class XuGuDbDatabaseCreator : RelationalDatabaseCreator
    {
        private readonly IXuGuDbConnection _connection;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;
        private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

        public XuGuDbDatabaseCreator(
            [NotNull] IXuGuDbConnection connection,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IMigrationCommandExecutor migrationCommandExecutor,
            [NotNull] IModel model,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder)
            : base(model, connection, modelDiffer, migrationsSqlGenerator, migrationCommandExecutor)
        {
            Check.NotNull(rawSqlCommandBuilder, nameof(rawSqlCommandBuilder));

            _connection = connection;
            _migrationsSqlGenerator = migrationsSqlGenerator;
            _rawSqlCommandBuilder = rawSqlCommandBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void Create()
        {
            using (var systemConnection = _connection.CreateSystemConnection())
            {
                MigrationCommandExecutor
                    .ExecuteNonQuery(CreateCreateOperations(), systemConnection);

                //ClearPool();
            }

            Exists(retryOnNotExists: true);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override async Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var systemConnection = _connection.CreateSystemConnection())
            {
                await MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateCreateOperations(), systemConnection, cancellationToken);

                //ClearPool();
            }

            await ExistsAsync(retryOnNotExists: true, cancellationToken: cancellationToken);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool HasTables()
            => Convert.ToInt32(CreateHasTablesCommand().ExecuteScalar(_connection)) != 0;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override async Task<bool> HasTablesAsync(CancellationToken cancellationToken = default(CancellationToken))
            => (int)await CreateHasTablesCommand().ExecuteScalarAsync(_connection, cancellationToken: cancellationToken) != 0;

        private IRelationalCommand CreateHasTablesCommand()
            => _rawSqlCommandBuilder
                .Build("SELECT CASE COUNT(*) WHEN 0 THEN 0 ELSE 1 END FROM ALL_TABLES;");

        private IReadOnlyList<MigrationCommand> CreateCreateOperations()
            => _migrationsSqlGenerator.Generate(new[] { new XuGuDbCreateDatabaseOperation { Name = GetDBName(_connection.ConnectionString)} });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool Exists()
            => Exists(retryOnNotExists: false);

        private bool Exists(bool retryOnNotExists)
        {
            var retryCount = 0;
            var giveUp = DateTime.UtcNow + TimeSpan.FromMinutes(1);
            while (true)
            {
                try
                {
                    return DatabaseExists(GetDBName(_connection.ConnectionString));
                    //_connection.Open();
                    //_connection.Close();
                    //return true;
                }
                catch (SqlException e)
                {
                    if (!retryOnNotExists
                        && IsDoesNotExist(e))
                    {
                        return false;
                    }

                    if (DateTime.UtcNow > giveUp
                        || !RetryOnExistsFailure(e, ref retryCount))
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
            => ExistsAsync(retryOnNotExists: false, cancellationToken: cancellationToken);

        private async Task<bool> ExistsAsync(bool retryOnNotExists, CancellationToken cancellationToken)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    return await DatabaseExistsAsync(GetDBName(_connection.ConnectionString), cancellationToken);
                    //await _connection.OpenAsync(cancellationToken);
                    //_connection.Close();
                    //return true;
                }
                catch (SqlException e)
                {
                    if (!retryOnNotExists
                        && IsDoesNotExist(e))
                    {
                        return false;
                    }

                    if (!RetryOnExistsFailure(e, ref retryCount))
                    {
                        throw;
                    }
                }
            }
        }

        // Login failed is thrown when database does not exist (See Issue #776)
        private static bool IsDoesNotExist(SqlException exception) => exception.Number == 4060;

        // See Issue #985
        private bool RetryOnExistsFailure(SqlException exception, ref int retryCount)
        {
            // This is to handle the case where Open throws (Number 233):
            //   System.Data.SqlClient.SqlException: A connection was successfully established with the
            //   server, but then an error occurred during the login process. (provider: Named Pipes
            //   Provider, error: 0 - No process is on the other end of the pipe.)
            // It appears that this happens when the database has just been created but has not yet finished
            // opening or is auto-closing when using the AUTO_CLOSE option. The workaround is to flush the pool
            // for the connection and then retry the Open call.
            // Also handling (Number -2):
            //   System.Data.SqlClient.SqlException: Connection Timeout Expired.  The timeout period elapsed while
            //   attempting to consume the pre-login handshake acknowledgment.  This could be because the pre-login
            //   handshake failed or the server was unable to respond back in time.
            // And (Number 4060):
            //   System.Data.SqlClient.SqlException: Cannot open database "X" requested by the login. The
            //   login failed.
            if ((exception.Number == 233 || exception.Number == -2 || exception.Number == 4060)
                && ++retryCount < 30)
            {
                //ClearPool();
                Thread.Sleep(100);
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
            //ClearAllPools();

            using (var systemConnection = _connection.CreateSystemConnection())
            {
                MigrationCommandExecutor
                    .ExecuteNonQuery(CreateDropCommands(), systemConnection);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override async Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            //ClearAllPools();

            using (var systemConnection = _connection.CreateSystemConnection())
            {
                await MigrationCommandExecutor
                    .ExecuteNonQueryAsync(CreateDropCommands(), systemConnection, cancellationToken);
            }
        }

        private IReadOnlyList<MigrationCommand> CreateDropCommands()
        {
            var operations = new MigrationOperation[]
            {
                // TODO Check DbConnection.Database always gives us what we want
                // Issue #775
                new XuGuDbDropDatabaseOperation { Name = GetDBName(_connection.ConnectionString).Replace("\u0001\0\0\0","") }
            };

            var systemCommands = _migrationsSqlGenerator.Generate(operations);
            return systemCommands;
        }

        // Clear connection pools in case there are active connections that are pooled
        //private static void ClearAllPools() => XGConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        //private void ClearPool() => SqlConnection.ClearPool((SqlConnection)_connection.DbConnection);

        private bool DatabaseExists(string name)
        {
            using (var system = new XGConnection("IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=UTF8"))
            {
                system.Open();

                using (var command = system.CreateCommand())
                {
                    command.CommandText = $@"SELECT COUNT(*) FROM sys_databases WHERE DB_NAME='{name}'";

                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        // Clear connection pools in case there are active connections that are pooled
        //private static void ClearAllPools() => XGConnection.ClearAllPools();

        // Clear connection pool for the database connection since after the 'create database' call, a previously
        // invalid connection may now be valid.
        //private void ClearPool() => SqlConnection.ClearPool((SqlConnection)_connection.DbConnection);

        private async Task<bool> DatabaseExistsAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var system = new XGConnection("IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=UTF8"))
            {
                await system.OpenAsync(cancellationToken);

                using (var command = system.CreateCommand())
                {
                    command.CommandText = $@"SELECT COUNT(*) FROM sys_databases WHERE DB_NAME='{name}'";

                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        private string GetDBName(string connectionString)
        {
            string pattern = @"DB=([^;]+)";

            Match match = Regex.Match(connectionString, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                throw new Exception("No DB name found in connection string");
            }
        }
    }
}
