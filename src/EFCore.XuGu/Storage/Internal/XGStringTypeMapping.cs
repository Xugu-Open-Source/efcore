// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGStringTypeMapping : StringTypeMapping
    {
        private const int UnicodeMax = 2000;
        private const int AnsiMax = 4000;

        private readonly int _maxSpecificSize;

        private readonly StoreTypePostfix? _storeTypePostfix;

        public XGStringTypeMapping(
            [NotNull] string storeType,
            [CanBeNull] DbType? dbType,
            bool unicode = false,
            int? size = null,
            bool fixedLength = false,
            StoreTypePostfix? storeTypePostfix = null)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(string)),
                    storeType,
                    GetStoreTypePostfix(storeTypePostfix, unicode, size),
                    dbType,
                    unicode,
                    size,
                    fixedLength))
        {
            _storeTypePostfix = storeTypePostfix;
        }

        protected XGStringTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
            _maxSpecificSize = CalculateSize(parameters.Unicode, parameters.Size);
        }

        private static StoreTypePostfix GetStoreTypePostfix(
            StoreTypePostfix? storeTypePostfix,
            bool unicode,
            int? size)
            => storeTypePostfix
               ?? (unicode
                   ? size.HasValue && size <= UnicodeMax
                       ? StoreTypePostfix.Size
                       : StoreTypePostfix.None
                   : size.HasValue && size <= AnsiMax
                       ? StoreTypePostfix.Size
                       : StoreTypePostfix.None);

        private static int CalculateSize(bool unicode, int? size)
            => unicode
                ? size.HasValue && size <= UnicodeMax
                    ? size.Value
                    : UnicodeMax
                : size.HasValue && size <= AnsiMax
                    ? size.Value
                    : AnsiMax;

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new XGStringTypeMapping(
                Parameters.WithStoreTypeAndSize(storeType, size, GetStoreTypePostfix(_storeTypePostfix, IsUnicode, size)));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new XGStringTypeMapping(Parameters.WithComposedConverter(converter));

        protected override void ConfigureParameter(DbParameter parameter)
        {
            // For strings and byte arrays, set the max length to the size facet if specified, or
            // _maxSpecificSize bytes if no size facet specified, if the data will fit so as to avoid query cache
            // fragmentation by setting lots of different Size values otherwise always set to
            // 0 to avoid SQL client size inference.

            var value = parameter.Value;
            var length = (value as string)?.Length;

            try
            {
                parameter.Size = value == null || value == DBNull.Value || length != null && length <= _maxSpecificSize
                    ? _maxSpecificSize
                    : 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected override string GenerateNonNullSqlLiteral(object value)
            => IsUnicode
                ? $"N'{EscapeSqlLiteral((string)value)}'" // Interpolation okay; strings
                : $"'{EscapeSqlLiteral((string)value)}'";
    }
}
