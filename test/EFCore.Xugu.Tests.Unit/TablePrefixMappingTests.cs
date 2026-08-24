using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Offline regression for the shared-store table-prefix invariant backing raw-SQL (FromSql) tests:
/// the physical table name produced by <see cref="XuguTestModelExtensions.ApplyTablePrefix"/> must
/// equal <c>XuguTestStoreFactory.Instance.FormatTableName(store, logical)</c> — the name raw SQL in
/// FromSql overrides must reference (OwnedQuery FromSql E5021 regression, 2026-08-24).
/// </summary>
public class TablePrefixMappingTests
{
    [Theory]
    [InlineData("OwnedQueryXuguTest", "OwnedPerson")]
    [InlineData("GearsOfWarFromSqlQueryXGTest", "Weapons")]
    [InlineData("store with spaces", "MixedCaseTable")]
    public void ApplyTablePrefix_matches_FormatTableName(string storeName, string logicalTableName)
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<EntityWithTable>(b => b.ToTable(logicalTableName));

        XuguTestModelExtensions.ApplyTablePrefix(modelBuilder, storeName);

        var entityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(EntityWithTable))!;
        Assert.Equal(
            XuguTestStoreFactory.Instance.FormatTableName(storeName, logicalTableName),
            entityType.GetTableName());
    }

    [Fact]
    public void FormatTableName_prefixes_and_uppercases()
    {
        var name = XuguTestStoreFactory.Instance.FormatTableName("OwnedQueryXuguTest", "OwnedPerson");

        Assert.StartsWith("EF_", name);
        Assert.EndsWith("_OWNEDPERSON", name);
        Assert.Equal(
            XuguTestStoreFactory.Instance.FormatTablePrefix("OwnedQueryXuguTest") + "OWNEDPERSON",
            name);
    }

    [Fact]
    public void ApplyTablePrefix_is_idempotent_for_already_prefixed_names()
    {
        var prefix = XuguTestStoreFactory.Instance.FormatTablePrefix("OwnedQueryXuguTest");
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<EntityWithTable>(b => b.ToTable(prefix + "OWNEDPERSON"));

        XuguTestModelExtensions.ApplyTablePrefix(modelBuilder, "OwnedQueryXuguTest");

        var entityType = (IEntityType)modelBuilder.Model.FindEntityType(typeof(EntityWithTable))!;
        Assert.Equal(prefix + "OWNEDPERSON", entityType.GetTableName());
    }

    private class EntityWithTable
    {
        public int Id { get; set; }
    }
}
