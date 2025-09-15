// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class QueryNoClientEvalXGTest : QueryNoClientEvalTestBase<QueryNoClientEvalXGFixture>
    {
        public QueryNoClientEvalXGTest(QueryNoClientEvalXGFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public override void Doesnt_throw_when_from_sql_not_composed()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Customers
                        .FromSqlRaw(@"select * from `Customers`")
                        .ToList();

                Assert.Equal(91, customers.Count);
            }
        }
    }
}
