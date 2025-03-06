// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class XuGuDbConnection : RelationalConnection, IXuGuDbConnection
    {
        private bool? _multipleActiveResultSetsEnabled;

        internal const int DefaultSystemConnectionCommandTimeout = 60;

        public XuGuDbConnection(
            [NotNull] IDbContextOptions options,
            [NotNull] ILogger<XuGuDbConnection> logger)
            : base(options, logger)
        {
        }

        private XuGuDbConnection(
            [NotNull] IDbContextOptions options, [NotNull] ILogger logger)
            : base(options, logger)
        {
        }

        
        protected override DbConnection CreateDbConnection() => new XGConnection(ConnectionString);

        public virtual IXuGuDbConnection CreateSystemConnection()
            => new XuGuDbConnection(new DbContextOptionsBuilder()
                .UseXuGuDb(
                    new XGConnectionStringBuilder { ConnectionString = "IP=127.0.0.1;DB=SYSTEM;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=on;CHAR_SET=UTF8" }.ConnectionString,
                    b => b.CommandTimeout(CommandTimeout ?? DefaultSystemConnectionCommandTimeout)).Options, Logger);

        //public override bool IsMultipleActiveResultSetsEnabled
        //    => (bool)(_multipleActiveResultSetsEnabled
        //              ?? (_multipleActiveResultSetsEnabled
        //                  = new SqlConnectionStringBuilder(ConnectionString).MultipleActiveResultSets));
    }
}
