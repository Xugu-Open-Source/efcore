// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using XuguClient;

namespace EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGConnectionSettings
    {
        public string ConnectionString { get; set; }
        public XGConnectionSettings()
        {
        }

        public XGConnectionSettings(DbConnection connection)
            : this(connection.ConnectionString)
        {
        }

        public XGConnectionSettings(string connectionString)
        {
            var csb = new XGConnectionStringBuilder();
            csb.ConnectionString = connectionString;
            ConnectionString = csb.ConnectionString;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj) || ((XGConnectionSettings)obj).ConnectionString==null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((XGConnectionSettings)obj);
        }

        public override int GetHashCode()
            => HashCode.Combine(ConnectionString);
    }
}
