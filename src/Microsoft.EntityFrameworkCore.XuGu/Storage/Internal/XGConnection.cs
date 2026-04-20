using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class XGConnection : RelationalConnection, IXGConnection
    {
        //private bool? _multipleActiveResultSetsEnabled;

        internal const int DefaultSystemConnectionCommandTimeout = 60;

        public XGConnection(
            [NotNull] IDbContextOptions options,
            [NotNull] ILogger<XGConnection> logger)
            : base(options, logger)
        {
        }

        private XGConnection(
            [NotNull] IDbContextOptions options, [NotNull] ILogger logger)
            : base(options, logger)
        {
        }


        protected override DbConnection CreateDbConnection() => new XuguClient.XGConnection(ConnectionString);

        public virtual IXGConnection CreateSystemConnection()
            => new XGConnection(new DbContextOptionsBuilder()
                .UseXG(
                    new XGConnectionStringBuilder { ConnectionString = "IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=UTF8" }.ConnectionString,
                    b => b.CommandTimeout(CommandTimeout ?? DefaultSystemConnectionCommandTimeout)).Options, Logger);

        //public override bool IsMultipleActiveResultSetsEnabled
        //    => (bool)(_multipleActiveResultSetsEnabled
        //              ?? (_multipleActiveResultSetsEnabled
        //                  = new SqlConnectionStringBuilder(ConnectionString).MultipleActiveResultSets));
    }
}
