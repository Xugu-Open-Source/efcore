// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGu.Update.Internal
{
    public class XGUpdateSqlGenerator : UpdateSqlGenerator, IXGUpdateSqlGenerator
    {

        public XGUpdateSqlGenerator(
            [NotNull] UpdateSqlGeneratorDependencies dependencies
        ) : base(dependencies)
        {
        }

        public ResultSetMapping AppendInsertOperationBulk(
            StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
        {
            Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
            Check.NotNull(command, nameof(command));

            var name = command.TableName;
            var schema = command.Schema;
            var operations = command.ColumnModifications;

            var writeOperations = operations.Where(o => o.IsWrite).ToList();
            var readOperations = operations.Where(o => o.IsRead).ToList();

            AppendInsertCommand(commandStringBuilder, name, schema, writeOperations);

            if (readOperations.Count > 0)
            {
                var keyOperations = operations.Where(o => o.IsKey).ToList();

                return AppendSelectAffectedCommand(commandStringBuilder, name, schema, readOperations, keyOperations, commandPosition);
            }

            commandStringBuilder.AppendLine("--GO");

            return ResultSetMapping.NoResultSet;
        }

        public virtual ResultSetMapping AppendBulkInsertOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            int commandPosition)
        {
            if (modificationCommands.Count == 1
                && modificationCommands[0].ColumnModifications.All(o =>
                    !o.IsKey
                    || !o.IsRead
                    || o.Property.XG().ValueGenerationStrategy == XGValueGenerationStrategy.IdentityColumn))
            {
                return AppendInsertOperationBulk(commandStringBuilder, modificationCommands[0], commandPosition);
            }

            var readOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsRead).ToList();
            var writeOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsWrite).ToList();
            var keyOperations = modificationCommands[0].ColumnModifications.Where(o => o.IsKey).ToList();

            var defaultValuesOnly = writeOperations.Count == 0;
            var nonIdentityOperations = modificationCommands[0].ColumnModifications
                .Where(o => o.Property.XG().ValueGenerationStrategy != XGValueGenerationStrategy.IdentityColumn)
                .ToList();

            if (defaultValuesOnly)
            {
                if (nonIdentityOperations.Count == 0
                    || readOperations.Count == 0)
                {
                    foreach (var modification in modificationCommands)
                    {
                        AppendInsertOperationBulk(commandStringBuilder, modification, commandPosition);
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

            foreach (var modification in modificationCommands)
            {
                AppendInsertOperationBulk(commandStringBuilder, modification, commandPosition);
            }

            return ResultSetMapping.LastInResultSet;
        }

        private ResultSetMapping AppendBulkInsertWithoutServerValues(
            StringBuilder commandStringBuilder,
            IReadOnlyList<ModificationCommand> modificationCommands,
            List<ColumnModification> writeOperations)
        {
            Debug.Assert(writeOperations.Count > 0);

            var name = modificationCommands[0].TableName;
            var schema = modificationCommands[0].Schema;

            
            for (var i = 0; i < modificationCommands.Count; i++)
            {
                AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
                AppendValuesHeader(commandStringBuilder, writeOperations);
                AppendValues(commandStringBuilder, modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList());
                commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator).AppendLine();
                #if UNIT_TEST
                    commandStringBuilder.AppendLine("--GO");
                #endif
                
            }

            return ResultSetMapping.NoResultSet;
        }

        //protected override void AppendValuesHeader(
        //    [NotNull] StringBuilder commandStringBuilder,
        //    [NotNull] IReadOnlyList<ColumnModification> operations)
        //{
        //    Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
        //    Check.NotNull(operations, nameof(operations));

        //    commandStringBuilder.AppendLine();
        //    commandStringBuilder.Append(operations.Count > 0 ? "VALUES " : "DEFAULT VALUES");
        //}

        //protected override void AppendValues(
        //    [NotNull] StringBuilder commandStringBuilder,
        //    [NotNull] IReadOnlyList<ColumnModification> operations)
        //{
        //    base.AppendValues(commandStringBuilder, operations);

        //    if (operations.Count == 0)
        //    {
        //        commandStringBuilder.Append("DEFAULT");
        //    }
        //}

        protected override ResultSetMapping AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
        {
            commandStringBuilder
                .Append("SELECT ROWNUM FROM DUAL")
                .Append(SqlGenerationHelper.StatementTerminator).AppendLine()
                .AppendLine()
                .AppendLine("--GO");

            return ResultSetMapping.LastInResultSet;
        }

        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            // TODO: what is the effect of this statment?
        }

        protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
        {
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);
            commandStringBuilder.Append(" = ")
                .Append("LAST_INSERT_ID()");
        }

        protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            => commandStringBuilder
                .Append("ROWNUM = ")
                .Append(expectedRowsAffected.ToString(CultureInfo.InvariantCulture));
    }
}
