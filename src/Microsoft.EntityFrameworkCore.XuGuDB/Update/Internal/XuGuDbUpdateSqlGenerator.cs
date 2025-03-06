// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XuGuDbUpdateSqlGenerator : UpdateSqlGenerator, IXuGuDbUpdateSqlGenerator
    {
        private readonly IRelationalTypeMapper _typeMapper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XuGuDbUpdateSqlGenerator([NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(sqlGenerationHelper)
        {
            _typeMapper = typeMapper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ResultSetMapping AppendBulkInsertOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(modificationCommands, nameof(modificationCommands));

            if ((modificationCommands.Count == 1)
                && modificationCommands[0].ColumnModifications.All(o =>
                    !o.IsKey
                    || !o.IsRead
                    || (o.Property.XuGuDb().ValueGenerationStrategy == XuGuDbValueGenerationStrategy.IdentityColumn)))
            {
                return AppendInsertOperation(commandStringBuilder, modificationCommands[0], commandPosition);
            }

            var readOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsRead).ToList();
            var writeOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsWrite).ToList();
            var defaultValuesOnly = writeOperations.Count == 0;
            var nonIdentityOperations = defaultValuesOnly
                ? modificationCommands.First().ColumnModifications
                    .Where(o => o.Property.XuGuDb().ValueGenerationStrategy != XuGuDbValueGenerationStrategy.IdentityColumn)
                    .ToList()
                : new List<ColumnModification>();

            if (defaultValuesOnly)
            {
                if ((nonIdentityOperations.Count == 0)
                    || (readOperations.Count == 0))
                {
                    foreach (var modification in modificationCommands)
                    {
                        AppendInsertOperation(commandStringBuilder, modification, commandPosition);
                    }

                    return readOperations.Count == 0
                        ? ResultSetMapping.NoResultSet
                        : ResultSetMapping.LastInResultSet;
                }

                if (nonIdentityOperations.Count > 1)
                {
                    nonIdentityOperations = new List<ColumnModification> { nonIdentityOperations.First() };
                }
            }

            if (readOperations.Count == 0)
            {
                return AppendBulkInsertWithoutServerValues(commandStringBuilder, modificationCommands, writeOperations);
            }

            if (defaultValuesOnly)
            {
                return AppendBulkInsertWithServerValuesOnly(commandStringBuilder, modificationCommands, commandPosition, nonIdentityOperations, readOperations);
            }

            return AppendBulkInsertWithServerValues(commandStringBuilder, modificationCommands, commandPosition, writeOperations, readOperations);
        }

        private ResultSetMapping AppendBulkInsertWithoutServerValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            List<ColumnModification> writeOperations)
        {
            Debug.Assert(writeOperations.Count > 0);

            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;

            AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations);
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.Append(",").AppendLine();
                AppendValues(commandStringBuilder, modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList());
            }
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();

            return ResultSetMapping.NoResultSet;
        }

        private const string InsertedTableBaseName = "inserted";
        private const string ToInsertTableBaseName = "toInsert";
        private const string ToInsertTableAlias = "i";
        private const string SourceInsertTableAlias = "s";
        private const string PositionColumnName = "_Position";
        private const string PositionColumnDeclaration = "`" + PositionColumnName + "`" + " integer";

        private ResultSetMapping AppendBulkInsertWithServerValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            List<ColumnModification> writeOperations,
            List<ColumnModification> readOperations)
        {
            AppendDeclareTable(
                commandStringBuilder,
                ToInsertTableBaseName,
                commandPosition,
                writeOperations,
                PositionColumnDeclaration);

            commandStringBuilder.Append("INSERT INTO `").Append(ToInsertTableBaseName).Append(commandPosition).Append("`");
            AppendValuesHeader(commandStringBuilder, writeOperations);
            AppendValues(commandStringBuilder, writeOperations, "0");
            for (var i = 1; i < modificationCommands.Count; i++)
            {
                commandStringBuilder.Append(",").AppendLine();
                AppendValues(
                    commandStringBuilder,
                    modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList(),
                    i.ToString());
            }
            commandStringBuilder
                .AppendLine(SqlGenerationHelper.StatementTerminator)
                .AppendLine();

            AppendDeclareTable(
                commandStringBuilder,
                InsertedTableBaseName,
                commandPosition,
                modificationCommands[0].ColumnModifications,
                PositionColumnDeclaration);

            AppendMergeCommandHeader(
                commandStringBuilder,
                modificationCommands[0].TableName,
                modificationCommands[0].Schema,
                ToInsertTableBaseName,
                commandPosition,
                ToInsertTableAlias,
                SourceInsertTableAlias,
                writeOperations,
                new List<ColumnModification>(),
                false,
                true);

            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

            AppendSelectCommand(commandStringBuilder, readOperations, InsertedTableBaseName, commandPosition, PositionColumnName);

            return ResultSetMapping.NotLastInResultSet;
        }

        private ResultSetMapping AppendBulkInsertWithServerValuesOnly(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition,
            List<ColumnModification> nonIdentityOperations,
            List<ColumnModification> readOperations)
        {
            AppendDeclareTable(commandStringBuilder, ToInsertTableBaseName, commandPosition, readOperations);

            AppendInsertCommandHeader(commandStringBuilder, modificationCommands[0].TableName, modificationCommands[0].Schema, nonIdentityOperations);
            AppendValuesHeader(commandStringBuilder, nonIdentityOperations);
            AppendValues(commandStringBuilder, nonIdentityOperations);
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
            AppendMergeCommandHeader(
                commandStringBuilder,
                modificationCommands[0].TableName,
                modificationCommands[0].Schema,
                ToInsertTableBaseName,
                commandPosition,
                ToInsertTableAlias,
                SourceInsertTableAlias,
                readOperations,
                new List<ColumnModification>());
            //AppendValuesHeader(commandStringBuilder, nonIdentityOperations);
            //AppendValues(commandStringBuilder, nonIdentityOperations);
            //for (var i = 1; i < modificationCommands.Count; i++)
            //{
            //    commandStringBuilder.Append(",").AppendLine();
            //    AppendValues(commandStringBuilder, nonIdentityOperations);
            //}
            //commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

            AppendSelectCommand(commandStringBuilder, readOperations, ToInsertTableBaseName, commandPosition);

            return ResultSetMapping.NotLastInResultSet;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override ResultSetMapping AppendUpdateOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            if (readOperations.Length > 0)
            {
                AppendDeclareTable(commandStringBuilder, ToInsertTableBaseName, commandPosition, readOperations);
            }
            AppendUpdateCommandHeader(commandStringBuilder, name, schema, writeOperations);
            commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
            if (readOperations.Length > 0)
            {
                AppendMergeCommandHeader(
                commandStringBuilder,
                name,
                schema,
                ToInsertTableBaseName,
                commandPosition,
                ToInsertTableAlias,
                SourceInsertTableAlias,
                readOperations,
                conditionOperations,
                true,
                false);
            }
            commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

            if (readOperations.Length > 0)
            {
                return AppendSelectCommand(commandStringBuilder, readOperations, ToInsertTableBaseName, commandPosition);
            }
            commandStringBuilder.AppendLine();

            return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
        }

        private void AppendMergeCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] string toInsertTableName,
            int toInsertTableIndex,
            [NotNull] string toInsertTableAlias,
            [NotNull] string sourceTableAlias,
            [NotNull] IReadOnlyList<ColumnModification> operations,
            [NotNull] IReadOnlyList<ColumnModification> conditionOperations,
            bool changeToInset=true,
            bool matched = false,
            bool notMatched = true)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.Append("MERGE INTO ");

            if (changeToInset)
            {
                commandStringBuilder
                    .Append("`")
                    .Append(toInsertTableName)
                    .Append(toInsertTableIndex)
                    .Append("`");
            }
            else
            {
                commandStringBuilder.Append(SqlGenerationHelper.DelimitIdentifier(name, schema));
            }

            commandStringBuilder.Append(" ")
                .Append(SqlGenerationHelper.DelimitIdentifier(toInsertTableAlias));

            commandStringBuilder
                .Append(" USING (")
                .Append("SELECT ")
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) => { helper.DelimitIdentifier(sb, o.ColumnName); })
                .Append(" FROM ")
                .Append(changeToInset ? SqlGenerationHelper.DelimitIdentifier(name, schema) : "`"+toInsertTableName)
                .Append(changeToInset ? "" : toInsertTableIndex.ToString()+"`")
                .Append(") ")
                /*.Append(toInsertTableIndex)*/
                .Append(SqlGenerationHelper.DelimitIdentifier(sourceTableAlias));

            if (conditionOperations.Count < 1)
            {
                commandStringBuilder.AppendLine(" ON(1=0)");
            }
            else
            {
                commandStringBuilder
                        .Append(" ON(")
                        .AppendJoin(conditionOperations, (sb, v) => AppendWhereCondition(sb, v, v.UseOriginalValueParameter), " AND ")
                        .AppendLine(")");
            }

            if (matched)
            {
                AppendMergeCommandMatched(commandStringBuilder, toInsertTableAlias, sourceTableAlias, operations);
            }
            if (notMatched)
            {
                AppendMergeCommandNotMatched(commandStringBuilder, toInsertTableAlias, sourceTableAlias, operations);
            }
        }

        private void AppendMergeCommandMatched(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string toInsertTableAlias,
            [NotNull] string sourceTableAlias,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder
                .AppendLine("WHEN MATCHED THEN");

            commandStringBuilder
                .Append("UPDATE ")
                .Append("SET ")
                .AppendJoin(
                    operations,
                    toInsertTableAlias,
                    sourceTableAlias,
                    SqlGenerationHelper,
                    (sb, o, toAlias,sourceAlias, helper) =>
                    {
                        sb.Append(SqlGenerationHelper.DelimitIdentifier(toAlias))
                        .Append(".")
                        .Append(SqlGenerationHelper.DelimitIdentifier(o.ColumnName));
                        sb.Append(" = ")
                        .Append(SqlGenerationHelper.DelimitIdentifier(sourceAlias))
                        .Append(".")
                        .Append(SqlGenerationHelper.DelimitIdentifier(o.ColumnName));
                    })
                .AppendLine();
        }

        private void AppendMergeCommandNotMatched(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string toInsertTableAlias,
            [NotNull] string sourceTableAlias,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder
                .AppendLine("WHEN NOT MATCHED THEN");

            commandStringBuilder
                .Append("INSERT ")
                .Append("(")
                .AppendJoin(
                    operations,
                    toInsertTableAlias,
                    SqlGenerationHelper,
                    (sb, o, alias, helper) =>
                    {
                        sb.Append(SqlGenerationHelper.DelimitIdentifier(alias)).Append(".");
                        helper.DelimitIdentifier(sb, o.ColumnName);
                    })
                .Append(")");

            AppendValuesHeader(commandStringBuilder, operations);
            commandStringBuilder
                .Append("(")
                .AppendJoin(
                    operations,
                    sourceTableAlias,
                    SqlGenerationHelper,
                    (sb, o, alias, helper) =>
                    {
                        sb.Append(SqlGenerationHelper.DelimitIdentifier(alias)).Append(".");
                        helper.DelimitIdentifier(sb, o.ColumnName);
                    })
                .Append(")");
        }

        private void AppendValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> operations,
            string additionalLiteral)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(operations, nameof(operations));

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append("(")
                    .AppendJoin(
                        operations,
                        SqlGenerationHelper,
                        (sb, o, helper) =>
                            {
                                if (o.IsWrite)
                                {
                                    helper.GenerateParameterName(sb, o.ParameterName);
                                }
                                else
                                {
                                    sb.Append("DEFAULT");
                                }
                            })
                    .Append(", ")
                    .Append(additionalLiteral)
                    .Append(")");
            }
        }

        private void AppendDeclareTable(
            StringBuilder commandStringBuilder,
            string name,
            int index,
            IReadOnlyList<ColumnModification> readOperations,
            string additionalColumns = null)
        {
            commandStringBuilder
                .Append("CREATE TEMPORARY TABLE IF NOT EXISTS ")
                .Append("`")
                .Append(name)
                .Append(index)
                .Append("` (")
                .AppendJoin(
                    readOperations,
                    this,
                    (sb, o, generator) =>
                        {
                            sb.Append(SqlGenerationHelper.DelimitIdentifier(o.ColumnName));
                            sb.Append(" ").Append(generator.GetTypeNameForCopy(o.Property));
                        });

            if (additionalColumns != null)
            {
                commandStringBuilder
                    .Append(", ")
                    .Append(additionalColumns);
            }
            commandStringBuilder
                .Append(")")
                .Append(SqlGenerationHelper.StatementTerminator)
                .AppendLine();
        }

        private string GetTypeNameForCopy(IProperty property)
        {
            var typeName = property.XuGuDb().ColumnType
                           ?? _typeMapper.GetMapping(property).StoreType;

            return typeName.Equals("rowversion", StringComparison.OrdinalIgnoreCase)
                   || typeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase)
                ? (property.IsNullable ? "varbinary(8)" : "binary(8)")
                : typeName;
        }

        private ResultSetMapping AppendSelectCommand(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> readOperations,
            string tableName,
            int tableIndex,
            string orderColumn = null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("SELECT ")
                .AppendJoin(
                    readOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => { helper.DelimitIdentifier(sb, o.ColumnName); })
                .Append(" FROM ").Append("`").Append(tableName).Append(tableIndex).Append("`");

            if (orderColumn != null)
            {
                commandStringBuilder
                    .AppendLine()
                    .Append("ORDER BY ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(orderColumn));
            }

            commandStringBuilder
                .Append(SqlGenerationHelper.StatementTerminator)
                .AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        private ResultSetMapping SelectAffectedCommand(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ColumnModification> readOperations,
            IReadOnlyList<ColumnModification> conditionOperations,
            string tableName,
            string schema,
            string orderColumn = null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("SELECT ")
                .AppendJoin(
                    readOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => { helper.DelimitIdentifier(sb, o.ColumnName); })
                .AppendLine()
                .Append("FROM ")
                .Append(SqlGenerationHelper.DelimitIdentifier(tableName, schema))
                .AppendLine()
                .Append("WHERE ");

            AppendRowsAffectedWhereCondition(commandStringBuilder, 1);
            commandStringBuilder
                .Append(" AND ")
                .AppendJoin(conditionOperations.Where(i=>i.IsKey), (sb, v) => AppendWhereCondition(sb, v, v.UseOriginalValueParameter), " AND ");

            if (orderColumn != null)
            {
                commandStringBuilder
                    .AppendLine()
                    .Append("ORDER BY ")
                    .Append(SqlGenerationHelper.DelimitIdentifier(orderColumn));
            }

            

            commandStringBuilder
                .Append(SqlGenerationHelper.StatementTerminator)
                .AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override ResultSetMapping AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("SELECT SQL%ROWCOUNT")
                .Append(SqlGenerationHelper.StatementTerminator).AppendLine();

            return ResultSetMapping.LastInResultSet;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .AppendLine();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(columnModification, nameof(columnModification));

            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
            commandStringBuilder.Append(" = ").Append("LAST_INSERT_ID()");
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            => Check.NotNull(commandStringBuilder, nameof(commandStringBuilder))
                .Append("SQL%ROWCOUNT = ")
                .Append(expectedRowsAffected);

        protected override void AppendInsertCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.Append("INSERT INTO ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);

            if (operations.Count > 0)
            {
                commandStringBuilder
                    .Append(" (")
                    .AppendJoin(operations,
                        SqlGenerationHelper,
                        (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName))
                    .Append(")");
            }
        }

        protected override void AppendWhereCondition(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] ColumnModification columnModification,
            bool useOriginalValue)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(columnModification, nameof(columnModification));

            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);

            var parameterValue = useOriginalValue
                ? columnModification.OriginalValue
                : columnModification.Value;

            if (parameterValue == null)
            {
                commandStringBuilder.Append(" IS NULL");
            }
            else
            {
                commandStringBuilder.Append(" = ");
                SqlGenerationHelper.GenerateParameterName(commandStringBuilder, useOriginalValue
                    ? columnModification.OriginalParameterName
                    : columnModification.ParameterName);
            }
        }

        public ResultSetMapping UpdateOperation([NotNull] StringBuilder commandStringBuilder, [NotNull] ModificationCommand command, int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToArray();
            var conditionOperations = operations.Where(o => o.IsCondition).ToArray();
            var readOperations = operations.Where(o => o.IsRead).ToArray();

            UpdateCommandHeader(commandStringBuilder, name, schema, writeOperations);

            commandStringBuilder
                .AppendLine()
                .Append("WHERE ")
                .AppendJoin(conditionOperations, (sb, v) => AppendWhereCondition(sb, v, v.UseOriginalValueParameter), " AND ")
                .Append(SqlGenerationHelper.StatementTerminator);

            if (readOperations.Length > 0)
            {
                return SelectAffectedCommand(commandStringBuilder, readOperations,conditionOperations, name,schema);
            }
            commandStringBuilder.AppendLine();

            return AppendSelectAffectedCountCommand(commandStringBuilder, name, schema, commandPosition);
        }

        protected virtual void UpdateCommandHeader(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] string name,
            [CanBeNull] string schema,
            [NotNull] IReadOnlyList<ColumnModification> operations)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(operations, nameof(operations));

            commandStringBuilder.Append("UPDATE ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);
            commandStringBuilder.Append(" SET ")
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) =>
                    {
                        helper.DelimitIdentifier(sb, o.ColumnName);
                        sb.Append(" = ");
                        helper.GenerateParameterName(sb, o.ParameterName);
                    });


        }

        public override void AppendNextSequenceValueOperation(StringBuilder commandStringBuilder, string name, string schema)
        {
            commandStringBuilder.Append("SELECT ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, Check.NotNull(name, nameof(name)), schema);
            commandStringBuilder.Append(".NEXTVAL FROM DUAL");
        }
    }
}
