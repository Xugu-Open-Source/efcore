# Directory structure

Implementation root: `src/EFCore.Xugu/`. Layout mirrors Pomelo for C# discoverability only — **never copy MySQL SQL from Pomelo**.

## Folder ownership

| Folder | Owns | Hub types |
|--------|------|-----------|
| `Extensions/` | Public Fluent API | `XuguDbContextOptionsBuilderExtensions` (`UseXugu`), `XuguServiceCollectionExtensions` |
| `Infrastructure/` | Options builder, server version, compatible mode | `XuguDbContextOptionsBuilder`, `XuguServerVersion`, `XuguCompatibleMode`, `IXuguOptions` |
| `Internal/` | Singleton options bag | `XuguOptions` |
| `Storage/` | Type mappings, connection, creator, retry | `XuguTypeMappingSource`, `XuguRelationalConnection`, `XuguRetryingExecutionStrategy` |
| `Query/` | LINQ → SQL | `XuguQuerySqlGenerator`, `XuguMethodCallTranslatorProvider`, expression visitors |
| `Update/` | INSERT/UPDATE/DELETE SQL + batches | `XuguUpdateSqlGenerator`, `XuguModificationCommandBatch` |
| `Migrations/` | DDL / history / differ / migrator | `XuguMigrationsSqlGenerator`, `XuguHistoryRepository` |
| `Metadata/` | Conventions, annotations, value-gen strategy | `XuguConventionSetBuilder` |
| `Scaffolding/` | Reverse engineering | `XuguDatabaseModelFactory`, `XuguCodeGenerator` |
| `Design/` | `dotnet ef` design-time DI | `XuguDesignTimeServices` |
| `ValueGeneration/` | Value generator selection | `XuguValueGeneratorSelector` |
| `Diagnostics/` | Logging definition stubs | `XuguLoggingDefinitions` |
| `Properties/` | User-visible strings | `XuguStrings.resx` |

## Rules

- New provider services belong under the matching folder + `.Internal` when they are implementation details.
- Public entry points stay in `Extensions/` with namespace `Microsoft.EntityFrameworkCore` (or `Microsoft.Extensions.DependencyInjection` for DI).
- Pomelo structure reference: read-only tree `external/Pomelo.EntityFrameworkCore.MySql/` (former pomelo-file-map deleted).

## Anti-patterns

- Adding a new top-level folder without a clear EF Core provider layer analogue.
- Putting SQL dialect constants in `Extensions/` or tests-only helpers instead of Query/Storage/Update/Migrations.
