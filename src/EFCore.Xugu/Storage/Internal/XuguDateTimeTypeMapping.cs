// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.Xugu.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class XuguDateTimeTypeMapping : DateTimeTypeMapping
    {
        private const string DateFormatConst = "'{0:yyyy-MM-dd}'";
        private const string DateTimeFormatConst = "'{0:yyyy-MM-ddTHH:mm:ss.fff}'";

        // Note: this array will be accessed using the precision as an index
        // so the order of the entries in this array is important
        private readonly string[] _dateTime2Formats =
        {
      "'{0:yyyy-MM-dd HH:mm:ss}'",
      "'{0:yyyy-MM-dd HH:mm:ss.f}'",
      "'{0:yyyy-MM-dd HH:mm:ss.ff}'",
      "'{0:yyyy-MM-dd HH:mm:ss.fff}'",
      "'{0:yyyy-MM-dd HH:mm:ss.ffff}'",
      "'{0:yyyy-MM-dd HH:mm:ss.fffff}'",
      "'{0:yyyy-MM-dd HH:mm:ss.ffffff}'"

        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguDateTimeTypeMapping(
            string storeType,
            DbType? dbType = System.Data.DbType.DateTime2,
            StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(DateTime)),
                    storeType,
                    storeTypePostfix,
                    dbType))
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected XuguDateTimeTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            // Workaround for a SQLClient bug
            if (DbType == System.Data.DbType.Date)
            {
                ((XGParameters)parameter).DbType = (DbType)SqlDbType.Date;
            }

            if (Size.HasValue
                && Size.Value != -1)
            {
                parameter.Size = Size.Value;
            }

            if (Precision.HasValue)
            {
                parameter.Precision = unchecked((byte)Precision.Value);
            }
        }

        /// <summary>
        ///     Creates a copy of this mapping.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        /// <returns>The newly created mapping.</returns>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XuguDateTimeTypeMapping(parameters);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string SqlLiteralFormatString
        {
            get
            {
                switch (StoreType)
                {
                    case "date":
                        return DateFormatConst;
                    case "datetime":
                        return DateTimeFormatConst;
                    case "smalldatetime":
                        return $"({DateTimeFormatConst})";
                    default:
                        if (Precision.HasValue)
                        {
                            var precision = Precision.Value;
                            if (precision <= 6
                                && precision >= 0)
                            {
                                return _dateTime2Formats[precision];
                            }
                        }

                        return _dateTime2Formats[6];
                }
            }
        }
    }
}
