// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Metadata.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Update.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Migrations
{
    // CHECK: Can we increase the usage of the new model over the old one, or are we done here?
    /// <summary>
    ///     XG-specific implementation of <see cref="MigrationsSqlGenerator" />.
    /// </summary>
    public class XGMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private static readonly Regex _typeRegex = new Regex(@"([a-z0-9]+)\s*?(?:\(\s*(\d+)?\s*\))?",
            RegexOptions.IgnoreCase);
        private static readonly Dictionary<string, string> _guidIdentityMap = new Dictionary<string, string>();

        private static readonly HashSet<string> _spatialStoreTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "geometry",
            "point",
            "curve",
            "linestring",
            "line",
            "linearring",
            "surface",
            "polygon",
            "geometrycollection",
            "multipoint",
            "multicurve",
            "multilinestring",
            "multisurface",
            "multipolygon",
        };

        private readonly IRelationalAnnotationProvider _annotationProvider;
        private readonly IXGOptions _options;
        private readonly RelationalTypeMapping _stringTypeMapping;

        public XGMigrationsSqlGenerator(
            [NotNull] MigrationsSqlGeneratorDependencies dependencies,
            [NotNull] IRelationalAnnotationProvider annotationProvider,
            [NotNull] IXGOptions options)
            : base(dependencies)
        {
            _annotationProvider = annotationProvider;
            _options = options;
            _stringTypeMapping = dependencies.TypeMappingSource.GetMapping(typeof(string));
        }

        /// <summary>
        ///     <para>
        ///         Builds commands for the given <see cref="MigrationOperation" /> by making calls on the given
        ///         <see cref="MigrationCommandListBuilder" />.
        ///     </para>
        ///     <para>
        ///         This method uses a double-dispatch mechanism to call one of the 'Generate' methods that are
        ///         specific to a certain subtype of <see cref="MigrationOperation" />. Typically database providers
        ///         will override these specific methods rather than this method. However, providers can override
        ///         this methods to handle provider-specific operations.
        ///     </para>
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            switch (operation)
            {
                case XGCreateDatabaseOperation createDatabaseOperation:
                    Generate(createDatabaseOperation, model, builder);
                    break;
                case XGDropDatabaseOperation dropDatabaseOperation:
                    Generate(dropDatabaseOperation, model, builder);
                    break;
                case XGDropPrimaryKeyAndRecreateForeignKeysOperation dropPrimaryKeyAndRecreateForeignKeysOperation:
                    Generate(dropPrimaryKeyAndRecreateForeignKeysOperation, model, builder);
                    break;
                case XGDropUniqueConstraintAndRecreateForeignKeysOperation dropUniqueConstraintAndRecreateForeignKeysOperation:
                    Generate(dropUniqueConstraintAndRecreateForeignKeysOperation, model, builder);
                    break;
                default:
                    base.Generate(operation, model, builder);
                    break;
            }
        }

        protected override void Generate(
            [NotNull] CreateTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            //base.Generate(operation, model, builder, false);
            builder
                .Append("CREATE TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema))
                .Append(".")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(" (");

            using (builder.Indent())
            {
                CreateTableColumns(operation, model, builder);
                CreateTableConstraints(operation, model, builder);
                builder.AppendLine();
            }

            builder.Append(")");

            if (operation.Comment != null)
            {
                builder.Append($" COMMENT '{operation.Comment}'");
            }

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
            GenerateComment(operation.Comment, builder);

            if (_guidIdentityMap.ContainsKey(string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema + "." + operation.Name))
            {
                builder.AppendLine(string.Format("DROP TABLE IF EXISTS `{0}`.`tmpIdentity_{1}`;", string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema, operation.Name));
                builder.AppendLine(string.Format("CREATE TABLE `{0}`.`tmpIdentity_{1}` (`guid` guid);", string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema, operation.Name));
                builder.AppendLine(string.Format("DROP TRIGGER IF EXISTS `{0}`.`{1}_IdentityTgr`;", string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema, operation.Name));
                builder.AppendLine(string.Format("CREATE TRIGGER `{0}`.`{1}_IdentityTgr` BEFORE INSERT ON `{0}`", string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema, operation.Name));
                builder.AppendLine("FOR EACH ROW BEGIN");
                builder.AppendLine(string.Format("NEW.{0} := sys_guid();", _guidIdentityMap.GetValueOrDefault(string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema + "." + operation.Name)));
                builder.AppendLine(string.Format("INSERT INTO `{0}`.`tmpIdentity_{1}` VALUES(New.{2});", string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema, operation.Name, _guidIdentityMap.GetValueOrDefault(string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema + "." + operation.Name)));
                builder.AppendLine("END;");
            }
        }
        protected override void Generate(
            DropTableOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            builder
                .Append("DROP TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema))
                .Append(".")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }


        protected override void CreateTableColumns([NotNull] CreateTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            for (var i = 0; i < operation.Columns.Count; i++)
            {
                ColumnDefinition(operation.Columns[i], model, builder);
                GenerateComment(operation.Columns[i].Comment, builder);

                if (i != operation.Columns.Count - 1)
                {
                    builder.AppendLine(",");
                }
            }
        }

        protected override void Generate(AlterTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            if (operation.Comment != operation.OldTable.Comment)
            {
                builder.Append("COMMENT ON TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Schema??"SYSDBA"))
                    .Append(".")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" IS '")
                    .Append(operation.Comment ?? "")
                    .Append("'");

                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="AlterColumnOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            AlterColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" MODIFY COLUMN ");

            ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation,
                model,
                builder);

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            if (operation.Comment != operation.OldColumn.Comment)
            {
                builder.Append("COMMENT ON COLUMN ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(".")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" IS '")
                    .Append(operation.Comment ?? "")
                    .Append("'");
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }
            builder.EndCommand();
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameIndexOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (string.IsNullOrEmpty(operation.Table))
            {
                throw new InvalidOperationException(XGStrings.IndexTableRequired);
            }

            if (operation.NewName != null)
            {
                if (_options.ServerVersion.Supports.RenameIndex)
                {
                    builder.Append("ALTER INDEX ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                        .Append(".")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                        .Append(" RENAME ")
                        .Append("TO ")
                        .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                        .AppendLine(";");

                    EndStatement(builder);
                }
                else
                {
                    var index = model?
                        .GetRelationalModel()
                        .FindTable(operation.Table, operation.Schema)
                        ?.Indexes
                        .FirstOrDefault(i => i.Name == operation.NewName);

                    if (index == null)
                    {
                        throw new InvalidOperationException(
                            $"Could not find the model index: {Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema)}.{Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName)}. Upgrade to XuGu or split the 'RenameIndex' call into 'DropIndex' and 'CreateIndex'");
                    }

                    Generate(new DropIndexOperation
                    {
                        Schema = operation.Schema,
                        Table = operation.Table,
                        Name = operation.Name
                    }, model, builder);

                    var createIndexOperation = CreateIndexOperation.CreateFrom(index);
                    createIndexOperation.Name = operation.NewName;

                    Generate(createIndexOperation, model, builder);
                }
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RestartSequenceOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RestartSequenceOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            //if (!_options.ServerVersion.Supports.Sequences)
            //{
            //    throw new InvalidOperationException(
            //        $"Cannot restart sequence '{operation.Name}' because sequences are not supported in server version {_options.ServerVersion}.");
            //}
            builder
                .Append("ALTER SEQUENCE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" RESTART WITH ")
                .Append(IntegerConstant(operation.StartValue))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="RenameTableOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameTableOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" RENAME TO ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName, operation.NewSchema))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        /// <summary>
        ///     Builds commands for the given <see cref="CreateIndexOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            CreateIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (!_options.ServerVersion.Supports.SpatialIndexes &&
                operation[XGAnnotationNames.SpatialIndex] is true)
            {
                Dependencies.MigrationsLogger.Logger.LogWarning(
                    $"Spatial indexes are not supported on {_options.ServerVersion}. The CREATE INDEX operation will be ignored.");
                return;
            }

            builder.Append("CREATE ");

            if (operation.IsUnique)
            {
                builder.Append("UNIQUE ");
            }

            IndexTraits(operation, model, builder);

            builder
                .Append("INDEX ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(Truncate(operation.Name, 64)))
                .Append(" ON ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" (")
                .Append(ColumnListWithIndexOrder(operation, operation.Columns))
                .Append(")");

            IndexOptions(operation, model, builder);

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        /// /// <summary>
        ///     Ignored, since schemas are not supported by XG and are silently ignored to improve testing compatibility.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (string.Equals(operation.Name, "SYSDBA", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            builder
                .Append("BEGIN ")
                .Append("IF (SELECT COUNT(*) FROM `ALL_SCHEMAS` WHERE `SCHEMA_NAME` = ")
                .Append(stringTypeMapping.GenerateSqlLiteral(operation.Name))
                .Append(") < 1 THEN ")
                .Append(
                        "CREATE SCHEMA "
                        + Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name)
                        + Dependencies.SqlGenerationHelper.StatementTerminator)
                .Append("END IF")
                .Append(Dependencies.SqlGenerationHelper.StatementTerminator)
                .Append("END")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }

        /// <summary>
        ///     Ignored, since schemas are not supported by XG and are silently ignored to improve testing compatibility.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(DropSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
        }

        /// <summary>
        ///     Builds commands for the given <see cref="CreateSequenceOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        //protected override void Generate(
        //    [NotNull] CreateSequenceOperation operation,
        //    [CanBeNull] IModel model,
        //    [NotNull] MigrationCommandListBuilder builder)
        //{
        //    Check.NotNull(operation, nameof(operation));
        //    Check.NotNull(builder, nameof(builder));
        //    if (!_options.ServerVersion.Supports.Sequences)
        //    {
        //        throw new InvalidOperationException(
        //            $"Cannot create sequence '{operation.Name}' because sequences are not supported in server version {_options.ServerVersion}.");
        //    }

        //    var oldValue = operation.ClrType;
        //    operation.ClrType = typeof(long);
        //    if (operation.StartValue <= 0)
        //    {
        //        operation.MinValue = operation.StartValue;
        //    }
        //    base.Generate(operation, model, builder);
        //    operation.ClrType = oldValue;
        //}

        protected override void Generate(
            CreateSequenceOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            builder
                .Append("CREATE SEQUENCE IF NOT EXISTS ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Schema??"SYSDBA"))
                .Append(".")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            var typeMapping = Dependencies.TypeMappingSource.GetMapping(operation.ClrType);

            //if (operation.ClrType != typeof(long))
            //{
            //    builder
            //        .Append(" AS ")
            //        .Append(typeMapping.StoreType);

            //    // set the typeMapping for use with operation.StartValue (i.e. a long) below
            //    typeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(long));
            //}

            builder
                .Append(" START WITH ")
                .Append(typeMapping.GenerateSqlLiteral(operation.StartValue));

            SequenceOptions(operation, model, builder);

            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }


        /// <summary>
        ///     Builds commands for the given <see cref="XGCreateDatabaseOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Generate(
            [NotNull] XGCreateDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE DATABASE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (operation.CharSet != null)
            {
                builder
                    .Append(" CHARACTER SET ")
                    .Append(operation.CharSet);
            }

            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand();
        }

        /// <summary>
        ///     Builds commands for the given <see cref="XGDropDatabaseOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected virtual void Generate(
            [NotNull] XGDropDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP DATABASE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(Dependencies.SqlGenerationHelper.StatementTerminator)
                .AppendLine(Dependencies.SqlGenerationHelper.BatchTerminator);
            EndStatement(builder);
        }

        protected override void Generate(AlterDatabaseOperation operation, IModel model, MigrationCommandListBuilder builder)
        {

        }

        protected override void Generate(
            [NotNull] DropIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX IF EXISTS ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(".")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (terminate)
            {
                builder
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                    .EndCommand();
            }
        }

        protected override void Generate(
            DropUniqueConstraintOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
            => Generate(
                new XGDropUniqueConstraintAndRecreateForeignKeysOperation
                {
                    IsDestructiveChange = operation.IsDestructiveChange,
                    Name = operation.Name,
                    Schema = operation.Schema,
                    Table = operation.Table,
                    RecreateForeignKeys = false,
                },
                model,
                builder);

        protected virtual void Generate(
            XGDropUniqueConstraintAndRecreateForeignKeysOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            void DropUniqueKey()
            {
                builder.Append("ALTER TABLE ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" DROP CONSTRAINT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                EndStatement(builder);
            }

            // A foreign key might reuse the alternate key for its own purposes and prohibit its deletion,
            // if the foreign key columns are listed as the first columns and in the same order as in the foreign key (#678).
            // We therefore drop and later recreate all foreign keys to ensure, that no other dependencies on the
            // alternate key exist, if explicitly requested by the user via `XGMigrationBuilderExtensions.DropUniqueConstraint()`.
            // This particularily targets FK contraints, that are on the same table as the PK and might reuse indexes already used by the PK.
            // A common case is a many-to-many relationship table, with a PK containing the 2 FK columns.
            if (operation.RecreateForeignKeys)
            {
                TemporarilyDropForeignKeys(
                    model,
                    builder,
                    operation.Schema,
                    operation.Table,
                    DropUniqueKey);
            }
            else
            {
                DropUniqueKey();
            }
        }

        protected void TemporarilyDropForeignKeys(
            IModel model,
            MigrationCommandListBuilder builder,
            string schemaName,
            string tableName,
            Action action)
        {
            var foreignKeys = model.GetRelationalModel()
                .FindTable(tableName, schemaName)
                ?.ForeignKeyConstraints
                .ToArray() ?? Array.Empty<IForeignKeyConstraint>();

            foreach (var foreignKey in foreignKeys)
            {
                Generate(new DropForeignKeyOperation
                {
                    Schema = foreignKey.Table.Schema,
                    Table = foreignKey.Table.Name,
                    Name = foreignKey.Name,
                }, model, builder);
            }

            action();

            foreach (var foreignKey in foreignKeys)
            {
                Generate(new AddForeignKeyOperation
                {
                    Schema = foreignKey.Table.Schema,
                    Table = foreignKey.Table.Name,
                    Name = foreignKey.Name,
                    Columns = foreignKey.Columns.Select(c => c.Name).ToArray(),
                    PrincipalSchema = foreignKey.PrincipalTable.Schema,
                    PrincipalTable = foreignKey.PrincipalTable.Name,
                    PrincipalColumns = foreignKey.PrincipalColumns.Select(c => c.Name).ToArray(),
                    OnDelete = foreignKey.OnDeleteAction,
                }, model, builder);
            }
        }

        protected static ReferentialAction ToReferentialAction(DeleteBehavior deleteBehavior)
        {
            switch (deleteBehavior)
            {
                case DeleteBehavior.SetNull:
                    return ReferentialAction.SetNull;
                case DeleteBehavior.Cascade:
                    return ReferentialAction.Cascade;
                case DeleteBehavior.NoAction:
                case DeleteBehavior.ClientNoAction:
                    return ReferentialAction.NoAction;
                default:
                    return ReferentialAction.Restrict;
            }
        }

        /// <summary>
        ///     Builds commands for the given <see cref="DropForeignKeyOperation" /> by making calls on the given
        ///     <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        /// <param name="terminate"> Indicates whether or not to terminate the command after generating SQL for the operation. </param>
        protected override void Generate(
            [NotNull] DropForeignKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP CONSTRAINT ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected override void Generate(
            AddColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");

            ColumnDefinition(operation, model, builder);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            if (!string.IsNullOrEmpty(operation.Comment))
            {
                builder.Append("COMMENT ON COLUMN ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(".")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" IS '")
                    .Append(operation.Comment)
                    .Append("'");
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }
            EndStatement(builder);
        }

        // CHECK: Can we improve this implementation?
        /// <summary>
        ///     Builds commands for the given <see cref="RenameColumnOperation" />
        ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to build the commands. </param>
        protected override void Generate(
            RenameColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));

            if (_options.ServerVersion.Supports.RenameColumn)
            {
                builder.Append(" RENAME COLUMN ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" TO ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                EndStatement(builder);
                return;
            }

            builder.Append(" CHANGE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ");

            var column = model?.GetRelationalModel().FindTable(operation.Table, operation.Schema).FindColumn(operation.NewName);
            if (column == null)
            {
                if (!(operation[RelationalAnnotationNames.ColumnType] is string type))
                {
                    throw new InvalidOperationException(
                        $"Could not find the column: {Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema)}.{Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName)}. Specify the column type explicitly on 'RenameColumn' using the \"{RelationalAnnotationNames.ColumnType}\" annotation");
                }

                builder
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                    .Append(" ")
                    .Append(type)
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

                EndStatement(builder);
                return;
            }

            var typeMapping = column.PropertyMappings.FirstOrDefault()?.TypeMapping;
            var converter = typeMapping?.Converter;
            var clrType = (converter?.ProviderClrType ?? typeMapping?.ClrType).UnwrapNullableType();
            var columnType = (string)(operation[RelationalAnnotationNames.ColumnType]
                                      ?? column[RelationalAnnotationNames.ColumnType]);
            var isNullable = column.IsNullable;

            var defaultValue = column.DefaultValue;
            defaultValue = converter != null
                ? converter.ConvertToProvider(defaultValue)
                : defaultValue;
            defaultValue = (defaultValue == DBNull.Value ? null : defaultValue)
                           ?? (isNullable
                               ? null
                               : clrType == typeof(string)
                                   ? string.Empty
                                   : clrType.IsArray
                                       ? Array.CreateInstance(clrType.GetElementType(), 0)
                                       : clrType.GetDefaultValue());

            var isRowVersion = (clrType == typeof(DateTime) || clrType == typeof(byte[])) &&
                               column.IsRowVersion;

            var addColumnOperation = new AddColumnOperation
            {
                Schema = operation.Schema,
                Table = operation.Table,
                Name = operation.NewName,
                ClrType = clrType,
                ColumnType = columnType,
                IsUnicode = column.IsUnicode,
                MaxLength = column.MaxLength,
                IsFixedLength = column.IsFixedLength,
                IsRowVersion = isRowVersion,
                IsNullable = isNullable,
                DefaultValue = defaultValue,
                DefaultValueSql = column.DefaultValueSql,
                ComputedColumnSql = column.ComputedColumnSql,
                IsStored = column.IsStored,
                Comment = column.Comment
            };

            ColumnDefinition(
                addColumnOperation,
                model,
                builder);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            if (!string.IsNullOrEmpty(addColumnOperation.Comment))
            {
                builder.Append("COMMENT ON COLUMN ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(".")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" IS '")
                    .Append(addColumnOperation.Comment)
                    .Append("'");
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }
            EndStatement(builder);
        }

        /// <summary>
        ///     Generates a SQL fragment configuring a sequence with the given options.
        /// </summary>
        /// <param name="schema"> The schema that contains the sequence, or <see langword="null"/> to use the default schema. </param>
        /// <param name="name"> The sequence name. </param>
        /// <param name="operation"> The sequence options. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void SequenceOptions(
            string schema,
            string name,
            SequenceOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(" INCREMENT BY ")
                .Append(IntegerConstant(operation.IncrementBy));

            if (operation.MinValue.HasValue)
            {
                builder
                    .Append(" MINVALUE ")
                    .Append(IntegerConstant(operation.MinValue.Value));
            }
            else
            {
                builder.Append(" NOMINVALUE");
            }

            if (operation.MaxValue.HasValue)
            {
                builder
                    .Append(" MAXVALUE ")
                    .Append(IntegerConstant(operation.MaxValue.Value));
            }
            else
            {
                builder.Append(" NOMAXVALUE");
            }

            builder.Append(operation.IsCyclic ? " CYCLE" : " NOCYCLE");
        }

        /// <summary>
        ///     Generates a SQL fragment for a column definition in an <see cref="AddColumnOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ColumnDefinition(AddColumnOperation operation, IModel model,
            MigrationCommandListBuilder builder)
            => ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation,
                model,
                builder);

        /// <summary>
        ///     Generates a SQL fragment for a column definition for the given column metadata.
        /// </summary>
        /// <param name="schema"> The schema that contains the table, or <see langword="null"/> to use the default schema. </param>
        /// <param name="table"> The table that contains the column. </param>
        /// <param name="name"> The column name. </param>
        /// <param name="operation"> The column metadata. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ColumnDefinition(
            [CanBeNull] string schema,
            [NotNull] string table,
            [NotNull] string name,
            [NotNull] ColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var matchType = GetColumnType(schema, table, name, operation, model);
            var matchLen = "";
            var match = _typeRegex.Match(matchType ?? "-");
            if (match.Success)
            {
                matchType = match.Groups[1].Value.ToLower();
                if (!string.IsNullOrWhiteSpace(match.Groups[2].Value))
                {
                    matchLen = match.Groups[2].Value;
                }
            }

            var valueGenerationStrategy = XGValueGenerationStrategyCompatibility.GetValueGenerationStrategy(operation.GetAnnotations().OfType<IAnnotation>().ToArray());

            var autoIncrement = false;
            if (valueGenerationStrategy == XGValueGenerationStrategy.IdentityColumn &&
                string.IsNullOrWhiteSpace(operation.DefaultValueSql) && operation.DefaultValue == null)
            {
                switch (matchType)
                {
                    case "tinyint":
                    case "smallint":
                    case "mediumint":
                    case "int":
                    case "bigint":
                        autoIncrement = true;
                        break;
                    case "guid":
                        autoIncrement = true;
                        _guidIdentityMap.Add(string.IsNullOrEmpty(operation.Schema) ? "SYSDBA" : operation.Schema + "." + operation.Name, name);
                        break;
                    case "datetime":
                        if (!_options.ServerVersion.Supports.DateTimeCurrentTimestamp)
                        {
                            throw new InvalidOperationException(
                                $"Error in {table}.{name}: DATETIME does not support values generated " +
                                $"on Add or Update in server version {_options.ServerVersion}. Try explicitly setting the column type to TIMESTAMP.");
                        }

                        goto case "timestamp";
                    case "timestamp":
                        operation.DefaultValueSql = $"CURRENT_TIMESTAMP";
                        break;
                }
            }

            string onUpdateSql = null;
            if (operation.IsRowVersion || valueGenerationStrategy == XGValueGenerationStrategy.ComputedColumn)
            {
                switch (matchType)
                {
                    case "datetime":
                        if (!_options.ServerVersion.Supports.DateTimeCurrentTimestamp)
                        {
                            throw new InvalidOperationException(
                                $"Error in {table}.{name}: DATETIME does not support values generated " +
                                $"on Add or Update in server version {_options.ServerVersion}. Try explicitly setting the column type to TIMESTAMP.");
                        }

                        goto case "timestamp";
                    case "timestamp":
                        if (string.IsNullOrWhiteSpace(operation.DefaultValueSql) && operation.DefaultValue == null)
                        {
                            operation.DefaultValueSql = $"CURRENT_TIMESTAMP";
                        }

                        onUpdateSql = $"CURRENT_TIMESTAMP";
                        break;
                }
            }
            ColumnDefinitionWithCharSet(schema, table, name, operation, model, builder);


            if (autoIncrement)
            {
                builder.Append(" IDENTITY");
            }
            builder.Append(operation.IsNullable ? " NULL" : " NOT NULL");

            //GenerateComment(operation.Comment, builder);
        }

        private void GenerateComment(string comment, MigrationCommandListBuilder builder)
        {
            if (comment == null)
                return;

            builder.Append(" COMMENT ")
                .Append($"'{comment}'"/*_stringTypeMapping.GenerateSqlLiteral(comment)*/);
        }

        private void ColumnDefinitionWithCharSet(string schema, string table, string name, ColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            //if (operation.ComputedColumnSql != null)
            //{
            //    ComputedColumnDefinition(schema, table, name, operation, model, builder);
            //    return;
            //}

            var columnType = GetColumnType(schema, table, name, operation, model);
            if (columnType == "time(6)")
            {
                columnType = "time";
            }
            if (columnType == "bit(1)" || columnType == "tinyint(1)" || columnType == "clob(1)")
            {
                columnType = columnType.Replace("(1)", "");
            }

            builder
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
                .Append(" ")
                .Append(columnType);



            var isSpatialStoreType = IsSpatialStoreType(columnType);

            if (columnType.IndexOf("blob", StringComparison.OrdinalIgnoreCase) < 0 &&
                columnType.IndexOf("text", StringComparison.OrdinalIgnoreCase) < 0 &&
                columnType.IndexOf("json", StringComparison.OrdinalIgnoreCase) < 0 &&
                !isSpatialStoreType)
            {
                DefaultValue(operation.DefaultValue, operation.DefaultValueSql, columnType, builder);
            }

            var srid = operation[XGAnnotationNames.SpatialReferenceSystemId];
            if (srid is int &&
                isSpatialStoreType)
            {
                builder.Append($" /*!80003 SRID {srid} */");
            }
        }

        protected override string GetColumnType(string schema, string table, string name, ColumnOperation operation, IModel model)
            => GetColumnTypeWithCharSetAndCollation(
                operation,
                operation.ColumnType ?? base.GetColumnType(schema, table, name, operation, model));

        private static string GetColumnTypeWithCharSetAndCollation(ColumnOperation operation, string columnType)
        {
            if (columnType.IndexOf("json", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return columnType;
            }

            return columnType;
        }

        protected override void DefaultValue(object defaultValue, string defaultValueSql, string columnType, MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if (defaultValueSql != null)
            {
                builder
                    .Append(" DEFAULT ")
                    .Append(defaultValueSql);
            }
            else if (defaultValue != null)
            {
                var typeMapping = Dependencies.TypeMappingSource.GetMappingForValue(defaultValue);
                builder
                    .Append(" DEFAULT ")
                    .Append(typeMapping.GenerateSqlLiteral(defaultValue));
            }
        }

        /// <summary>
        ///     Generates a SQL fragment for the primary key constraint of a <see cref="CreateTableOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void CreateTablePrimaryKeyConstraint(
            [NotNull] CreateTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var primaryKey = operation.PrimaryKey;
            if (primaryKey == null || operation.Columns.Where(i => i.ColumnType!=null && i.ColumnType.Contains("binary")).Select(i => i.Name).Any(primaryKey.Columns.Contains)) return;
            if (primaryKey != null)
            {
                builder.AppendLine(",");

                // XG InnoDB has the requirement, that an AUTO_INCREMENT column has to be the first
                // column participating in an index.

                var sortedColumnNames = primaryKey.Columns.Length > 1
                    ? primaryKey.Columns
                        .Select(columnName => operation.Columns.First(co => co.Name == columnName))
                        .OrderBy(co => co[XGAnnotationNames.ValueGenerationStrategy] is XGValueGenerationStrategy generationStrategy
                                       && generationStrategy == XGValueGenerationStrategy.IdentityColumn
                            ? 0
                            : 1)
                        .Select(co => co.Name)
                        .ToArray()
                    : primaryKey.Columns;

                var sortedPrimaryKey = new AddPrimaryKeyOperation()
                {
                    Schema = primaryKey.Schema,
                    Table = primaryKey.Table,
                    Name = primaryKey.Name,
                    Columns = sortedColumnNames,
                    IsDestructiveChange = primaryKey.IsDestructiveChange,
                };

                foreach (var annotation in primaryKey.GetAnnotations())
                {
                    sortedPrimaryKey[annotation.Name] = annotation.Value;
                }

                PrimaryKeyConstraint(
                    sortedPrimaryKey,
                    model,
                    builder);
            }
        }

        protected override void PrimaryKeyConstraint(
            [NotNull] AddPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("PRIMARY KEY ");

            IndexTraits(operation, model, builder);

            builder.Append("(")
                .Append(ColumnListWithIndexPrefixLength(operation, operation.Columns))
                .Append(")");
        }

        protected override void UniqueConstraint(
            [NotNull] AddUniqueConstraintOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.Name != null)
            {
                builder
                    .Append("CONSTRAINT ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" ");
            }

            builder
                .Append("UNIQUE ");

            IndexTraits(operation, model, builder);

            builder.Append("(")
                .Append(ColumnListWithIndexPrefixLength(operation, operation.Columns))
                .Append(")");
        }

        protected override void Generate(AddPrimaryKeyOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");
            PrimaryKeyConstraint(operation, model, builder);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            if (operation.Columns.Length == 1)
            {
                builder.Append(
                    $"EXECUTE IMMEDIATE 'EXEC XuGu_AFTER_ADD_PRIMARY_KEY('{_stringTypeMapping.GenerateSqlLiteral(operation.Schema??"SYSDBA")}', '{_stringTypeMapping.GenerateSqlLiteral(operation.Table)}', '{_stringTypeMapping.GenerateSqlLiteral(operation.Columns.First())}');';");

                builder.AppendLine();
            }

            EndStatement(builder);
        }

        protected override void Generate(
            [NotNull] DeleteDataOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var sqlBuilder = new StringBuilder();
            foreach (var modificationCommand in GenerateModificationCommands(operation, model))
            {
                SqlGenerator.AppendDeleteOperation(
                    sqlBuilder,
                    modificationCommand,
                    0);
            }

            builder.Append(sqlBuilder.ToString());
            EndStatement(builder);
        }

        protected override void Generate(
            DropPrimaryKeyOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
            => Generate(
                new XGDropPrimaryKeyAndRecreateForeignKeysOperation
                {
                    IsDestructiveChange = operation.IsDestructiveChange,
                    Name = operation.Name,
                    Schema = operation.Schema,
                    Table = operation.Table,
                    RecreateForeignKeys = false,
                },
                model,
                builder,
                terminate);

        protected virtual void Generate(
            XGDropPrimaryKeyAndRecreateForeignKeysOperation operation,
            IModel model,
            MigrationCommandListBuilder builder,
            bool terminate = true)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            void DropPrimaryKey()
            {
                builder
                    .AppendLine("BEGIN")
                    .AppendLine($"IF (SELECT COUNT(CONS_NAME) FROM ALL_CONSTRAINTS WHERE CONS_TYPE='P' AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='{operation.Table}' AND SCHEMA_ID=(SELECT SCHEMA_ID FROM ALL_SCHEMAS WHERE SCHEMA_NAME='{operation.Schema??"SYSDBA"}' LIMIT 1) LIMIT 1) LIMIT 1)>0 THEN")
                    .AppendLine($"EXECUTE IMMEDIATE 'ALTER TABLE `{operation.Table}` DROP CONSTRAINT '||(SELECT CONS_NAME FROM ALL_CONSTRAINTS WHERE CONS_TYPE='P' AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='{operation.Table}' AND SCHEMA_ID=(SELECT SCHEMA_ID FROM ALL_SCHEMAS WHERE SCHEMA_NAME='{operation.Schema ?? "SYSDBA"}' LIMIT 1) LIMIT 1) LIMIT 1);")
                    .AppendLine("END IF;")
                    .AppendLine("END;");

                //if (terminate)
                //{
                //    builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                //    EndStatement(builder);
                //}
            }

            // A foreign key might reuse the primary key for its own purposes and prohibit its deletion,
            // if the foreign key columns are listed as the first columns and in the same order as in the foreign key (#678).
            // We therefore drop and later recreate all foreign keys to ensure, that no other dependencies on the
            // primary key exist, if explicitly requested by the user via `XGMigrationBuilderExtensions.DropPrimaryKey()`.
            if (operation.RecreateForeignKeys)
            {
                TemporarilyDropForeignKeys(
                    model,
                    builder,
                    operation.Schema,
                    operation.Table,
                    DropPrimaryKey);
            }
            else
            {
                DropPrimaryKey();
            }
        }

        /// <summary>
        ///     Generates a SQL fragment for a foreign key constraint of an <see cref="AddForeignKeyOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ForeignKeyConstraint(
            AddForeignKeyOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            operation.Name = Truncate(operation.Name, 64);
            base.ForeignKeyConstraint(operation, model, builder);
        }

        /// <summary>
        ///     Generates a SQL fragment for traits of an index from a <see cref="CreateIndexOperation" />,
        ///     <see cref="AddPrimaryKeyOperation" />, or <see cref="AddUniqueConstraintOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model which may be <see langword="null"/> if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void IndexTraits(MigrationOperation operation, IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            //var fullText = operation[XGAnnotationNames.FullTextIndex] as bool?;
            //if (fullText == true)
            //{
            //    builder.Append("FULLTEXT ");
            //}

            var spatial = operation[XGAnnotationNames.SpatialIndex] as bool?;
            if (spatial == true)
            {
                builder.Append("SPATIAL ");
            }
        }

        protected override void IndexOptions(CreateIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            // The base implementation supports index filters in form of a WHERE clause.
            // This is not supported by XG, so we don't call it here.

            var fullText = operation[XGAnnotationNames.FullTextIndex] as bool?;
            if (fullText == true)
            {
                var fullTextParser = operation[XGAnnotationNames.FullTextParser] as string;
                if (!string.IsNullOrEmpty(fullTextParser))
                {
                    builder.AppendLine(" USING VOCABLE TABLE 'vocab_table'")
                        .AppendLine("USING FILTER 'default_filter'")
                        .Append("USING LEXER 'default_lexer'");
                }
            }
        }

        /// <summary>
        ///     Generates a SQL fragment for the given referential action.
        /// </summary>
        /// <param name="referentialAction"> The referential action. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ForeignKeyAction(ReferentialAction referentialAction,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if (referentialAction == ReferentialAction.Restrict)
            {
                builder.Append("RESTRICT");
            }
            else
            {
                base.ForeignKeyAction(referentialAction, builder);
            }
        }

        private string ColumnListWithIndexPrefixLength(MigrationOperation operation, string[] columns)
            => operation[XGAnnotationNames.IndexPrefixLength] is int[] prefixValues
                ? ColumnList(
                    columns,
                    (c, i) => prefixValues.Length > i && prefixValues[i] > 0
                        ? $"({prefixValues[i]})"
                        : null)
                : ColumnList(columns);

        private string ColumnListWithIndexOrder(CreateIndexOperation operation, string[] columns)
        {
            string result = "";
            if (operation.IsDescending!=null && operation.IsDescending is bool[] orderValues)
            {
                result = string.Join(", ", columns.Select((c, i) => $"{Dependencies.SqlGenerationHelper.DelimitIdentifier(c)}{(orderValues.Length>i? (orderValues[i] ? " DESC" : " ASC"):" DESC")}"));
            }
            else
            {
                result = ColumnList(columns);
            }
            return result;
        }

        protected virtual string ColumnList([NotNull] string[] columns, Func<string, int, string> columnPostfix)
            => string.Join(", ", columns.Select((c, i) => Dependencies.SqlGenerationHelper.DelimitIdentifier(c) + columnPostfix?.Invoke(c, i)));

        private string IntegerConstant(long? value)
            => string.Format(CultureInfo.InvariantCulture, "{0}", value);

        private static string Truncate(string source, int maxLength)
        {
            if (source == null
                || source.Length <= maxLength)
            {
                return source;
            }

            return source.Substring(0, maxLength);
        }

        private static bool IsSpatialStoreType(string storeType)
            => _spatialStoreTypes.Contains(storeType);
    }
}
