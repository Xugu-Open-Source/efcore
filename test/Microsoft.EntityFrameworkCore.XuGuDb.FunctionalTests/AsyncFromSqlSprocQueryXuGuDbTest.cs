// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class AsyncFromSqlSprocQueryXuGuDbTest : AsyncFromSqlSprocQueryTestBase<NorthwindSprocQueryXuGuDbFixture>
    {
        public AsyncFromSqlSprocQueryXuGuDbTest(NorthwindSprocQueryXuGuDbFixture fixture)
            : base(fixture)
        {
        }

        protected override string TenMostExpensiveProductsSproc => "`SYSDBA`.`TenMostExpensiveProducts`";

        protected override string CustomerOrderHistorySproc => "`SYSDBA`.`CustOrderHist` CustomerID = {0}";
    }
}
