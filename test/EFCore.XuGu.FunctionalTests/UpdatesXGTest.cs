using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Tests;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Microsoft.EntityFrameworkCore.XuGu.ValueGeneration.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class UpdatesXGTest : UpdatesRelationalTestBase<UpdatesXGTest.UpdatesXGFixture>
    {
        public UpdatesXGTest(UpdatesXGFixture fixture)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        [ConditionalFact]
        public override void Identifiers_are_generated_correctly()
        {
            using (var context = CreateContext())
            {
                var entityType = context.Model.FindEntityType(typeof(
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly));
                Assert.Equal("LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIs~", entityType.GetTableName());
                Assert.Equal("PK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameTha~", entityType.GetKeys().Single().GetName());
                Assert.Equal("FK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameTha~", entityType.GetForeignKeys().Single().GetConstraintName());
                Assert.Equal("IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameTha~", entityType.GetIndexes().Single().GetDatabaseName());
            }
        }

        [SupportedServerVersionCondition(nameof(ServerVersionSupport.DefaultExpression), nameof(ServerVersionSupport.AlternativeDefaultExpression))]
        [SupportedServerVersionCondition(nameof(ServerVersionSupport.Returning))]
        public override void Save_with_shared_foreign_key()
        {
            base.Save_with_shared_foreign_key();
        }

        public class UpdatesXGFixture : UpdatesRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                // Necessary for test `Save_with_shared_foreign_key` to run correctly.
                if (AppConfig.ServerVersion.Supports.DefaultExpression ||
                    AppConfig.ServerVersion.Supports.AlternativeDefaultExpression)
                {
                    modelBuilder.Entity<ProductBase>()
                        .Property(p => p.Id).HasDefaultValueSql("(UUID())");
                }

                Models.Issue1300.Setup(modelBuilder, context);
            }

            public static class Models
            {
                public static class Issue1300
                {
                    public static void Setup(ModelBuilder modelBuilder, DbContext context)
                    {
                        modelBuilder.Entity<Flavor>(
                            entity =>
                            {
                                entity.HasKey(e => new {e.FlavorId, e.DiscoveryDate});
                                entity.Property(e => e.FlavorId)
                                    .ValueGeneratedOnAdd();
                                entity.Property(e => e.DiscoveryDate)
                                    .ValueGeneratedOnAdd();
                            });
                    }

                    public class Flavor
                    {
                        public int FlavorId { get; set; }
                        public DateTime DiscoveryDate { get; set; }
                    }
                }
            }
        }
    }
}
