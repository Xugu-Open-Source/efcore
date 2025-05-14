// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class ManyToManyFieldsLoadXGTest : ManyToManyFieldsLoadTestBase<
        ManyToManyFieldsLoadXGTest.ManyToManyFieldsLoadXGFixture>
    {
        public ManyToManyFieldsLoadXGTest(ManyToManyFieldsLoadXGFixture fixture)
            : base(fixture)
        {
        }

        public class ManyToManyFieldsLoadXGFixture : ManyToManyFieldsLoadFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory
                => XGTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder
                    .Entity<JoinOneSelfPayload>()
                    .Property(e => e.Payload)
                    .ValueGeneratedOnAdd();

                modelBuilder
                    .SharedTypeEntity<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                    .IndexerProperty<string>("Payload")
                        .HasMaxLength(255)
                    .HasDefaultValue("Generated");

                modelBuilder
                    .Entity<JoinOneToThreePayloadFull>()
                    .Property(e => e.Payload)
                        .HasMaxLength(255)
                    .HasDefaultValue("Generated");
            }
        }
    }
}
