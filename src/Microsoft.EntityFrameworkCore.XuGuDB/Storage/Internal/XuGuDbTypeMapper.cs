// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XuGuDbTypeMapper : RelationalTypeMapper
    {
        private readonly XuGuDbMaxLengthMapping _varcharmaxU 
            = new XuGuDbMaxLengthMapping("varchar", typeof(string), dbType: null, unicode: true, size: null);

        private readonly XuGuDbMaxLengthMapping _varchar450U 
            = new XuGuDbMaxLengthMapping("varchar(450)", typeof(string), dbType: null, unicode: true, size: 450);

        private readonly XuGuDbMaxLengthMapping _varcharmax
            = new XuGuDbMaxLengthMapping("varchar", typeof(string), dbType: DbType.AnsiString, unicode: false, size: null, hasNonDefaultUnicode: true);

        private readonly XuGuDbMaxLengthMapping _varchar900
            = new XuGuDbMaxLengthMapping("varchar(900)", typeof(string), dbType: DbType.AnsiString, unicode: false, size: 900, hasNonDefaultUnicode: true);

        private readonly XuGuDbMaxLengthMapping _binarymax
            = new XuGuDbMaxLengthMapping("binary", typeof(byte[]), dbType: DbType.Binary, unicode: false, size: null);

        private readonly XuGuDbMaxLengthMapping _binary900
            = new XuGuDbMaxLengthMapping("binary(900)", typeof(byte[]), dbType: DbType.Binary, unicode: false, size: 900);

        private readonly RelationalTypeMapping _integer
            = new RelationalTypeMapping("integer", typeof(int), dbType: DbType.Int32);

        private readonly RelationalTypeMapping _bigint
            = new RelationalTypeMapping("bigint", typeof(long), dbType: DbType.Int64);

        private readonly RelationalTypeMapping _smallint
            = new RelationalTypeMapping("smallint", typeof(short), dbType: DbType.Int16);

        private readonly RelationalTypeMapping _tinyint
            = new RelationalTypeMapping("tinyint", typeof(sbyte), dbType: DbType.SByte);

        private readonly RelationalTypeMapping _bit
            = new RelationalTypeMapping("bit", typeof(bool));

        private readonly RelationalTypeMapping _boolean
            = new RelationalTypeMapping("boolean", typeof(bool));

        //private readonly XuGuDbMaxLengthMapping _char 
        //    = new XuGuDbMaxLengthMapping("char", typeof(string), dbType: DbType.AnsiStringFixedLength, unicode: false, size: null, hasNonDefaultUnicode: true);

        private readonly XuGuDbMaxLengthMapping _char
                    = new XuGuDbMaxLengthMapping("char", typeof(char), dbType: null, unicode: false, size: null);

        private readonly XuGuDbMaxLengthMapping _varchar 
            = new XuGuDbMaxLengthMapping("varchar", typeof(string), dbType: null, unicode: false, size: null, hasNonDefaultUnicode: true);

        private readonly XuGuDbMaxLengthMapping _binary 
            = new XuGuDbMaxLengthMapping("binary", typeof(byte[]), dbType: DbType.Binary);

        private readonly RelationalTypeMapping _date
            = new RelationalTypeMapping("date", typeof(DateTime), dbType: DbType.DateTime);

        private readonly RelationalTypeMapping _datetime
            = new RelationalTypeMapping("datetime", typeof(DateTime), dbType: DbType.DateTime);

        private readonly RelationalTypeMapping _float 
            = new RelationalTypeMapping("float", typeof(float));

        private readonly RelationalTypeMapping _double
            = new RelationalTypeMapping("double", typeof(double));

        private readonly RelationalTypeMapping _timewithtimezone
            = new RelationalTypeMapping("timewithtimezone", typeof(string));

        private readonly RelationalTypeMapping _datetimewithtimezone
            = new RelationalTypeMapping("datetimewithtimezone", typeof(string));

        private readonly RelationalTypeMapping _blob
            = new RelationalTypeMapping("blob", typeof(byte[]), dbType: DbType.Binary);

        private readonly RelationalTypeMapping _clob
            = new RelationalTypeMapping("clob", typeof(string));

        private readonly RelationalTypeMapping _numeric
            = new RelationalTypeMapping("numeric(18, 2)", typeof(decimal));

        private readonly RelationalTypeMapping _time
            = new RelationalTypeMapping("time", typeof(DateTime));

        private readonly RelationalTypeMapping _guid
            = new RelationalTypeMapping("guid", typeof(Guid));

        private readonly RelationalTypeMapping _timestamp
            = new RelationalTypeMapping("timestamp", typeof(string));

        private readonly RelationalTypeMapping _intervalyear
            = new RelationalTypeMapping("intervalyear", typeof(string));

        private readonly RelationalTypeMapping _intervalmonth
            = new RelationalTypeMapping("intervalmonth", typeof(string));

        private readonly RelationalTypeMapping _intervalday
            = new RelationalTypeMapping("intervalday", typeof(string));

        private readonly RelationalTypeMapping _intervalhour
            = new RelationalTypeMapping("intervalhour", typeof(string));

        private readonly RelationalTypeMapping _intervalminute
            = new RelationalTypeMapping("intervalminute", typeof(string));

        private readonly RelationalTypeMapping _intervalsecond
            = new RelationalTypeMapping("intervalsecond", typeof(string));

        private readonly RelationalTypeMapping _intervaldaytohour
            = new RelationalTypeMapping("intervaldaytohour", typeof(string));

        private readonly RelationalTypeMapping _intervaldaytominute
            = new RelationalTypeMapping("intervaldaytominute", typeof(string));

        private readonly RelationalTypeMapping _intervaldaytosecond
            = new RelationalTypeMapping("intervaldaytosecond", typeof(string));

        private readonly RelationalTypeMapping _intervalhourtominute
            = new RelationalTypeMapping("intervalhourtominute", typeof(string));

        private readonly RelationalTypeMapping _intervalhourtosecond
            = new RelationalTypeMapping("intervalhourtosecond", typeof(string));

        private readonly RelationalTypeMapping _intervalminutetosecond
            = new RelationalTypeMapping("intervalminutetosecond", typeof(string));

        private readonly RelationalTypeMapping _intervalyeartomonth
            = new RelationalTypeMapping("intervalyeartomonth", typeof(string));

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private readonly HashSet<string> _disallowedMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public XuGuDbTypeMapper()
        {
            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "varchar(450)", _varchar450U },
                    { "varchar(900)", _varchar900 },
                    { "varbinary", _binarymax },
                    { "varbinary(900)", _binary900 },
                    { "integer", _integer },
                    { "int", _integer },
                    { "bigint", _bigint },
                    { "smallint", _smallint },
                    { "tinyint", _tinyint },
                    { "boolean", _boolean },
                    { "bool", _boolean },
                    { "bit", _bit },
                    { "char", _char },
                    { "varchar", _varchar },
                    { "string", _varchar },
                    { "binary", _binary },
                    { "date", _date },
                    { "datetime", _datetime },
                    { "float", _float },
                    { "double", _double },
                    { "timewithtimezone", _timewithtimezone },
                    { "datetimewithtimezone", _datetimewithtimezone },
                    { "blob", _blob },
                    { "image", _blob },
                    { "text", _clob },
                    { "guid", _guid },
                    { "numeric", _numeric },
                    { "time", _time },
                    { "timestamp", _timestamp },
                    { "intervalyear", _intervalyear },
                    { "intervalmonth", _intervalmonth },
                    { "intervalday", _intervalday },
                    { "intervalhour", _intervalhour },
                    { "intervalminute", _intervalminute },
                    { "intervalsecond", _intervalsecond },
                    { "intervaldaytohour", _intervaldaytohour },
                    { "intervaldaytominute", _intervaldaytominute },
                    { "intervaldaytosecond", _intervaldaytosecond },
                    { "intervalhourtominute", _intervalhourtominute },
                    { "intervalhourtosecond", _intervalhourtosecond },
                    { "intervalminutetosecond", _intervalminutetosecond },
                    { "intervalyeartomonth", _intervalyeartomonth }
                };

            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _integer },
                    { typeof(long), _bigint },
                    { typeof(DateTime), _datetime },
                    { typeof(bool), _boolean },
                    { typeof(sbyte), _tinyint },
                    { typeof(double), _double },
                    { typeof(string), _varchar },
                    { typeof(char), _char },
                    { typeof(short), _smallint },
                    { typeof(float), _float },
                    { typeof(decimal), _numeric },
                    { typeof(byte[]), _binary },
                    { typeof(Guid), _guid }
                };

            // These are disallowed only if specified without any kind of length specified in parenthesis.
            // This is because we don't try to make a new type from this string and any max length value
            // specified in the model, which means use of these strings is almost certainly an error, and
            // if it is not an error, then using, for example, varbinary(1) will work instead.
            _disallowedMappings
                = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "blob",
                    "clob",
                };

            ByteArrayMapper
                = new ByteArrayRelationalTypeMapper(
                    8000,
                    _binary,
                    _binarymax,
                    _binary900,
                    _blob, size => new XuGuDbMaxLengthMapping(
                        "binary(" + size + ")",
                        typeof(byte[]),
                        DbType.Binary,
                        unicode: false,
                        size: size,
                        hasNonDefaultUnicode: false,
                        hasNonDefaultSize: true));

            StringMapper
                = new StringRelationalTypeMapper(
                    8000,
                    _varchar,
                    _varcharmax,
                    _varchar900,
                    size => new XuGuDbMaxLengthMapping(
                        "varchar(" + size + ")",
                        typeof(string),
                        dbType: DbType.AnsiString,
                        unicode: false,
                        size: size,
                        hasNonDefaultUnicode: true,
                        hasNonDefaultSize: true),
                    4000,
                    _varcharmaxU,
                    _varcharmaxU,
                    _varchar450U,
                    size => new XuGuDbMaxLengthMapping(
                        "varchar(" + size + ")",
                        typeof(string),
                        dbType: null,
                        unicode: true,
                        size: size,
                        hasNonDefaultUnicode: false,
                        hasNonDefaultSize: true));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IByteArrayRelationalTypeMapper ByteArrayMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IStringRelationalTypeMapper StringMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void ValidateTypeName(string storeType)
        {
            if (_disallowedMappings.Contains(storeType))
            {
                throw new ArgumentException(XuGuDbStrings.UnqualifiedDataType(storeType));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GetColumnType(IProperty property) => property.XuGuDb().ColumnType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _storeTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            clrType = clrType.UnwrapNullableType().UnwrapEnumType();

            return clrType == typeof(string)
                ? _varcharmax
                : (clrType == typeof(byte[])
                    ? _binarymax
                    : base.FindMapping(clrType));
        }

        // Indexes in SQL Server have a max size of 900 bytes
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool RequiresKeyMapping(IProperty property)
            => base.RequiresKeyMapping(property) || property.IsIndex();
    }
}
