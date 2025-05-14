// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class NotificationEntitiesXGTest : NotificationEntitiesTestBase<NotificationEntitiesXGTest.NotificationEntitiesXGFixture>
    {
        public NotificationEntitiesXGTest(NotificationEntitiesXGFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesXGFixture : NotificationEntitiesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}
