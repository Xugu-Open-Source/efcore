// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using XuGu.EntityFrameworkCore.Extensions;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class GraphUpdatesWithIdentityXGTest : GraphUpdatesXGTestBase<GraphUpdatesWithIdentityXGTest.GraphUpdatesWithIdentityXGFixture>
    {
        public GraphUpdatesWithIdentityXGTest(GraphUpdatesWithIdentityXGFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesWithIdentityXGFixture : GraphUpdatesXGFixtureBase
        {
            protected override string DatabaseName => "GraphIdentityUpdatesTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ForXGUseIdentityColumns(); // ensure model uses identity

                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
