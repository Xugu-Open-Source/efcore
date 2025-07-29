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

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    //public class XGTypeMappingSource1 : RelationalTypeMappingSource
    //{
    //    // boolean
    //    private readonly XGBoolTypeMapping _bit1 = new XGBoolTypeMapping("bit");
    //    private readonly XGBoolTypeMapping _tinyint1 = new XGBoolTypeMapping("tinyint");

    //    // bit
    //    private readonly ULongTypeMapping _bit = new ULongTypeMapping("bit", DbType.Int64);

    //    // integers
    //    private readonly SByteTypeMapping _tinyint = new SByteTypeMapping("tinyint", DbType.SByte);
    //    //private readonly ByteTypeMapping _utinyint = new ByteTypeMapping("tinyint", DbType.Int16);
    //    private readonly ShortTypeMapping _smallint = new ShortTypeMapping("smallint", DbType.Int16);
    //    //private readonly UShortTypeMapping _usmallint = new UShortTypeMapping("smallint", DbType.Int32);
    //    private readonly IntTypeMapping _int = new IntTypeMapping("int", DbType.Int32);
    //    //private readonly UIntTypeMapping _uint = new UIntTypeMapping("int", DbType.Int64);
    //    private readonly LongTypeMapping _bigint = new LongTypeMapping("bigint", DbType.Int64);
    //    //private readonly ULongTypeMapping _ubigint = new ULongTypeMapping("bigint", DbType.Int64);

    //    private readonly BoolTypeMapping _boolean = new BoolTypeMapping("boolean", DbType.Boolean);

    //    // decimals
    //    private readonly XGDecimalTypeMapping _decimal = new XGDecimalTypeMapping("decimal", precision: 38, scale: 17);
    //    private readonly XGDoubleTypeMapping _double = new XGDoubleTypeMapping("double");
    //    private readonly XGFloatTypeMapping _float = new XGFloatTypeMapping("float");

    //    // binary
    //    private readonly RelationalTypeMapping _binary = new XGByteArrayTypeMapping(fixedLength: true);
    //    private readonly RelationalTypeMapping _varbinary = new XGByteArrayTypeMapping();

    //    //
    //    // String mappings depend on the XGOptions.NoBackslashEscapes setting:
    //    //

    //    private XGStringTypeMapping _charUnicode;
    //    private XGStringTypeMapping _varcharUnicode;
    //    private XGStringTypeMapping _tinytextUnicode;
    //    private XGStringTypeMapping _textUnicode;
    //    private XGStringTypeMapping _mediumtextUnicode;
    //    private XGStringTypeMapping _longtextUnicode;

    //    private XGStringTypeMapping _nchar;
    //    private XGStringTypeMapping _nvarchar;

    //    private XGStringTypeMapping _enum;

    //    private XGStringTypeMapping _varcharMax;

    //    // DateTime
    //    private readonly XGDateTypeMapping _dateDateTime = new XGDateTypeMapping("date");
    //    private readonly XGDateTypeMapping _date = new XGDateTypeMapping("date");
    //    private readonly XGTimeSpanTypeMapping _time = new XGTimeSpanTypeMapping("time");
    //    private readonly XGDateTimeTypeMapping _dateTime = new XGDateTimeTypeMapping("datetime");
    //    private readonly XGDateTimeTypeMapping _timeStamp = new XGDateTimeTypeMapping("timestamp");
    //    private readonly XGDateTimeOffsetTypeMapping _dateTimeOffset = new XGDateTimeOffsetTypeMapping("DATETIME WITH TIME ZONE");
    //    private readonly XGDateTimeOffsetTypeMapping _timeStampOffset = new XGDateTimeOffsetTypeMapping("TIMESTAMP WITH TIME ZONE");

    //    private readonly RelationalTypeMapping _binaryRowVersion
    //        = new XGDateTimeTypeMapping(
    //            "timestamp",
    //            new BytesToDateTimeConverter(),
    //            new ByteArrayComparer(),
    //            null);
    //    private readonly RelationalTypeMapping _binaryRowVersion6
    //        = new XGDateTimeTypeMapping(
    //            "timestamp",
    //            new BytesToDateTimeConverter(),
    //            new ByteArrayComparer(),
    //            6);

    //    // guid
    //    private readonly GuidTypeMapping _uniqueidentifier = new GuidTypeMapping("guid", DbType.Guid);


    //    readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
    //    readonly Dictionary<string, RelationalTypeMapping> _unicodeStoreTypeMappings;
    //    readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

    //    // These are disallowed only if specified without any kind of length specified in parenthesis.
    //    private readonly HashSet<string> _disallowedMappings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    //    {
    //        "binary",
    //        "char",
    //        "nchar",
    //        "varbinary",
    //        "varchar",
    //        "nvarchar"
    //    };

    //    private readonly IXGOptions _options;

    //    public XGTypeMappingSource1(
    //        [NotNull] TypeMappingSourceDependencies dependencies,
    //        [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies,
    //        [NotNull] IXGOptions options)
    //        : base(dependencies, relationalDependencies)
    //    {
    //        _options = options;

    //        //
    //        // String mappings depend on the XGOptions.NoBackslashEscapes setting:
    //        //
    //        _varcharMax = new XGStringTypeMapping("varchar", DbType.String);

    //        _charUnicode = new XGStringTypeMapping("char", DbType.String);
    //        _varcharUnicode = new XGStringTypeMapping("varchar", DbType.String);
    //        _tinytextUnicode = new XGStringTypeMapping("tinytext", DbType.String);
    //        _textUnicode = new XGStringTypeMapping("text", DbType.String);
    //        _mediumtextUnicode = new XGStringTypeMapping("mediumtext", DbType.String);
    //        _longtextUnicode = new XGStringTypeMapping("longtext", DbType.String);

    //        _nchar = new XGStringTypeMapping("nchar", DbType.String);
    //        _nvarchar = new XGStringTypeMapping("nvarchar", DbType.String);

    //        _enum = new XGStringTypeMapping("enum", DbType.Int32);

    //        _storeTypeMappings
    //            = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
    //            {
    //                // boolean
    //                { "bit", _bit },

    //                // integers
    //                { "tinyint", _tinyint },
    //                //{ "tinyint unsigned", _utinyint },
    //                { "smallint", _smallint },
    //                //{ "smallint unsigned", _usmallint },
    //                { "mediumint", _int },
    //                //{ "mediumint unsigned", _uint },
    //                { "int", _int },
    //                //{ "int unsigned", _uint },
    //                { "bigint", _bigint },
    //                //{ "bigint unsigned", _ubigint },

    //                // decimals
    //                { "decimal", _decimal },
    //                { "dec", _decimal },
    //                { "fixed", _decimal },
    //                { "double", _double },
    //                { "double precision", _double },
    //                { "real", _double },
    //                { "float", _float },

    //                // binary
    //                { "binary", _binary },
    //                { "varbinary", _varbinary },
    //                { "tinyblob", _varbinary },
    //                { "blob", _varbinary },
    //                { "mediumblob", _varbinary },
    //                { "longblob", _varbinary },

    //                // string
    //                { "char", _charUnicode },
    //                { "varchar", _varcharUnicode },
    //                { "nchar", _nchar },
    //                { "nvarchar", _nvarchar },
    //                { "tinytext", _varcharMax },
    //                { "text", _varcharMax },
    //                { "mediumtext", _varcharMax },
    //                { "longtext", _varcharMax },
    //                { "enum", _enum },

    //                // DateTime
    //                { "date", _date }
    //            };

    //        _unicodeStoreTypeMappings = options.NoBackslashEscapes
    //            ? new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
    //            {
    //                { "char", _charUnicode },
    //                { "varchar", _varcharUnicode },
    //                { "nchar", _nchar },
    //                { "nvarchar", _nvarchar },
    //                { "tinytext", _varcharMax },
    //                { "text", _varcharMax },
    //                { "mediumtext", _varcharMax },
    //                { "longtext", _varcharMax },
    //                { "enum", _enum },
    //            }
    //            : new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
    //            {
    //                {"char", _nchar},
    //                {"varchar", _nvarchar},
    //                {"nchar", _nchar},
    //                {"nvarchar", _nvarchar},
    //                {"tinytext", _varcharMax},
    //                {"text", _varcharMax},
    //                {"mediumtext", _varcharMax},
    //                {"longtext", _varcharMax},
    //                {"enum", _enum}
    //            };

    //        _clrTypeMappings
    //            = new Dictionary<Type, RelationalTypeMapping>
    //            {
	   //             // boolean
	   //             { typeof(bool), _bit },

	   //             // integers
	   //             { typeof(short), _smallint },
    //                { typeof(ushort), _int },
    //                { typeof(int), _int },
    //                { typeof(uint), _bigint },
    //                { typeof(long), _bigint },
	   //             //{ typeof(ulong), _ubigint },

	   //             // decimals
	   //             { typeof(decimal), _decimal },
    //                { typeof(float), _float },
    //                { typeof(double), _double },

	   //             // byte / char
	   //             { typeof(sbyte), _tinyint },
    //                { typeof(byte), _int }
    //            };

    //        // guid
    //        _clrTypeMappings[typeof(Guid)] = _uniqueidentifier;

    //        // DateTime

    //        _storeTypeMappings["time"] = _time;
    //        _clrTypeMappings[typeof(DateTime)] = _dateTime;
    //        _clrTypeMappings[typeof(DateTimeOffset)] = _dateTimeOffset;
    //        _clrTypeMappings[typeof(TimeSpan)] = _time;
    //    }

    //    /// <summary>
    //    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    //    ///     directly from your code. This API may change or be removed in future releases.
    //    /// </summary>
    //    //protected override void ValidateMapping(CoreTypeMapping mapping, IProperty property)
    //    //{
    //    //    var relationalMapping = mapping as RelationalTypeMapping;

    //    //    if (_disallowedMappings.Contains(relationalMapping?.StoreType))
    //    //    {
    //    //        throw new ArgumentException("Unqualified data type " + relationalMapping?.StoreType);
    //    //    }
    //    //}

    //    /// <summary>
    //    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    //    ///     directly from your code. This API may change or be removed in future releases.
    //    /// </summary>
    //    protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
    //        => FindRawMapping(mappingInfo)?.Clone(mappingInfo);

    //    private RelationalTypeMapping FindRawMapping(RelationalTypeMappingInfo mappingInfo)
    //    {
    //        var clrType = mappingInfo.ClrType;
    //        var storeTypeName = mappingInfo.StoreTypeName;
    //        var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

    //        if (storeTypeName != null)
    //        {
    //            if (storeTypeName.Equals("guid", StringComparison.OrdinalIgnoreCase)
    //                    && clrType == typeof(Guid))
    //            {
    //                return _uniqueidentifier;
    //            }

    //            if (mappingInfo.IsUnicode == true)
    //            {
    //                if (_unicodeStoreTypeMappings.TryGetValue(storeTypeName, out var mapping)
    //                    || _unicodeStoreTypeMappings.TryGetValue(storeTypeNameBase, out mapping))
    //                {
    //                    return clrType == null
    //                           || mapping.ClrType == clrType
    //                        ? mapping
    //                        : null;
    //                }
    //            }
    //            else
    //            {
    //                if (storeTypeNameBase.Equals("datetime", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    if (clrType == null
    //                        || clrType == typeof(DateTime))
    //                    {
    //                        return _dateTime;
    //                    }
    //                    if (clrType == typeof(DateTimeOffset))
    //                    {
    //                        return _dateTimeOffset;
    //                    }
    //                }
    //                else if (storeTypeNameBase.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    if (clrType == null
    //                        || clrType == typeof(DateTime))
    //                    {
    //                        return _timeStamp;
    //                    }
    //                    if (clrType == typeof(DateTimeOffset))
    //                    {
    //                        return _timeStampOffset;
    //                    }
    //                }

    //                if (_storeTypeMappings.TryGetValue(storeTypeName, out var mapping)
    //                    || _storeTypeMappings.TryGetValue(storeTypeNameBase, out mapping))
    //                {
    //                    return clrType == null
    //                           || mapping.ClrType == clrType
    //                        ? mapping
    //                        : null;
    //                }
    //            }
    //        }

    //        if (clrType != null)
    //        {
    //            if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
    //            {
    //                return mapping;
    //            }

    //            if (clrType.TryGetElementType(typeof(JsonObject<>)) != null)
    //            {
    //                return new XGJsonTypeMapping(clrType, unicode: mappingInfo.IsUnicode);
    //            }

    //            if (clrType == typeof(string))
    //            {
    //                // Some of this logic could be moved into XGStringTypeMapping once EF #11896 is fixed
    //                var isAnsi = mappingInfo.IsUnicode == false;
    //                var isFixedLength = mappingInfo.IsFixedLength == true;
    //                var charSetSuffix = "";
    //                var bytesPerChar = isAnsi
    //                    ? _options.AnsiCharSetInfo.BytesPerChar
    //                    : _options.UnicodeCharSetInfo.BytesPerChar;

    //                //if (isAnsi && (
    //                //    (mappingInfo.IsKeyOrIndex &&
    //                //        (_options.CharSetBehavior & CharSetBehavior.AppendToAnsiIndexAndKeyColumns)!= 0)
    //                //    ||
    //                //    (!mappingInfo.IsKeyOrIndex &&
    //                //        (_options.CharSetBehavior & CharSetBehavior.AppendToAnsiNonIndexAndKeyColumns) != 0)
    //                //    ))
    //                //{
    //                //    charSetSuffix = $" CHARACTER SET {_options.AnsiCharSetInfo.CharSetName}";
    //                //}

    //                //if (!isAnsi && (
    //                //    (mappingInfo.IsKeyOrIndex &&
    //                //         (_options.CharSetBehavior & CharSetBehavior.AppendToUnicodeIndexAndKeyColumns)!= 0)
    //                //    ||
    //                //    (!mappingInfo.IsKeyOrIndex &&
    //                //         (_options.CharSetBehavior & CharSetBehavior.AppendToUnicodeNonIndexAndKeyColumns) != 0)
    //                //    ))
    //                //{
    //                //    charSetSuffix = $" CHARACTER SET {_options.UnicodeCharSetInfo.CharSetName}";
    //                //}

    //                var maxSize = 8000 / bytesPerChar;

    //                var size = mappingInfo.Size ??
    //                           (mappingInfo.IsKeyOrIndex
    //                               // Allow to use at most half of the max key length, so at least 2 columns can fit
    //                               ? Math.Min(_options.ServerVersion.IndexMaxBytes / (bytesPerChar * 2), 255)
    //                               : (int?)null);
    //                if (size > maxSize)
    //                {
    //                    size = null;
    //                }

    //                var dbType = isAnsi
    //                    ? (isFixedLength ? DbType.AnsiStringFixedLength : DbType.AnsiString)
    //                    : (isFixedLength ? DbType.StringFixedLength : DbType.String);

    //                return new XGStringTypeMapping(
    //                    size == null
    //                        ? "varchar" + charSetSuffix
    //                        : (isFixedLength ? "char(" : "varchar(") + size + ")" + charSetSuffix,
    //                    dbType,
    //                    !isAnsi,
    //                    size,
    //                    isFixedLength,
    //                    _options.NoBackslashEscapes);
    //            }

    //            if (clrType == typeof(byte[]))
    //            {
    //                if (mappingInfo.IsRowVersion == true)
    //                {
    //                    return _options.ServerVersion.SupportsDateTime6 ? _binaryRowVersion6 : _binaryRowVersion;
    //                }

    //                var size = mappingInfo.Size ??
    //                           (mappingInfo.IsKeyOrIndex ? _options.ServerVersion.IndexMaxBytes : (int?)null);

    //                return new XGByteArrayTypeMapping(
    //                    size: size,
    //                    fixedLength: mappingInfo.IsFixedLength == true);
    //            }
    //        }

    //        return null;
    //    }
    //}
}
