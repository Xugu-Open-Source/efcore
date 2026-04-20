// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    public static class XGEntityFrameworkServicesBuilderExtensions
    {
        public static IServiceCollection AddEntityFrameworkXG([NotNull] this IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            services.AddRelational();

            services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<XGDatabaseProviderServices, XGOptionsExtension>>());

            services.TryAdd(new ServiceCollection()
                .AddSingleton<XGValueGeneratorCache>()
                .AddSingleton<XGTypeMapper>()
                .AddSingleton<XGSqlGenerationHelper>()
                .AddSingleton<XGModelSource>()
                .AddSingleton<XGAnnotationProvider>()
                .AddSingleton<XGMigrationsAnnotationProvider>()
                .AddScoped<XGBatchExecutor>()
                .AddScoped(p => GetProviderServices(p).BatchExecutor)
                .AddScoped<XGConventionSetBuilder>()
                .AddScoped<TableNameFromDbSetConvention>()
                .AddScoped<IXGUpdateSqlGenerator, XGUpdateSqlGenerator>()
                .AddScoped<IXGConnection, XGConnection>()
                .AddScoped<XGModificationCommandBatchFactory>()
                .AddScoped<XGValueGeneratorSelector>()
                .AddScoped<XGDatabaseProviderServices>()
                .AddScoped<XGRelationalConnection>()
                .AddScoped<XGDatabaseCreator>()
                .AddScoped<XGHistoryRepository>()
                .AddScoped<XGMigrationsSqlGenerationHelper>()
                .AddScoped<XGModificationCommandBatchFactory>()
                .AddQuery());

            services
                .AddScoped<IChangeDetector, XGChangeDetector>()
                .AddScoped<IPropertyListener, IChangeDetector>(p => p.GetService<IChangeDetector>());

            return services;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<XGQueryCompilationContextFactory>()
                .AddScoped<XGCompositeMemberTranslator>()
                .AddScoped<XGCompositeMethodCallTranslator>()
                .AddScoped<XGQuerySqlGenerationHelperFactory>();
        }

        private static IRelationalDatabaseProviderServices GetProviderServices(IServiceProvider serviceProvider)
        {
            var providerServices = serviceProvider.GetRequiredService<IDbContextServices>().DatabaseProviderServices
                as IRelationalDatabaseProviderServices;

            if (providerServices == null)
            {
                throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
            }

            return providerServices;
        }
    }
}
