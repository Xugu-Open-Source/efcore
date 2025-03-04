using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Configuration;
using XuguClient;
using EntityFrameworkCore.XuGu.Tests;

namespace EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGTestStore : RelationalTestStore
    {
        private const string NoBackslashEscapes = "NO_BACKSLASH_ESCAPES";

        public const int DefaultCommandTimeout = 600;

        private readonly string _scriptPath;
        private readonly bool _useConnectionString;
        private readonly bool _noBackslashEscapes;

        protected override string OpenDelimiter => "`";
        protected override string CloseDelimiter => "`";

        public static XGTestStore GetOrCreate(string name, bool useConnectionString = false, bool noBackslashEscapes = false)
            => new XGTestStore(name, useConnectionString: useConnectionString, shared: true, noBackslashEscapes: noBackslashEscapes);

        public static XGTestStore GetOrCreate(string name, string scriptPath, bool noBackslashEscapes = false)
            => new XGTestStore(name, scriptPath: scriptPath, noBackslashEscapes: noBackslashEscapes);

        public static XGTestStore GetOrCreateInitialized(string name)
            => new XGTestStore(name, shared: true).InitializeXG(null, (Func<DbContext>)null, null);

        public static XGTestStore Create(string name, bool useConnectionString = false, bool noBackslashEscapes = false)
            => new XGTestStore(name, useConnectionString: useConnectionString, shared: false, noBackslashEscapes: noBackslashEscapes);

        public static XGTestStore CreateInitialized(string name)
            => new XGTestStore(name, shared: false).InitializeXG(null, null, null);

        public static XGTestStore RecreateInitialized(string name)
            => new XGTestStore(name, shared: false).InitializeXG(null, null, null, c =>
            {
                c.Database.EnsureDeleted();
                c.Database.EnsureCreated();
            });

        public Lazy<ServerVersion> ServerVersion { get; }
        public string DatabaseCharSet { get; }

        // ReSharper disable VirtualMemberCallInConstructor
        private XGTestStore(
            string name,
            string databaseCharSet = null,
            bool useConnectionString = false,
            string scriptPath = null,
            bool shared = true,
            bool noBackslashEscapes = false)
            : base(name, shared)
        {
            DatabaseCharSet = databaseCharSet ?? "utf8";
            _useConnectionString = useConnectionString;
            _noBackslashEscapes = noBackslashEscapes;

            if (scriptPath != null)
            {
                _scriptPath = Path.Combine(
                    Path.GetDirectoryName(
                        typeof(XGTestStore).GetTypeInfo()
                            .Assembly.Location), scriptPath);
            }

            ConnectionString = CreateConnectionString(name, _noBackslashEscapes);
            Connection = new XGConnection(ConnectionString);
            ServerVersion = new Lazy<ServerVersion>(() => Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect((XGConnection)Connection));
        }

        public static string CreateConnectionString(string name, bool noBackslashEscapes = false)
            => new XGConnectionStringBuilder(AppConfig.ConnectionString)
            {
                Database = name
            }.ConnectionString;

        private static int GetCommandTimeout() => AppConfig.Config.GetValue("Data:CommandTimeout", DefaultCommandTimeout);

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => _useConnectionString
                ? builder.UseXG(ConnectionString, AppConfig.ServerVersion, x => AddOptions(x, _noBackslashEscapes))
                : builder.UseXG(Connection, AppConfig.ServerVersion, x => AddOptions(x, _noBackslashEscapes));

        public static XGDbContextOptionsBuilder AddOptions(XGDbContextOptionsBuilder builder)
        {
            return builder
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)
                .CommandTimeout(GetCommandTimeout())
                .ExecutionStrategy(d => new TestXGRetryingExecutionStrategy(d));
            // .EnableIndexOptimizedBooleanColumns(); // TODO: Activate for all test for .NET 5. Tests should use
            //       `ONLY_FULL_GROUP_BY` to ensure correct working of the
            //       expression visitor in all cases, which is blocked by
            //       #1167 for MariaDB.
        }

        public static void AddOptions(XGDbContextOptionsBuilder builder, bool noBackslashEscapes)
        {
            AddOptions(builder);
            if (noBackslashEscapes)
            {
                builder.DisableBackslashEscaping();
            }
        }

        public XGTestStore InitializeXG(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean = null)
            => (XGTestStore)Initialize(serviceProvider, createContext, seed, clean);

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
        {
            if (CreateDatabase(clean))
            {
                if (_scriptPath != null)
                {
                    ExecuteScript(new FileInfo(_scriptPath));
                }
                else
                {
                    using (var context = createContext())
                    {
                        context.Database.EnsureCreatedResiliently();
                        seed?.Invoke(context);
                    }
                }
            }
        }

        private bool CreateDatabase(Action<DbContext> clean)
        {
            using var master = new XGConnection(CreateAdminConnectionString());
            master.Open();

            //SetupCurrentDatabaseCollation(master);

            string databaseSetupSql="SHOW AUTO_COMMIT;";
            if (!DatabaseExists(Name))
            {
                databaseSetupSql = GetCreateDatabaseStatement(Name, DatabaseCharSet);
            }

            ExecuteNonQuery(master, databaseSetupSql);

            return true;
        }

        private void DeleteDatabase()
        {
            using var master = new XGConnection(CreateAdminConnectionString());
            ExecuteNonQuery(master, $@"DROP DATABASE IF EXISTS `{Name}`;");
        }

        private static string GetCreateDatabaseStatement(string name, string charset = null)
            => $@"CREATE DATABASE `{name}`{(string.IsNullOrEmpty(charset) ? null : $" CHARACTER SET {charset}")};";


        private static bool DatabaseExists(string name)
        {
            using (var master = new XGConnection(CreateAdminConnectionString()))
                return ExecuteScalar<long>(master, $@"SELECT COUNT(*) FROM `ALL_DATABASES` WHERE `DB_NAME` = '{name}';") > 0;
        }

        private static string CreateAdminConnectionString()
            => CreateConnectionString(null);

        public void ExecuteScript(FileInfo scriptFile)
            => ExecuteScript(File.ReadAllText(scriptFile.FullName));

        public void ExecuteScript(string script)
            => Execute(
                Connection, command =>
                {
                    foreach (var batch in
                        new Regex(@"^/\*\s*GO\s*\*/", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromMilliseconds(1000.0))
                            .Split(EnsureBackwardsCompatibleCollations(Connection, script))
                            .Where(b => !string.IsNullOrEmpty(b)))
                    {
                        command.CommandText = batch;
                        command.ExecuteNonQuery();
                    }

                    return 0;
                }, string.Empty);

        public override void Clean(DbContext context)
            => context.Database.EnsureClean();

        private static T ExecuteScalar<T>(DbConnection connection, string sql, params object[] parameters)
            => Execute(connection, command => (T)command.ExecuteScalar(), sql, false, parameters);

        private static T Execute<T>(
            DbConnection connection, Func<DbCommand, T> execute, string sql,
            bool useTransaction = false, object[] parameters = null)
            => ExecuteCommand(connection, execute, sql, useTransaction, parameters);

        private static T ExecuteCommand<T>(
            DbConnection connection, Func<DbCommand, T> execute, string sql, bool useTransaction, object[] parameters)
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }

            connection.Open();

            try
            {
                using (var transaction = useTransaction
                    ? connection.BeginTransaction()
                    : null)
                {
                    T result;
                    using (var command = CreateCommand(connection, sql, parameters))
                    {
                        command.Transaction = transaction;
                        result = execute(command);
                    }

                    transaction?.Commit();

                    return result;
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
            => ExecuteNonQuery(Connection, sql, parameters);

        private static int ExecuteNonQuery(DbConnection connection, string sql, object[] parameters = null)
            => Execute(connection, command => command.ExecuteNonQuery(), sql, false, parameters);

        public override void OpenConnection()
        {
            base.OpenConnection();

            if (_noBackslashEscapes)
            {
                AppendToSqlMode(NoBackslashEscapes, (XGConnection)Connection);
            }
        }

        public override async Task OpenConnectionAsync()
        {
            await base.OpenConnectionAsync();

            if (_noBackslashEscapes)
            {
                await AppendToSqlModeAsync(NoBackslashEscapes, (XGConnection)Connection);
            }
        }

        private static DbCommand CreateCommand(DbConnection connection, string commandText, object[] parameters)
        {
            var command = (XGCommand)connection.CreateCommand();

            command.CommandText = commandText;
            command.CommandTimeout = GetCommandTimeout();

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Length; i++)
                {
                    command.Parameters.AddWithValue(":p" + i, parameters[i]);
                }
            }

            return command;
        }

        public virtual void AppendToSqlMode(string mode, XGConnection connection)
        {
            var command = connection.CreateCommand();

            command.CommandText = @"SET SESSION sql_mode = CONCAT(@@sql_mode, ',', @p0);";
            command.Parameters.Add(new XGParameters("@p0", mode));

            command.ExecuteNonQuery();
        }

        public virtual Task AppendToSqlModeAsync(string mode, XGConnection connection)
        {
            var command = connection.CreateCommand();

            command.CommandText = @"SET SESSION sql_mode = CONCAT(@@sql_mode, ',', @p0);";
            command.Parameters.Add(new XGParameters("@p0", mode));

            return command.ExecuteNonQueryAsync();
        }

        public const string ModernCsCollation = "utf8mb4_0900_as_cs";
        public const string LegacyCsCollation = "utf8mb4_bin";
        public const string ModernCiCollation = "utf8mb4_0900_ai_ci";
        public const string LegacyCiCollation = "utf8mb4_general_ci";

        private string EnsureBackwardsCompatibleCollations(DbConnection connection, string script)
        {
            if (GetCaseSensitiveUtf8Mb4Collation((XGConnection)connection) != ModernCsCollation)
            {
                script = script.Replace(ModernCsCollation, LegacyCsCollation, StringComparison.OrdinalIgnoreCase);
            }

            if (GetCaseInsensitiveUtf8Mb4Collation((XGConnection)connection) != ModernCiCollation)
            {
                script = script.Replace(ModernCiCollation, LegacyCiCollation, StringComparison.OrdinalIgnoreCase);
            }

            return script;
        }

        //private void SetupCurrentDatabaseCollation(DbConnection connection)
        //    => DatabaseCollation = EnsureBackwardsCompatibleCollations(connection, DatabaseCollation);

        public string GetCaseSensitiveUtf8Mb4Collation()
            => ServerVersion.Value.Supports.DefaultCharSetUtf8Mb4
                ? ModernCsCollation
                : LegacyCsCollation;

        public string GetCaseInsensitiveUtf8Mb4Collation()
            => ServerVersion.Value.Supports.DefaultCharSetUtf8Mb4
                ? ModernCiCollation
                : LegacyCiCollation;

        private string GetCaseSensitiveUtf8Mb4Collation(XGConnection connection)
            => Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(connection).Supports.DefaultCharSetUtf8Mb4
                ? ModernCsCollation
                : LegacyCsCollation;

        private string GetCaseInsensitiveUtf8Mb4Collation(XGConnection connection)
            => Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(connection).Supports.DefaultCharSetUtf8Mb4
                ? ModernCiCollation
                : LegacyCiCollation;
    }
}
