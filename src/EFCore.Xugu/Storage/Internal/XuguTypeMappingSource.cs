// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

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
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class XuguTypeMappingSource : RelationalTypeMappingSource
    {

        private readonly FloatTypeMapping _real
            = new XuguFloatTypeMapping("real");

        private readonly ByteTypeMapping _byte
            = new XuguByteTypeMapping("tinyint", DbType.Byte);

        private readonly ShortTypeMapping _short
            = new XuguShortTypeMapping("smallint", DbType.Int16);

        private readonly LongTypeMapping _long
            = new XuguLongTypeMapping("bigint", DbType.Int64);

        private readonly IntTypeMapping _int
            = new IntTypeMapping("int", DbType.Int32);

        private readonly GuidTypeMapping _guid
    = new GuidTypeMapping("guid", DbType.Guid);


        private readonly BoolTypeMapping _bool
            = new XuguBoolTypeMapping("boolean", DbType.Boolean);

        private readonly XuguStringTypeMapping _variableLengthMaxUnicodeString
            = new XuguStringTypeMapping("varchar", unicode: true, storeTypePostfix: StoreTypePostfix.None);

        private readonly XuguStringTypeMapping _fixedLengthAnsiString
            = new XuguStringTypeMapping(fixedLength: true);

        private readonly XuguStringTypeMapping _variableLengthAnsiString
            = new XuguStringTypeMapping();

        private readonly XuguStringTypeMapping _variableLengthMaxAnsiString
            = new XuguStringTypeMapping("varchar", storeTypePostfix: StoreTypePostfix.None);


        private readonly XuguByteArrayTypeMapping _variableLengthMaxBinary
            = new XuguByteArrayTypeMapping("binary", storeTypePostfix: StoreTypePostfix.None);

        private readonly XuguByteArrayTypeMapping _fixedLengthBinary
            = new XuguByteArrayTypeMapping("binary", fixedLength: true);

        private readonly XuguDateTimeTypeMapping _date
            = new XuguDateTimeTypeMapping("date", DbType.Date);

        private readonly XuguDateTimeTypeMapping _datetime
            = new XuguDateTimeTypeMapping("datetime", DbType.DateTime);

        private readonly DoubleTypeMapping _double
            = new XuguDoubleTypeMapping("float");

        private readonly XuguDateTimeOffsetTypeMapping _datetimeoffset
            = new XuguDateTimeOffsetTypeMapping("datetimeoffset");

        private readonly GuidTypeMapping _uniqueidentifier
            = new GuidTypeMapping("uniqueidentifier", DbType.Guid);

        private readonly DecimalTypeMapping _decimal
            = new XuguDecimalTypeMapping(
                "decimal(18, 2)", precision: 18, scale: 2, storeTypePostfix: StoreTypePostfix.PrecisionAndScale);


        private readonly TimeSpanTypeMapping _time
            = new XuguTimeSpanTypeMapping("time");

        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;

        private readonly HashSet<string> _disallowedMappings
    = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
                "binary",
                "binary varying",
                "binary",
                "char",
                "character",
                "char varying",
                "character varying",
                "varchar",
                "national char",
                "national character",
                "nchar",
                "national char varying",
                "national character varying",
                "varchar"
    };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                   { typeof(short), _short },
                    { typeof(ushort), _short },
                    { typeof(bool), _bool},
                    { typeof(int), _int },
                    { typeof(uint), _int },
                    { typeof(long), _long },
                    { typeof(ulong), _long },

                    // byte / char
                    { typeof(byte), _byte },

                    // DateTime
                    { typeof(DateTime), _datetime },
                    { typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(TimeSpan), _time },


                      // decimals
                      { typeof(float), _real },
                      { typeof(double), _double },
                      { typeof(decimal), _decimal },
                };
            //  cyj 数据库生成C#代码  {"数据库类型名称"， .net 对应类型对象}
            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "bigint", _long },
                    { "bigint unsigned", _long  },
                    { "int", _int },
                    { "int unsigned", _int },
                    { "integer", _int },
                    { "integer unsigned", _int },
                    { "smallint", _short},
                    { "smallint unsigned", _short},
                    { "tinyint", _byte },
                    { "tinyint unsigned", _byte },
                    { "boolean",_bool},
                    { "guid",_guid},
                     { "decimal", _decimal },
                     { "numeric", _decimal },
                     { "dec", _decimal },
                     { "double", _double },
                     { "float", _real },
                     { "real", _double },

                     { "binary", _fixedLengthBinary },
                     { "tinyblob", _fixedLengthBinary },
                     { "blob", _fixedLengthBinary },
                     { "mediumblob", _fixedLengthBinary },
                     { "longblob", _fixedLengthBinary },

                     { "char", _fixedLengthAnsiString },
                     { "varchar", _variableLengthAnsiString },
                     { "clob", _variableLengthAnsiString },

                      { "year", _int },
                      { "date", _date },
                      { "time", _time },
                      { "timestamp", _datetime },
                      { "datetime", _datetime },


                };
        }




        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => base.FindMapping(mappingInfo) ?? FindRawMapping(mappingInfo)?.Clone(mappingInfo);

        private RelationalTypeMapping? FindRawMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                if (clrType == typeof(float)
                    && mappingInfo.Size != null
                    && mappingInfo.Size <= 24
                    && (storeTypeNameBase!.Equals("float", StringComparison.OrdinalIgnoreCase)
                        || storeTypeNameBase.Equals("double precision", StringComparison.OrdinalIgnoreCase)))
                {
                    return _real;
                }

                if (_storeTypeMappings.TryGetValue(storeTypeName, out var mapping)
                    || _storeTypeMappings.TryGetValue(storeTypeNameBase!, out mapping))
                {
                    return clrType == null
                        || mapping.ClrType == clrType
                            ? mapping
                            : null;
                }
            }

            if (clrType != null)
            {
                if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
                {
                    return mapping;
                }

                if (clrType == typeof(string))
                {
                    var isAnsi = mappingInfo.IsUnicode == false;
                    var isFixedLength = mappingInfo.IsFixedLength == true;
                    var maxSize = isAnsi ? 8000 : 4000;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)(isAnsi ? 900 : 450) : null);
                    if (size > maxSize)
                    {
                        size = isFixedLength ? maxSize : (int?)null;
                    }

                    return size == null
                        ? isAnsi ? _variableLengthMaxAnsiString : _variableLengthMaxUnicodeString
                        : new XuguStringTypeMapping(
                            unicode: !isAnsi,
                            size: size,
                            fixedLength: isFixedLength);
                }

                if (clrType == typeof(byte[]))
                {
                    var isFixedLength = mappingInfo.IsFixedLength == true;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)900 : null);
                    if (size > 8000)
                    {
                        size = isFixedLength ? 8000 : (int?)null;
                    }

                    return size == null
                        ? _variableLengthMaxBinary
                        : new XuguByteArrayTypeMapping(size: size, fixedLength: isFixedLength);
                }
            }

            return null;
        }

    }
}
