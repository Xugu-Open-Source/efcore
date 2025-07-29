// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Data;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGDecimalTypeMapping : DecimalTypeMapping
    {
        public XGDecimalTypeMapping(
            [NotNull] string storeType,
            DbType? dbType = null,
            int? precision = null,
            int? scale = null,
            StoreTypePostfix storeTypePostfix = StoreTypePostfix.None)
            : base(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(decimal)),
                    storeType,
                    storeTypePostfix,
                    dbType,
                    precision: precision,
                    scale: scale))
        {
        }

        protected XGDecimalTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGDecimalTypeMapping(parameters);
    }
}
