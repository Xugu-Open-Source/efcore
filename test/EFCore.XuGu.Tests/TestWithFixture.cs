// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Microsoft.EntityFrameworkCore;
using XuguClient;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu
{
    public class RawSqlTestWithFixture<TFixture> : TestWithFixture<TFixture>
        where TFixture : XGTestFixtureBase
    {
        protected DbContext Context { get; }
        protected XGConnection Connection { get; }

        protected RawSqlTestWithFixture(TFixture fixture)
            : base(fixture)
        {
            Context = Fixture.CreateDefaultDbContext();
            Context.Database.OpenConnection();

            Connection = (XGConnection)Context.Database.GetDbConnection();
        }

        protected override void Dispose(bool disposing)
        {
            Context.Dispose();
            base.Dispose(disposing);
        }
    }

    public class TestWithFixture<TFixture>
        : IClassFixture<TFixture>, IDisposable
        where TFixture : XGTestFixtureBase
    {
        protected TestWithFixture(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TestWithFixture()
        {
            Dispose(false);
        }

        protected TFixture Fixture { get; }
    }
}
