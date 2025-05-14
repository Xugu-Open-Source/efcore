// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class OwnedQueryXGTest : OwnedQueryRelationalTestBase<OwnedQueryXGTest.OwnedQueryXGFixture>
    {
        public OwnedQueryXGTest(OwnedQueryXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        public class OwnedQueryXGFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory
                => XGTestStoreFactory.Instance;
        }
    }
}
