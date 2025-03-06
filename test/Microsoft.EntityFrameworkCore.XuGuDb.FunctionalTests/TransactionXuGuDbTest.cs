// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class TransactionXuGuDbTest : TransactionTestBase<XuGuDbTestStore, TransactionXuGuDbFixture>
    {
        public TransactionXuGuDbTest(TransactionXuGuDbFixture fixture)
            : base(fixture)
        {
        }

        protected override bool SnapshotSupported => true;
    }
}
