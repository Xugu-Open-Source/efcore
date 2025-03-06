// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class XuguConnection : RelationalConnection, IXuguConnection
    {
        // Compensate for slow SQL Server database creation
        private const int DefaultMasterConnectionCommandTimeout = 60;

        private static readonly ConcurrentDictionary<string, bool> _multipleActiveResultSetsEnabledMap = new();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguConnection(RelationalConnectionDependencies dependencies)
            : base(dependencies)
        {
        }

        /*        /// <summary>
                ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
                ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
                ///     any release. You should only use it directly in your code with extreme caution and knowing that
                ///     doing so can result in application failures when updating to a new Entity Framework Core release.
                /// </summary>
                protected override void OpenDbConnection(bool errorsExpected)
                {
                    // Note: Not needed for the Async overload: see https://github.com/dotnet/SqlClient/issues/615
                    if (errorsExpected
                        && DbConnection is XGConnection sqlConnection)
                    {
                        sqlConnection.Open(SqlConnectionOverrides.OpenWithoutRetry);
                    }
                    else
                    {
                        DbConnection.Open();
                    }
                }*/

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override DbConnection CreateDbConnection()
            => new XGConnection(ConnectionString);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IXuguConnection CreateMasterConnection()
        {
            var connectionStringBuilder = new XGConnectionStringBuilder(ConnectionString);
            connectionStringBuilder.Add("DB", "SYSTEM");

            var contextOptions = new DbContextOptionsBuilder()
                .UseXugu(
                    connectionStringBuilder.ConnectionString,
                    b => b.CommandTimeout(CommandTimeout ?? DefaultMasterConnectionCommandTimeout))
                .Options;

            return new XuguConnection(Dependencies with { ContextOptions = contextOptions });
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool IsMultipleActiveResultSetsEnabled
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Indicates whether the store connection supports ambient transactions
        /// </summary>
        protected override bool SupportsAmbientTransactions
            => true;
    }
}
