# Testing guidelines

User-facing overview: [docs/TESTING.md](../../../../docs/TESTING.md), [docs/TEST-ARCHITECTURE.md](../../../../docs/TEST-ARCHITECTURE.md).

## Projects

| Project | Role | DB? |
|---------|------|-----|
| `test/EFCore.Xugu.Tests.Unit` | L1 — translators, mappings, migration SQL, DI | No |
| `test/EFCore.Xugu.Tests.Integration` | L2 — CRUD, ExecuteUpdate/Delete, concurrency, JSON | Yes |
| `test/EFCore.Xugu.Tests.Functional` | Spec matrix (`*XuguTest` / fixtures) | Yes |
| `test/EFCore.Xugu.Tests.Shared` | `XuguTestStore`, fixtures, dialect helpers | — |
| `test/EFCore.Xugu.Tests` | README-only stub project | — |
| `test/integration-sample` | L3 experiential sample | Yes |

## Patterns to copy

- Unit SQL assertions: `TranslatorSqlTests`, `NativeSqlBaselineTests`, `Migration*SqlTests`
- Shared store: `Tests.Shared/TestUtilities/XuguTestStore.cs`, `XuguTestStoreFactory`
- Functional fixtures: `Query/*XuguFixture.cs` + `*XuguTest.cs`
- Skip / capability gates: follow stub-and-exclusion + `SupportedServerVersionConditionAttribute` in Shared

## Local gates

```powershell
scripts/run-unit-gate.ps1 -Configuration Release
scripts/run-native-gate.ps1 -Configuration Release -MaxAttempts 3
scripts/run-compat-gate.ps1 -Configuration Release -MaxAttempts 3
scripts/verify.ps1 -RunTests   # when DB available
```

Start DB helper: `scripts/start-xugudb.ps1`. Connection via `XUGU_CONNECTION_STRING`.

## Anti-patterns

- Referencing deleted `harness/scripts/*` paths in new tests or docs.
- Skipping dialect contract updates when a test permanently changes expected SQL.
- Adding Functional tests that assume MySQL-only fixtures without Xugu fixture overrides.

## Functional shared-store rules

- Tables are isolated by **table name prefix** on shared SYSTEM (`XuguTestStoreFactory.FormatTablePrefix` / `FormatTableName`).
- FromSql / raw SQL in Functional tests **must** use prefixed names — bare `Weapons`/`Entities1` → E5021 under shared DB.
- Prefer result assertions over brittle AssertSql when baselines still assume unprefixed identifiers.
- Skip attributes need **concrete reason** (server code, doc gap, residual semantics) — not empty Skip.
- Class-isolated matrix evidence lives under `artifacts/live-db/` (gitignored); summarize in task `implement.md`.
