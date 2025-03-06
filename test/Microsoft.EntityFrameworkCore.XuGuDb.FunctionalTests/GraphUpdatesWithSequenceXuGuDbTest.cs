// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    [XuGuDbCondition(XuGuDbCondition.SupportsSequences)]
    public class GraphUpdatesWithSequenceXuGuDbTest : GraphUpdatesXuGuDbTestBase<GraphUpdatesWithSequenceXuGuDbTest.GraphUpdatesWithSequenceXuGuDbFixture>
    {
        public GraphUpdatesWithSequenceXuGuDbTest(GraphUpdatesWithSequenceXuGuDbFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesWithSequenceXuGuDbFixture : GraphUpdatesXuGuDbFixtureBase
        {
            protected override string DatabaseName => "GraphSequenceUpdatesTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ForXuGuDbUseSequenceHiLo(); // ensure model uses sequences
                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
