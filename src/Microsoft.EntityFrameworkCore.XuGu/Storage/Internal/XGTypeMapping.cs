// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using XuguClient;


namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class XGTypeMapping : RelationalTypeMapping
    {
        public new XGDbType? StoreType { get; }

        internal XGTypeMapping([NotNull] string defaultTypeName, [NotNull] Type clrType, XGDbType storeType)
            : base(defaultTypeName, clrType)
        {
            StoreType = storeType;
            
        }

        internal XGTypeMapping([NotNull] string defaultTypeName, [NotNull] Type clrType)
            : base(defaultTypeName, clrType)
        { }

        protected override void ConfigureParameter([NotNull] DbParameter parameter)
        {
            //base.ConfigureParameter(parameter);
            if (StoreType.HasValue)
            {
                //if(((XGParameters)parameter).m_DbType == XGDbType.DateTimeOffset)
                //{
                //    parameter.Value = ((DateTimeOffset)parameter.Value).ToString("yyyy-MM-dd HH:mm:ss.fff");
                //}
                ((XGParameters) parameter).m_DbType = StoreType.Value;
            }
        }
    }
}
