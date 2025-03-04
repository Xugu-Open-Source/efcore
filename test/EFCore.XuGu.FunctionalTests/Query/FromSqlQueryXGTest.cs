using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using XuguClient;
using Xunit;

namespace EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class FromSqlQueryXGTest : FromSqlQueryTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public FromSqlQueryXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new XGParameters
            {
                ParameterName = name,
                Value = value
            };

        public override void FromSqlRaw_queryable_simple_projection_composed()
        {
            // The full Northwind data set is not yet being preloaded.
            // https://github.com/aspnet/EntityFrameworkCore/issues/18111
            using (var context = CreateContext())
            {
                var boolMapping = (RelationalTypeMapping)context.GetService<ITypeMappingSource>().FindMapping(typeof(bool));
                var actual = context.Set<Product>().FromSqlRaw(
                        NormalizeDelimitersInRawString(
                            @"SELECT *
FROM [Products]
WHERE [Discontinued] <> " + boolMapping.GenerateSqlLiteral(true)))
                    .Select(p => p.ProductName)
                    .ToArray();

                Assert.Equal(69, actual.Length);
            }
        }
    }
}
