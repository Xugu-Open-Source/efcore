using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query;

public class TPCManyToManyQueryXGFixture : TPCManyToManyQueryRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => XGTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        // We default to mapping DateTime to 'timestamp with time zone', but the seeding data has Unspecified DateTimes which aren't
        // supported.
        // modelBuilder.Entity<EntityCompositeKey>().Property(e => e.Key3).HasColumnType("timestamp without time zone");
        // modelBuilder.Entity<JoinCompositeKeyToLeaf>().Property(e => e.CompositeId3).HasColumnType("timestamp without time zone");
        // modelBuilder.Entity<UnidirectionalEntityCompositeKey>().Property(e => e.Key3).HasColumnType("timestamp without time zone");
        // modelBuilder.Entity<UnidirectionalJoinOneSelfPayload>().Property(e => e.Payload).HasColumnType("timestamp without time zone");
        // modelBuilder.Entity<JoinOneSelfPayload>().Property(e => e.Payload).HasColumnType("timestamp without time zone");
        // modelBuilder.Entity<JoinThreeToCompositeKeyFull>().Property(e => e.CompositeId3).HasColumnType("timestamp without time zone");
    }
}
