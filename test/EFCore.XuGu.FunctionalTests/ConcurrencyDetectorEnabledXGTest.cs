// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class ConcurrencyDetectorEnabledXGTest : ConcurrencyDetectorEnabledRelationalTestBase<
        ConcurrencyDetectorEnabledXGTest.ConcurrencyDetectorXGFixture>
    {
        public ConcurrencyDetectorEnabledXGTest(ConcurrencyDetectorXGFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected override async Task ConcurrencyDetectorTest(Func<ConcurrencyDetectorDbContext, Task<object>> test)
        {
            await base.ConcurrencyDetectorTest(test);

            Assert.Empty(Fixture.TestSqlLoggerFactory.SqlStatements);
        }

        public class ConcurrencyDetectorXGFixture : ConcurrencyDetectorFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => XGTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;
        }
    }
}
