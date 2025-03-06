// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class XuGuDatabaseProviderServices : RelationalDatabaseProviderServices
    {
        public XuGuDatabaseProviderServices([NotNull] IServiceProvider services)
            : base(services)
        {
        }

        public override string InvariantName => GetType().GetTypeInfo().Assembly.GetName().Name;

        public override IDatabaseCreator Creator => GetService<XuGuDbDatabaseCreator>();

        public override IRelationalConnection RelationalConnection => GetService<IXuGuDbConnection>();

        public override ISqlGenerationHelper SqlGenerationHelper => GetService<XuGuDbSqlGenerationHelper>();

        public override IValueGeneratorSelector ValueGeneratorSelector => GetService<XuGuDbValueGeneratorSelector>();

        public override IRelationalDatabaseCreator RelationalDatabaseCreator => GetService<XuGuDbDatabaseCreator>();

        public override IConventionSetBuilder ConventionSetBuilder => GetService<XuGuDbConventionSetBuilder>();

        public override IMigrationsAnnotationProvider MigrationsAnnotationProvider => GetService<XuGuDbMigrationsAnnotationProvider>();

        public override IHistoryRepository HistoryRepository => GetService<XuGuDbHistoryRepository>();

        public override IMigrationsSqlGenerator MigrationsSqlGenerator => GetService<XuGuDbMigrationsSqlGenerator>();

        public override IModelSource ModelSource => GetService<XuGuDbModelSource>();

        public override IUpdateSqlGenerator UpdateSqlGenerator => GetService<IXuGuDbUpdateSqlGenerator>();

        public override IValueGeneratorCache ValueGeneratorCache => GetService<IXuGuDbValueGeneratorCache>();

        public override IRelationalTypeMapper TypeMapper => GetService<XuGuDbTypeMapper>();

        public override IModificationCommandBatchFactory ModificationCommandBatchFactory => GetService<XuGuDbModificationCommandBatchFactory>();

        public override IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory => GetService<UntypedRelationalValueBufferFactoryFactory>();

        public override IRelationalAnnotationProvider AnnotationProvider => GetService<XuGuDbAnnotationProvider>();

        public override IMethodCallTranslator CompositeMethodCallTranslator => GetService<XuGuDbCompositeMethodCallTranslator>();

        public override IMemberTranslator CompositeMemberTranslator => GetService<XuGuDbCompositeMemberTranslator>();

        public override IQueryCompilationContextFactory QueryCompilationContextFactory => GetService<XuGuDbQueryCompilationContextFactory>();

        public override IQuerySqlGeneratorFactory QuerySqlGeneratorFactory => GetService<XuGuDbQuerySqlGeneratorFactory>();

        public override IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory => GetService<XuGuDbQueryModelVisitorFactory>();

        public override ICompiledQueryCacheKeyGenerator CompiledQueryCacheKeyGenerator => GetService<XuGuDbCompiledQueryCacheKeyGenerator>();
    }
}
