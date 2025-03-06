// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Xugu.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Internal;
using Microsoft.Extensions.Logging;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Scaffolding.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class XuguDatabaseModelFactory : DatabaseModelFactory
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;




        private const string NamePartRegex
            = @"(?:(?:\[(?<part{0}>(?:(?:\]\])|[^\]])+)\])|(?<part{0}>[^\.\[\]]+))";

        private static readonly Regex _partExtractor
            = new Regex(
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"^{0}(?:\.{1})?$",
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)),
                RegexOptions.Compiled,
                TimeSpan.FromMilliseconds(1000));


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguDatabaseModelFactory([NotNull] IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger)
        {
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            Check.NotNull(options, nameof(options));

            using (var connection = new XGConnection(connectionString))
            {
                return Create(connection, options);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(options, nameof(options));

            var databaseModel = new DatabaseModel();

            var connectionStartedOpen = connection.State == ConnectionState.Open;
            if (!connectionStartedOpen)
            {
                connection.Open();
            }

            try
            {
                databaseModel.DatabaseName = connection.Database;
                databaseModel.DefaultSchema = GetDefaultSchema(connection);

                var schemaList = options.Schemas.ToList();
                var schemaFilter = GenerateSchemaFilter(schemaList, databaseModel.DefaultSchema);
                var tableList = options.Tables.ToList();
                var tableFilter = GenerateTableFilter(tableList.Select(Parse).ToList()!, schemaFilter!);
                foreach (var table in GetTables(connection, tableFilter!))
                {
                    table.Database = databaseModel;
                    databaseModel.Tables.Add(table);
                }
                return databaseModel;
            }
            finally
            {
                if (!connectionStartedOpen)
                {
                    connection.Close();
                }
            }
        }

        private string? GetDefaultSchema(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SHOW CURRENT_SCHEMA;";

                if (command.ExecuteScalar() is string schema)
                {
                    _logger.DefaultSchemaFound(schema);

                    return schema;
                }

                return null;
            }
        }

        private static Func<string, string>? GenerateSchemaFilter(IReadOnlyList<string> schemas, string? defaultSchema)
        {
            return schemas.Count > 0 || defaultSchema != null
                ? (s =>
                {
                    var schemaFilterBuilder = new StringBuilder();
                    schemaFilterBuilder.Append(s);
                    schemaFilterBuilder.Append(" IN (");
                    if (schemas.Count > 0)
                        schemaFilterBuilder.Append(string.Join(", ", schemas.Select(EscapeLiteral)));
                    else
                        schemaFilterBuilder.Append(EscapeLiteral(defaultSchema!));
                    schemaFilterBuilder.Append(")");
                    return schemaFilterBuilder.ToString();
                })
                : (Func<string, string>?)null;
        }

        private static (string? Schema, string Table) Parse(string table)
        {
            var match = _partExtractor.Match(table.Trim());

            if (!match.Success)
            {
                throw new InvalidOperationException(XuguStrings.InvalidTableToIncludeInScaffolding(table));
            }

            var part1 = match.Groups["part1"].Value.Replace("]]", "]");
            var part2 = match.Groups["part2"].Value.Replace("]]", "]");

            return string.IsNullOrEmpty(part2) ? (null, part1) : (part1, part2);
        }

        private static Func<string, string, string>? GenerateTableFilter(
            IReadOnlyList<(string Schema, string Table)> tables,
            Func<string, string> schemaFilter)
        {
            return schemaFilter != null
                || tables.Count > 0
                    ? ((s, t) =>
                    {
                        var tableFilterBuilder = new StringBuilder();

                        var openBracket = false;
                        if (schemaFilter != null)
                        {
                            tableFilterBuilder
                                .Append("(")
                                .Append(schemaFilter(s));
                            openBracket = true;
                        }

                        if (tables.Count > 0)
                        {
                            if (openBracket)
                            {
                                tableFilterBuilder
                                    .AppendLine()
                                    .Append("OR ");
                            }
                            else
                            {
                                tableFilterBuilder.Append("(");
                                openBracket = true;
                            }

                            var tablesWithoutSchema = tables.Where(e => string.IsNullOrEmpty(e.Schema)).ToList();
                            if (tablesWithoutSchema.Count > 0)
                            {
                                tableFilterBuilder.Append(t);
                                tableFilterBuilder.Append(" IN (");
                                tableFilterBuilder.Append(string.Join(", ", tablesWithoutSchema.Select(e => EscapeLiteral(e.Table))));
                                tableFilterBuilder.Append(")");
                            }

                            var tablesWithSchema = tables.Where(e => !string.IsNullOrEmpty(e.Schema)).ToList();
                            if (tablesWithSchema.Count > 0)
                            {
                                if (tablesWithoutSchema.Count > 0)
                                {
                                    tableFilterBuilder.Append(" OR ");
                                }

                                tableFilterBuilder.Append(t);
                                tableFilterBuilder.Append(" IN (");
                                tableFilterBuilder.Append(string.Join(", ", tablesWithSchema.Select(e => EscapeLiteral(e.Table))));
                                tableFilterBuilder.Append(") AND (");
                                tableFilterBuilder.Append(s);
                                tableFilterBuilder.Append(" + '.' + ");
                                tableFilterBuilder.Append(t);
                                tableFilterBuilder.Append(") IN (");
                                tableFilterBuilder.Append(
                                    string.Join(", ", tablesWithSchema.Select(e => EscapeLiteral($"{e.Schema}.{e.Table}"))));
                                tableFilterBuilder.Append(")");
                            }
                        }

                        if (openBracket)
                        {
                            tableFilterBuilder.Append(")");
                        }

                        return tableFilterBuilder.ToString();
                    })
                    : null;
        }

        private static string EscapeLiteral(string s)
        {
            return $"'{s}'";
        }

        private static ReferentialAction? ConvertToReferentialAction(string deleteAction)
        {
            switch (deleteAction.ToUpperInvariant())
            {
                case "RESTRICT":
                    return ReferentialAction.Restrict;

                case "CASCADE":
                    return ReferentialAction.Cascade;

                case "SET NULL":
                    return ReferentialAction.SetNull;

                case "SET DEFAULT":
                    return ReferentialAction.SetDefault;

                case "NO ACTION":
                    return ReferentialAction.NoAction;

                default:
                    return null;
            }
        }

        private const string GetTablesQuery = @"select s.schema_name as TABLE_SCHEMA,table_name,comments as TABLE_COMMENT from all_tables t
left join all_schemas s on t.schema_id = s.schema_id WHERE {0};";
        /*
                private const string GetPrimaryQuery = @"SELECT  TABLE_SCHEMA,TABLE_NAME,CONS_NAME AS INDEX_NAME,DEFINE AS COLUMNS
        FROM (select T.TABLE_ID,s.schema_name as TABLE_SCHEMA,table_name,comments as TABLE_COMMENT from all_tables t 
        left join all_schemas s on t.schema_id = s.schema_id) S
        LEFT JOIN all_constraints C on C.TABLE_ID = s.TABLE_ID WHERE {0} AND `CONS_TYPE` = 'P';";*/

        // 存在列信息表存得列名为大写，但是约束表内定义得列为小写对不上得情况
        private const string GetPrimaryQuery = @"SELECT s.schema_name AS TABLE_SCHEMA, TABLE_NAME, CONS_NAME AS INDEX_NAME, COLUMNS FROM (SELECT table_id, WM_CONCAT(col.col_name) AS COLUMNS FROM (SELECT table_id, TABLE_SCHEMA, TABLE_NAME, CONS_NAME AS INDEX_NAME, DEFINE AS COLUMNS
FROM (select T.TABLE_ID, s.schema_name as TABLE_SCHEMA, table_name, comments as TABLE_COMMENT from all_tables t
left join all_schemas s on t.schema_id = s.schema_id) S
LEFT JOIN all_constraints C on C.TABLE_ID = s.TABLE_ID WHERE {0}
        AND `CONS_TYPE` = 'P' ) pk
LEFT JOIN all_columns col on col.table_id = pk.table_id AND POSITION(LCASE(col.col_name) IN LCASE(COLUMNS)) > 0 GROUP BY table_id) pk
LEFT JOIN all_tables t ON pk.table_id = t.table_id
LEFT JOIN all_schemas s ON t.schema_id = s.schema_id
LEFT JOIN all_constraints c ON c.table_id = pk.table_id WHERE `CONS_TYPE` = 'P';";


/*        private const string GetPrimaryQuery = @"SELECT
   `TABLE_SCHEMA`,
   `TABLE_NAME`,
   `INDEX_NAME`,
   GROUP_CONCAT(`COLUMN_NAME` ORDER BY `SEQ_IN_INDEX` SEPARATOR ',') AS COLUMNS
   FROM `INFORMATION_SCHEMA`.`STATISTICS`
   WHERE {0} 
   AND `CONS_TYPE` = 'P'
   GROUP BY `TABLE_SCHEMA`, `TABLE_NAME`, `INDEX_NAME`, `NON_UNIQUE`;";
*/
        private const string GetColumnsQuery = @"SELECT s.schema_name  as TABLE_SCHEMA,TABLE_NAME,COL_NAME AS COLUMN_NAME,col_no+1 AS ORDINAL_POSITION,def_val AS COLUMN_DEFAULT,IS_SERIAL,TIMESTAMP_T,
CASE NOT_NULL WHEN TRUE THEN 'NO' ELSE 'YES' END AS IS_NULLABLE, 
TYPE_NAME AS DATA_TYPE,COMMENTS AS COLUMN_COMMENT FROM ALL_COLUMNS C LEFT join  ALL_TABLES T ON C.table_id = T.table_id
LEFT JOIN all_schemas s ON t.schema_id = s.schema_id WHERE {0} ;";

        private const string GetIndexesQuery = @"SELECT s.schema_name as TABLE_SCHEMA,TABLE_NAME ,
index_name,IS_UNIQUE AS NON_UNIQUE,COLUMNS FROM (SELECT table_id,WM_CONCAT(col.col_name) AS COLUMNS FROM 
(SELECT table_id,s.schema_name as TABLE_SCHEMA,TABLE_NAME ,
index_name,IS_UNIQUE AS NON_UNIQUE,keys AS COLUMNS FROM all_indexes c
LEFT join  ALL_TABLES T ON C.table_id = T.table_id LEFT JOIN all_schemas s ON t.schema_id = s.schema_id WHERE {0} ) pk 
LEFT JOIN all_columns col  on col.table_id = pk.table_id AND POSITION(LCASE(col.col_name) IN LCASE(COLUMNS)) > 0 GROUP BY table_id) idx
LEFT JOIN all_tables t ON idx.table_id = t.table_id
LEFT JOIN all_schemas s ON t.schema_id = s.schema_id
LEFT JOIN all_indexes c ON C.table_id = T.table_id;";

/*        private const string GetIndexesQuery = @"SELECT s.schema_name as TABLE_SCHEMA,TABLE_NAME ,
index_name,IS_UNIQUE AS NON_UNIQUE,keys AS COLUMNS FROM all_indexes c
LEFT join  ALL_TABLES T ON C.table_id = T.table_id LEFT JOIN all_schemas s ON t.schema_id = s.schema_id WHERE {0} ;";*/

        private const string GetConstraintsQuery = @"SELECT
    `TABLE_SCHEMA`,
    `TABLE_NAME`,
    `CONSTRAINT_NAME`,
    `REFERENCED_TABLE_NAME`,
    GROUP_CONCAT(CONCAT_WS('|', `COLUMN_NAME`, `REFERENCED_COLUMN_NAME`) ORDER BY `ORDINAL_POSITION` SEPARATOR ',') AS PAIRED_COLUMNS,
    (SELECT `DELETE_RULE` FROM `INFORMATION_SCHEMA`.`REFERENTIAL_CONSTRAINTS` 
      WHERE `REFERENTIAL_CONSTRAINTS`.`CONSTRAINT_NAME` = `KEY_COLUMN_USAGE`.`CONSTRAINT_NAME` 
      AND `REFERENTIAL_CONSTRAINTS`.`CONSTRAINT_SCHEMA` = `KEY_COLUMN_USAGE`.`CONSTRAINT_SCHEMA`) AS `DELETE_RULE`
    FROM `INFORMATION_SCHEMA`.`KEY_COLUMN_USAGE`
    WHERE {0}
    AND `CONSTRAINT_NAME` <> 'PRIMARY'
    AND `REFERENCED_TABLE_NAME` IS NOT NULL
    GROUP BY `TABLE_SCHEMA`, `TABLE_NAME`, 
    `CONSTRAINT_SCHEMA`, `CONSTRAINT_NAME`,
    `TABLE_NAME`, `REFERENCED_TABLE_NAME`;";


        private IEnumerable<DatabaseTable> GetTables(DbConnection connection, Func<string, string, string> tableFilter)
        {
            using (var command = connection.CreateCommand())
            {
                var tables = new List<DatabaseTable>();
                string filter = tableFilter!("TABLE_SCHEMA", "TABLE_NAME");
                command.CommandText = string.Format(GetTablesQuery, filter);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var schema = reader.GetValueOrDefault<string>("TABLE_SCHEMA");
                        var name = reader.GetValueOrDefault<string>("TABLE_NAME");
                        var comment = reader.GetValueOrDefault<string>("TABLE_COMMENT");

                        var table = new DatabaseTable();

                        table.Schema = schema;
                        table.Name = name!;
                        table.Comment = string.IsNullOrEmpty(comment) ? null : comment;

                        tables.Add(table);
                    }
                }

                // This is done separately due to MARS property may be turned off
                GetColumns(connection, tables, filter);
                GetPrimaryKeys(connection, tables, filter);
                GetIndexes(connection, tables, filter);
                //GetConstraints(connection, tables, filter);

                return tables;
            }
        }

        private void GetColumns(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(GetColumnsQuery, tableFilter);
                using (var reader = command.ExecuteReader())
                {
                    var tableColumnGroups = reader.Cast<DbDataRecord>()
                      .GroupBy(
                        ddr => (tableSchema: ddr.GetValueOrDefault<string>("TABLE_SCHEMA"),
                        tableName: ddr.GetValueOrDefault<string>("TABLE_NAME")));

                    foreach (var tableColumnGroup in tableColumnGroups)
                    {
                        var tableSchema = tableColumnGroup.Key.tableSchema;
                        var tableName = tableColumnGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        foreach (var dataRecord in tableColumnGroup)
                        {
                            var name = dataRecord.GetValueOrDefault<string>("COLUMN_NAME");
                            var defaultValue = dataRecord.GetValueOrDefault<string>("COLUMN_DEFAULT");
                            var nullable = dataRecord.GetValueOrDefault<string>("IS_NULLABLE")!.Contains("YES");
                            var dataType = dataRecord.GetValueOrDefault<string>("DATA_TYPE");
                            
                            var extra = dataRecord.GetValueOrDefault<bool>("IS_SERIAL");
                            var comment = dataRecord.GetValueOrDefault<string>("COLUMN_COMMENT");
                            var timestamp_t = dataRecord.GetValueOrDefault<string>("TIMESTAMP_T");
                            ValueGenerated valueGenerated;

                            if (extra || timestamp_t == "i")
                            {
                                valueGenerated = ValueGenerated.OnAdd;
                            }

                            else if (timestamp_t == "u")
                            {
                                valueGenerated = ValueGenerated.OnUpdate;
                            }
                            else
                            {
                                valueGenerated = ValueGenerated.Never;
                            }
                          //  defaultValue = FilterClrDefaults(dataType!, nullable, defaultValue!);

                            var column = new DatabaseColumn
                            {
                                Table = table,
                                Name = name!,
                                StoreType = dataType,
                                IsNullable = nullable,
                                //DefaultValueSql = CreateDefaultValueString(defaultValue!, dataType!),
                                DefaultValueSql = defaultValue,
                                ValueGenerated = valueGenerated,
                                Comment = string.IsNullOrEmpty(comment) ? null : comment
                            };

                            table.Columns.Add(column);
                        }
                    }
                }
            }
        }

        private void GetPrimaryKeys(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(GetPrimaryQuery, tableFilter);
                using (var reader = command.ExecuteReader())
                {
                    var tablePrimaryKeyGroups = reader.Cast<DbDataRecord>()
                        .GroupBy(
                          ddr => (tableSchema: ddr.GetValueOrDefault<string>("TABLE_SCHEMA"),
                            tableName: ddr.GetValueOrDefault<string>("TABLE_NAME")));

                    foreach (var tablePrimaryKeyGroup in tablePrimaryKeyGroups)
                    {
                        var tableSchema = tablePrimaryKeyGroup.Key.tableSchema;
                        var tableName = tablePrimaryKeyGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        foreach (var dataRecord in tablePrimaryKeyGroup)
                        {
                            try
                            {
                                var index = new DatabasePrimaryKey
                                {
                                    Table = table,
                                    Name = dataRecord.GetValueOrDefault<string>("INDEX_NAME")
                                };

                                foreach (var column in dataRecord.GetValueOrDefault<string>("COLUMNS")!.Split(','))
                                {
                                    index.Columns.Add(table.Columns.Single(y => y.Name == column.Trim('\"')));
                                }

                                table.PrimaryKey = index;
                            }
                            catch (Exception ex)
                            {
                                _logger.Logger.LogError(ex, "Error assigning PK for {table}.", table.Name);
                            }
                        }
                    }
                }
            }
        }

        private void GetIndexes(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(GetIndexesQuery, tableFilter);
                using (var reader = command.ExecuteReader())
                {
                    var tableIndexGroups = reader.Cast<DbDataRecord>()
                      .GroupBy(
                        ddr => (tableSchema: ddr.GetValueOrDefault<string>("TABLE_SCHEMA"),
                        tableName: ddr.GetValueOrDefault<string>("TABLE_NAME")));

                    foreach (var tableIndexGroup in tableIndexGroups)
                    {
                        var tableSchema = tableIndexGroup.Key.tableSchema;
                        var tableName = tableIndexGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        foreach (var dataRecord in tableIndexGroup)
                        {
                            try
                            {
                                var index = new DatabaseIndex
                                {
                                    Table = table,
                                    Name = dataRecord.GetValueOrDefault<string>("INDEX_NAME"),
                                    IsUnique = dataRecord.GetValueOrDefault<bool>("NON_UNIQUE")
                                };

                                foreach (var column in dataRecord.GetValueOrDefault<string>("COLUMNS")!.Split(','))
                                {
                                    index.Columns.Add(table.Columns.Single(y => y.Name == column.Trim('\"')));
                                }

                                table.Indexes.Add(index);
                            }
                            catch (Exception ex)
                            {
                                _logger.Logger.LogError(ex, "Error assigning index for {table}.", table.Name);
                            }
                        }
                    }
                }
            }
        }

        private void GetConstraints(DbConnection connection, IReadOnlyList<DatabaseTable> tables, string tableFilter)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = string.Format(GetConstraintsQuery, tableFilter);

                using (var reader = command.ExecuteReader())
                {
                    var tableConstraintGroups = reader.Cast<DbDataRecord>()
                      .GroupBy(
                        ddr => (tableSchema: ddr.GetValueOrDefault<string>("TABLE_SCHEMA"),
                        tableName: ddr.GetValueOrDefault<string>("TABLE_NAME")));

                    foreach (var tableConstraintGroup in tableConstraintGroups)
                    {
                        var tableSchema = tableConstraintGroup.Key.tableSchema;
                        var tableName = tableConstraintGroup.Key.tableName;

                        var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

                        foreach (var dataRecord in tableConstraintGroup)
                        {
                            var referencedTableName = dataRecord.GetValueOrDefault<string>("REFERENCED_TABLE_NAME");
                            var referencedTable = tables.FirstOrDefault(t => t.Name == referencedTableName);
                            if (referencedTable != null)
                            {
                                var fkInfo = new DatabaseForeignKey
                                {
                                    Name = dataRecord.GetValueOrDefault<string>("CONSTRAINT_NAME"),
                                    OnDelete = ConvertToReferentialAction(dataRecord.GetValueOrDefault<string>("DELETE_RULE")!),
                                    Table = table,
                                    PrincipalTable = referencedTable
                                };
                                foreach (var pair in dataRecord.GetValueOrDefault<string>("PAIRED_COLUMNS")!.Split(','))
                                {
                                    fkInfo.Columns.Add(table.Columns.Single(y =>
                                      string.Equals(y.Name, pair.Split('|')[0], StringComparison.OrdinalIgnoreCase)));
                                    fkInfo.PrincipalColumns.Add(fkInfo.PrincipalTable.Columns.Single(y =>
                                      string.Equals(y.Name, pair.Split('|')[1], StringComparison.OrdinalIgnoreCase)));
                                }

                                table.ForeignKeys.Add(fkInfo);
                            }
                            else
                            {
                                _logger.Logger.LogWarning($"Referenced table `{referencedTableName}` is not in dictionary.");
                            }
                        }
                    }
                }
            }
        }

        /*private static string? FilterClrDefaults(string dataTypeName, bool nullable, string defaultValue)
        {
            if (defaultValue == null)
            {
                return null;
            }

            if (nullable)
            {
                return defaultValue;
            }

            if (defaultValue == "0")
            {
                if (dataTypeName == "bit"
                  || dataTypeName == "tinyint"
                  || dataTypeName == "smallint"
                  || dataTypeName == "int"
                  || dataTypeName == "bigint"
                  || dataTypeName == "decimal"
                  || dataTypeName == "double"
                  || dataTypeName == "float")
                {
                    return null;
                }
            }
            else if (Regex.IsMatch(defaultValue, @"^0\.0+$"))
            {
                if (dataTypeName == "decimal"
                  || dataTypeName == "double"
                  || dataTypeName == "float")
                {
                    return null;
                }
            }

            return defaultValue;
        }*/

       /* private string? CreateDefaultValueString(string defaultValue, string dataType)
        {
            if (defaultValue == null)
            {
                return null;
            }

            if ((string.Equals(dataType, "timestamp", StringComparison.OrdinalIgnoreCase) ||
              string.Equals(dataType, "datetime", StringComparison.OrdinalIgnoreCase)) &&
              string.Equals(defaultValue, "CURRENT_TIMESTAMP", StringComparison.OrdinalIgnoreCase))
            {
                return defaultValue;
            }

            // Handle bit values.
#pragma warning disable CA1310 // 为了确保正确，请指定 StringComparison
            if (string.Equals(dataType, "bit", StringComparison.OrdinalIgnoreCase)
              && defaultValue.StartsWith("b'"))
            {
                return defaultValue;
            }
#pragma warning restore CA1310 // 为了确保正确，请指定 StringComparison

            return "'" + defaultValue.Replace(@"\", @"\\").Replace("'", "''") + "'";
        }*/
    }
}
