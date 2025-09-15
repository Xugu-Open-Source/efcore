// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class TwoDatabasesXGTest : TwoDatabasesTestBase, IClassFixture<XGFixture>
    {
        public TwoDatabasesXGTest(XGFixture fixture)
            : base(fixture)
        {
        }

        protected new XGFixture Fixture
            => (XGFixture)base.Fixture;

        protected override DbContextOptionsBuilder CreateTestOptions(
            DbContextOptionsBuilder optionsBuilder,
            bool withConnectionString = false,
            bool withNullConnectionString = false)
            => withConnectionString
                ? withNullConnectionString
                    ? optionsBuilder.UseXG((string)null, AppConfig.ServerVersion)
                    : optionsBuilder.UseXG(DummyConnectionString, AppConfig.ServerVersion)
                : optionsBuilder.UseXG(AppConfig.ServerVersion);

        protected override TwoDatabasesWithDataContext CreateBackingContext(string databaseName)
            => new TwoDatabasesWithDataContext(Fixture.CreateOptions(XGTestStore.Create(databaseName)));

        protected override string DummyConnectionString { get; } = "Server=localhost;Database=DoesNotExist;Allow User Variables=True;Use Affected Rows=False";
    }
}
