// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class XGDatabaseProviderServices : RelationalDatabaseProviderServices
    {
        public XGDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }
        public override string InvariantName => GetType().GetTypeInfo().Assembly.GetName().Name;
        public override IBatchExecutor BatchExecutor => GetService<XGBatchExecutor>();
        public override IDatabaseCreator Creator => GetService<XGDatabaseCreator>();
        public override IRelationalConnection RelationalConnection => GetService<XGRelationalConnection>();
        public override ISqlGenerationHelper SqlGenerationHelper => GetService<XGSqlGenerationHelper>();
        public override IRelationalDatabaseCreator RelationalDatabaseCreator => GetService<XGDatabaseCreator>();
        public override IMigrationsAnnotationProvider MigrationsAnnotationProvider => GetService<XGMigrationsAnnotationProvider>();
        public override IHistoryRepository HistoryRepository => GetService<XGHistoryRepository>();
        public override IMigrationsSqlGenerator MigrationsSqlGenerator => GetService<XGMigrationsSqlGenerationHelper>();
        public override IModelSource ModelSource => GetService<XGModelSource>();
        public override IUpdateSqlGenerator UpdateSqlGenerator => GetService<XGUpdateSqlGenerator>();
        public override IValueGeneratorCache ValueGeneratorCache => GetService<XGValueGeneratorCache>();
        public override IRelationalTypeMapper TypeMapper => GetService<XGTypeMapper>();
        public override IConventionSetBuilder ConventionSetBuilder => GetService<XGConventionSetBuilder>();
        public override IModificationCommandBatchFactory ModificationCommandBatchFactory => GetService<XGModificationCommandBatchFactory>();
        public override IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory => GetService<TypedRelationalValueBufferFactoryFactory>();
        public override IRelationalAnnotationProvider AnnotationProvider => GetService<XGAnnotationProvider>();
        public override IMethodCallTranslator CompositeMethodCallTranslator => GetService<XGCompositeMethodCallTranslator>();
        public override IMemberTranslator CompositeMemberTranslator => GetService<XGCompositeMemberTranslator>();
        public override IQueryCompilationContextFactory QueryCompilationContextFactory => GetService<XGQueryCompilationContextFactory>();
        public override IQuerySqlGeneratorFactory QuerySqlGeneratorFactory => GetService<XGQuerySqlGenerationHelperFactory>();
        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<XGValueGeneratorSelector>();
    }
}
