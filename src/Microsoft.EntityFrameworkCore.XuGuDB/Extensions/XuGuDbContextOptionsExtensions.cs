// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;


namespace Microsoft.EntityFrameworkCore
{
    public static class XuGuDbContextOptionsExtensions
    {
        
        public static DbContextOptionsBuilder UseXuGuDb(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<XuGuDbContextOptionsBuilder> xuGuDbOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotEmpty(connectionString, nameof(connectionString));

            var extension = GetOrCreateExtension(optionsBuilder);
            extension.ConnectionString = connectionString;
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            xuGuDbOptionsAction?.Invoke(new XuGuDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        public static DbContextOptionsBuilder UseXuGuDb(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<XuGuDbContextOptionsBuilder> xuGuDbOptionsAction = null)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));
            Check.NotNull(connection, nameof(connection));

            var extension = GetOrCreateExtension(optionsBuilder);
            extension.Connection = connection;
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            xuGuDbOptionsAction?.Invoke(new XuGuDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        
        public static DbContextOptionsBuilder<TContext> UseXuGuDb<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<XuGuDbContextOptionsBuilder> xuGuDbOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseXuGuDb(
                (DbContextOptionsBuilder)optionsBuilder, connectionString, xuGuDbOptionsAction);

        
        public static DbContextOptionsBuilder<TContext> UseXuGuDb<TContext>(
            [NotNull] this DbContextOptionsBuilder<TContext> optionsBuilder,
            [NotNull] DbConnection connection,
            [CanBeNull] Action<XuGuDbContextOptionsBuilder> xuGuDbOptionsAction = null)
            where TContext : DbContext
            => (DbContextOptionsBuilder<TContext>)UseXuGuDb(
                (DbContextOptionsBuilder)optionsBuilder, connection, xuGuDbOptionsAction);

        private static XuGuDbOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        {
            var existing = optionsBuilder.Options.FindExtension<XuGuDbOptionsExtension>();
            return existing != null
                ? new XuGuDbOptionsExtension(existing)
                : new XuGuDbOptionsExtension();
        }

        private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
        {
            // Set warnings defaults
            optionsBuilder.ConfigureWarnings(w =>
                {
                    w.Configuration.TryAddExplicit(
                        RelationalEventId.AmbientTransactionWarning, WarningBehavior.Throw);
                });
        }
    }
}
