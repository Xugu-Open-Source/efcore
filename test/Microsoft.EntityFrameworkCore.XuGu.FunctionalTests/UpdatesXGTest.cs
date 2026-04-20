// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class UpdatesXGTest : UpdatesRelationalTestBase<UpdatesXGFixture, XGTestStore>
    {
        public UpdatesXGTest(UpdatesXGFixture fixture)
            : base(fixture)
        {
        }
    }
}
