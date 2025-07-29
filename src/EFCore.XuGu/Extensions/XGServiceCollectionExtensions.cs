// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Migrations.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Update.Internal;
using Microsoft.EntityFrameworkCore.XuGu.ValueGeneration.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class XGServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkXG([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<IDatabaseProvider, DatabaseProvider<XGOptionsExtension>>()
                .TryAdd<IRelationalTypeMappingSource, XGTypeMappingSource>()
                .TryAdd<IRelationalTransactionFactory, XGRelationalTransactionFactory>()
                .TryAdd<ISqlGenerationHelper, XGSqlGenerationHelper>()
                .TryAdd<IMigrationsAnnotationProvider, XGMigrationsAnnotationProvider>()
                .TryAdd<IConventionSetBuilder, XGConventionSetBuilder>()
                .TryAdd<IUpdateSqlGenerator>(p => p.GetService<IXGUpdateSqlGenerator>())
                .TryAdd<IModificationCommandBatchFactory, XGModificationCommandBatchFactory>()
                .TryAdd<IValueGeneratorSelector, XGValueGeneratorSelector>()
                .TryAdd<IRelationalConnection>(p => p.GetService<IXGRelationalConnection>())
                .TryAdd<IMigrationsSqlGenerator, XGMigrationsSqlGenerator>()
                .TryAdd<IRelationalDatabaseCreator, XGDatabaseCreator>()
                .TryAdd<IHistoryRepository, XGHistoryRepository>()
                .TryAdd<IExecutionStrategyFactory, XGExecutionStrategyFactory>()
                .TryAdd<IMemberTranslator, XGCompositeMemberTranslator>()
                .TryAdd<ICompositeMethodCallTranslator, XGCompositeMethodCallTranslator>()
                .TryAdd<IQuerySqlGeneratorFactory, XGQuerySqlGeneratorFactory>()
                .TryAdd<ISingletonOptions, IXGOptions>(p => p.GetService<IXGOptions>())
                .TryAddProviderSpecificServices(b => b
                    .TryAddSingleton<IXGOptions, XGOptions>()
                    .TryAddScoped<IXGUpdateSqlGenerator, XGUpdateSqlGenerator>()
                    .TryAddScoped<IXGRelationalConnection, XGRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
