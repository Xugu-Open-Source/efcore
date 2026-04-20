// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var csb = new XGConnectionStringBuilder();
            csb.ConnectionString = connectionString;
            connectionString = csb.ConnectionString;

            var extension = GetOrCreateExtension(optionsBuilder);
            extension.ConnectionString = connectionString;
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            XGOptionsAction?.Invoke(new XGDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder UseXG(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var csb = new XGConnectionStringBuilder();
            csb.ConnectionString = connection.ConnectionString;

            connection.ConnectionString = csb.ConnectionString;
            var extension = GetOrCreateExtension(optionsBuilder);
            extension.Connection = connection;
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            XGOptionsAction?.Invoke(new XGDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder<TContext> UseXG<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
            where TContext : DbContext
        {
            return (DbContextOptionsBuilder<TContext>)UseXG(
                (DbContextOptionsBuilder)optionsBuilder, new XGConnection(connectionString), XGOptionsAction);
        }

        public static DbContextOptionsBuilder<TContext> UseXG<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<XGDbContextOptionsBuilder> XGOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseXG(
                (DbContextOptionsBuilder)optionsBuilder, connection, XGOptionsAction);

        private static XGOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        {
            var existing = optionsBuilder.Options.FindExtension<XGOptionsExtension>();
            return existing != null
                ? new XGOptionsExtension(existing)
                : new XGOptionsExtension();
        }
    }
}
