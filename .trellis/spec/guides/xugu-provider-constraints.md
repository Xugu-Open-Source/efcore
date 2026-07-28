# XuguDB EF Core Provider — Hard Constraints

> **All agents must follow these rules before changing SQL dialect, types, or provider behavior.**

## Project goal

Implement `Microsoft.EntityFrameworkCore.Xugu` for XuguDB. **XuguDB official docs are the only SQL dialect authority.** Pomelo/MySQL is **C# architecture reference only** (layout, DI, naming) — not a dialect source and not a migration target.

## Repository layout

```text
src/EFCore.Xugu/          Provider implementation
test/                     Unit / integration / functional tests
docs/contracts/           Living dialect & driver contracts
docs/references/          Docs map, Pomelo file map, driver analysis
scripts/                  Local verify / gates / release helpers
.trellis/                 Trellis workflow (specs, tasks, workspace)
external/                 Read-only submodules (Pomelo, csharp-driver)
```

## Hard constraints (do not violate)

### 1. XuguDB docs are the only SQL dialect authority

Any SQL syntax, function names, data types, DDL/DML must follow:

```text
E:\BaiduSyncdisk\docs\content\
```

Before implementing:

1. Read [docs/references/xugudb-docs-map.md](../../../docs/references/xugudb-docs-map.md)
2. Read [docs/contracts/sql-dialect.contract.md](../../../docs/contracts/sql-dialect.contract.md)
3. If the capability is undocumented, read [docs/contracts/stub-and-exclusion.contract.md](../../../docs/contracts/stub-and-exclusion.contract.md) before stub/skip

Forbidden:

- Inventing SQL from memory or from Pomelo/MySQL alone
- Assuming XuguDB is 100% MySQL-compatible

Document differences in `docs/contracts/sql-dialect.contract.md`.

### 2. Do not modify EF Core core

All provider code lives under `src/EFCore.Xugu/`, depending on `Microsoft.EntityFrameworkCore.Relational` NuGet.

### 3. Do not modify Pomelo sources

`external/Pomelo.EntityFrameworkCore.MySql/` is read-only reference. See [external/Pomelo.EntityFrameworkCore.MySql/](../../../external/Pomelo.EntityFrameworkCore.MySql/).

### 4. Architecture may mirror Pomelo (C# only)

Directory layout, service registration, and naming may follow Pomelo. **Pomelo never supplies SQL dialect strings.**

### 5. Contracts first

- Read `docs/contracts/` before work
- Update contracts when dialect/API behavior changes

### 6. Local verification

Before finishing work, run:

```powershell
scripts/verify-module.ps1
# or fuller:
scripts/verify.ps1
```

Failures must be fixed; do not mark work done while verify fails.

### 7. User-visible errors use .resx

Messages go through `src/EFCore.Xugu/Properties/XuguStrings.resx`.

## Naming

| Kind | Convention | Example |
|------|------------|---------|
| Public API namespace | `Microsoft.EntityFrameworkCore` | `UseXugu()` |
| Implementation namespace | `Microsoft.EntityFrameworkCore.Xugu.*` | e.g. `...Xugu.Query.Internal` |
| Class prefix | `Xugu` | `XuguQuerySqlGenerator` |
| NuGet package | `Microsoft.EntityFrameworkCore.Xugu` | |
| Connection extension | `UseXugu(connectionString)` | |

## Reference priority

1. `E:\BaiduSyncdisk\docs\content\` — SQL dialect (highest)
2. `docs/contracts/` — confirmed project contracts
3. `external/Pomelo.EntityFrameworkCore.MySql/` — C# structure only
4. `E:\Work\efcore\` — EF Core base / official providers
5. `E:\Work\docs\efcore\` — architecture notes

## Related docs

- Driver contract: [docs/contracts/ado-driver-contract.md](../../../docs/contracts/ado-driver-contract.md)
- Driver analysis: [docs/references/csharp-driver-analysis.md](../../../docs/references/csharp-driver-analysis.md)
- Retry strategy: [docs/references/retrying-execution-strategy.md](../../../docs/references/retrying-execution-strategy.md)
- Testing: [docs/TESTING.md](../../../docs/TESTING.md)
