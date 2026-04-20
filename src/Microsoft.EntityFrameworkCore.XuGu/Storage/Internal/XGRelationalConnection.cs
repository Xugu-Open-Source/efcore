// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using XuguClient;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class XGRelationalConnection : RelationalConnection
    {
        public XGRelationalConnection(
            [NotNull] IDbContextOptions options,
            // ReSharper disable once SuggestBaseTypeForParameter
            [NotNull] ILogger<XGConnection> logger)
            : base(options, logger)
        {
        }

        private XGRelationalConnection(
            [NotNull] IDbContextOptions options, [NotNull] ILogger logger)
            : base(options, logger)
        {
        }

        // TODO: Consider using DbProviderFactory to create connection instance
        // Issue #774
        protected override DbConnection CreateDbConnection() => new XuguClient.XGConnection(ConnectionString);

        public XGRelationalConnection CreateMasterConnection()
        {
            var csb = new XGConnectionStringBuilder() {
                Database = "xg"
            };
            
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseXG(csb.ConnectionString);
            return new XGRelationalConnection(optionsBuilder.Options, Logger);
        }
    }
}
