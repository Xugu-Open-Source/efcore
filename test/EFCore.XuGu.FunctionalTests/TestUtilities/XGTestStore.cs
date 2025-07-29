using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Configuration;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGTestStore : RelationalTestStore
    {
        public const int CommandTimeout = 30;
        private readonly bool _useConnectionString;
        private readonly bool _noBackslashEscapes;

        public static XGTestStore GetOrCreate(string name, bool useConnectionString = false, bool noBackslashEscapes = false)
            => new XGTestStore(name, useConnectionString: useConnectionString, noBackslashEscapes: noBackslashEscapes);

        public static XGTestStore GetOrCreateInitialized(string name)
            => new XGTestStore(name).InitializeXG(null, (Func<DbContext>)null, null);

        public static XGTestStore Create(string name, bool useConnectionString = false, bool noBackslashEscapes = false)
            => new XGTestStore(name, useConnectionString: useConnectionString, shared: false, noBackslashEscapes: noBackslashEscapes);

        public static XGTestStore CreateInitialized(string name)
            => new XGTestStore(name, shared: false).InitializeXG(null, (Func<DbContext>)null, null);

        private XGTestStore(string name, bool useConnectionString = false, bool shared = true, bool noBackslashEscapes = false)
            : base(name, shared)
        {
            _useConnectionString = useConnectionString;
            _noBackslashEscapes = noBackslashEscapes;

            ConnectionString = new XGConnectionStringBuilder(LazyConfig.Value["Data:ConnectionString"])
            {
                Database = name
            }.ToString();

            Connection = new XGConnection(ConnectionString);
        }

        private static readonly Lazy<IConfigurationRoot> LazyConfig = new Lazy<IConfigurationRoot>(() => new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json")
            .Build());

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => _useConnectionString
                ? builder.UseXG(ConnectionString,x => AddOptions(x, _noBackslashEscapes))
                : builder.UseXG(Connection, x => AddOptions(x, _noBackslashEscapes));

        public static void AddOptions(XGDbContextOptionsBuilder builder)
        {
            builder.CommandTimeout(CommandTimeout).ServerVersion(LazyConfig.Value["Data:ServerVersion"]);
        }

        public static void AddOptions(XGDbContextOptionsBuilder builder, bool noBackslashEscapes)
        {
            AddOptions(builder);
            if (noBackslashEscapes)
            {
                builder.DisableBackslashEscaping();
            }
        }

        public XGTestStore InitializeXG(IServiceProvider serviceProvider, Func<DbContext> createContext, Action<DbContext> seed)
            => (XGTestStore)Initialize(serviceProvider, createContext, seed);

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed)
        {
            using (var context = createContext())
            {
                if (!context.Database.EnsureCreated())
                {
                    Clean(context);
                }
                seed(context);
            }
        }

        public override void Clean(DbContext context)
            => context.Database.EnsureClean();

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            var connection = _useConnectionString
                ? new XGConnection(ConnectionString)
                : (XGConnection)Connection;
            try
            {
                using (var command = CreateCommand(connection, sql, parameters))
                {
                    return command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (_useConnectionString)
                {
                    connection.Dispose();
                }
            }
        }

        private DbCommand CreateCommand(XGConnection connection, string commandText, object[] parameters)
        {
            var command = connection.CreateCommand();

            command.CommandText = commandText;
            command.CommandTimeout = CommandTimeout;

            for (var i = 0; i < parameters.Length; i++)
            {
                command.Parameters.AddWithValue("@p" + i, parameters[i]);
            }

            return command;
        }
    }
}
