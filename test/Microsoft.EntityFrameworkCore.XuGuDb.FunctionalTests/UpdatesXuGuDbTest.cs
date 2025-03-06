// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class UpdatesXuGuDbTest : UpdatesRelationalTestBase<UpdatesXuGuDbFixture, XuGuDbTestStore>
    {
        public UpdatesXuGuDbTest(UpdatesXuGuDbFixture fixture)
            : base(fixture)
        {
        }
    }
}
