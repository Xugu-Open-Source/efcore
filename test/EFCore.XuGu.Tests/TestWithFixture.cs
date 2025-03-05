using System;
using Microsoft.EntityFrameworkCore;
using XuguClient;
using Xunit;

namespace EntityFrameworkCore.XuGu.Tests
{
    public class RawSqlTestWithFixture<TFixture> : TestWithFixture<TFixture>
        where TFixture : XGTestFixtureBase
    {
        private readonly DbContext _context;
        protected XGConnection Connection { get; }

        protected RawSqlTestWithFixture(TFixture fixture)
            : base(fixture)
        {
            _context = Fixture.CreateDefaultDbContext();
            _context.Database.OpenConnection();

            Connection = (XGConnection)_context.Database.GetDbConnection();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
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
