// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data.Common;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using XuguClient;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides extension methods on <see cref="DbContextOptionsBuilder"/> and <see cref="DbContextOptionsBuilder{T}"/>
    /// to configure a <see cref="DbContext"/> to use with XG/MariaDB and EntityFrameworkCore.XuGu.
    /// </summary>
    public static class XGDbContextOptionsBuilderExtensions
    {
        /// <summary>
        ///     <para>
        ///         Configures the context to connect to a XG compatible database, but without initially setting any
        ///         <see cref="DbConnection" /> or connection string.
        ///     </para>
        ///     <para>
        ///         The connection or connection string must be set before the <see cref="DbContext" /> is used to connect
        ///         to a database. Set a connection using <see cref="RelationalDatabaseFacadeExtensions.SetDbConnection" />.
        ///         Set a connection string using <see cref="RelationalDatabaseFacadeExtensions.SetConnectionString" />.
        ///     </para>
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="serverVersion">
        ///     <para>
        ///         The version of the database server.
        ///     </para>
        ///     <para>
        ///         Create an object for this parameter from the classes <see cref="XGServerVersion"/> (for XG) and
        ///         <see cref="MariaDbServerVersion"/> (for MariaDB), through a call to the static method
        ///         <see cref="ServerVersion.AutoDetect(string)"/> (which retrieves the server version directly from the
        ///         database server), or by parsing a version string using the static methods
        ///         <see cref="ServerVersion.Parse(string)"/> or <see cref="ServerVersion.TryParse(string,out ServerVersion)"/>.
        ///      </para>
        /// </param>
        /// <param name="XGOptionsAction"> An optional action to allow additional XG specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseXG(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] ServerVersion serverVersion,
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            var extension = GetOrCreateExtension(optionsBuilder)
                .WithServerVersion(serverVersion);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            ConfigureWarnings(optionsBuilder);
            XGOptionsAction?.Invoke(new XGDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder UseXG(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = (XGOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            ConfigureWarnings(optionsBuilder);
            XGOptionsAction?.Invoke(new XGDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to a XG compatible database.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <param name="serverVersion">
        ///     <para>
        ///         The version of the database server.
        ///     </para>
        ///     <para>
        ///         Create an object for this parameter from the classes <see cref="XGServerVersion"/> (for XG) and
        ///         <see cref="MariaDbServerVersion"/> (for MariaDB), through a call to the static method
        ///         <see cref="ServerVersion.AutoDetect(string)"/> (which retrieves the server version directly from the
        ///         database server), or by parsing a version string using the static methods
        ///         <see cref="ServerVersion.Parse(string)"/> or <see cref="ServerVersion.TryParse(string,out ServerVersion)"/>.
        ///      </para>
        /// </param>
        /// <param name="XGOptionsAction"> An optional action to allow additional XG specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseXG(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string connectionString,
            [NotNull] ServerVersion serverVersion,
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var csb = new XGConnectionStringBuilder(connectionString);

            connectionString = csb.ConnectionString;

            var extension = (XGOptionsExtension)GetOrCreateExtension(optionsBuilder)
                .WithServerVersion(serverVersion)
                .WithConnectionString(connectionString);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            ConfigureWarnings(optionsBuilder);
            XGOptionsAction?.Invoke(new XGDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to connect to a XG compatible database.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connection">
        ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
        ///     in the open state then EF will not open or close the connection. If the connection is in the closed
        ///     state then EF will open and close the connection as needed.
        /// </param>
        /// <param name="serverVersion">
        ///     <para>
        ///         The version of the database server.
        ///     </para>
        ///     <para>
        ///         Create an object for this parameter from the classes <see cref="XGServerVersion"/> (for XG) and
        ///         <see cref="MariaDbServerVersion"/> (for MariaDB), through a call to the static method
        ///         <see cref="ServerVersion.AutoDetect(string)"/> (which retrieves the server version directly from the
        ///         database server), or by parsing a version string using the static methods
        ///         <see cref="ServerVersion.Parse(string)"/> or <see cref="ServerVersion.TryParse(string,out ServerVersion)"/>.
        ///      </para>
        /// </param>
        /// <param name="XGOptionsAction"> An optional action to allow additional XG specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseXG(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] DbConnection connection,
            [NotNull] ServerVersion serverVersion,
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var csb = new XGConnectionStringBuilder(connection.ConnectionString);

            var extension = (XGOptionsExtension)GetOrCreateExtension(optionsBuilder)
                .WithServerVersion(serverVersion)
                .WithConnection(connection);

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            ConfigureWarnings(optionsBuilder);
            XGOptionsAction?.Invoke(new XGDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        /// <summary>
        ///     <para>
        ///         Configures the context to connect to a XG compatible database, but without initially setting any
        ///         <see cref="DbConnection" /> or connection string.
        ///     </para>
        ///     <para>
        ///         The connection or connection string must be set before the <see cref="DbContext" /> is used to connect
        ///         to a database. Set a connection using <see cref="RelationalDatabaseFacadeExtensions.SetDbConnection" />.
        ///         Set a connection string using <see cref="RelationalDatabaseFacadeExtensions.SetConnectionString" />.
        ///     </para>
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="serverVersion">
        ///     <para>
        ///         The version of the database server.
        ///     </para>
        ///     <para>
        ///         Create an object for this parameter from the classes <see cref="XGServerVersion"/> (for XG) and
        ///         <see cref="MariaDbServerVersion"/> (for MariaDB), through a call to the static method
        ///         <see cref="ServerVersion.AutoDetect(string)"/> (which retrieves the server version directly from the
        ///         database server), or by parsing a version string using the static methods
        ///         <see cref="ServerVersion.Parse(string)"/> or <see cref="ServerVersion.TryParse(string,out ServerVersion)"/>.
        ///      </para>
        /// </param>
        /// <param name="XGOptionsAction"> An optional action to allow additional XG specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseXG<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] ServerVersion serverVersion,
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseXG(
                (DbContextOptionsBuilder)optionsBuilder, serverVersion, XGOptionsAction);

        /// <summary>
        ///     Configures the context to connect to a XG compatible database.
        /// </summary>
        /// <typeparam name="TContext"> The type of context to be configured. </typeparam>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <param name="serverVersion">
        ///     <para>
        ///         The version of the database server.
        ///     </para>
        ///     <para>
        ///         Create an object for this parameter from the classes <see cref="XGServerVersion"/> (for XG) and
        ///         <see cref="MariaDbServerVersion"/> (for MariaDB), through a call to the static method
        ///         <see cref="ServerVersion.AutoDetect(string)"/> (which retrieves the server version directly from the
        ///         database server), or by parsing a version string using the static methods
        ///         <see cref="ServerVersion.Parse(string)"/> or <see cref="ServerVersion.TryParse(string,out ServerVersion)"/>.
        ///      </para>
        /// </param>
        /// <param name="XGOptionsAction"> An optional action to allow additional XG specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseXG<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string connectionString,
            [NotNull] ServerVersion serverVersion,
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseXG(
                (DbContextOptionsBuilder)optionsBuilder, connectionString, serverVersion, XGOptionsAction);

        /// <summary>
        ///     Configures the context to connect to a XG compatible database.
        /// </summary>
        /// <param name="optionsBuilder"> The builder being used to configure the context. </param>
        /// <param name="connection">
        ///     An existing <see cref="DbConnection" /> to be used to connect to the database. If the connection is
        ///     in the open state then EF will not open or close the connection. If the connection is in the closed
        ///     state then EF will open and close the connection as needed.
        /// </param>
        /// <typeparam name="TContext"> The type of context to be configured. </typeparam>
        /// <param name="serverVersion">
        ///     <para>
        ///         The version of the database server.
        ///     </para>
        ///     <para>
        ///         Create an object for this parameter from the classes <see cref="XGServerVersion"/> (for XG) and
        ///         <see cref="MariaDbServerVersion"/> (for MariaDB), through a call to the static method
        ///         <see cref="ServerVersion.AutoDetect(string)"/> (which retrieves the server version directly from the
        ///         database server), or by parsing a version string using the static methods
        ///         <see cref="ServerVersion.Parse(string)"/> or <see cref="ServerVersion.TryParse(string,out ServerVersion)"/>.
        ///      </para>
        /// </param>
        /// <param name="XGOptionsAction"> An optional action to allow additional XG specific configuration. </param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder<TContext> UseXG<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] DbConnection connection,
            [NotNull] ServerVersion serverVersion,
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseXG(
                (DbContextOptionsBuilder)optionsBuilder, connection, serverVersion, XGOptionsAction);

        private static XGOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.Options.FindExtension<XGOptionsExtension>()
               ?? new XGOptionsExtension();

        private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            var coreOptionsExtension
                = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
                  ?? new CoreOptionsExtension();

            coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
                coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                    RelationalEventId.AmbientTransactionWarning, WarningBehavior.Throw));

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
        }
    }
}
