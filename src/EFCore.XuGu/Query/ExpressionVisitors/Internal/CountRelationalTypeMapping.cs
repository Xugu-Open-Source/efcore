using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EFCore.XuGu.Query.ExpressionVisitors.Internal
{
    class CountRelationalTypeMapping: RelationalTypeMapping
    {
        public override string StoreType { get; }= typeof(long).Name;
        public override DbType? DbType { get; }= System.Data.DbType.Int64;
        public CountRelationalTypeMapping()
        : base(new RelationalTypeMappingParameters(new CoreTypeMappingParameters(typeof(long)), "Int64"))
        {
        }

        protected CountRelationalTypeMapping(RelationalTypeMappingParameters parameters) : base(parameters)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        {
            return new CountRelationalTypeMapping(parameters);
        }
    }
}
