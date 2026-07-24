using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

/// <summary>
/// Applies per-store table/view prefixes for EF Specification Tests hosted against shared SYSTEM database.
/// Also renames entity-splitting / table-mapping fragments (e.g. SplitToTable "BlogsPart1") so DROP/CREATE
/// cleanup by store prefix can isolate NonShared suites and avoid E9016 collisions.
/// </summary>
public static class XuguTestModelExtensions
{
    public static void ApplyTablePrefix(ModelBuilder modelBuilder, string storeName)
    {
        var prefix = XuguTestStoreFactory.Instance.FormatTablePrefix(storeName);

        // TPC: materialize concrete table names from declared annotations (self or nearest abstract
        // base) BEFORE clearing abstract TPC types.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.GetMappingStrategy() != RelationalAnnotationNames.TpcMappingStrategy
                || entityType.IsAbstract())
            {
                continue;
            }

            var chosen = GetDeclaredTableName(entityType);
            if (chosen is null)
            {
                for (var baseType = entityType.BaseType;
                     baseType is not null && baseType.IsAbstract();
                     baseType = baseType.BaseType)
                {
                    chosen = GetDeclaredTableName(baseType);
                    if (chosen is not null)
                    {
                        break;
                    }
                }
            }

            entityType.SetTableName(chosen ?? entityType.ClrType.Name);
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsAbstract()
                && entityType.GetMappingStrategy() == RelationalAnnotationNames.TpcMappingStrategy)
            {
                entityType.SetTableName(null);
                continue;
            }

            // TPH derived types must share the root table. Conventions may assign a distinct
            // TableName (e.g. Lilt); remove it so only the root is prefixed.
            if (IsTphDerived(entityType))
            {
                entityType.RemoveAnnotation(RelationalAnnotationNames.TableName);
                entityType.RemoveAnnotation(RelationalAnnotationNames.Schema);
                continue;
            }

            var tableName = entityType.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                entityType.SetTableName(PrefixIfNeeded(tableName, prefix));
            }

            var viewName = entityType.GetViewName();
            if (!string.IsNullOrEmpty(viewName) && entityType.BaseType is null)
            {
                entityType.SetViewName(PrefixIfNeeded(viewName, prefix));
            }

            PrefixMappingFragments(entityType, prefix);
        }
    }

    private static void PrefixMappingFragments(IMutableEntityType entityType, string prefix)
    {
        // Materialize first — Remove/GetOrCreate mutates the fragment collection.
        var fragments = entityType.GetMappingFragments(StoreObjectType.Table).ToList();
        if (fragments.Count == 0)
        {
            return;
        }

        foreach (var fragment in fragments)
        {
            var oldStoreObject = fragment.StoreObject;
            var oldName = oldStoreObject.Name;
            if (string.IsNullOrEmpty(oldName))
            {
                continue;
            }

            var newName = PrefixIfNeeded(oldName, prefix);
            if (string.Equals(oldName, newName, StringComparison.Ordinal))
            {
                continue;
            }

            var newStoreObject = StoreObjectIdentifier.Table(newName, oldStoreObject.Schema);
            var excluded = fragment.IsTableExcludedFromMigrations;

            // Preserve per-table property overrides (column names on the split fragment).
            var propertyMoves = new List<(IMutableProperty Property, string? ColumnName)>();
            foreach (var property in entityType.GetProperties())
            {
                var overrides = property.FindOverrides(in oldStoreObject);
                if (overrides is null)
                {
                    continue;
                }

                propertyMoves.Add((property, property.GetColumnName(in oldStoreObject)));
            }

            entityType.RemoveMappingFragment(in oldStoreObject);
            var created = entityType.GetOrCreateMappingFragment(in newStoreObject);
            if (excluded is not null)
            {
                created.IsTableExcludedFromMigrations = excluded;
            }

            foreach (var (property, columnName) in propertyMoves)
            {
                property.RemoveOverrides(in oldStoreObject);
                if (!string.IsNullOrEmpty(columnName))
                {
                    property.SetColumnName(columnName, in newStoreObject);
                }
            }
        }
    }

    private static string PrefixIfNeeded(string name, string prefix)
    {
        var normalized = name.ToUpperInvariant();
        if (!normalized.StartsWith(prefix, StringComparison.Ordinal))
        {
            normalized = prefix + normalized;
        }

        return normalized;
    }

    private static bool IsTphDerived(IMutableEntityType entityType)
    {
        var root = entityType.GetRootType();
        if (ReferenceEquals(root, entityType))
        {
            return false;
        }

        var strategy = root.GetMappingStrategy();
        if (strategy == RelationalAnnotationNames.TpcMappingStrategy
            || strategy == RelationalAnnotationNames.TptMappingStrategy)
        {
            return false;
        }

        // Default inheritance with a discriminator is TPH.
        return root.FindDiscriminatorProperty() is not null
            || strategy == RelationalAnnotationNames.TphMappingStrategy;
    }

    private static string? GetDeclaredTableName(IMutableEntityType entityType)
        => entityType.FindAnnotation(RelationalAnnotationNames.TableName)?.Value as string;
}
