// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections;
using System.Linq;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Properties;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGTypeMappingSource : RelationalTypeMappingSource
    {
        private readonly XGStringTypeMapping _unboundedUnicodeString
            = new XGStringTypeMapping(
                "CLOB",
                dbType: null,
                unicode: true);

        private readonly XGByteArrayTypeMapping _rowversion
            = new XGByteArrayTypeMapping(
                "RAW(8)",
                dbType: DbType.Binary,
                size: 8,
                comparer: new ValueComparer<byte[]>(
                    (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                    v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                    v => v == null ? null : v.ToArray()));

        private readonly IntTypeMapping _int
            = new IntTypeMapping("INTEGER", DbType.Int32);

        private readonly LongTypeMapping _long
            = new LongTypeMapping("BIGINT", DbType.Int64);


        private readonly ShortTypeMapping _short
            = new ShortTypeMapping("SMALLINT", DbType.Int16);

        private readonly ByteTypeMapping _byte
            = new ByteTypeMapping("TINYINT", DbType.Byte);

        private readonly XGStringTypeMapping _fixedLengthUnicodeString
            = new XGStringTypeMapping("NCHAR", dbType: DbType.String, unicode: true, fixedLength: true);

        private readonly XGStringTypeMapping _variableLengthUnicodeString
            = new XGStringTypeMapping("VARCHAR", dbType: null, unicode: true);

        private readonly XGStringTypeMapping _fixedLengthAnsiString
            = new XGStringTypeMapping("CHAR", dbType: DbType.AnsiString, fixedLength: true);

        private readonly XGStringTypeMapping _variableLengthAnsiString
            = new XGStringTypeMapping("VARCHAR", dbType: DbType.AnsiString);

        private readonly XGByteArrayTypeMapping _variableLengthBinary
            = new XGByteArrayTypeMapping("BLOB");

        private readonly XGByteArrayTypeMapping _fixedLengthBinary
            = new XGByteArrayTypeMapping("RAW");

        private readonly XGDateTimeTypeMapping _date
            = new XGDateTimeTypeMapping("DATE");

        private readonly XGDateTimeTypeMapping _datetime
            = new XGDateTimeTypeMapping("DATETIME");

        private readonly XGDateTimeTypeMapping _timeStamp = new XGDateTimeTypeMapping("TIMESTAMP");

        private readonly DoubleTypeMapping _double
            = new XGDoubleTypeMapping("FLOAT(49)");

        private readonly XGDateTimeOffsetTypeMapping _datetimeoffset
            = new XGDateTimeOffsetTypeMapping("TIMESTAMP WITH TIME ZONE");

        private readonly XGDateTimeOffsetTypeMapping _datetimeoffset3
            = new XGDateTimeOffsetTypeMapping("TIMESTAMP(3) WITH TIME ZONE");

        private readonly FloatTypeMapping _real
            = new XGFloatTypeMapping("REAL");

        private readonly DecimalTypeMapping _decimal
            = new XGDecimalTypeMapping("DECIMAL(29,4)", precision: 29, scale: 4, storeTypePostfix: StoreTypePostfix.PrecisionAndScale);

        private readonly TimeSpanTypeMapping _time
            = new XGTimeSpanTypeMapping("INTERVAL DAY TO SECOND");

        private readonly XGStringTypeMapping _xml
            = new XGStringTypeMapping("XML", dbType: null, unicode: true);

        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;

        private readonly GuidTypeMapping _uniqueidentifier = new GuidTypeMapping("guid", DbType.Guid);

        // These are disallowed only if specified without any kind of length specified in parenthesis.
        private readonly HashSet<string> _disallowedMappings
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "binary varying",
                //"binary",
                "char varying",
                //"char",
                "character varying",
                "character",
                "national char varying",
                "national character varying",
                "national character",
                "nchar",
                "nvarchar2",
                "varchar2"
            };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XGTypeMappingSource(
            [NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _long },
                    { typeof(DateTime), _datetime },
                    { typeof(byte), _byte },
                    { typeof(double), _double },
                    { typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(short), _short },
                    { typeof(float), _real },
                    { typeof(decimal), _decimal },
                    { typeof(TimeSpan), _time }
                };

            _clrTypeMappings[typeof(Guid)] = _uniqueidentifier;

            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "bigint", _long },
                    { "blob", _variableLengthBinary },
                    { "raw", _fixedLengthBinary },
                    { "char", _fixedLengthAnsiString },
                    { "date", _date },
                    { "datetime", _datetime },
                    { "timestamp", _timeStamp },
                    { "timestamp(3) with time zone", _datetimeoffset3 },
                    { "timestamp with time zone", _datetimeoffset },
                    { "decimal(29,4)", _decimal },
                    { "float(49)", _double },
                    { "integer", _int },
                    { "nchar", _fixedLengthUnicodeString },
                    { "nvarchar2", _variableLengthUnicodeString },
                    { "short", _short },
                    { "interval", _time },
                    { "tinyint", _byte },
                    { "varchar2", _variableLengthAnsiString },
                    { "clob", _unboundedUnicodeString },
                    { "xml", _xml },
                    { "number", _int }
                };
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var mapping = FindRawMapping(mappingInfo)?.Clone(mappingInfo);

            if (_disallowedMappings.Contains(mapping?.StoreType))
            {
                throw new ArgumentException(XGStrings.UnqualifiedDataType(mapping.StoreType));
            }

            return mapping;
        }

        private RelationalTypeMapping FindRawMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                if (storeTypeName.Equals("guid", StringComparison.OrdinalIgnoreCase)
                        && clrType == typeof(Guid))
                {
                    return _uniqueidentifier;
                }

                if (clrType == typeof(float)
                    && mappingInfo.Size != null
                    && mappingInfo.Size <= 24
                    && (storeTypeNameBase.Equals("float", StringComparison.OrdinalIgnoreCase)
                        || storeTypeNameBase.Equals("double precision", StringComparison.OrdinalIgnoreCase)))
                {
                    return _real;
                }

                if (storeTypeNameBase.Equals("datetime", StringComparison.OrdinalIgnoreCase))
                {
                    if (clrType == null
                        || clrType == typeof(DateTime))
                    {
                        return _datetime;
                    }
                    if (clrType == typeof(DateTimeOffset))
                    {
                        return _datetimeoffset;
                    }
                }
                else if (storeTypeNameBase.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
                {
                    if (clrType == null
                        || clrType == typeof(DateTime))
                    {
                        return _timeStamp;
                    }
                    if (clrType == typeof(DateTimeOffset))
                    {
                        return _datetimeoffset;
                    }
                }

                if (_storeTypeMappings.TryGetValue(storeTypeName, out var mapping)
                    || _storeTypeMappings.TryGetValue(storeTypeNameBase, out mapping))
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
                    var baseName = isFixedLength ? (isAnsi ? "" : "N") + "CHAR" : "VARCHAR";//(isAnsi ? "" : "N") + (isFixedLength ? "CHAR" : "VARCHAR");
                    var unboundedName = isAnsi ? "CLOB" : "NCLOB";
                    var maxSize = 4000; //isAnsi ? 4000 : 2000;
                    var storeTypePostfix = (StoreTypePostfix?)null;

                    var size = (int?)(mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (isAnsi ? 900 : 450) : maxSize));
                    if (size > maxSize)
                    {
                        size = null;
                        storeTypePostfix = StoreTypePostfix.None;
                    }

                    return new XGStringTypeMapping(
                        storeTypePostfix == StoreTypePostfix.None ? unboundedName : baseName + "(" + size + ")",
                        isAnsi ? DbType.AnsiString : (DbType?)null,
                        !isAnsi,
                        size,
                        isFixedLength,
                        storeTypePostfix);
                }

                if (clrType == typeof(byte[]))
                {
                    if (mappingInfo.IsRowVersion == true)
                    {
                        return _rowversion;
                    }

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)900 : null);
                    var storeTypePostfix = (StoreTypePostfix?)null;
                    if (size > 2000)
                    {
                        size = null;
                        storeTypePostfix = StoreTypePostfix.None;
                    }

                    return new XGByteArrayTypeMapping(
                        (size == -1 || size == null) ? "BLOB" : "RAW(" + size + ")",
                        DbType.Binary,
                        size,
                        storeTypePostfix: storeTypePostfix);
                }
            }

            return null;
        }
    }
}
