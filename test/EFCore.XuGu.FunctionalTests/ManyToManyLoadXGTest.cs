// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class ManyToManyLoadXGTest : ManyToManyLoadTestBase<ManyToManyLoadXGTest.ManyToManyLoadXGFixture>
    {
        public ManyToManyLoadXGTest(ManyToManyLoadXGFixture fixture)
            : base(fixture)
        {
        }

        public class ManyToManyLoadXGFixture : ManyToManyLoadFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder
                    .Entity<JoinOneSelfPayload>()
                    .Property(e => e.Payload)
                    .ValueGeneratedOnAdd(); // uses UTC in the original SQL Server implementation

                modelBuilder
                    .SharedTypeEntity<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                    .IndexerProperty<string>("Payload")
                    .HasMaxLength(255) // longtext does not support default values
                    .HasDefaultValue("Generated");

                modelBuilder
                    .Entity<JoinOneToThreePayloadFull>()
                    .Property(e => e.Payload)
                    .HasMaxLength(255) // longtext does not support default values
                    .HasDefaultValue("Generated");
            }
        }
    }
}
