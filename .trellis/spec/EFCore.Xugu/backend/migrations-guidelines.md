# Migrations guidelines

## When this applies

Work under `src/EFCore.Xugu/Migrations/` and related Metadata conventions that affect DDL.

## Local pattern

- DDL: `Migrations/XuguMigrationsSqlGenerator.cs` — IDENTITY columns, indexes, sequences, renames.
- History: `XuguHistoryRepository`
- Differ / migrator: `XuguMigrationsModelDiffer`, `XuguMigrator`
- Unsupported operations throw `NotSupportedException` / `InvalidOperationException` with `XuguStrings.*` (filtered indexes, some identity renames, sequence restart, etc.).

Identity / compatible mode interaction is documented in dialect contract and system parameters (`IDENTITY_MODE`, `COMPATIBLE_MODE`). Do not invent `AUTO_INCREMENT` clauses from MySQL.

## Tests

- `test/EFCore.Xugu.Tests.Unit/MigrationColumnSqlTests.cs`
- `MigrationIndexSqlTests.cs`
- `MigrationSequenceSqlTests.cs`

## Anti-patterns

- Copying Pomelo migration SQL fragments.
- Generating idempotent scripts or operations that the generator explicitly rejects — fix the model or document OUT OF SCOPE instead of silent no-op.
- Changing history table SQL without checking Xugu DDL docs.
