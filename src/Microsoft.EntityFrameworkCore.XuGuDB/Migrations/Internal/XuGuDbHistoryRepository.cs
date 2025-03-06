// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XuGuDbHistoryRepository : HistoryRepository
    {
        private readonly IMigrationsModelDiffer _modelDiffer;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;
        private readonly LazyRef<IModel> _model;
        public XuGuDbHistoryRepository(
            [NotNull] IDatabaseCreator databaseCreator,
            [NotNull] IRawSqlCommandBuilder rawSqlCommandBuilder,
            [NotNull] IXuGuDbConnection connection,
            [NotNull] IDbContextOptions options,
            [NotNull] IMigrationsModelDiffer modelDiffer,
            [NotNull] IMigrationsSqlGenerator migrationsSqlGenerator,
            [NotNull] IRelationalAnnotationProvider annotations,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper)
            : base(
                databaseCreator,
                rawSqlCommandBuilder,
                connection,
                options,
                modelDiffer,
                migrationsSqlGenerator,
                annotations,
                sqlGenerationHelper)
        {
            _modelDiffer = modelDiffer;
            _migrationsSqlGenerator = migrationsSqlGenerator;
            _model = new LazyRef<IModel>(
                () =>
                {
                    var modelBuilder = new ModelBuilder(new ConventionSet());
                    modelBuilder.Entity<HistoryRow>(
                        x =>
                        {
                            ConfigureTable(x);
                            x.ToTable(TableName, TableSchema);
                        });

                    return modelBuilder.Model;
                });
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string ExistsSql
        {
            get
            {
                var builder = new StringBuilder();

                builder.Append("SELECT `TABLE_ID` FROM `ALL_TABLES` WHERE `TABLE_NAME`='");
                builder
                    .Append(TableName)
                    .Append("';");

                return builder.ToString();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool InterpretExistsResult(object value) => value != DBNull.Value && value!=null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetInsertScript(HistoryRow row)
        {
            Check.NotNull(row, nameof(row));

            return new StringBuilder().Append("INSERT INTO ")
                .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append(" (")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(", ")
                .Append(SqlGenerationHelper.DelimitIdentifier(ProductVersionColumnName))
                .AppendLine(")")
                .Append("VALUES ('")
                .Append(SqlGenerationHelper.EscapeLiteral(row.MigrationId))
                .Append("', '")
                .Append(SqlGenerationHelper.EscapeLiteral(row.ProductVersion))
                .AppendLine("');")
                .ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetDeleteScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder().Append("DELETE FROM ")
                .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append("WHERE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = '")
                .Append(SqlGenerationHelper.EscapeLiteral(migrationId))
                .AppendLine("';")
                .ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetCreateIfNotExistsScript()
        {
            var builder = new IndentedStringBuilder();
            using (builder.Indent())
            {
                builder.AppendLines(GetCreateScript());
            }

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetBeginIfNotExistsScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder()
                .AppendLine("BEGIN")
                .Append("IF NOT EXISTS(SELECT * FROM ")
                .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append(" WHERE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = '")
                .Append(SqlGenerationHelper.EscapeLiteral(migrationId))
                .Append("')")
                .AppendLine(" THEN")
                .ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetBeginIfExistsScript(string migrationId)
        {
            Check.NotEmpty(migrationId, nameof(migrationId));

            return new StringBuilder()
                .AppendLine("BEGIN")
                .Append("IF EXISTS(SELECT * FROM ")
                .Append(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
                .Append(" WHERE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
                .Append(" = '")
                .Append(SqlGenerationHelper.EscapeLiteral(migrationId))
                .Append("')")
                .AppendLine(" THEN")
                .ToString();
        }

        public override string GetCreateScript()
        {
            var operations = _modelDiffer.GetDifferences(null, _model.Value);
            bool hasEnd = operations.Count > 1 && operations.Any(i => i is EnsureSchemaOperation);
            var xuGuMigrationsSqlGenerator =_migrationsSqlGenerator as XuGuDbMigrationsSqlGenerator;
            var commandList = hasEnd? xuGuMigrationsSqlGenerator.Generate(operations,hasEnd,_model.Value): _migrationsSqlGenerator.Generate(operations, _model.Value);

            return string.Concat(commandList.Select(c => c.CommandText));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override string GetEndIfScript() => "END IF;"+Environment.NewLine+"END;" + Environment.NewLine;
    }
}
