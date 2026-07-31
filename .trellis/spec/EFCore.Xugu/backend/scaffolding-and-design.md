# Scaffolding and design-time

## When this applies

`src/EFCore.Xugu/Scaffolding/` and `src/EFCore.Xugu/Design/`.

## Local pattern

1. `Design/Internal/XuguDesignTimeServices` registers `AddEntityFrameworkXugu()` plus scaffolding services for `dotnet ef`.
2. `Scaffolding/Internal/XuguDatabaseModelFactory` reads catalog via Xugu `ALL_*` views (`ALL_TABLES` / `ALL_COLUMNS` / `ALL_INDEXES` / `ALL_CONSTRAINTS` — ordinary-user privilege; not MySQL `INFORMATION_SCHEMA`, not `DBA_*` / `SYS_*`).
3. `XuguCodeGenerator` emits `UseXugu(...)` for scaffolded contexts.
4. Store types must round-trip through `XuguTypeMappingSource` names.

Lineage checks: `scripts/verify-source-lineage.ps1` fails on MySQL-only scaffolding patterns (`INFORMATION_SCHEMA`, `AUTO_INCREMENT`, MySqlConnector imports).

## Tests

- `test/EFCore.Xugu.Tests.Unit/ScaffoldingStoreTypeTests.cs`
- `DesignTimeExtensionTests.cs`

## Anti-patterns

- Scaffolding against MySQL information_schema SQL.
- Generating `UseMySql` or Pomelo namespaces in code templates.
- Skipping design-time registration when adding new scaffolding services.
