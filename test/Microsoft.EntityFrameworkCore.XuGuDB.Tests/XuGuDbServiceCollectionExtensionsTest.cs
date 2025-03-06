// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.Tests
{
    public class XuGuDbServiceCollectionExtensionsTest : RelationalServiceCollectionExtensionsTest
    {
        [Fact]
        public void Calling_AddEntityFramework_explicitly_does_not_change_services()
            => AssertServicesSame(
                new ServiceCollection().AddEntityFrameworkXuGuDb(),
                new ServiceCollection().AddEntityFramework().AddEntityFrameworkXuGuDb());

        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // SQL Server dingletones
            VerifySingleton<IXuGuDbValueGeneratorCache>();
            VerifySingleton<XuGuDbTypeMapper>();
            VerifySingleton<XuGuDbModelSource>();
            VerifySingleton<XuGuDbAnnotationProvider>();
            VerifySingleton<XuGuDbMigrationsAnnotationProvider>();

            // SQL Server scoped
            VerifyScoped<XuGuDbConventionSetBuilder>();
            VerifyScoped<IXuGuDbUpdateSqlGenerator>();
            VerifyScoped<IXuGuDbSequenceValueGeneratorFactory>();
            VerifyScoped<XuGuDbModificationCommandBatchFactory>();
            VerifyScoped<XuGuDbValueGeneratorSelector>();
            VerifyScoped<XuGuDatabaseProviderServices>();
            VerifyScoped<IXuGuDbConnection>();
            VerifyScoped<XuGuDbMigrationsSqlGenerator>();
            VerifyScoped<XuGuDbDatabaseCreator>();
            VerifyScoped<XuGuDbHistoryRepository>();
            VerifyScoped<XuGuDbCompositeMethodCallTranslator>();
            VerifyScoped<XuGuDbCompositeMemberTranslator>();
        }

        public XuGuDbServiceCollectionExtensionsTest()
            : base(XuGuDbTestHelpers.Instance)
        {
        }
    }
}
