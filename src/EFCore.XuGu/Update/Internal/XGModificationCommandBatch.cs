// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace EntityFrameworkCore.XuGu.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGModificationCommandBatch : AffectedCountModificationCommandBatch
    {
        private const int DefaultNetworkPacketSizeBytes = 4096;
        private const int MaxScriptLength = 65536 * DefaultNetworkPacketSizeBytes / 2;
        private const int MaxParameterCount = 2100;
        private const int MaxRowCount = 1000;
        private int _parameterCount = 1; // Implicit parameter for the command text
        private readonly int _maxBatchSize;
        private readonly List<ModificationCommand> _bulkInsertCommands = new List<ModificationCommand>();
        private readonly List<ModificationCommand> _bulkDeleteCommands = new List<ModificationCommand>();
        private int _commandsLeftToLengthCheck = 50;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XGModificationCommandBatch(
            [NotNull] ModificationCommandBatchFactoryDependencies dependencies,
            int? maxBatchSize)
            : base(dependencies)
        {
            if (maxBatchSize.HasValue
                && maxBatchSize.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBatchSize), RelationalStrings.InvalidMaxBatchSize);
            }

            _maxBatchSize = Math.Min(maxBatchSize ?? int.MaxValue, MaxRowCount);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected new virtual IXGUpdateSqlGenerator UpdateSqlGenerator => (IXGUpdateSqlGenerator)base.UpdateSqlGenerator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool CanAddCommand(ModificationCommand modificationCommand)
        {
            if (ModificationCommands.Count >= _maxBatchSize)
            {
                return false;
            }

            var additionalParameterCount = CountParameters(modificationCommand);

            if (_parameterCount + additionalParameterCount >= MaxParameterCount)
            {
                return false;
            }

            _parameterCount += additionalParameterCount;
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool IsCommandTextValid()
        {
            if (--_commandsLeftToLengthCheck < 0)
            {
                var commandTextLength = GetCommandText().Length;
                if (commandTextLength >= MaxScriptLength)
                {
                    return false;
                }

                var avarageCommandLength = commandTextLength / ModificationCommands.Count;
                var expectedAdditionalCommandCapacity = (MaxScriptLength - commandTextLength) / avarageCommandLength;
                _commandsLeftToLengthCheck = Math.Max(1, expectedAdditionalCommandCapacity / 4);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override int GetParameterCount()
            => _parameterCount;

        private static int CountParameters(ModificationCommand modificationCommand)
        {
            var parameterCount = 0;
            foreach (var columnModification in modificationCommand.ColumnModifications)
            {
                if (columnModification.UseCurrentValueParameter)
                {
                    parameterCount++;
                }

                if (columnModification.UseOriginalValueParameter)
                {
                    parameterCount++;
                }
            }

            return parameterCount;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ResetCommandText()
        {
            base.ResetCommandText();
            _bulkInsertCommands.Clear();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GetCommandText()
            => base.GetCommandText() + GetBulkInsertCommandText(ModificationCommands.Count);
        protected List<string> GetCommandTextList()
        {
            List<string> textList = new List<string>();
            for (int i = 0; i < ModificationCommands.Count; i++)
            {
                textList.Add(base.GetCommandText() + GetBulkInsertCommandText(ModificationCommands[i],i));
            }
            return textList;
        }

        private string GetBulkInsertCommandText(int lastIndex)
        {
            if (_bulkInsertCommands.Count == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            var resultSetMapping = UpdateSqlGenerator.AppendBulkInsertOperation(stringBuilder, _bulkInsertCommands, lastIndex - _bulkInsertCommands.Count);
            for (var i = lastIndex - _bulkInsertCommands.Count; i < lastIndex; i++)
            {
                CommandResultSet[i] = resultSetMapping;
            }

            if (resultSetMapping != ResultSetMapping.NoResultSet)
            {
                CommandResultSet[lastIndex - 1] = ResultSetMapping.LastInResultSet;
            }

            return stringBuilder.ToString();
        }
        private string GetBulkInsertCommandText([NotNull] ModificationCommand modificationCommand,int i)
        {
            if (_bulkInsertCommands.Count == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            var resultSetMapping = UpdateSqlGenerator.AppendInsertOperation(stringBuilder, modificationCommand, 0);

            CommandResultSet[i] = resultSetMapping;

            //if (resultSetMapping != ResultSetMapping.NoResultSet)
            //{
            //    CommandResultSet[0] = ResultSetMapping.LastInResultSet;
            //}

            return stringBuilder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void UpdateCachedCommandText(int commandPosition)
        {
            var newModificationCommand = ModificationCommands[commandPosition];

            if (newModificationCommand.EntityState == EntityState.Added)
            {
                //if (_bulkInsertCommands.Count > 0
                //    && !CanBeInsertedInSameStatement(_bulkInsertCommands[0], newModificationCommand))
                //{
                //    CachedCommandText.Append(GetBulkInsertCommandText(commandPosition));
                //    _bulkInsertCommands.Clear();
                //}
                _bulkInsertCommands.Add(newModificationCommand);

                LastCachedCommandIndex = commandPosition;
            }
            else
            {
                CachedCommandText.Append(GetBulkInsertCommandText(commandPosition));
                _bulkInsertCommands.Clear();

                UpdateCachedCommand(commandPosition);
            }
        }

        protected void UpdateCachedCommand(int commandPosition)
        {
            var newModificationCommand = ModificationCommands[commandPosition];

            switch (newModificationCommand.EntityState)
            {
                case EntityState.Added:
                    CommandResultSet[commandPosition] =
                        UpdateSqlGenerator.AppendInsertOperation(CachedCommandText, newModificationCommand, commandPosition);
                    break;
                case EntityState.Modified:
                    CommandResultSet[commandPosition] =
                        UpdateSqlGenerator.AppendUpdateOperation(CachedCommandText, newModificationCommand, commandPosition);
                    break;
                case EntityState.Deleted:
                    CommandResultSet[commandPosition] =
                        UpdateSqlGenerator.AppendDeleteOperation(CachedCommandText, newModificationCommand, commandPosition);
                    break;
            }

            LastCachedCommandIndex = commandPosition;
        }
        protected override int ConsumeResultSetWithPropagation(int commandIndex, [NotNull] RelationalDataReader reader)
        {
            var rowsAffected = 0;
            do
            {
                var tableModification = ModificationCommands[commandIndex];
                Check.DebugAssert(tableModification.RequiresResultPropagation, "RequiresResultPropagation is false");

                if (!reader.Read())
                {
                    var expectedRowsAffected = rowsAffected + 1;
                    while (++commandIndex < CommandResultSet.Count
                        && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet)
                    {
                        expectedRowsAffected++;
                    }

                    ThrowAggregateUpdateConcurrencyException(commandIndex, expectedRowsAffected, rowsAffected);
                }

                var valueBufferFactory = CreateValueBufferFactory(tableModification.ColumnModifications);

                tableModification.PropagateResults(valueBufferFactory.Create(reader.DbDataReader));
                rowsAffected++;
            }
            while (++commandIndex < CommandResultSet.Count
                && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet);

            return commandIndex;
        }

        protected override int ConsumeResultSetWithoutPropagation(int commandIndex, [NotNull] RelationalDataReader reader)
        {
            var expectedRowsAffected = 1;
            while (++commandIndex < CommandResultSet.Count
                && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet)
            {
                Check.DebugAssert(!ModificationCommands[commandIndex].RequiresResultPropagation, "RequiresResultPropagation is true");

                expectedRowsAffected++;
            }

            if (reader.Read())
            {
                var rowsAffected = reader.DbDataReader.GetInt32(0);
                if (rowsAffected != expectedRowsAffected)
                {
                    ThrowAggregateUpdateConcurrencyException(commandIndex, expectedRowsAffected, rowsAffected);
                }
            }
            else
            {
                ThrowAggregateUpdateConcurrencyException(commandIndex, 1, 0);
            }

            return commandIndex;
        }

        protected void Consume(RelationalDataReader reader,int commandIndex)
        {
            Check.DebugAssert(
                CommandResultSet.Count == ModificationCommands.Count,
                $"CommandResultSet.Count of {CommandResultSet.Count} != ModificationCommands.Count of {ModificationCommands.Count}");

            //var commandIndex = 0;

            try
            {
                var actualResultSetCount = 0;
                do
                {
                    //while (commandIndex < CommandResultSet.Count
                    //    && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                    //{
                    //    commandIndex++;
                    //}

                    if (commandIndex < CommandResultSet.Count)
                    {
                        commandIndex = ModificationCommands[commandIndex].RequiresResultPropagation
                            ? ConsumeResultSetWithPropagation(commandIndex, reader)
                            : ConsumeResultSetWithoutPropagation(commandIndex, reader);
                        if (commandIndex == 0 &&  ModificationCommands[commandIndex].RequiresResultPropagation)
                        {
                            commandIndex = ModificationCommands[commandIndex].Entries.Count;
                        }
                        actualResultSetCount++;
                    }
                }
                while (commandIndex < CommandResultSet.Count && reader.DbDataReader.NextResult());

#if DEBUG
                while (commandIndex < CommandResultSet.Count
                    && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                {
                    commandIndex++;
                }

                //Check.DebugAssert(
                //    commandIndex == ModificationCommands.Count,
                //    "Expected " + ModificationCommands.Count + " results, got " + commandIndex);

                //var expectedResultSetCount = CommandResultSet.Count(e => e == ResultSetMapping.LastInResultSet);

                //Check.DebugAssert(
                //    actualResultSetCount == expectedResultSetCount,
                //    "Expected " + expectedResultSetCount + " result sets, got " + actualResultSetCount);
#endif
            }
            catch (Exception ex) when (!(ex is DbUpdateException))
            {
                throw new DbUpdateException(
                    RelationalStrings.UpdateStoreException,
                    ex,
                    ModificationCommands[commandIndex].Entries);
            }
        }

        /// <summary>
        ///     Generates a <see cref="RawSqlCommand" /> for the batch.
        /// </summary>
        /// <returns> The command. </returns>
        //protected override RawSqlCommand CreateStoreCommand()
        //{
        //    var commandBuilder = Dependencies.CommandBuilderFactory
        //        .Create()
        //        .Append(GetCommandText());

        //    var parameterValues = new Dictionary<string, object>(GetParameterCount());

        //    // ReSharper disable once ForCanBeConvertedToForeach
        //    for (var commandIndex = 0; commandIndex < ModificationCommands.Count; commandIndex++)
        //    {
        //        var command = ModificationCommands[commandIndex];
        //        // ReSharper disable once ForCanBeConvertedToForeach
        //        for (var columnIndex = 0; columnIndex < command.ColumnModifications.Count; columnIndex++)
        //        {
        //            var columnModification = command.ColumnModifications[columnIndex];
        //            if (columnModification.UseCurrentValueParameter)
        //            {
        //                commandBuilder.AddParameter(
        //                    columnModification.ParameterName,
        //                    Dependencies.SqlGenerationHelper.GenerateParameterName(columnModification.ParameterName),
        //                    columnModification.TypeMapping,
        //                    columnModification.IsNullable);

        //                parameterValues.Add(columnModification.ParameterName, columnModification.Value);
        //            }

        //            if (columnModification.UseOriginalValueParameter)
        //            {
        //                commandBuilder.AddParameter(
        //                    columnModification.OriginalParameterName,
        //                    Dependencies.SqlGenerationHelper.GenerateParameterName(columnModification.OriginalParameterName),
        //                    columnModification.TypeMapping,
        //                    columnModification.IsNullable);

        //                parameterValues.Add(columnModification.OriginalParameterName, columnModification.OriginalValue);
        //            }
        //        }
        //    }

        //    return new RawSqlCommand(commandBuilder.Build(), parameterValues);
        //}
        protected new List<RawSqlCommand> CreateStoreCommand()
        {
            List<RawSqlCommand> commands = new List<RawSqlCommand>();
            foreach (var text in GetCommandTextList())
            {
                var commandBuilder = Dependencies.CommandBuilderFactory
                .Create()
                .Append(text);
                var parameterValues = new Dictionary<string, object>(GetParameterCount());

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var commandIndex = 0; commandIndex < ModificationCommands.Count; commandIndex++)
                {
                    var command = ModificationCommands[commandIndex];
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var columnIndex = 0; columnIndex < command.ColumnModifications.Count; columnIndex++)
                    {
                        var columnModification = command.ColumnModifications[columnIndex];
                        if (columnModification.UseCurrentValueParameter)
                        {
                            commandBuilder.AddParameter(
                                columnModification.ParameterName,
                                Dependencies.SqlGenerationHelper.GenerateParameterName(columnModification.ParameterName),
                                columnModification.TypeMapping,
                                columnModification.IsNullable);

                            parameterValues.Add(columnModification.ParameterName, columnModification.Value);
                        }

                        if (columnModification.UseOriginalValueParameter)
                        {
                            commandBuilder.AddParameter(
                                columnModification.OriginalParameterName,
                                Dependencies.SqlGenerationHelper.GenerateParameterName(columnModification.OriginalParameterName),
                                columnModification.TypeMapping,
                                columnModification.IsNullable);

                            parameterValues.Add(columnModification.OriginalParameterName, columnModification.OriginalValue);
                        }
                    }
                }
                commands.Add(new RawSqlCommand(commandBuilder.Build(), parameterValues));
            }
            return commands;
        }

        /// <summary>
        ///     Executes the command generated by <see cref="CreateStoreCommand" /> against a
        ///     database using the given connection.
        /// </summary>
        /// <param name="connection"> The connection to the database to update. </param>
        public override void Execute(IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var storeCommands = CreateStoreCommand();

            int i= 0;
            foreach (var storeCommand in storeCommands)
            {
                try
                {
                    using var dataReader = storeCommand.RelationalCommand.ExecuteReader(
                        new RelationalCommandParameterObject(
                            connection,
                            storeCommand.ParameterValues,
                            null,
                            Dependencies.CurrentContext.Context,
                            Dependencies.Logger));

                    if (CommandResultSet[i]==ResultSetMapping.LastInResultSet)
                    {
                        Consume(dataReader, i);
                    }
                    
                    i++;
                }
                catch (DbUpdateException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new DbUpdateException(
                        RelationalStrings.UpdateStoreException,
                        ex,
                        ModificationCommands.SelectMany(c => c.Entries).ToList());
                }
            }
        }

        private static bool CanBeInsertedInSameStatement(ModificationCommand firstCommand, ModificationCommand secondCommand)
            => string.Equals(firstCommand.TableName, secondCommand.TableName, StringComparison.Ordinal)
               && string.Equals(firstCommand.Schema, secondCommand.Schema, StringComparison.Ordinal)
               && firstCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName).SequenceEqual(
                   secondCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName))
               && firstCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName).SequenceEqual(
                   secondCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName));
    }
}
