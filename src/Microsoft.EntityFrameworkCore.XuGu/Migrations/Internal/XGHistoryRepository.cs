// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Text;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    public class XGHistoryRepository : HistoryRepository
    {

        private XGRelationalConnection _connection;

        public XGHistoryRepository(
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] IRawSqlCommandBuilder sqlCommandBuilder,
            [NotNull] XGRelationalConnection connection,
            [NotNull] IDbContextOptions options,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] XGMigrationsSqlGenerationHelper migrationsSqlGenerationHelper,
            [NotNull] XGAnnotationProvider annotations,
            [NotNull] ISqlGenerationHelper SqlGenerationHelper)
            : base(
                  databaseCreator,
                  sqlCommandBuilder,
                  connection,
                  options,
                  modelDiffer,
                  migrationsSqlGenerationHelper,
                  annotations,
                  SqlGenerationHelper)
        {
            _connection = connection;
        }

        protected override void ConfigureTable([NotNull] EntityTypeBuilder<HistoryRow> history)
        {
            base.ConfigureTable(history);
            history.Property(h => h.MigrationId).HasMaxLength(150);
            history.Property(h => h.ProductVersion).HasMaxLength(32).IsRequired();
        }

        protected override string ExistsSql
        {
            get
            {
                var builder = new StringBuilder();

                builder.Append("SELECT 1 FROM ALL_TABLES WHERE ");

                builder
                    .Append("SCHEMA_ID=(SELECT SCHEMA_ID FROM ALL_SCHEMAS WHERE SCHEMA_NAME=")
                    .Append(
                        SqlGenerationHelper.EscapeLiteral(TableSchema ?? "SYSDBA"))
                    .Append(") AND TABLE_NAME=")
                    .Append(
                        SqlGenerationHelper.EscapeLiteral(TableName))
                    .Append(";");

                return builder.ToString();
            }
        }

        protected override bool InterpretExistsResult(object value) => value != DBNull.Value;

        public override string GetCreateIfNotExistsScript()
        {
            return GetCreateScript();
        }

        public override string GetBeginIfNotExistsScript(string migrationId)
        {
            throw new NotSupportedException("Generating idempotent scripts for migration is not currently supported by XG");
        }

        public override string GetBeginIfExistsScript(string migrationId)
        {
            throw new NotSupportedException("Generating idempotent scripts for migration is not currently supported by XG");
        }

        public override string GetEndIfScript()
        {
            throw new NotSupportedException("Generating idempotent scripts for migration is not currently supported by XG");
        }
    }
}
