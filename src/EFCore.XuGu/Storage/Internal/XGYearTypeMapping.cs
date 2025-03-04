// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using XuguClient;

namespace EntityFrameworkCore.XuGu.Storage.Internal
{
    public class XGYearTypeMapping : XGTypeMapping
    {
        public XGYearTypeMapping([NotNull] string storeType)
            : base(
                storeType,
                typeof(short),
                XGDbType.TinyInt,
                System.Data.DbType.Int16)
        {
        }

        protected XGYearTypeMapping(RelationalTypeMappingParameters parameters, XGDbType XGDbType)
            : base(parameters, XGDbType)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new XGYearTypeMapping(parameters, XGDbType);
    }
}
