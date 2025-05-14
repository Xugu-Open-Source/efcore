// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using XuguClient;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class FromSqlQueryXGTest : FromSqlQueryTestBase<NorthwindQueryXGFixture<NoopModelCustomizer>>
    {
        public FromSqlQueryXGTest(NorthwindQueryXGFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.CommonTableExpressions))]
        public override Task FromSqlRaw_composed_with_common_table_expression(bool async)
        {
            return base.FromSqlRaw_composed_with_common_table_expression(async);
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new XGParameters
            {
                ParameterName = name,
                Value = value
            };

        public override async Task FromSqlInterpolated_with_inlined_db_parameter(bool async)
        {
            var parameter = CreateDbParameter("somename", "ALFKI");

            await AssertQuery(
                async,
                ss => ((DbSet<Customer>)ss.Set<Customer>())
                    .FromSqlInterpolated(
                        NormalizeDelimitersInInterpolatedString($"SELECT * FROM `Customers` WHERE `CustomerID` = {parameter}")),
                ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI"));
        }

        public override async Task FromSql_with_inlined_db_parameter(bool async)
        {
            var parameter = CreateDbParameter("somename", "ALFKI");

            await AssertQuery(
                async,
                ss => ((DbSet<Customer>)ss.Set<Customer>())
                    .FromSql(
                        NormalizeDelimitersInInterpolatedString($"SELECT * FROM `Customers` WHERE `CustomerID` = {parameter}")),
                ss => ss.Set<Customer>().Where(x => x.CustomerID == "ALFKI"));
        }

        public override Task FromSqlRaw_in_subquery_with_positional_dbParameter_with_name(bool async)
            => AssertQuery(
                async,
                ss => ss.Set<Order>().Where(
                    o => ((DbSet<Customer>)ss.Set<Customer>())
                        .FromSqlRaw(
                            NormalizeDelimitersInRawString(@"SELECT * FROM `Customers` WHERE `City` = {0}"),
                            // ReSharper disable once FormatStringProblem
                            CreateDbParameter("city", "London"))
                        .Select(c => c.CustomerID)
                        .Contains(o.CustomerID)),
                ss => ss.Set<Order>().Where(
                    o => ss.Set<Customer>().Where(x => x.City == "London")
                        .Select(c => c.CustomerID)
                        .Contains(o.CustomerID)));
    }
}
