// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    using RelationalStrings = Microsoft.EntityFrameworkCore.Internal.RelationalStrings;
    public class XGModificationCommandBatch : AffectedCountModificationCommandBatch
    {

        private const int DefaultNetworkPacketSizeBytes = 4096;
        private const int MaxScriptLength = 65536 * DefaultNetworkPacketSizeBytes / 2;
        private const int MaxParameterCount = 2100;
        private const int MaxRowCount = 1000;
        private int _parameterCount = 1; // Implicit parameter for the command text
        private readonly int _maxBatchSize;
        private readonly List<ModificationCommand> _bulkInsertCommands = new List<ModificationCommand>();
        private int _commandsLeftToLengthCheck = 50;

        public XGModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper SqlGenerationHelper,
            [NotNull] IXGUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [CanBeNull] int? maxBatchSize)
            : base(commandBuilderFactory, SqlGenerationHelper, updateSqlGenerator, valueBufferFactoryFactory)
        {
            if (maxBatchSize.HasValue
                && (maxBatchSize.Value <= 0))
            {
                throw new ArgumentOutOfRangeException(nameof(maxBatchSize), RelationalStrings.InvalidMaxBatchSize);
            }

            _maxBatchSize = Math.Min(maxBatchSize ?? int.MaxValue, MaxRowCount);
        }

        protected new virtual IXGUpdateSqlGenerator UpdateSqlGenerator => (IXGUpdateSqlGenerator)base.UpdateSqlGenerator;


        protected override bool CanAddCommand(ModificationCommand modificationCommand)
        {
            if (_maxBatchSize <= ModificationCommands.Count)
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

        private static int CountParameters(ModificationCommand modificationCommand)
        {
            var parameterCount = 0;
            foreach (var columnModification in modificationCommand.ColumnModifications)
            {
                if (columnModification.ParameterName != null)
                {
                    parameterCount++;
                }

                if (columnModification.OriginalParameterName != null)
                {
                    parameterCount++;
                }
            }

            return parameterCount;
        }

        protected override void ResetCommandText()
        {
            base.ResetCommandText();
            _bulkInsertCommands.Clear();
        }

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
        protected override string GetCommandText()
            => base.GetCommandText() + GetBulkInsertCommandText(ModificationCommands.Count);


        public override void Execute(IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var storeCommand = CreateStoreCommand();

            try
            {
                using (var dataReader = storeCommand.RelationalCommand.ExecuteReader(
                    connection,
                    parameterValues: storeCommand.ParameterValues))
                {
                    Consume(dataReader.DbDataReader);
                }
            }
            catch (DbUpdateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DbUpdateException(RelationalStrings.UpdateStoreException, ex);
            }
        }
        private string GetBulkInsertCommandText(int lastIndex)
        {
            if (_bulkInsertCommands.Count == 0)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            var grouping = UpdateSqlGenerator.AppendBulkInsertOperation(stringBuilder, _bulkInsertCommands, lastIndex);
            for (var i = lastIndex - _bulkInsertCommands.Count; i < lastIndex; i++)
            {
                CommandResultSet[i] = grouping;
            }

            if (grouping != ResultSetMapping.NoResultSet)
            {
                CommandResultSet[lastIndex - 1] = ResultSetMapping.LastInResultSet;
            }

            return stringBuilder.ToString();
        }

        protected override void UpdateCachedCommandText(int commandPosition)
        {
            var newModificationCommand = ModificationCommands[commandPosition];

            if (newModificationCommand.EntityState == EntityState.Added)
            {
                if ((_bulkInsertCommands.Count > 0)
                    && !CanBeInsertedInSameStatement(_bulkInsertCommands[0], newModificationCommand))
                {
                    CachedCommandText.Append(GetBulkInsertCommandText(commandPosition));
                    _bulkInsertCommands.Clear();
                }
                _bulkInsertCommands.Add(newModificationCommand);

                LastCachedCommandIndex = commandPosition;
            }
            else
            {
                CachedCommandText.Append(GetBulkInsertCommandText(commandPosition));
                _bulkInsertCommands.Clear();

                base.UpdateCachedCommandText(commandPosition);
            }
        }

        private static bool CanBeInsertedInSameStatement(ModificationCommand firstCommand, ModificationCommand secondCommand)
            => string.Equals(firstCommand.TableName, secondCommand.TableName, StringComparison.Ordinal)
               && string.Equals(firstCommand.Schema, secondCommand.Schema, StringComparison.Ordinal)
               && firstCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName).SequenceEqual(
                   secondCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName))
               && firstCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName).SequenceEqual(
                   secondCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName));

        /*protected override void Consume(DbDataReader reader)
        {
            var XGReader = (XGDataReader)reader;
            Debug.Assert(XGReader.Statements.Count == ModificationCommands.Count, $"Reader has {XGReader.Statements.Count} statements, expected {ModificationCommands.Count}");
            var commandIndex = 0;

            try
            {
                while (true)
                {
                    // Find the next propagating command, if any
                    int nextPropagating;
                    for (nextPropagating = commandIndex;
                        nextPropagating < ModificationCommands.Count &&
                        !ModificationCommands[nextPropagating].RequiresResultPropagation;
                        nextPropagating++) ;

                    // Go over all non-propagating commands before the next propagating one,
                    // make sure they executed
                    for (; commandIndex < nextPropagating; commandIndex++)
                    {
                        if (XGReader.Statements[commandIndex].Rows == 0)
                        {
                            throw new DbUpdateConcurrencyException(
                                RelationalStrings.UpdateConcurrencyException(1, 0),
                                ModificationCommands[commandIndex].Entries
                            );
                        }
                    }

                    if (nextPropagating == ModificationCommands.Count)
                    {
                        Debug.Assert(!reader.NextResult(), "Expected less resultsets");
                        break;
                    }

                    // Propagate to results from the reader to the ModificationCommand

                    var modificationCommand = ModificationCommands[commandIndex++];

                    if (!reader.Read())
                    {
                        throw new DbUpdateConcurrencyException(
                            RelationalStrings.UpdateConcurrencyException(1, 0),
                            modificationCommand.Entries);
                    }

                    var valueBufferFactory = CreateValueBufferFactory(modificationCommand.ColumnModifications);
                    modificationCommand.PropagateResults(valueBufferFactory.Create(reader));

                    reader.NextResult();
                }
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
                    ModificationCommands[commandIndex].Entries);
            }
        }*/

        /*protected override async Task ConsumeAsync(
            DbDataReader reader,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var XGReader = (XGDataReader)reader;
            Debug.Assert(XGReader.Statements.Count == ModificationCommands.Count, $"Reader has {XGReader.Statements.Count} statements, expected {ModificationCommands.Count}");
            var commandIndex = 0;

            try
            {
                while (true)
                {
                    // Find the next propagating command, if any
                    int nextPropagating;
                    for (nextPropagating = commandIndex;
                        nextPropagating < ModificationCommands.Count &&
                        !ModificationCommands[nextPropagating].RequiresResultPropagation;
                        nextPropagating++)
                        ;

                    // Go over all non-propagating commands before the next propagating one,
                    // make sure they executed
                    for (; commandIndex < nextPropagating; commandIndex++)
                    {
                        if (XGReader.Statements[commandIndex].Rows == 0)
                        {
                            throw new DbUpdateConcurrencyException(
                                RelationalStrings.UpdateConcurrencyException(1, 0),
                                ModificationCommands[commandIndex].Entries
                            );
                        }
                    }

                    if (nextPropagating == ModificationCommands.Count)
                    {
                        Debug.Assert(!(await reader.NextResultAsync(cancellationToken)), "Expected less resultsets");
                        break;
                    }

                    // Extract result from the command and propagate it

                    var modificationCommand = ModificationCommands[commandIndex++];

                    if (!(await reader.ReadAsync(cancellationToken)))
                    {
                        throw new DbUpdateConcurrencyException(
                            RelationalStrings.UpdateConcurrencyException(1, 0),
                            modificationCommand.Entries
                        );
                    }

                    var valueBufferFactory = CreateValueBufferFactory(modificationCommand.ColumnModifications);
                    modificationCommand.PropagateResults(valueBufferFactory.Create(reader));

                    await reader.NextResultAsync(cancellationToken);
                }
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
                    ModificationCommands[commandIndex].Entries);
            }
        }*/
//        protected override void Consume(DbDataReader reader)
//        {
//            Debug.Assert(
//                CommandResultSet.Count == ModificationCommands.Count,
//                $"CommandResultSet.Count of {CommandResultSet.Count} != ModificationCommands.Count of {ModificationCommands.Count}");

//            var commandIndex = 0;

//            try
//            {
//                var actualResultSetCount = 0;
//                do
//                {
//                    //while (commandIndex < CommandResultSet.Count
//                    //    && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
//                    //{
//                    //    commandIndex++;
//                    //}
//                    var resultSetMapping = CommandResultSet[commandIndex];
//                    if (commandIndex < CommandResultSet.Count)
//                    {
//                        commandIndex = ModificationCommands[commandIndex].RequiresResultPropagation
//                            ? ConsumeResultSetWithPropagation(commandIndex, reader)
//                            : ConsumeResultSetWithoutPropagation(commandIndex, reader);
//                        if (commandIndex == 0 && resultSetMapping.HasFlag(ResultSetMapping.LastInResultSet))
//                        {
//                            commandIndex = ModificationCommands[commandIndex].Entries.Count;
//                        }
//                        actualResultSetCount++;
//                    }
//                }
//                while (commandIndex < CommandResultSet.Count && reader.NextResult());

//#if DEBUG
//                while (commandIndex < CommandResultSet.Count
//                    && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
//                {
//                    commandIndex++;
//                }

//                //Check.DebugAssert(
//                //    commandIndex == ModificationCommands.Count,
//                //    "Expected " + ModificationCommands.Count + " results, got " + commandIndex);

//                //var expectedResultSetCount = CommandResultSet.Count(e => e == ResultSetMapping.LastInResultSet);

//                //Check.DebugAssert(
//                //    actualResultSetCount == expectedResultSetCount,
//                //    "Expected " + expectedResultSetCount + " result sets, got " + actualResultSetCount);
//#endif
//            }
//            catch (Exception ex) when (!(ex is DbUpdateException))
//            {
//                throw new DbUpdateException(
//                    RelationalStrings.UpdateStoreException,
//                    ex,
//                    ModificationCommands[commandIndex].Entries);
//            }
//        }

//        protected override int ConsumeResultSetWithPropagation(int commandIndex, [NotNull] DbDataReader reader)
//        {
//            var rowsAffected = 0;
//            do
//            {
//                var tableModification = ModificationCommands[commandIndex];
//                Debug.Assert(tableModification.RequiresResultPropagation);

//                if (!reader.Read())
//                {
//                    var expectedRowsAffected = rowsAffected + 1;
//                    while ((++commandIndex < CommandResultSet.Count)
//                           && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet)
//                    {
//                        expectedRowsAffected++;
//                    }

//                    ThrowAggregateUpdateConcurrencyException(commandIndex, expectedRowsAffected, rowsAffected);
//                }

//                var valueBufferFactory = CreateValueBufferFactory(tableModification.ColumnModifications);

//                tableModification.PropagateResults(valueBufferFactory.Create(reader));
//                rowsAffected++;
//            }
//            while ((++commandIndex < CommandResultSet.Count)
//                   && CommandResultSet[commandIndex - 1] == ResultSetMapping.NotLastInResultSet);

//            return commandIndex;
//        }
    }
}
