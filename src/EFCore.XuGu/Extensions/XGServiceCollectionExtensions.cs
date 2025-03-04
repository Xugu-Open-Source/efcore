// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Internal;
using EntityFrameworkCore.XuGu.Migrations.Internal;
using EntityFrameworkCore.XuGu.Storage.Internal;
using EntityFrameworkCore.XuGu.Update.Internal;
using EntityFrameworkCore.XuGu.ValueGeneration.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using EntityFrameworkCore.XuGu.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using EntityFrameworkCore.XuGu.Metadata.Internal;
using EntityFrameworkCore.XuGu.Migrations;
using EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;
using EntityFrameworkCore.XuGu.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class XGServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkXG([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, XGLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<XGOptionsExtension>>()
                //.TryAdd<IValueGeneratorCache>(p => p.GetService<IXGValueGeneratorCache>())
                .TryAdd<IRelationalTypeMappingSource, XGTypeMappingSource>()
                .TryAdd<ISqlGenerationHelper, XGSqlGenerationHelper>()
                .TryAdd<IRelationalAnnotationProvider, XGAnnotationProvider>()
                .TryAdd<IModelValidator, XGModelValidator>()
                .TryAdd<IProviderConventionSetBuilder, XGConventionSetBuilder>()
                //.TryAdd<IRelationalValueBufferFactoryFactory, TypedRelationalValueBufferFactoryFactory>() // What is that?
                .TryAdd<IUpdateSqlGenerator, XGUpdateSqlGenerator>()
                .TryAdd<IModificationCommandBatchFactory, XGModificationCommandBatchFactory>()
                .TryAdd<IValueGeneratorSelector, XGValueGeneratorSelector>()
                .TryAdd<IRelationalConnection>(p => p.GetService<IXGRelationalConnection>())
                .TryAdd<IMigrationsSqlGenerator, XGMigrationsSqlGenerator>()
                .TryAdd<IRelationalDatabaseCreator, XGDatabaseCreator>()
                .TryAdd<IHistoryRepository, XGHistoryRepository>()
                .TryAdd<ICompiledQueryCacheKeyGenerator, XGCompiledQueryCacheKeyGenerator>()
                .TryAdd<IExecutionStrategyFactory, XGExecutionStrategyFactory>()
                .TryAdd<IRelationalQueryStringFactory, XGQueryStringFactory>()
                .TryAdd<IMethodCallTranslatorProvider, XGMethodCallTranslatorProvider>()
                .TryAdd<IMemberTranslatorProvider, XGMemberTranslatorProvider>()
                .TryAdd<IEvaluatableExpressionFilter, XGEvaluatableExpressionFilter>()
                .TryAdd<IQuerySqlGeneratorFactory, XGQuerySqlGeneratorFactory>()
                .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, XGSqlTranslatingExpressionVisitorFactory>()
                .TryAdd<IRelationalParameterBasedSqlProcessorFactory, XGParametersBasedSqlProcessorFactory>()
                .TryAdd<ISqlExpressionFactory, XGSqlExpressionFactory>()
                .TryAdd<ISingletonOptions, IXGOptions>(p => p.GetService<IXGOptions>())
                //.TryAdd<IValueConverterSelector, XGValueConverterSelector>()
                .TryAdd<IQueryCompilationContextFactory, XGQueryCompilationContextFactory>()
                .TryAdd<IQueryTranslationPostprocessorFactory, XGQueryTranslationPostprocessorFactory>()
                .TryAdd<IMigrationsModelDiffer, XGMigrationsModelDiffer>()
                .TryAdd<IMigrator, XGMigrator>()
                .TryAddProviderSpecificServices(m => m
                    //.TryAddSingleton<IXGValueGeneratorCache, XGValueGeneratorCache>()
                    .TryAddSingleton<IXGOptions, XGOptions>()
                    //.TryAddScoped<IXGSequenceValueGeneratorFactory, XGSequenceValueGeneratorFactory>()
                    .TryAddScoped<IXGUpdateSqlGenerator, XGUpdateSqlGenerator>()
                    .TryAddScoped<IXGRelationalConnection, XGRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
