// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using XuGu.EntityFrameworkCore.Extensions;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    [XGCondition(XGCondition.SupportsSequences)]
    public class GraphUpdatesWithSequenceXGTest : GraphUpdatesXGTestBase<GraphUpdatesWithSequenceXGTest.GraphUpdatesWithSequenceXGFixture>
    {
        public GraphUpdatesWithSequenceXGTest(GraphUpdatesWithSequenceXGFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesWithSequenceXGFixture : GraphUpdatesXGFixtureBase
        {
            protected override string DatabaseName => "GraphSequenceUpdatesTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ForXGUseSequenceHiLo(); // ensure model uses sequences
                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
