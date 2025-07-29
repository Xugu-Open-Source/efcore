// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         Represents the mapping between a .NET <see cref="DateTime" /> type and a database type.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class XGDateTimeTypeMapping : RelationalTypeMapping
    {
        private const string DateTimeFormatConst = "'{0:yyyy-MM-dd HH:mm:ss}'";

        public XGDateTimeTypeMapping(
            [NotNull] string storeType,
            ValueConverter converter = null,
            ValueComparer comparer = null,
            int? precision = null)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(DateTime), converter, comparer),
                    storeType,
                    precision == null ? StoreTypePostfix.None : StoreTypePostfix.Precision,
                    System.Data.DbType.DateTime,
                    precision: precision))
        {
        }

        protected XGDateTimeTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        public override RelationalTypeMapping Clone(string storeType, int? size)
            => new XGDateTimeTypeMapping(Parameters.WithStoreTypeAndSize(storeType, size));

        public override CoreTypeMapping Clone(ValueConverter converter)
            => new XGDateTimeTypeMapping(Parameters.WithComposedConverter(converter));

        protected override string SqlLiteralFormatString => DateTimeFormatConst;
    }
}
