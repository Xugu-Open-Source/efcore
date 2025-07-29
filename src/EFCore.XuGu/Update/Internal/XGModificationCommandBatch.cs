// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGu.Update.Internal
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
        private int _commandsLeftToLengthCheck = 50;
        private readonly IRelationalCommandBuilderFactory _commandBuilderFactory;

        protected virtual IList<ResultSetMapping> ResultSetMappings { get; } = new List<ResultSetMapping>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XGModificationCommandBatch(
            [NotNull] IRelationalCommandBuilderFactory commandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            // ReSharper disable once SuggestBaseTypeForParameter
            [NotNull] IXGUpdateSqlGenerator updateSqlGenerator,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            int? maxBatchSize)
            : base(
                commandBuilderFactory,
                sqlGenerationHelper,
                updateSqlGenerator,
                valueBufferFactoryFactory)
        {
            _commandBuilderFactory = commandBuilderFactory;
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
                ResultSetMappings.Add(resultSetMapping);
            }

            if (resultSetMapping != ResultSetMapping.NoResultSet)
            {
                CommandResultSet[lastIndex - 1] = ResultSetMapping.LastInResultSet;
            }
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
                if (_bulkInsertCommands.Count > 0
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

        //protected override RawSqlCommand CreateStoreCommand()
        //{
        //    var commandBuilder = _commandBuilderFactory
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
        //                    SqlGenerationHelper.GenerateParameterName(columnModification.ParameterName),
        //                    columnModification.Property);

        //                parameterValues.Add(columnModification.ParameterName, columnModification.Value);
        //            }

        //            if (columnModification.UseOriginalValueParameter)
        //            {
        //                commandBuilder.AddParameter(
        //                    columnModification.OriginalParameterName,
        //                    SqlGenerationHelper.GenerateParameterName(columnModification.OriginalParameterName),
        //                    columnModification.Property);

        //                parameterValues.Add(columnModification.OriginalParameterName, columnModification.OriginalValue);
        //            }
        //        }
        //    }

        //    return new RawSqlCommand(commandBuilder.Build(), parameterValues);
        //}

        protected new List<RawSqlCommand> CreateStoreCommand()
        {
            var fullText= _commandBuilderFactory
                .Create()
                .Append(GetCommandText()).ToString();
            List<RawSqlCommand> commands = new List<RawSqlCommand>();
            string result = Regex.Replace(fullText, @"--GO\s*\z", "");
            string[] split = result.Split(new string[] { "--GO" }, StringSplitOptions.None);
            int commandIndex = 0;
            foreach (var text in split)
            {
                var commandBuilder = _commandBuilderFactory
                .Create()
                .Append(text);
                var parameterValues = new Dictionary<string, object>();

                var command = ModificationCommands[commandIndex];
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var columnIndex = 0; columnIndex < command.ColumnModifications.Count; columnIndex++)
                {
                    var columnModification = command.ColumnModifications[columnIndex];
                    if (columnModification.UseCurrentValueParameter)
                    {
                        commandBuilder.AddParameter(
                            columnModification.ParameterName,
                            SqlGenerationHelper.GenerateParameterName(columnModification.ParameterName).Replace("@", ":"),
                            columnModification.Property);

                        parameterValues.Add(columnModification.ParameterName, columnModification.Value);
                    }

                    if (columnModification.UseOriginalValueParameter)
                    {
                        commandBuilder.AddParameter(
                            columnModification.OriginalParameterName,
                            SqlGenerationHelper.GenerateParameterName(columnModification.OriginalParameterName).Replace("@", ":"),
                            columnModification.Property);

                        parameterValues.Add(columnModification.OriginalParameterName, columnModification.OriginalValue);
                    }
                }
                commands.Add(new RawSqlCommand(commandBuilder.Build(), parameterValues));
                commandIndex++;
            }
            return commands;
        }
        public virtual ResultSetMapping CheckResult(RawSqlCommand command)
        {
            Match match = Regex.Match(command.RelationalCommand.CommandText, @"SELECT\s+.*?\s+FROM\s+.*?(?:;|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return match.Success ? ResultSetMapping.LastInResultSet : ResultSetMapping.NoResultSet;
        }

        protected void Consume(RelationalDataReader reader,int commandIndex)
        {
            Debug.Assert(CommandResultSet.Count == ModificationCommands.Count);
            //var commandIndex = 0;

            try
            {
                var actualResultSetCount = 0;
                do
                {
                    //while (commandIndex < CommandResultSet.Count
                    //       && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                    //{
                    //    commandIndex++;
                    //}

                    if (commandIndex < CommandResultSet.Count)
                    {
                        commandIndex = ModificationCommands[commandIndex].RequiresResultPropagation
                            ? ConsumeResultSetWithPropagation(commandIndex, reader)
                            : ConsumeResultSetWithoutPropagation(commandIndex, reader);
                        actualResultSetCount++;
                    }
                }
                while (commandIndex < CommandResultSet.Count
                       && reader.DbDataReader.NextResult());

#if DEBUG
                //while (commandIndex < CommandResultSet.Count
                //       && CommandResultSet[commandIndex] == ResultSetMapping.NoResultSet)
                //{
                //    commandIndex++;
                //}

                //Debug.Assert(
                //    commandIndex == ModificationCommands.Count,
                //    "Expected " + ModificationCommands.Count + " results, got " + commandIndex);

                //var expectedResultSetCount = CommandResultSet.Count(e => e == ResultSetMapping.LastInResultSet);

                //Debug.Assert(
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

        public override void Execute(IRelationalConnection connection)
        {
            Check.NotNull(connection, nameof(connection));

            var storeCommands = CreateStoreCommand();
            int i = 0;
            foreach (var storeCommand in storeCommands)
            {
                try
                {
                    using (var dataReader = storeCommand.RelationalCommand.ExecuteReader(
                        connection, storeCommand.ParameterValues))
                    {
                        if (CommandResultSet.Count!=0)
                        {
                            CommandResultSet[i] = CheckResult(storeCommand);
                            if (CommandResultSet[i] == ResultSetMapping.LastInResultSet)
                            {
                                Consume(dataReader, i);
                            }
                        }
                        
                    }
                    i++;
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
