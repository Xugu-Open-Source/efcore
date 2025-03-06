// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.Xugu.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Query.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Update.Internal;
using Microsoft.EntityFrameworkCore.Xugu.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class XuguServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Registers the given Entity Framework <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />
        ///         and configures it to connect to a SQL Server database.
        ///     </para>
        ///     <para>
        ///         This method is a shortcut for configuring a <see cref="DbContext" /> to use SQL Server. It does not support all options.
        ///         Use <see cref="O:EntityFrameworkServiceCollectionExtensions.AddDbContext" /> and related methods for full control of
        ///         this process.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure the SQL Server provider and connection string.
        ///     </para>
        ///     <para>
        ///         To configure the <see cref="DbContextOptions{TContext}" /> for the context, either override the
        ///         <see cref="DbContext.OnConfiguring" /> method in your derived context, or supply
        ///         an optional action to configure the <see cref="DbContextOptions" /> for the context.
        ///     </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-xugu">Accessing SQL Server and SQL Azure databases with EF Core</see>
        ///     for more information.
        /// </remarks>
        /// <typeparam name="TContext">The type of context to be registered.</typeparam>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="connectionString">The connection string of the database to connect to.</param>
        /// <param name="xuguOptionsAction">An optional action to allow additional SQL Server specific configuration.</param>
        /// <param name="optionsAction">An optional action to configure the <see cref="DbContextOptions" /> for the context.</param>
        /// <returns>The same service collection so that multiple calls can be chained.</returns>
        public static IServiceCollection AddXugu<TContext>(
            this IServiceCollection serviceCollection,
            string connectionString,
            Action<XuguDbContextOptionsBuilder>? xuguOptionsAction = null,
            Action<DbContextOptionsBuilder>? optionsAction = null)
            where TContext : DbContext
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));
            Check.NotEmpty(connectionString, nameof(connectionString));

            return serviceCollection.AddDbContext<TContext>(
                (serviceProvider, options) =>
                    {
                        optionsAction?.Invoke(options);
                        options.UseXugu(connectionString, xuguOptionsAction);
                    });
        }

        /// <summary>
        ///     <para>
        ///         Adds the services required by the Microsoft SQL Server database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Warning: Do not call this method accidentally. It is much more likely you need
        ///         to call <see cref="AddXugu{TContext}" />.
        ///     </para>
        ///     <para>
        ///         Calling this method is no longer necessary when building most applications, including those that
        ///         use dependency injection in ASP.NET or elsewhere.
        ///         It is only needed when building the internal service provider for use with
        ///         the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
        ///         This is not recommend other than for some advanced scenarios.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IServiceCollection AddEntityFrameworkXugu(this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, XuguLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<XuguOptionsExtension>>()
                .TryAdd<IRelationalTypeMappingSource, XuguTypeMappingSource>()
                .TryAdd<ISqlGenerationHelper, XuguSqlGenerationHelper>()
                .TryAdd<IRelationalAnnotationProvider, XuguAnnotationProvider>()
                .TryAdd<IMigrationsAnnotationProvider, XuguMigrationsAnnotationProvider>()
                .TryAdd<IModelValidator, XuguModelValidator>()
                .TryAdd<IProviderConventionSetBuilder, XuguConventionSetBuilder>()
                .TryAdd<IUpdateSqlGenerator>(p => p.GetRequiredService<IXuguUpdateSqlGenerator>())
                .TryAdd<IEvaluatableExpressionFilter, XuguEvaluatableExpressionFilter>()
                .TryAdd<IModificationCommandBatchFactory, XuguModificationCommandBatchFactory>()
                .TryAdd<IValueGeneratorSelector, XuguValueGeneratorSelector>()
                .TryAdd<IRelationalConnection>(p => p.GetRequiredService<IXuguConnection>())
                .TryAdd<IMigrationsSqlGenerator, XuguMigrationsSqlGenerator>()
                .TryAdd<IRelationalDatabaseCreator, XuguDatabaseCreator>()
                .TryAdd<IHistoryRepository, XuguHistoryRepository>()
                .TryAdd<IExecutionStrategyFactory, XuguExecutionStrategyFactory>()
                .TryAdd<IRelationalQueryStringFactory, XuguQueryStringFactory>()
                .TryAdd<ICompiledQueryCacheKeyGenerator, XuguCompiledQueryCacheKeyGenerator>()
                .TryAdd<IQueryCompilationContextFactory, XuguQueryCompilationContextFactory>()
                .TryAdd<IMethodCallTranslatorProvider, XuguMethodCallTranslatorProvider>()
                .TryAdd<IMemberTranslatorProvider, XuguMemberTranslatorProvider>()
                .TryAdd<IQuerySqlGeneratorFactory, XuguQuerySqlGeneratorFactory>()
                .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, XuguSqlTranslatingExpressionVisitorFactory>()
                .TryAdd<IRelationalParameterBasedSqlProcessorFactory, XuguParameterBasedSqlProcessorFactory>()
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddSingleton<IXuguUpdateSqlGenerator, XuguUpdateSqlGenerator>()
                        .TryAddScoped<IXuguConnection, XuguConnection>())
                .TryAddCoreServices();

            return serviceCollection;
        }
    }
}
