using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.XuGu.Diagnostics.Internal;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGDatabaseCleaner : RelationalDatabaseCleaner
    {
        private readonly IXGOptions _options;
        private readonly IRelationalTypeMappingSource _relationalTypeMappingSource;

        public XGDatabaseCleaner(IXGOptions options, IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            _options = options;
            _relationalTypeMappingSource = relationalTypeMappingSource;
        }

        public override void Clean(DatabaseFacade facade)
        {
            var creator = facade.GetService<IRelationalDatabaseCreator>();
            var connection = facade.GetService<IRelationalConnection>();

            if (creator.Exists())
            {
                OpenConnection(connection);

                try
                {
                    var commands = new StringBuilder();

                    var getRoutinesSql = $@"SELECT 
	SCHEMA_NAME AS `ROUTINE_SCHEMA`,PROC_NAME AS `ROUTINE_NAME`,CASE WHEN DEFINE LIKE '%CREATE PROCEDURE%' THEN 'PROCEDURE' ELSE 'FUNCTION' END  AS `ROUTINE_TYPE`
FROM ALL_PROCEDURES AS p
JOIN ALL_SCHEMAS AS s 
ON p.SCHEMA_ID=s.SCHEMA_ID;";

                    using var command = connection.DbConnection.CreateCommand();
                    command.CommandText = getRoutinesSql;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (string.Equals(reader["ROUTINE_TYPE"] as string, "PROCEDURE", StringComparison.OrdinalIgnoreCase))
                            {
                                commands.AppendLine($"DROP PROCEDURE IF EXISTS `{reader["ROUTINE_SCHEMA"]}`.`{reader["ROUTINE_NAME"]}`;");
                            }
                            else if (string.Equals(reader["ROUTINE_TYPE"] as string, "FUNCTION", StringComparison.OrdinalIgnoreCase))
                            {
                                commands.AppendLine($"DROP FUNCTION IF EXISTS `{reader["ROUTINE_SCHEMA"]}`.`{reader["ROUTINE_NAME"]}`;");
                            }
                        }
                    }

                    if (commands.Length > 0)
                    {
                        command.CommandText = commands.ToString();
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            base.Clean(facade);
        }

        protected override IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
            => new XGDatabaseModelFactory(
                new DiagnosticsLogger<DbLoggerCategory.Scaffolding>(
                    loggerFactory,
                    new LoggingOptions(),
                    new DiagnosticListener("Fake"),
                    new XGLoggingDefinitions(),
                    new NullDbContextLogger()),
                _relationalTypeMappingSource,
                _options);

        protected override bool AcceptIndex(DatabaseIndex index) => false;
        protected override bool AcceptTable(DatabaseTable table) => !(table is DatabaseView);

        protected override string BuildCustomSql(DatabaseModel databaseModel)
            => @"DECLARE
CURSOR fk_cursor IS
(SELECT `VIEW_NAME` FROM `ALL_VIEWS`);
sqlstr VARCHAR;
BEGIN
FOR fk IN fk_cursor LOOP
sqlstr:='DROP VIEW IF EXISTS ""' || `fk`.`VIEW_NAME` || '"" CASCADE;';
EXECUTE IMMEDIATE sqlstr;
END LOOP;
END;";
    }
}
