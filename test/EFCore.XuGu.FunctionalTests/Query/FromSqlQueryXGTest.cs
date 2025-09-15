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
    }
}
