// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class MonsterFixupChangedChangingXGTest : MonsterFixupTestBase<MonsterFixupChangedChangingXGTest.MonsterFixupChangedChangingXGFixture>
    {
        public MonsterFixupChangedChangingXGTest(MonsterFixupChangedChangingXGFixture fixture)
            : base(fixture)
        {
        }

        public class MonsterFixupChangedChangingXGFixture : MonsterFixupChangedChangingFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            protected override void OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(
                ModelBuilder builder)
            {
                base.OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(builder);

                builder.Entity<TMessage>().HasKey(e => e.MessageId);
                builder.Entity<TProductPhoto>().HasKey(e => e.PhotoId);
                builder.Entity<TProductReview>().HasKey(e => e.ReviewId);
            }
        }
    }
}
