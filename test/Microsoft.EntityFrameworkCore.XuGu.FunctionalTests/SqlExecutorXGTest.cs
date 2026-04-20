// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class SqlExecutorXGTest : SqlExecutorTestBase<NorthwindQueryXGFixture>
    {
        [Fact]
        public override void Executes_stored_procedure()
        {
            base.Executes_stored_procedure();

            Assert.Equal(
                "EXEC `SYSDBA`.`TENMOSTEXPENSIVEPRODUCTS`",
                Sql);
        }

        [Fact]
        public override void Executes_stored_procedure_with_parameter()
        {
            //base.Executes_stored_procedure_with_parameter();

            using (var context = CreateContext())
            {
                var parameter = CreateDbParameter("CustomerID", "ALFKI");

                Assert.Equal(-1, context.Database.ExecuteSqlCommand(CustomerOrderHistorySproc, parameter));
            }

            Assert.Equal(
                @"?: ALFKI (Nullable = false) (Size = 5)

EXEC `SYSDBA`.`CUSTORDERHIST` (?)",
                Sql);
        }

        [Fact]
        public override void Executes_stored_procedure_with_generated_parameter()
        {
            base.Executes_stored_procedure_with_generated_parameter();

            Assert.Equal(
                @":p0: ALFKI (Size = 4000)

EXEC `SYSDBA`.`CustOrderHist` (:p0)",
                Sql);
        }

        public SqlExecutorXGTest(NorthwindQueryXGFixture fixture)
            : base(fixture)
        {
        }

        protected override DbParameter CreateDbParameter(string name, object value)
            => new XGParameters
            {
                ParameterName = name,
                Value = value
            };

        protected override string TenMostExpensiveProductsSproc => "EXEC `SYSDBA`.`TENMOSTEXPENSIVEPRODUCTS`";

        protected override string CustomerOrderHistorySproc => "EXEC `SYSDBA`.`CUSTORDERHIST` (?)";

        protected override string CustomerOrderHistoryWithGeneratedParameterSproc => "EXEC `SYSDBA`.`CustOrderHist` ({0})";

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
