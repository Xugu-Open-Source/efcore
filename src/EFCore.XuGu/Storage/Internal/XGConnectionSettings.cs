// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Data.Common;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGConnectionSettings
    {
        public XGConnectionSettings()
        {
        }

        public XGConnectionSettings(DbConnection connection)
            : this(connection.ConnectionString)
        {
        }

        public XGConnectionSettings(string connectionString)
        {
            var csb = new XGConnectionStringBuilder(connectionString);
        }

        public readonly bool OldGuids;
        public readonly bool TreatTinyAsBoolean;
    }
}
