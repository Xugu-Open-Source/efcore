// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using XuguClient;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class XGDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder UseXG(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<XGDbContextOptionsBuilder> xgOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var csb = new XGConnectionStringBuilder(connectionString)
            {
            };

            connectionString = csb.ConnectionString;
            var extension = GetOrCreateExtension(optionsBuilder).WithConnectionString(connectionString);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            ConfigureWarnings(optionsBuilder);
            xgOptionsAction?.Invoke(new XGDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder UseXG(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<XGDbContextOptionsBuilder> xgOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var csb = new XGConnectionStringBuilder(connection.ConnectionString);
            
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.ConnectionString = csb.ConnectionString;
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("The XG Connection string used with Microsoft.EntityFrameworkCore.XuGu " +
                    "must contain \"AllowUserVariables=true;UseAffectedRows=false\"", e);
            }

            var extension = GetOrCreateExtension(optionsBuilder).WithConnection(connection);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            ConfigureWarnings(optionsBuilder);
            xgOptionsAction?.Invoke(new XGDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseXG<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<XGDbContextOptionsBuilder> xgOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseXG(
                (DbContextOptionsBuilder)optionsBuilder, connectionString, xgOptionsAction);

        public static DbContextOptionsBuilder<TContext> UseXG<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<XGDbContextOptionsBuilder> xgOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseXG(
                (DbContextOptionsBuilder)optionsBuilder, connection, xgOptionsAction);

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
