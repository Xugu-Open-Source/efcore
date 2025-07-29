// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Logging;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal
{
    public class XGDatabaseModelFactory : IDatabaseModelFactory
    {
        private XGConnection _connection;
        private DatabaseModel _databaseModel;
        private Dictionary<string, DatabaseTable> _tables;

        public XGDatabaseModelFactory(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateCommandsLogger();
        }

        protected virtual ILogger Logger { get; }

        private void ResetState()
        {
            _connection = null;
            _databaseModel = new DatabaseModel();
            _tables = new Dictionary<string, DatabaseTable>();
        }

        public DatabaseModel Create(string connectionString, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            using (var connection = new XGConnection(connectionString))
            {
                return Create(connection, tables, schemas);
            }
        }

        public DatabaseModel Create(DbConnection connection, IEnumerable<string> tables, IEnumerable<string> schemas)
        {
            ResetState();

            _connection = (XGConnection)connection;

            var connectionStartedOpen = _connection.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                _connection.Open();
            }

            try
            {
                _databaseModel.DatabaseName = _connection.Database;
                _databaseModel.DefaultSchema = null;

                GetTables(tables.ToList());
                GetColumns();
                GetPrimaryKeys();
                GetIndexes();
                GetConstraints();
                GetUConstraints();

                return _databaseModel;
            }
            finally
            {
                if (!connectionStartedOpen)
                {
                    _connection.Close();
                }
            }
        }

        private const string GetTablesQuery = @"SELECT `s`.`SCHEMA_NAME`,`t`.`TABLE_NAME` FROM `ALL_TABLES` AS `t`
LEFT JOIN `ALL_SCHEMAS` AS `s`
ON `t`.`SCHEMA_ID`=`s`.`SCHEMA_ID`;";

        private void GetTables(List<string> allowedTables)
        {
            using (var command = new XGCommand(GetTablesQuery, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var table = new DatabaseTable
                    {
                        Schema = reader.GetString(0).Replace("`", ""),
                        Name = reader.GetString(1).Replace("`", "")
                    };

                    if (allowedTables.Count == 0
                        || allowedTables.Contains(table.Name))
                    {
                        _databaseModel.Tables.Add(table);
                        _tables[table.Name] = table;
                    }
                }
            }
        }

        private const string GetColumnsQuery = @"SELECT COL_NAME AS `COLUMN_NAME`,
DEF_VAL AS `COLUMN_DEFAULT`,
IF(`NOT_NULL`=TRUE,'NO','YES') AS `IS_NULLABLE`,
TYPE_NAME AS `DATA_TYPE`,
CONS_TYPE AS `KEY`,
`VARYING`,
IF(`TYPE_NAME` = 'GUID' AND `DEF_VAL`='""UUID""()', 'IDENTITY', IF(`IS_SERIAL` = TRUE, 'IDENTITY', '')) AS `EXTRA`,
(SCALE/65536)::INT AS `PRECISION`,
CAST(MOD(SCALE,65536) AS INT) AS `SCALE`
FROM ALL_COLUMNS AS `c`
LEFT JOIN `ALL_CONSTRAINTS` AS `s`
ON `s`.`DEFINE` LIKE '%""' || `c`.`COL_NAME` ||'""%' AND `s`.`TABLE_ID`=`c`.`TABLE_ID` AND `s`.`CONS_TYPE`!='F'
WHERE c.TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME= '{0}')
UNION ALL
SELECT COL_NAME AS `COLUMN_NAME`,
NULL AS `COLUMN_DEFAULT`,
'NO' AS `IS_NULLABLE`,
TYPE_NAME AS `DATA_TYPE`,
NULL AS `KEY`,
`VARYING`,
'' AS `EXTRA`,
(SCALE/65536)::INT AS `PRECISION`,
CAST(MOD(SCALE,65536) AS INT) AS `SCALE`
FROM all_view_columns
WHERE VIEW_ID = (SELECT VIEW_ID FROM ALL_VIEWS WHERE VIEW_NAME='{0}');";

        private void GetColumns()
        {
            foreach (var x in _tables)
            {
                using (var command = new XGCommand(string.Format(GetColumnsQuery, x.Key), _connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columnScale = reader.GetInt32(8);
                        var columnPrecision = reader.GetInt32(7);
                        var extra = reader.GetString(6);
                        ValueGenerated valueGenerated;
                        if (extra.IndexOf("IDENTITY", StringComparison.Ordinal) >= 0)
                        {
                            valueGenerated = ValueGenerated.OnAdd;
                        }
                        else if (extra.IndexOf("on update", StringComparison.Ordinal) >= 0)
                        {
                            if (reader[1] != DBNull.Value && extra.IndexOf(reader[1].ToString(), StringComparison.Ordinal) > 0)
                            {
                                valueGenerated = ValueGenerated.OnAddOrUpdate;
                            }
                            else
                            {
                                valueGenerated = ValueGenerated.OnUpdate;
                            }
                        }
                        else
                        {
                            valueGenerated = ValueGenerated.Never;
                        }

                        string storeType = reader.GetString(3);
                        string typePrefix =storeType.Equals("char",StringComparison.OrdinalIgnoreCase) ? reader.GetString(5) ?? "":"";

                        var column = new DatabaseColumn
                        {
                            Table = x.Value,
                            Name = reader.GetString(0),
                            StoreType = GetColumnType(typePrefix + reader.GetString(3), columnPrecision, columnScale),
                            IsNullable = reader.GetString(2) == "YES",
                            DefaultValueSql = reader[1] == DBNull.Value
                                ? null
                                : '\'' + ParseToXGString(reader[1].ToString()) + '\'',
                            ValueGenerated = valueGenerated
                        };
                        x.Value.Columns.Add(column);
                    }
                }
            }
        }
        private readonly List<string> ColumnCharTypes = new List<string> { "char", "varchar" };
        protected virtual string GetColumnType(string dataType, int precision, int scale)
        {
            if (dataType.ToLower() == "numeric")
            {
                return $"numeric({precision},{scale})";
            }
            else if (ColumnCharTypes.Contains(dataType.ToLower()) && scale > 0)
            {
                return $"{dataType}({scale})";
            }
            return dataType;
        }

        private string ParseToXGString(string str)
        {
            // Pending the XGConnector implement XGCommandBuilder class
            return str
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"");
        }

        private const string GetPrimaryQuery = @"SELECT INDEX_NAME,REPLACE(REPLACE(KEYS, '""""', '"",""'), '""', '') AS `COLUMNS` FROM ALL_INDEXES WHERE IS_PRIMARY=TRUE AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='{0}');";

        /// <remarks>
        /// Primary keys are handled as in <see cref="GetConstraints"/>, not here
        /// </remarks>
        private void GetPrimaryKeys()
        {
            foreach (var x in _tables)
            {
                using (var command =
                    new XGCommand(string.Format(GetPrimaryQuery, x.Key),
                        _connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            var index = new DatabasePrimaryKey
                            {
                                Table = x.Value,
                                Name = reader.GetString(0)
                            };

                            foreach (var column in reader.GetString(1).Split(','))
                            {
                                index.Columns.Add(x.Value.Columns.Single(y => y.Name == column.Replace("\"", "")));
                            }

                            x.Value.PrimaryKey = index;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Error assigning primary key for {table}.", x.Key);
                        }
                    }
                }
            }
        }

        private const string GetIndexesQuery = @"SELECT INDEX_NAME,IS_UNIQUE AS `NON_UNIQUE`,REPLACE(REPLACE(KEYS, '""""', '"",""'), '""', '') AS `COLUMNS` FROM ALL_INDEXES WHERE IS_PRIMARY=FALSE AND TABLE_ID=(SELECT TABLE_ID FROM ALL_TABLES WHERE TABLE_NAME='{0}');";

        /// <remarks>
        /// Primary keys are handled as in <see cref="GetConstraints"/>, not here
        /// </remarks>
        private void GetIndexes()
        {
            foreach (var x in _tables)
            {
                using (var command =
                    new XGCommand(string.Format(GetIndexesQuery, x.Key),
                        _connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            var index = new DatabaseIndex
                            {
                                Table = x.Value,
                                Name = reader.GetString(0),
                                IsUnique = reader.GetBoolean(1)
                            };

                            foreach (var column in reader.GetString(2).Split(','))
                            {
                                index.Columns.Add(x.Value.Columns.Single(y => y.Name == column.Replace("\"", "")));
                            }

                            x.Value.Indexes.Add(index);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Error assigning index for {table}.", x.Key);
                        }
                    }
                }
            }
        }

        private const string GetConstraintsQuery = @"SELECT CONS_NAME AS `CONSTRAINT_NAME`,t1.TABLE_NAME AS `TABLE_NAME`,t2.TABLE_NAME AS `REFERENCED_TABLE_NAME`,
REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(DEFINE,',','|'),')(',','),'(',''),')',''),'""','') AS PAIRED_COLUMNS,DELETE_ACTION AS `DELETE_RULE` FROM ALL_CONSTRAINTS c LEFT JOIN ALL_TABLES t1 ON c.TABLE_ID=t1.TABLE_ID LEFT JOIN ALL_TABLES t2 ON c.REF_TABLE_ID=t2.TABLE_ID WHERE c.CONS_TYPE!='P' AND t1.TABLE_NAME='{0}' AND t2.TABLE_NAME IS NOT NULL;";

        private void GetConstraints()
        {
            foreach (var x in _tables)
            {
                using (var command =
                    new XGCommand(string.Format(GetConstraintsQuery, x.Key),
                        _connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var referencedTableName = reader.GetString(2);
                        if (_tables.TryGetValue(referencedTableName, out var referencedTable))
                        {
                            var fkInfo = new DatabaseForeignKey
                            {
                                Name = reader.GetString(0),
                                OnDelete = ConvertToReferentialAction(reader.GetString(4)),
                                Table = x.Value,
                                PrincipalTable = referencedTable
                            };
                            var pair = reader.GetString(3).Split(',');
                            if (x.Value.Name != fkInfo.PrincipalTable.Name)
                            {
                                foreach (var item in pair[0].Split('|'))
                                {
                                    fkInfo.Columns.Add(x.Value.Columns.FirstOrDefault(y => string.Equals(y.Name, item, StringComparison.OrdinalIgnoreCase)));
                                }
                            }

                            foreach (var item in pair[1].Split('|'))
                            {
                                fkInfo.PrincipalColumns.Add(fkInfo.PrincipalTable.Columns.FirstOrDefault(y => string.Equals(y.Name, item, StringComparison.OrdinalIgnoreCase)));
                            }

                            x.Value.ForeignKeys.Add(fkInfo);
                        }
                        else
                        {
                            Logger.LogWarning($"Referenced table `{referencedTableName}` is not in dictionary.");
                        }
                    }
                }
            }
        }

        private const string GetUConstraintsQuery = @"SELECT CONS_NAME AS `CONSTRAINT_NAME`,t1.TABLE_NAME AS `TABLE_NAME`,
REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(DEFINE,',','|'),')(',','),'(',''),')',''),'""','') AS DEFINE FROM ALL_CONSTRAINTS c LEFT JOIN ALL_TABLES t1 ON c.TABLE_ID=t1.TABLE_ID LEFT JOIN ALL_TABLES t2 ON c.REF_TABLE_ID=t2.TABLE_ID WHERE c.CONS_TYPE='U' AND t1.TABLE_NAME='{0}';";

        private void GetUConstraints()
        {
            foreach (var x in _tables)
            {
                using (var command =
                    new XGCommand(string.Format(GetUConstraintsQuery, x.Key),
                        _connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var ukInfo = new DatabaseUniqueConstraint
                        {
                            Name = reader.GetString(0),
                            Table = x.Value,
                        };
                        var pair = reader.GetString(2);

                        foreach (var item in pair.Split('|'))
                        {
                            ukInfo.Columns.Add(ukInfo.Table.Columns.FirstOrDefault(y => string.Equals(y.Name, item, StringComparison.OrdinalIgnoreCase)));
                        }

                        x.Value.UniqueConstraints.Add(ukInfo);
                    }
                }
            }
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

                case "N":
                    return ReferentialAction.NoAction;

                default:
                    return null;
            }
        }
    }
}
