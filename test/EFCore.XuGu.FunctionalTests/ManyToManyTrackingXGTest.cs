// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class ManyToManyTrackingXGTest
        : ManyToManyTrackingXGTestBase<ManyToManyTrackingXGTest.ManyToManyTrackingXGFixture>
    {
        public ManyToManyTrackingXGTest(ManyToManyTrackingXGFixture fixture)
            : base(fixture)
        {
        }

        public class ManyToManyTrackingXGFixture : ManyToManyTrackingXGFixtureBase
        {
            protected override string StoreName
                => "ManyToManyTrackingXGTest";
        }
    }
}
