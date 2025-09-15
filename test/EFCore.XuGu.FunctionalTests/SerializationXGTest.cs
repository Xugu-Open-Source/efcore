// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class SerializationXGTest : SerializationTestBase<F1XGFixture>
    {
        public SerializationXGTest(F1XGFixture fixture)
            : base(fixture)
        {
        }
    }
}
