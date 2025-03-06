// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class XuGuDbMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private int _variableCounter;
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;

        public XuGuDbMigrationsSqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IRelationalTypeMapper typeMapper,
            [NotNull] IRelationalAnnotationProvider annotations)
            : base(commandBuilderFactory, sqlGenerationHelper, typeMapper, annotations)
        {
            _commandBuilderFactory = commandBuilderFactory;
        }

        public virtual IReadOnlyList<MigrationCommand> Generate(
            IReadOnlyList<MigrationOperation> operations,
            bool hasEnd,
            IModel model = null)
        {
            Check.NotNull(operations, nameof(operations));

            var builder = new MigrationCommandListBuilder(_commandBuilderFactory);
            foreach (var operation in operations)
            {
                Generate(operation, model, builder);
            }
            if (hasEnd)
            {
                builder
                    .EndCommand()
                    .Append("END")
                    .Append(SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }

            return builder.GetCommandList();
        }

        protected override void Generate(MigrationOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation is CreateIndexOperation createIndexOperation)
            {
                var indexColumns = createIndexOperation.Columns;
                var primaryKey=model.GetEntityTypes().FirstOrDefault(i=>i.GetAnnotation("Relational:TableName").Value.ToString() ==createIndexOperation.Table).FindPrimaryKey();
                if (indexColumns.Count()==1 && indexColumns.Any(i=>i==primaryKey.Properties.First().Name))
                {
                    return;
                }
            }

            var createDatabaseOperation = operation as XuGuDbCreateDatabaseOperation;
            var dropDatabaseOperation = operation as XuGuDbDropDatabaseOperation;
            var createSchemaOperation=operation as XuGuDbCreateSchemaOperation;
            if (createDatabaseOperation != null)
            {
                Generate(createDatabaseOperation, model, builder);
            }
            else if (dropDatabaseOperation != null)
            {
                Generate(dropDatabaseOperation, model, builder);
            }
            else if (createSchemaOperation != null)
            {
                Generate(createSchemaOperation, model, builder);
            }
            else
            {
                base.Generate(operation, model, builder);
            }
        }

        protected override void Generate(
            AlterColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);

            builder
                .Append("ALTER TABLE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ALTER COLUMN ");

            ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation.ClrType,
                operation.ColumnType,
                operation.IsUnicode,
                operation.MaxLength,
                operation.IsRowVersion,
                operation.IsNullable,
                operation.DefaultValue,
                /*identity:*/ false,
                operation,
                model,
                builder);

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);

            if (operation.DefaultValue != null)
            {
                builder
                    .Append("ALTER TABLE ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                    .Append(" ALTER COLUMN ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                    .Append(" SET");
                DefaultValue(operation.DefaultValue, builder);
                builder
                    .AppendLine(SqlGenerationHelper.StatementTerminator);
            }

            EndStatement(builder);
        }

        protected override void Generate(
            RenameIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER INDEX ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(".")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" RENAME TO ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(
            RenameTableOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .Append(" RENAME TO ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.NewName))
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(
            [NotNull] CreateIndexOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append("CREATE ");

            if (operation.IsUnique)
            {
                builder.Append("UNIQUE ");
            }

            builder
                .Append("INDEX ")
                .Append("IF NOT EXISTS ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" ON ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" (")
                .Append(ColumnList(operation.Columns))
                .Append(")");

            
            builder.AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(
            [NotNull] CreateSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE SEQUENCE IF NOT EXISTS ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

            //if (operation.ClrType != typeof(long))
            //{
            //    builder
            //        .Append(" AS ")
            //        .Append(TypeMapper.GetMapping(operation.ClrType).StoreType);
            //}

            builder
                .Append(" START WITH ")
                .Append(SqlGenerationHelper.GenerateLiteral(operation.StartValue));

            SequenceOptions(operation, model, builder);

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void SequenceOptions(
            [NotNull] AlterSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder) =>
                SequenceOptions(
                    operation.Schema,
                    operation.Name,
                    operation.IncrementBy,
                    operation.MinValue,
                    operation.MaxValue,
                    operation.IsCyclic,
                    model,
                    builder);

        protected override void SequenceOptions(
            [NotNull] CreateSequenceOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder) =>
                SequenceOptions(
                    operation.Schema,
                    operation.Name,
                    operation.IncrementBy,
                    operation.MinValue,
                    operation.MaxValue,
                    operation.IsCyclic,
                    model,
                    builder);

        protected override void SequenceOptions(
            [CanBeNull] string schema,
            [NotNull] string name,
            int increment,
            long? minimumValue,
            long? maximumValue,
            bool cycle,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(increment, nameof(increment));
            Check.NotNull(cycle, nameof(cycle));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(" INCREMENT BY ")
                .Append(SqlGenerationHelper.GenerateLiteral(increment));

            if (minimumValue != null)
            {
                builder
                    .Append(" MINVALUE ")
                    .Append(SqlGenerationHelper.GenerateLiteral(minimumValue));
            }
            else
            {
                builder.Append(" NOMINVALUE");
            }

            if (maximumValue != null)
            {
                builder
                    .Append(" MAXVALUE ")
                    .Append(SqlGenerationHelper.GenerateLiteral(maximumValue));
            }
            else
            {
                builder.Append(" NOMAXVALUE");
            }

            builder.Append(cycle ? " CYCLE" : " NOCYCLE");
        }

        protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            bool type = operation is XuGuDbCreateSchemaOperation;

            if (string.Equals(operation.Name, "SYSDBA", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .AppendLine("BEGIN")
                .Append("IF NOT EXISTS (SELECT 1 FROM `ALL_SCHEMAS` WHERE `SCHEMA_NAME` = '")
                .Append(operation.Name)
                .AppendLine("') THEN")
                .Append("CREATE SCHEMA ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .Append("END IF")
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            if (type)
            {
                builder
                    .Append("END")
                    .Append(SqlGenerationHelper.StatementTerminator);
            }

            EndStatement(builder);
        }

        protected virtual void Generate(
            [NotNull] XuGuDbCreateDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .EndCommand()
                .Append($"CREATE DATABASE ")
                .Append($"IF NOT EXISTS ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .EndCommand(suppressTransaction: true);

            EndStatement(builder, suppressTransaction: true);
        }

        protected virtual void Generate(
            [NotNull] XuGuDbDropDatabaseOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP DATABASE IF EXISTS ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder, suppressTransaction: true);
        }

        protected override void Generate(
            DropIndexOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("DROP INDEX ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(".")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(
            DropColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            DropDefaultConstraint(operation.Schema, operation.Table, operation.Name, builder);
            builder
                .Append("ALTER TABLE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" DROP COLUMN ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name))
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(
            RenameColumnOperation operation,
            IModel model,
            MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" RENAME ");

            Rename(operation.Name, operation.NewName, "COLUMN", builder);
            EndStatement(builder);
        }

        protected override void Generate(
            [NotNull] AddColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");

            ColumnDefinition(operation, model, builder);
            

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(
            [NotNull] AddPrimaryKeyOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("ALTER TABLE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ADD ");
            PrimaryKeyConstraint(operation, model, builder);
            builder.AppendLine(SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }

        protected override void Generate(
            [NotNull] CreateTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append("CREATE TABLE IF NOT EXISTS ")
                .Append(SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
                .AppendLine(" (");

            using (builder.Indent())
            {
                for (var i = 0; i < operation.Columns.Count; i++)
                {
                    var column = operation.Columns[i];
                    ColumnDefinition(column, model, builder);

                    if (i != operation.Columns.Count - 1)
                    {
                        builder.AppendLine(",");
                    }
                }

                if (operation.PrimaryKey != null)
                {
                    builder.AppendLine(",");
                    PrimaryKeyConstraint(operation.PrimaryKey, model, builder);
                }

                foreach (var uniqueConstraint in operation.UniqueConstraints)
                {
                    builder.AppendLine(",");
                    UniqueConstraint(uniqueConstraint, model, builder);
                }

                foreach (var foreignKey in operation.ForeignKeys)
                {
                    builder.AppendLine(",");
                    ForeignKeyConstraint(foreignKey, model, builder);
                }

                builder.AppendLine();
            }

            builder
                .Append(")")
                .AppendLine(SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
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
                    .Append(operation.Name)
                    .Append(" ");
            }

            builder
                .Append("PRIMARY KEY ");

            builder.Append("(")
                .Append(ColumnList(operation.Columns))
                .Append(")");
        }

        private string ColumnList(string[] columns) => string.Join(", ", columns.Select(SqlGenerationHelper.DelimitIdentifier));

        protected override void ColumnDefinition(
            [NotNull] AddColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder) =>
                ColumnDefinition(
                operation.Schema,
                operation.Table,
                operation.Name,
                operation.ClrType,
                operation.ColumnType,
                operation.IsUnicode,
                operation.MaxLength,
                operation.IsRowVersion,
                operation.IsNullable,
                operation.DefaultValue,
                GetIdentity(operation),
                operation,
                model,
                builder);

        protected virtual bool GetIdentity([NotNull] IAnnotatable annotatable)
        {
            if (annotatable.FindAnnotation(XuGuDbFullAnnotationNames.Instance.ValueGenerationStrategy)!=null)
            {
                return (XuGuDbValueGenerationStrategy)annotatable[XuGuDbFullAnnotationNames.Instance.ValueGenerationStrategy] == XuGuDbValueGenerationStrategy.IdentityColumn;
            };
            return false;
        }

        protected virtual void DefaultValue(
            [CanBeNull] object defaultValue,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(" DEFAULT ")
                .Append(SqlGenerationHelper.GenerateLiteral(defaultValue));
        }

        protected virtual void ColumnDefinition(
            [CanBeNull] string schema,
            [NotNull] string table,
            [NotNull] string name,
            [NotNull] Type clrType,
            [CanBeNull] string type,
            [CanBeNull] bool? unicode,
            [CanBeNull] int? maxLength,
            bool rowVersion,
            bool nullable,
            [CanBeNull] object defaultValue,
            bool identity,
            [NotNull] IAnnotatable annotatable,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(clrType, nameof(clrType));
            Check.NotNull(annotatable, nameof(annotatable));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(SqlGenerationHelper.DelimitIdentifier(name))
                .Append(" ")
                .Append((type ?? GetColumnType(schema, table, name, clrType, unicode, maxLength, rowVersion, model)).ToUpper());

            if (identity)
            {
                builder.Append(" IDENTITY");
            }

            if (!nullable)
            {
                builder.Append(" NOT NULL");
            }
            if (defaultValue!= null)
            {
                DefaultValue(defaultValue, builder);
            }
        }

        protected virtual void Rename(
            [NotNull] string name,
            [NotNull] string newName,
            [CanBeNull] string type,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(newName, nameof(newName));
            Check.NotNull(builder, nameof(builder));

            builder
                .Append(type)
                .Append(" ")
                .Append(SqlGenerationHelper.DelimitIdentifier(name))
                .Append(" TO ")
                .Append(SqlGenerationHelper.DelimitIdentifier(newName));

            builder.AppendLine(SqlGenerationHelper.StatementTerminator);
        }

        protected override void ForeignKeyAction(ReferentialAction referentialAction, MigrationCommandListBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            if (referentialAction == ReferentialAction.Restrict)
            {
                builder.Append("NO ACTION");
            }
            else
            {
                base.ForeignKeyAction(referentialAction, builder);
            }
        }

        protected virtual void DropDefaultConstraint(
            [CanBeNull] string schema,
            [NotNull] string tableName,
            [NotNull] string columnName,
            [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(tableName, nameof(tableName));
            Check.NotEmpty(columnName, nameof(columnName));
            Check.NotNull(builder, nameof(builder));

            builder
                .AppendLine("DECLARE")
                .AppendLine("CURSOR fk_cursor IS")
                .Append("(SELECT `c`.`CONS_NAME` FROM `sys_constraints` `c` INNER JOIN `sys_tables` `t` ON `c`.`TABLE_ID`=`t`.`TABLE_ID` WHERE `t`.`TABLE_ID`=(SELECT `TABLE_ID` FROM `sys_tables` WHERE `TABLE_NAME`= ")
                .Append(SqlGenerationHelper.GenerateLiteral(tableName));

            if (!string.IsNullOrWhiteSpace(schema) && !string.IsNullOrEmpty(schema))
            {
                builder
                .Append(" AND `SCHEMA_ID` IN (SELECT `SCHEMA_ID` FROM `sys_schemas` WHERE `SCHEMA_NAME`=")
                .Append(SqlGenerationHelper.GenerateLiteral(schema))
                .Append(")");
            }

            builder
                .Append(") AND `c`.`DEFINE` LIKE '%\"")
                .Append(columnName)
                .AppendLine("\"%');")
                .AppendLine("sqlstr VARCHAR;")
                .AppendLine("BEGIN")
                .AppendLine("FOR fk IN fk_cursor LOOP")
                .Append("sqlstr:='ALTER TABLE ")
                .Append(SqlGenerationHelper.DelimitIdentifier(tableName, schema))
                .AppendLine(" DROP CONSTRAINT ' || `fk`.`CONS_NAME`;")
                .AppendLine("EXECUTE IMMEDIATE sqlstr;")
                .AppendLine("END LOOP;")
                .AppendLine("END;");
        }
    }
}
