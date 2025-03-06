// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XuGuDbDatabaseModelFactory : IDatabaseModelFactory
    {
        private XGConnection _connection;
        private Version _serverVersion;
        private TableSelectionSet _tableSelectionSet;
        private DatabaseModel _databaseModel;
        private Dictionary<string, TableModel> _tables;
        private Dictionary<string, ColumnModel> _tableColumns;

        private static string TableKey(TableModel table) => TableKey(table.Name, table.SchemaName);
        private static string TableKey(string name, string schema = null) => "`" + (schema ?? "") + "`.`" + name + "`";
        private static string ColumnKey(TableModel table, string columnName) => TableKey(table) + ".`" + columnName + "`";

        private static readonly ISet<string> _dateTimePrecisionTypes = new HashSet<string> { "DATETIME", "TIME" };
        private const int DefaultDateTimePrecision = 7;
        // see https://msdn.microsoft.com/en-us/library/ff878091.aspx
        private static readonly Dictionary<string, long[]> _defaultSequenceMinMax = new Dictionary<string, long[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "tinyint", new[] { 0L, 255L } },
            { "smallint", new[] { -32768L, 32767L } },
            { "int", new[] { -2147483648L, 2147483647L } },
            { "bigint", new[] { -9223372036854775808L, 9223372036854775807L } },
            { "int64", new[] { -9223372036854775808L, 9223372036854775807L } },
            { "decimal", new[] { -999999999999999999L, 999999999999999999L } },
            { "numeric", new[] { -999999999999999999L, 999999999999999999L } }
        };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XuGuDbDatabaseModelFactory([NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            Logger = loggerFactory.CreateCommandsLogger();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ILogger Logger { get; }

        private void ResetState()
        {
            _connection = null;
            _serverVersion = null;
            _tableSelectionSet = null;
            _databaseModel = new DatabaseModel();
            _tables = new Dictionary<string, TableModel>();
            _tableColumns = new Dictionary<string, ColumnModel>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual DatabaseModel Create(string connectionString, TableSelectionSet tableSelectionSet)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(tableSelectionSet, nameof(tableSelectionSet));

            ResetState();

            using (_connection = new XGConnection(connectionString))
            {
                _connection.Open();
                _tableSelectionSet = tableSelectionSet;

                _databaseModel.DatabaseName = _connection.Database;

                Version.TryParse(_connection.ServerVersion, out _serverVersion);
                
                GetSequences();

                GetDefaultSchema();
                GetTypeAliases();
                GetTables();
                GetColumns();
                GetIndexes();
                GetForeignKeys();
                return _databaseModel;
            }
        }

        private bool SupportsSequences => _serverVersion?.Major >= 11;

        private string TemporalTableWhereClause =>
            _serverVersion?.Major >= 13 ? " AND t.TEMP_TYPE = 0" : string.Empty;

        private void GetDefaultSchema()
        {
            var command = _connection.CreateCommand();
            command.CommandText = "SELECT current_schema";
            var schema = command.ExecuteScalar() as string ?? "SYSDBA";
            Logger.LogTrace(XuGuDbDesignStrings.FoundDefaultSchema(schema));
            _databaseModel.DefaultSchemaName = schema;
        }

        private void GetTypeAliases()
        {
            var command = _connection.CreateCommand();
            _connection.ChangeDatabase("SYSTEM");
            command.CommandText = @"SELECT
                        `t`.`TYPE_NAME` `type_name`,
                        `t`.`MEMBER_DT` `underlying_system_type`
                        FROM
                        `all_types` `t`
                        WHERE `t`.`MEMBER_DT` IS NOT NULL
						UNION
						SELECT DISTINCT
                        `s`.`NAME`,
                        NULL `underlying_system_type`
                        FROM
                        `sys_datatypes` `s`";

            var typeAliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var alias = reader.GetValueOrDefault<string>("type_name");
                    var underlyingSystemType = reader.GetValueOrDefault<string>("underlying_system_type");
                    Logger.LogTrace(XuGuDbDesignStrings.FoundTypeAlias(alias, underlyingSystemType));
                    typeAliasMap.Add(alias, underlyingSystemType);
                }
                _connection.ChangeDatabase("`"+_databaseModel.DatabaseName+"`");
            }

            _databaseModel.XuGuDb().TypeAliases = typeAliasMap;
        }

        private void GetSequences()
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"SELECT s.SEQ_NAME AS `name`,
                        s.IS_CYCLE AS `is_cycling`,
                        s.MIN_VAL AS `minimum_value`,
                        s.MAX_VAL AS `maximum_value`,
                        s.CURR_VAL AS `start_value`,
                        s.STEP_VAL AS `increment`,
                        'int64' AS `type_name`,
                        c.SCHEMA_NAME AS schema_name
                        FROM ALL_SEQUENCES s
                        LEFT JOIN ALL_SCHEMAS c
                        ON (s.SCHEMA_ID=c.SCHEMA_ID)";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var sequence = new SequenceModel
                    {
                        Database = _databaseModel,
                        SchemaName = reader.GetValueOrDefault<string>("schema_name"),
                        Name = reader.GetValueOrDefault<string>("name"),
                        DataType = reader.GetValueOrDefault<string>("type_name"),
                        IsCyclic = reader.GetValueOrDefault<bool?>("is_cycling"),
                        IncrementBy = reader.GetValueOrDefault<int?>("increment"),
                        Start = reader.GetValueOrDefault<long?>("start_value"),
                        Min = reader.GetValueOrDefault<long?>("minimum_value"),
                        Max = reader.GetValueOrDefault<long?>("maximum_value")
                    };

                    Logger.LogTrace(XuGuDbDesignStrings.FoundSequence(
                        sequence.SchemaName, sequence.Name, sequence.DataType, sequence.IsCyclic,
                        sequence.IncrementBy, sequence.Start, sequence.Min, sequence.Max));

                    if (string.IsNullOrEmpty(sequence.Name))
                    {
                        Logger.LogWarning(XuGuDbDesignStrings.SequenceNameEmpty(sequence.SchemaName));
                        continue;
                    }

                    if (_defaultSequenceMinMax.ContainsKey(sequence.DataType))
                    {
                        var defaultMin = _defaultSequenceMinMax[sequence.DataType][0];
                        sequence.Min = sequence.Min == defaultMin ? null : sequence.Min;
                        sequence.Start = sequence.Start == defaultMin ? null : sequence.Start;

                        var defaultMax = _defaultSequenceMinMax[sequence.DataType][1];
                        sequence.Max = sequence.Max == defaultMax ? null : sequence.Max;
                    }

                    _databaseModel.Sequences.Add(sequence);
                }
            }
        }

        private void GetTables()
        {
            var command = _connection.CreateCommand();
            // for origin of the sys.extended_properties SELECT statement
            // below see https://github.com/aspnet/EntityFramework/issues/5126
            command.CommandText =
                @"SELECT
					    s.SCHEMA_NAME AS `schema`,
					    t.TABLE_NAME AS `name`
					    FROM ALL_TABLES AS t
                        LEFT JOIN ALL_SCHEMAS s
                        ON (s.SCHEMA_ID=t.SCHEMA_ID)
                        WHERE t.TABLE_NAME <> " +
                        $"'{HistoryRepository.DefaultTableName}'" + TemporalTableWhereClause;
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var table = new TableModel
                    {
                        Database = _databaseModel,
                        SchemaName = reader.GetValueOrDefault<string>("schema"),
                        Name = reader.GetValueOrDefault<string>("name")
                    };

                    Logger.LogTrace(XuGuDbDesignStrings.FoundTable(table.SchemaName, table.Name));

                    if (_tableSelectionSet.Allows(table.SchemaName, table.Name))
                    {
                        _databaseModel.Tables.Add(table);
                        _tables[TableKey(table)] = table;
                    }
                    else
                    {
                        Logger.LogTrace(
                            XuGuDbDesignStrings.TableNotInSelectionSet(table.SchemaName, table.Name));
                    }
                }
            }
        }

        private void GetColumns()
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"SELECT DISTINCT
                        s.SCHEMA_NAME AS `schema`,
					    t.TABLE_NAME AS `table`,
					    c.TYPE_NAME AS `typename`,
					    c.COL_NAME AS `column_name`,
					    c.COL_NO AS `ordinal`,
					    c.NOT_NULL AS `nullable`,
					    (LENGTH(SUBSTR(i.KEYS, 1, INSTR(i.KEYS, '""' || `c`.`COL_NAME` || '""') * 1)) - LENGTH(REPLACE(SUBSTR(i.KEYS, 1, INSTR(i.KEYS, '""' || `c`.`COL_NAME` || '""') * 1), ',', '')) + 1) AS `primary_key_ordinal`,
                        c.DEF_VAL AS `default_sql`,
					    NULL AS `definition`,
					    (c.SCALE/65536)::INT AS `precision`,
					    CAST(MOD(c.SCALE,65536) AS INT) AS `scale`,
					    c.SCALE AS `max_length`,
					    c.IS_SERIAL AS `is_identity`,
					    false AS `is_computed`
                        FROM ALL_COLUMNS AS c
                        LEFT JOIN ALL_TABLES AS t
                        ON(c.TABLE_ID=t.TABLE_ID)
                        LEFT JOIN ALL_SCHEMAS s
                        ON (s.SCHEMA_ID=t.SCHEMA_ID)
                        LEFT JOIN ALL_INDEXES AS i
                        ON(i.TABLE_ID=t.TABLE_ID AND i.KEYS LIKE '%""' || `c`.`COL_NAME` || '""%')
                        WHERE t.TABLE_NAME <> '" + HistoryRepository.DefaultTableName + "'" +
                                  TemporalTableWhereClause;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var schemaName = reader.GetValueOrDefault<string>("schema");
                    var tableName = reader.GetValueOrDefault<string>("table");
                    var columnName = reader.GetValueOrDefault<string>("column_name");
                    var dataTypeName = reader.GetValueOrDefault<string>("typename");
                    var ordinal = reader.GetValueOrDefault<int>("ordinal");
                    var nullable = !reader.GetValueOrDefault<bool>("nullable");
                    int? primary_key_ordinal = reader.GetValueOrDefault<int>("primary_key_ordinal");
                    var primaryKeyOrdinal = primary_key_ordinal!=0?primary_key_ordinal:null;
                    var defaultValue = reader.GetValueOrDefault<string>("default_sql");
                    var computedValue = string.Empty;//reader.GetValueOrDefault<string>("computed_sql");
                    var precision = reader.GetValueOrDefault<int?>("precision");
                    var scale = reader.GetValueOrDefault<int?>("scale");
                    var maxLength = reader.GetValueOrDefault<int?>("max_length");
                    var isIdentity = reader.GetValueOrDefault<bool>("is_identity");
                    var isComputed = reader.GetValueOrDefault<bool>("is_computed");

                    Logger.LogTrace(XuGuDbDesignStrings.FoundColumn(
                        schemaName, tableName, columnName, dataTypeName, ordinal, nullable,
                        primaryKeyOrdinal, defaultValue, computedValue, precision, scale, maxLength, isIdentity, isComputed));

                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                    {
                        Logger.LogTrace(
                            XuGuDbDesignStrings.ColumnNotInSelectionSet(columnName, schemaName, tableName));
                        continue;
                    }

                    if (string.IsNullOrEmpty(columnName))
                    {
                        Logger.LogWarning(XuGuDbDesignStrings.ColumnNameEmptyOnTable(schemaName, tableName));
                        continue;
                    }

                    TableModel table;
                    if (!_tables.TryGetValue(TableKey(tableName, schemaName), out table))
                    {
                        Logger.LogWarning(
                            XuGuDbDesignStrings.UnableToFindTableForColumn(columnName, schemaName, tableName));
                        continue;
                    }

                    if (dataTypeName == "varchar"
                        || dataTypeName == "char")
                    {
                        maxLength /= 2;
                    }

                    if (dataTypeName == "decimal"
                        || dataTypeName == "numeric")
                    {
                        // maxlength here represents storage bytes. The server determines this, not the client.
                        maxLength = null;
                    }

                    var dateTimePrecision = default(int?);
                    if (_dateTimePrecisionTypes.Contains(dataTypeName))
                    {
                        dateTimePrecision = scale ?? DefaultDateTimePrecision;
                        scale = null;
                    }

                    var column = new ColumnModel
                    {
                        Table = table,
                        DataType = dataTypeName,
                        Name = columnName,
                        Ordinal = ordinal,
                        IsNullable = nullable,
                        PrimaryKeyOrdinal = primaryKeyOrdinal,
                        DefaultValue = defaultValue,
                        ComputedValue = computedValue,
                        Precision = precision,
                        Scale = scale,
                        MaxLength = maxLength <= 0 ? default(int?) : maxLength
                    };
                    column.XuGuDb().IsIdentity = isIdentity;
                    column.XuGuDb().DateTimePrecision = dateTimePrecision;

                    if (!table.Columns.Any(i=>i.Name==column.Name)&& !_tableColumns.ContainsKey(ColumnKey(table, column.Name)))
                    {
                        table.Columns.Add(column);
                        _tableColumns.Add(ColumnKey(table, column.Name), column);
                    }
                    
                }
            }
        }

        private void GetIndexes()
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"SELECT
                        s.SCHEMA_NAME AS `schema_name`,
					    t.TABLE_NAME AS `table_name`,
					    i.INDEX_NAME AS `index_name`,
					    i.IS_UNIQUE AS `is_unique`,
					    c.COL_NAME AS `column_name`,
					    i.KEYS AS `type_desc`,
					    (LENGTH(SUBSTR(i.KEYS, 1, INSTR(i.KEYS, '""' || `c`.`COL_NAME` || '""') * 1)) - LENGTH(REPLACE(SUBSTR(i.KEYS, 1, INSTR(i.KEYS, '""' || `c`.`COL_NAME` || '""') * 1), ',', '')) + 1) AS `key_ordinal`
                        FROM ALL_COLUMNS AS c
                        LEFT JOIN ALL_TABLES AS t
                        ON(c.TABLE_ID = t.TABLE_ID)
                        LEFT JOIN ALL_SCHEMAS s
                        ON(s.SCHEMA_ID = t.SCHEMA_ID)
                        LEFT JOIN ALL_INDEXES AS i
                        ON(i.TABLE_ID = t.TABLE_ID AND i.KEYS LIKE '%""' || `c`.`COL_NAME` || '""%')
                        WHERE t.TABLE_NAME <> '" + HistoryRepository.DefaultTableName + @"'" +
                                  TemporalTableWhereClause + @"
                        ORDER BY s.SCHEMA_NAME,t.TABLE_NAME,i.INDEX_NAME,`key_ordinal`";

            using (var reader = command.ExecuteReader())
            {
                IndexModel index = null;
                while (reader.Read())
                {
                    var schemaName = reader.GetValueOrDefault<string>("schema_name");
                    var tableName = reader.GetValueOrDefault<string>("table_name");
                    var indexName = reader.GetValueOrDefault<string>("index_name");
                    var isUnique = reader.GetValueOrDefault<bool>("is_unique");
                    var typeDesc = reader.GetValueOrDefault<string>("type_desc");
                    var columnName = reader.GetValueOrDefault<string>("column_name");
                    var indexOrdinal = reader.GetValueOrDefault<byte>("key_ordinal");

                    Logger.LogTrace(XuGuDbDesignStrings.FoundIndexColumn(
                        schemaName, tableName, indexName, isUnique, typeDesc, columnName, indexOrdinal));

                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                    {
                        Logger.LogTrace(XuGuDbDesignStrings.IndexColumnNotInSelectionSet(
                            columnName, indexName, schemaName, tableName));
                        continue;
                    }

                    if (string.IsNullOrEmpty(indexName))
                    {
                        Logger.LogWarning(XuGuDbDesignStrings.IndexNameEmpty(schemaName, tableName));
                        continue;
                    }

                    Debug.Assert(index == null || index.Table != null);
                    if (index == null
                        || index.Name != indexName
                        || index.Table.Name != tableName
                        || index.Table.SchemaName != schemaName)
                    {
                        TableModel table;
                        if (!_tables.TryGetValue(TableKey(tableName, schemaName), out table))
                        {
                            Logger.LogWarning(
                                XuGuDbDesignStrings.UnableToFindTableForIndex(indexName, schemaName, tableName));
                            continue;
                        }

                        index = new IndexModel
                        {
                            Table = table,
                            Name = indexName,
                            IsUnique = isUnique
                        };

                        table.Indexes.Add(index);
                    }

                    ColumnModel column;
                    if (string.IsNullOrEmpty(columnName))
                    {
                        Logger.LogWarning(
                            XuGuDbDesignStrings.ColumnNameEmptyOnIndex(
                                schemaName, tableName, indexName));
                    }
                    else if (!_tableColumns.TryGetValue(ColumnKey(index.Table, columnName), out column))
                    {
                        Logger.LogWarning(
                            XuGuDbDesignStrings.UnableToFindColumnForIndex(
                                indexName, columnName, schemaName, tableName));
                    }
                    else
                    {
                        var indexColumn = new IndexColumnModel
                        {
                            Index = index,
                            Column = column,
                            Ordinal = indexOrdinal
                        };

                        index.IndexColumns.Add(indexColumn);
                    }
                }
            }
        }

        private void GetForeignKeys()
        {
            List<string> foreignKeys = new List<string>();
            var command = _connection.CreateCommand();
            command.CommandText = @"SELECT
                        s.SCHEMA_NAME AS `schema_name`,
					    t.TABLE_NAME AS `table_name`,
					    i.CONS_NAME AS `foreign_key_name`,
					    s2.SCHEMA_NAME AS `principal_table_schema_name`,
					    t2.TABLE_NAME AS `principal_table_name`,
					    TRIM(BOTH '""' FROM SUBSTRING(i.DEFINE,INSTR(i.DEFINE, '(') + 1,INSTR(i.DEFINE, ')') - INSTR(i.DEFINE, '(') - 1)) AS `constraint_column_name`,
                        TRIM(BOTH '""' FROM SUBSTRING(i.DEFINE, INSTR(i.DEFINE, '(""', INSTR(i.DEFINE, '(""') + 1) + 2, INSTR(i.DEFINE, '"")', INSTR(i.DEFINE, '"")') + 1) - INSTR(i.DEFINE, '(""', INSTR(i.DEFINE, '(""') + 1) - 2)) AS `referenced_column_name`,
                        i.ENABLE AS `is_disabled`,
                        i.DELETE_ACTION AS `delete_referential_action_desc`,
                        i.UPDATE_ACTION AS `update_referential_action_desc`,
                        c.COL_NO AS `constraint_column_id`
                        FROM ALL_COLUMNS AS c
                        LEFT JOIN ALL_TABLES AS t
                        ON(c.TABLE_ID = t.TABLE_ID)
                        LEFT JOIN ALL_SCHEMAS s
                        ON(s.SCHEMA_ID = t.SCHEMA_ID)
                        LEFT JOIN ALL_CONSTRAINTS AS i
                        ON(i.TABLE_ID = t.TABLE_ID AND i.DEFINE LIKE '%""' || `c`.`COL_NAME` || '""%')
                        LEFT JOIN ALL_TABLES AS t2
                        ON(i.REF_TABLE_ID = t2.TABLE_ID)
                        LEFT JOIN ALL_SCHEMAS s2
                        ON(s2.SCHEMA_ID = t2.SCHEMA_ID)
                        WHERE i.CONS_TYPE = 'F'
                        ORDER BY s.SCHEMA_NAME, t.TABLE_NAME, i.CONS_NAME";
            using (var reader = command.ExecuteReader())
            {
                var lastFkName = string.Empty;
                var lastFkSchemaName = string.Empty;
                var lastFkTableName = string.Empty;
                ForeignKeyModel fkInfo = null;
                while (reader.Read())
                {
                    var schemaName = reader.GetValueOrDefault<string>("schema_name");
                    var tableName = reader.GetValueOrDefault<string>("table_name");
                    var fkName = reader.GetValueOrDefault<string>("foreign_key_name");
                    var principalTableSchemaName = reader.GetValueOrDefault<string>("principal_table_schema_name");
                    var principalTableName = reader.GetValueOrDefault<string>("principal_table_name");
                    var fromColumnName = reader.GetValueOrDefault<string>("constraint_column_name");
                    var toColumnName = reader.GetValueOrDefault<string>("referenced_column_name");
                    var updateAction = reader.GetValueOrDefault<string>("update_referential_action_desc");
                    var deleteAction = reader.GetValueOrDefault<string>("delete_referential_action_desc");
                    var ordinal = reader.GetValueOrDefault<int>("constraint_column_id");

                    string fkInfoKey = $"{schemaName}.{tableName}.{fkName}";
                    if (foreignKeys.Contains(fkInfoKey))
                    {
                        SetFkColumns(fromColumnName, toColumnName, ordinal, fkInfo, fkName);
                        break;
                    }
                    foreignKeys.Add(fkInfoKey);

                    Logger.LogTrace(XuGuDbDesignStrings.FoundForeignKeyColumn(
                        schemaName, tableName, fkName, principalTableSchemaName, principalTableName,
                        fromColumnName, toColumnName, updateAction, deleteAction, ordinal));

                    if (string.IsNullOrEmpty(fkName))
                    {
                        Logger.LogWarning(XuGuDbDesignStrings.ForeignKeyNameEmpty(schemaName, tableName));
                        continue;
                    }

                    if (!_tableSelectionSet.Allows(schemaName, tableName))
                    {
                        Logger.LogTrace(XuGuDbDesignStrings.ForeignKeyColumnNotInSelectionSet(
                            fromColumnName, fkName, schemaName, tableName));
                        continue;
                    }

                    if (fkInfo == null
                        || lastFkSchemaName != schemaName
                        || lastFkTableName != tableName
                        || lastFkName != fkName)
                    {
                        lastFkName = fkName;
                        lastFkSchemaName = schemaName;
                        lastFkTableName = tableName;
                        var table = _tables[TableKey(tableName, schemaName)];

                        TableModel principalTable = null;
                        if (!string.IsNullOrEmpty(principalTableSchemaName)
                            && !string.IsNullOrEmpty(principalTableName))
                        {
                            _tables.TryGetValue(TableKey(principalTableName, principalTableSchemaName), out principalTable);
                        }

                        if (principalTable == null)
                        {
                            Logger.LogTrace(XuGuDbDesignStrings.PrincipalTableNotInSelectionSet(
                                fkName, schemaName, tableName, principalTableSchemaName, principalTableName));
                        }

                        fkInfo = new ForeignKeyModel
                        {
                            Name = fkName,
                            Table = table,
                            PrincipalTable = principalTable,
                            OnDelete = ConvertToReferentialAction(deleteAction)
                        };

                        table.ForeignKeys.Add(fkInfo);
                    }

                    SetFkColumns(fromColumnName, toColumnName, ordinal, fkInfo, fkName);
                }
            }
        }

        public void SetFkColumns(string fromColumnName,string toColumnName,int ordinal, ForeignKeyModel fkInfo, string fkName)
        {
            if (fkInfo == null) return;
            var fkColumn = new ForeignKeyColumnModel
            {
                Ordinal = ordinal
            };
            ColumnModel fromColumn;
            if ((fromColumn = FindColumnForForeignKey(GetColName(fromColumnName, ordinal), fkInfo.Table, fkName)) != null)
            {
                fkColumn.Column = fromColumn;
            }
            else
            {
                return;
            }

            if (fkInfo.PrincipalTable != null)
            {
                ColumnModel toColumn;
                if ((toColumn = FindColumnForForeignKey(GetColName(toColumnName, ordinal), fkInfo.PrincipalTable, fkName)) != null)
                {
                    fkColumn.PrincipalColumn = toColumn;
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
            if (!fkInfo.Columns.Any(i=>i.Column.DisplayName== fkColumn.Column.DisplayName))
            {
                fkInfo.Columns.Add(fkColumn);
            }
            
        }

        public string GetColName(string str,int index)
        {
            string pattern = @"[^a-zA-Z0-9]";

            if (Regex.IsMatch(str, pattern))
            {
                return Regex.Split(str, "\",\"")[index];
            }

            return str;
        }

        private ColumnModel FindColumnForForeignKey(
            string columnName, TableModel table, string fkName)
        {
            ColumnModel column;
            if (string.IsNullOrEmpty(columnName))
            {
                Logger.LogWarning(
                    XuGuDbDesignStrings.ColumnNameEmptyOnForeignKey(
                        table.SchemaName, table.Name, fkName));
                return null;
            }

            if (!_tableColumns.TryGetValue(
                ColumnKey(table, columnName), out column))
            {
                Logger.LogWarning(
                    XuGuDbDesignStrings.UnableToFindColumnForForeignKey(
                        fkName, columnName, table.SchemaName, table.Name));
                return null;
            }

            return column;
        }

        private static ReferentialAction? ConvertToReferentialAction(string onDeleteAction)
        {
            switch (onDeleteAction.ToUpperInvariant())
            {
                case "R":
                    return ReferentialAction.Restrict;

                case "C":
                    return ReferentialAction.Cascade;

                case "U":
                    return ReferentialAction.SetNull;

                case "D":
                    return ReferentialAction.SetDefault;

                case "N":
                    return ReferentialAction.NoAction;

                default:
                    return null;
            }
        }
    }
}
