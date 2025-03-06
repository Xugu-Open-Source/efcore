// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         SQL Server-specific implementation of <see cref="MigrationsSqlGenerator" />.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
    ///     <see href="https://aka.ms/efcore-docs-xugu">Accessing SQL Server and SQL Azure databases with EF Core</see>
    ///     for more information.
    /// </remarks>
    internal class XuguMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        private static readonly Regex _typeRegex = new Regex(@"([a-z0-9]+)\s*?(?:\(\s*(\d+)?\s*\))?",
        RegexOptions.IgnoreCase);

        private readonly RelationalTypeMapping _typeMapper;
        private readonly IRelationalAnnotationProvider _annotationProvider;

        public XuguMigrationsSqlGenerator(
        [NotNull] MigrationsSqlGeneratorDependencies dependencies,
        [NotNull] IRelationalAnnotationProvider annotationProviders)
          : base(dependencies)
        {
            _annotationProvider = annotationProviders;
            _typeMapper = dependencies.TypeMappingSource.GetMapping(typeof(string));
        }

        protected override void Generate(
        MigrationOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            switch (operation)
            {
                case XuguCreateDatabaseOperation createDatabaseOperation:
                    Generate(createDatabaseOperation, model, builder);
                    break;
                case XuguDropDatabaseOperation dropDatabaseOperation:
                    Generate(dropDatabaseOperation, model!, builder);
                    break;
                case XuguDropPrimaryKeyAndRecreateForeignKeysOperation dropPrimaryKeyAndRecreateForeignKeysOperation:
                    Generate(dropPrimaryKeyAndRecreateForeignKeysOperation, model, builder);
                    break;
                default:
                    base.Generate(operation, model, builder);
                    break;
            }
        }

        protected override void Generate(RenameColumnOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder.Append("ALTER TABLE ")
              .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema));

            builder.Append(" CHANGE ")
              .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
              .Append(" ");

            var column = model?.GetRelationalModel().FindTable(operation.Table, operation.Schema)?.FindColumn(operation.NewName);
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
#pragma warning disable CS8604 // 引用类型参数可能为 null。
            var clrType = (converter?.ProviderClrType ?? typeMapping?.ClrType).UnwrapNullableType();
#pragma warning restore CS8604 // 引用类型参数可能为 null。
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
            var columnType = (string)(operation[RelationalAnnotationNames.ColumnType]
                            ?? column[RelationalAnnotationNames.ColumnType]);
#pragma warning restore CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
            var isNullable = column.IsNullable;

            var defaultValue = column.DefaultValue;
            defaultValue = converter != null
              ? converter.ConvertToProvider(defaultValue)
              : defaultValue;
#pragma warning disable CS8604 // 引用类型参数可能为 null。
            defaultValue = (defaultValue == DBNull.Value ? null : defaultValue)
                     ?? (isNullable
                       ? null
                       : clrType == typeof(string)
                           ? string.Empty
                           : clrType.IsArray
                             ? Array.CreateInstance(clrType.GetElementType(), 0)
                             : clrType.GetDefaultValue());
#pragma warning restore CS8604 // 引用类型参数可能为 null。

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
            };

            ColumnDefinition(
              addColumnOperation,
              model,
              builder);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);

        }

        protected override void Generate(EnsureSchemaOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
              .Append("CREATE DATABASE IF NOT EXISTS ")
              .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            EndStatement(builder, suppressTransaction: true);
        }

        protected virtual void Generate(
          XuguCreateDatabaseOperation operation,
          IModel? model,
          MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
              .Append("CREATE DATABASE ")
              .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

            EndStatement(builder, suppressTransaction: true);
        }

        protected virtual void Generate([NotNull] XuguDropDatabaseOperation operation, IModel model,
          [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
              .Append("DROP DATABASE IF EXISTS ")
              .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
              .Append(Dependencies.SqlGenerationHelper.StatementTerminator)
              .AppendLine(Dependencies.SqlGenerationHelper.BatchTerminator);

            EndStatement(builder);
        }

        /// <summary>
        ///   Generates a SQL fragment for a column definition in an <see cref="AddColumnOperation" />.
        /// </summary>
        /// <param name="operation"> The operation. </param>
        /// <param name="model"> The target model, which may beif the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        protected override void ColumnDefinition(AddColumnOperation operation, IModel? model,
          MigrationCommandListBuilder builder)
          => ColumnDefinition(
            operation.Schema!,
            operation.Table,
            operation.Name,
            operation,
            model!,
            builder);

        /// <summary>
        ///   Generates a SQL fragment for a column definition for the given column metadata.
        /// </summary>
        /// <param name="schema"> The schema that contains the table, orto use the default schema. </param>
        /// <param name="table"> The table that contains the column. </param>
        /// <param name="name"> The column name. </param>
        /// <param name="operation"> The column metadata. </param>
        /// <param name="model"> The target model, which may be if the operations exist without a model. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
#pragma warning disable CS8765 // 参数类型的为 Null 性与重写成员不匹配(可能是由于为 Null 性特性)。
        protected override void ColumnDefinition(
#pragma warning restore CS8765 // 参数类型的为 Null 性与重写成员不匹配(可能是由于为 Null 性特性)。
    string schema,
    string table,
    string name,
    ColumnOperation operation,
    IModel model,
    MigrationCommandListBuilder builder)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            if (operation.ComputedColumnSql != null)
            {
                ComputedColumnDefinition(schema, table, name, operation, model, builder);
                return;
            }

#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
            string text = operation.ColumnType ?? GetColumnType(schema, table, name, operation, model);
#pragma warning restore CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
            builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name)).Append(" ").Append(text!);
            if (operation.Collation != null)
            {
                builder.Append(" COLLATE ").Append(operation.Collation);
            }


            var identity = operation[XuguAnnotationNames.Identity] as string;
            if (identity != null
                || operation[XuguAnnotationNames.ValueGenerationStrategy] as XuguValueGenerationStrategy?
                == XuguValueGenerationStrategy.IdentityColumn)
            {
                builder.Append(" IDENTITY");

                if (!string.IsNullOrEmpty(identity)
                    && identity != "1, 1")
                {
                    builder
                        .Append("(")
                        .Append(identity)
                        .Append(")");
                }
            }

            builder.Append(operation.IsNullable ? " NULL" : " NOT NULL");
            DefaultValue(operation.DefaultValue, operation.DefaultValueSql, text, builder);
        }

        /// <summary>
        /// Generates a SQL fragment for the default constraint of a column.
        /// </summary>
        /// <param name="defaultValue"> The default value for the column. </param>
        /// <param name="defaultValueSql"> The SQL expression to use for the column's default constraint. </param>
        /// <param name="builder"> The command builder to use to add the SQL fragment. </param>
        /// <param name="columnType"> The column. </param>
        protected override void DefaultValue(object? defaultValue, string? defaultValueSql, string? columnType, MigrationCommandListBuilder builder)
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
          IModel? model,
          [NotNull] MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            var primaryKey = operation.PrimaryKey;
            if (primaryKey != null)
            {
                builder.AppendLine(",");

                var sortedColumnNames = primaryKey.Columns.Length > 1
                  ? primaryKey.Columns
                    .Select(columnName => operation.Columns.First(co => co.Name == columnName))
                    .OrderBy(co => co[XuguAnnotationNames.ValueGenerationStrategy] is XuguValueGenerationStrategy generationStrategy
                      && generationStrategy == XuguValueGenerationStrategy.IdentityColumn
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
                    sortedPrimaryKey[annotation.Name] = annotation.Value;

                PrimaryKeyConstraint(sortedPrimaryKey, model, builder);
            }
        }

        protected override void PrimaryKeyConstraint(
          AddPrimaryKeyOperation operation,
          IModel? model,
          MigrationCommandListBuilder builder)
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

            builder.Append("PRIMARY KEY ");

            IndexTraits(operation, model, builder);

            builder.Append("(")
              .Append(ColumnListWithIndexPrefixLength(operation, operation.Columns))
              .Append(")");
        }

        private string ColumnListWithIndexPrefixLength(MigrationOperation operation, string[] columns)
#pragma warning disable CS8603 // 可能返回 null 引用。
        => operation[XuguAnnotationNames.IndexPrefixLength] is int[] prefixValues
            ? ColumnList(
              columns, (c, i) => prefixValues.Length > i && prefixValues[i] > 0
                ? $"({prefixValues[i]})"
              : null)
            : ColumnList(columns);
#pragma warning restore CS8603 // 可能返回 null 引用。

        protected virtual string ColumnList([NotNull] string[] columns, Func<string, int, string> columnPostfix)
        => string.Join(", ", columns.Select((c, i) => Dependencies.SqlGenerationHelper.DelimitIdentifier(c) + columnPostfix?.Invoke(c, i)));

        protected override void Generate(AlterColumnOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
             .Append("ALTER TABLE ")
             .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
             .Append(" MODIFY ");
            ColumnDefinition(operation.Schema!, operation.Table, operation.Name, operation, model!, builder);
            builder
              .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }

        protected override void Generate(RenameTableOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
            .Append("ALTER TABLE " + operation.Name)
            .Append(" RENAME " + operation.NewName)
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(CreateIndexOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
            .Append("CREATE " + (operation.IsUnique ? "UNIQUE " : "") + "INDEX ");

            string schema = string.IsNullOrWhiteSpace(operation.Schema) ? string.Empty : $"`{operation.Schema}`.";

            builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name) +
              $" ON {schema}`{operation.Table}` ({string.Join(", ", operation.Columns.Select(Dependencies.SqlGenerationHelper.DelimitIdentifier))})")
                 .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

            EndStatement(builder);
        }

        protected override void Generate(RenameIndexOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));
            string tableName = operation.Table ?? string.Empty;

            builder.Append("ALTER TABLE ")
              .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(tableName, operation.Schema))
              .Append(" RENAME INDEX ")
              .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
              .Append(" TO ")
              .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
              .AppendLine(";");

            EndStatement(builder);
        }

        protected override void Generate(DropIndexOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate)
        {
            Check.NotNull(operation, nameof(operation));
            Check.NotNull(builder, nameof(builder));

            builder
            .Append("DROP INDEX ")
            .Append(operation.Name)
            .Append(" ON " + operation.Table)
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }

        /*        protected override void Generate(CreateTableOperation operation, IModel? model, MigrationCommandListBuilder builder,
                  bool terminate = true)
                {
                    base.Generate(operation, model, builder, false);


                    var tableOptions = new List<(string, string)>();
                    tableOptions.AddRange(XuguEntityTypeExtensions.DeserializeTableOptions(operation[XuguAnnotationNames.StoreOptions] as string)
                      .Select(kvp => (kvp.Key, kvp.Value)));

                    foreach (var (key, value) in tableOptions)
                    {
                        builder
                            .Append(" ")
                            .Append(key)
                            .Append("=")
                            .Append(value);
                    }

                    if (terminate)
                    {
                        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                        EndStatement(builder);
                    }
                }*/

        protected override void Generate(
        DropPrimaryKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
        {
            // It does nothing due to this operation should not be isolated for MySQL
            EndStatement(builder);
        }

        protected override void Generate(
        AddPrimaryKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
        {
            // It does nothing due to this operation should not be isolated for MySQL
            EndStatement(builder);
        }
    }
}
