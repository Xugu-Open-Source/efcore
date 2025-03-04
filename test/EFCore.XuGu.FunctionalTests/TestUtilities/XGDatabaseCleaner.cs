using EntityFrameworkCore.XuGu.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
using EntityFrameworkCore.XuGu.Diagnostics.Internal;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;

namespace EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
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
	v_sql VARCHAR;
BEGIN
	FOR v IN (SELECT VIEW_NAME FROM ALL_VIEWS) LOOP
		v_sql:='DROP VIEW IF EXISTS `' || v.VIEW_NAME ||'`';
		EXECUTE IMMEDIATE v_sql;
	END LOOP;
END;";
    }
}
