// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class F1XGFixture : F1XGFixture<byte[]>
    {
    }

    public abstract class F1XGFixture<TRowVersion> : F1RelationalFixture<TRowVersion>
    {
        protected override ITestStoreFactory TestStoreFactory
            => XGTestStoreFactory.Instance;

        public override TestHelpers TestHelpers
            => XGTestHelpers.Instance;
    }
}
