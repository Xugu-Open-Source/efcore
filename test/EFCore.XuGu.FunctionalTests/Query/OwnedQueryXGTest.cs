// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
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

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.WhereSubqueryReferencesOuterQuery))]
        public override Task Union_over_owned_collection(bool async)
        {
            return base.Union_over_owned_collection(async);
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.WhereSubqueryReferencesOuterQuery))]
        public override Task Distinct_over_owned_collection(bool async)
        {
            return base.Distinct_over_owned_collection(async);
        }

        public class OwnedQueryXGFixture : RelationalOwnedQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory
                => XGTestStoreFactory.Instance;
        }
    }
}
