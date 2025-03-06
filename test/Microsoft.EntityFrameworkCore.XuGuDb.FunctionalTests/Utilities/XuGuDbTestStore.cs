// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities
{
    public class XuGuDbTestStore : RelationalTestStore
    {
        public const int CommandTimeout = 90;

#if NETCOREAPP1_0
        private static string BaseDirectory => AppContext.BaseDirectory;
#else
        private static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;
#endif

        public static XuGuDbTestStore GetOrCreateShared(string name, Action initializeDatabase)
            => new XuGuDbTestStore(name).CreateShared(initializeDatabase);

        /// <summary>
        ///     A non-transactional, transient, isolated test database. Use this in the case
        ///     where transactions are not appropriate.
        /// </summary>
        public static Task<XuGuDbTestStore> CreateScratchAsync(bool createDatabase = true)
            => new XuGuDbTestStore(GetScratchDbName()).CreateTransientAsync(createDatabase);

        public static XuGuDbTestStore CreateScratch(bool createDatabase = true)
            => new XuGuDbTestStore(GetScratchDbName()).CreateTransient(createDatabase);

        private XGConnection _connection;
        private XGTransaction _transaction;
        private readonly string _name;
        private string _connectionString;
        private bool _deleteDatabase;

        public override string ConnectionString => _connectionString;

        // Use async static factory method
        private XuGuDbTestStore(string name)
        {
            _name = name;
        }

        private static string GetScratchDbName()
        {
            string name;
            do
            {
                name = "Scratch_" + Guid.NewGuid();
            }
            while (DatabaseExists(name)
                   || DatabaseFilesExist(name));

            return name;
        }

        private XuGuDbTestStore CreateShared(Action initializeDatabase)
        {
            CreateShared(typeof(XuGuDbTestStore).Name + _name, initializeDatabase);

            _connectionString = CreateConnectionString(_name);
            _connection = new XGConnection(_connectionString);

            _connection.Open();

            _transaction = _connection.BeginTransaction();

            return this;
        }

        public static void CreateDatabase(string name, string scriptPath = null, bool nonMasterScript = false, bool recreateIfAlreadyExists = false)
        {
            using (var system = new XGConnection(CreateConnectionString("SYSTEM")))
            {
                system.Open();

                using (var command = system.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;

                    var exists = DatabaseExists(name);
                    if (exists && (recreateIfAlreadyExists || !TablesExist(name)))
                    {
                        // if scriptPath is non-null assume that the script will handle dropping DB
                        if (scriptPath == null
                            || nonMasterScript)
                        {
                            command.CommandText = GetDeleteDatabaseSql(name);

                            command.ExecuteNonQuery();
                        }
                    }

                    if (!exists || recreateIfAlreadyExists)
                    {
                        if (scriptPath == null
                            || nonMasterScript)
                        {
                            command.CommandText = $@"CREATE DATABASE IF NOT EXISTS {name}";

                            command.ExecuteNonQuery();

                            using (var newConnection = new XGConnection(CreateConnectionString(name)))
                            {
                                WaitForExists(newConnection);
                            }
                        }

                        if (scriptPath != null)
                        {
                            // HACK: Probe for script file as current dir
                            // is different between k build and VS run.
                            if (File.Exists(@"..\..\" + scriptPath))
                            {
                                //executing in VS - so path is relative to bin\<config> dir
                                scriptPath = @"..\..\" + scriptPath;
                            }
                            else
                            {
                                scriptPath = Path.Combine(BaseDirectory, scriptPath);
                            }

                            if (nonMasterScript)
                            {
                                using (var newConnection = new XGConnection(CreateConnectionString(name)))
                                {
                                    newConnection.Open();
                                    using (var nonMasterCommand = newConnection.CreateCommand())
                                    {
                                        ExecuteScript(scriptPath, nonMasterCommand);
                                    }
                                }
                            }
                            else
                            {
                                ExecuteScript(scriptPath, command);
                            }
                        }
                    }
                }
            }
        }

        private static void ExecuteScript(string scriptPath, XGCommand scriptCommand)
        {
            var script = File.ReadAllText(scriptPath);
            foreach (var batch in new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromMilliseconds(1000.0))
                .Split(script).Where(b => !string.IsNullOrEmpty(b)))
            {
                scriptCommand.CommandText = batch;

                scriptCommand.ExecuteNonQuery();
            }
        }

        private static async Task WaitForExistsAsync(XGConnection connection)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    await connection.OpenAsync();

                    connection.Close();

                    return;
                }
                catch (SqlException e)
                {
                    if (++retryCount >= 30
                        || (e.Number != 233 && e.Number != -2 && e.Number != 4060))
                    {
                        throw;
                    }

                    Thread.Sleep(100);
                }
            }
        }

        private static void WaitForExists(XGConnection connection)
        {
            var retryCount = 0;
            while (true)
            {
                try
                {
                    connection.Open();

                    connection.Close();

                    return;
                }
                catch (SqlException e)
                {
                    if (++retryCount >= 30
                        || (e.Number != 233 && e.Number != -2 && e.Number != 4060))
                    {
                        throw;
                    }

                    Thread.Sleep(100);
                }
            }
        }

        private async Task<XuGuDbTestStore> CreateTransientAsync(bool createDatabase)
        {
            _connectionString = CreateConnectionString(_name);
            _connection = new XGConnection(_connectionString);

            if (createDatabase)
            {
                using (var system = new XGConnection(CreateConnectionString("SYSTEM")))
                {
                    await system.OpenAsync();
                    using (var command = system.CreateCommand())
                    {
                        command.CommandTimeout = CommandTimeout;
                        command.CommandText = $"{Environment.NewLine}CREATE DATABASE `{_name}`";

                        await command.ExecuteNonQueryAsync();

                        await WaitForExistsAsync(_connection);
                    }
                }
                await _connection.OpenAsync();
            }

            _deleteDatabase = true;
            return this;
        }

        private XuGuDbTestStore CreateTransient(bool createDatabase)
        {
            _connectionString = CreateConnectionString(_name);
            _connection = new XGConnection(_connectionString);

            if (createDatabase)
            {
                using (var system = new XGConnection(CreateConnectionString("SYSTEM")))
                {
                    system.Open();
                    using (var command = system.CreateCommand())
                    {
                        command.CommandTimeout = CommandTimeout;
                        command.CommandText = $"{Environment.NewLine}CREATE DATABASE `{_name}`";

                        command.ExecuteNonQuery();

                        WaitForExists(_connection);
                    }
                }
                _connection.Open();
            }

            _deleteDatabase = true;
            return this;
        }

        private static bool DatabaseExists(string name)
        {
            using (var system = new XGConnection(CreateConnectionString("SYSTEM")))
            {
                system.Open();

                using (var command = system.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText = $@"SELECT COUNT(*) FROM sys_databases WHERE DB_NAME='{name}'";

                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        private static bool TablesExist(string name)
        {
            using (var connection = new XGConnection(CreateConnectionString(name)))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout;
                    command.CommandText = @"SELECT COUNT(*) FROM ALL_TABLES";

                    var result = Convert.ToInt32(command.ExecuteScalar()) > 0;

                    connection.Close();

                    return result;
                }
            }
        }

        private static bool DatabaseFilesExist(string name)
        {
            var userFolder = Environment.GetEnvironmentVariable("USERPROFILE") ?? Environment.GetEnvironmentVariable("HOME");
            return userFolder != null
                   && (File.Exists(Path.Combine(userFolder, name + ".mdf"))
                       || File.Exists(Path.Combine(userFolder, name + "_log.ldf")));
        }

        private async Task DeleteDatabaseAsync(string name)
        {
            using (var system = new XGConnection(CreateConnectionString("SYSTEM")))
            {
                await system.OpenAsync();

                using (var command = system.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout; // Query will take a few seconds if (and only if) there are active connections

                    // SET SINGLE_USER will close any open connections that would prevent the drop
                    command.CommandText
                        = string.Format(@"DROP DATABASE IF EXISTS `{0}`;", name);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private void DeleteDatabase(string name)
        {
            using (var system = new XGConnection(CreateConnectionString("SYSTEM")))
            {
                system.Open();

                using (var command = system.CreateCommand())
                {
                    command.CommandTimeout = CommandTimeout; // Query will take a few seconds if (and only if) there are active connections

                    // SET SINGLE_USER will close any open connections that would prevent the drop
                    command.CommandText = GetDeleteDatabaseSql(name);

                    command.ExecuteNonQuery();
                }
            }
        }

        private static string GetDeleteDatabaseSql(string name)
            => string.Format(@"DROP DATABASE IF EXISTS `{0}`;", name);

        public override DbConnection Connection => _connection;

        public override DbTransaction Transaction => _transaction;

        public async Task<T> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return (T)await command.ExecuteScalarAsync(cancellationToken);
            }
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteNonQuery();
            }
        }

        public Task<int> ExecuteNonQueryAsync(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, params object[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    var results = Enumerable.Empty<T>();

                    while (await dataReader.ReadAsync())
                    {
                        try
                        {
                            results = results.Concat(new[] { await dataReader.GetFieldValueAsync<T>(0) });
                        }
                        catch (NotImplementedException)
                        {
                            // TODO remove workaround for mono limitation.
                            results = results.Concat(new[] { (T)dataReader.GetValue(0) });
                        }
                    }

                    return results;
                }
            }
        }

        private DbCommand CreateCommand(string commandText, object[] parameters)
        {
            var command = _connection.CreateCommand();

            if (_transaction != null)
            {
                command.Transaction = _transaction;
            }

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            for (var i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue("p" + i, parameters[i]);
            }

            return command;
        }

        public override void Dispose()
        {
            _transaction?.Dispose();

            _connection.Dispose();

            if (_deleteDatabase)
            {
                DeleteDatabase(_name);
            }
        }

        public static string CreateConnectionString(string name)
        {
            var connectionString = new StringBuilder();
            connectionString.Append("IP=127.0.0.1;DB=")
                .Append(name)
                .Append(";User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=GBK");

            return connectionString.ToString();
        }
    }
}
