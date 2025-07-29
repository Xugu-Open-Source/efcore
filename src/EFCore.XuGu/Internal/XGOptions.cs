// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;

namespace Microsoft.EntityFrameworkCore.XuGu.Internal
{
    public class XGOptions : IXGOptions
    {
        public XGOptions()
        {
            ConnectionSettings = new XGConnectionSettings();
            ServerVersion = new ServerVersion(null);
            CharSetBehavior = CharSetBehavior.AppendToAllAnsiColumns;
            AnsiCharSetInfo = new CharSetInfo(CharSet.Latin1);
            UnicodeCharSetInfo = new CharSetInfo(CharSet.Utf8mb4);
        }

        public virtual void Initialize(IDbContextOptions options)
        {
            var xgOptions = options.FindExtension<XGOptionsExtension>() ?? new XGOptionsExtension();

            ConnectionSettings = GetConnectionSettings(xgOptions);
            ServerVersion = xgOptions.ServerVersion ?? ServerVersion;
            CharSetBehavior = xgOptions.NullableCharSetBehavior ?? CharSetBehavior;
            AnsiCharSetInfo = xgOptions.AnsiCharSetInfo ?? AnsiCharSetInfo;
            UnicodeCharSetInfo = xgOptions.UnicodeCharSetInfo ?? UnicodeCharSetInfo;
            NoBackslashEscapes = xgOptions.NoBackslashEscapes;
        }

        public virtual void Validate(IDbContextOptions options)
        {
            var xgOptions = options.FindExtension<XGOptionsExtension>() ?? new XGOptionsExtension();

            if (!Equals(ServerVersion, xgOptions.ServerVersion ?? new ServerVersion(null)))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsBuilder.ServerVersion),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            var connectionSettings = GetConnectionSettings(xgOptions);

            if (!Equals(ConnectionSettings.OldGuids, connectionSettings.OldGuids)
                || !Equals(ConnectionSettings.TreatTinyAsBoolean, connectionSettings.TreatTinyAsBoolean))
            {
                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(XGDbContextOptionsExtensions.UseXG),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }

        private static XGConnectionSettings GetConnectionSettings(XGOptionsExtension relationalOptions)
        {
            if (relationalOptions.Connection != null)
            {
                return new XGConnectionSettings(relationalOptions.Connection);
            }

            if (relationalOptions.ConnectionString != null)
            {
                return new XGConnectionSettings(relationalOptions.ConnectionString);
            }

            throw new InvalidOperationException(RelationalStrings.NoConnectionOrConnectionString);
        }

        public virtual XGConnectionSettings ConnectionSettings { get; private set; }
        public virtual ServerVersion ServerVersion { get; private set; }
        public virtual CharSetBehavior CharSetBehavior { get; private set; }
        public virtual CharSetInfo AnsiCharSetInfo { get; private set; }
        public virtual CharSetInfo UnicodeCharSetInfo { get; private set; }
        public virtual bool NoBackslashEscapes { get; private set; }
    }
}
