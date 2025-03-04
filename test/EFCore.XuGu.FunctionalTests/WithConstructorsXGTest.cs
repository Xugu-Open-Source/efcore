using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class WithConstructorsXGTest : WithConstructorsTestBase<WithConstructorsXGTest.WithConstructorsXGFixture>
    {
        public WithConstructorsXGTest(WithConstructorsXGFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class WithConstructorsXGFixture : WithConstructorsFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);
                modelBuilder.Entity<BlogQuery>()
                    .HasNoKey()
                    .ToSqlQuery("select * from `Blog`");
            }
        }
    }
}
