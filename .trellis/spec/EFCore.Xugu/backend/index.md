# EFCore.Xugu — backend specs

NuGet package `Microsoft.EntityFrameworkCore.Xugu` for XuguDB. Single implementation package under `src/EFCore.Xugu/` (no separate frontend layer).

## Read first

| Doc | When |
|-----|------|
| [Xugu provider constraints](../../guides/xugu-provider-constraints.md) | Any dialect, type mapping, or provider change |
| [SQL dialect contract](../../../../docs/contracts/sql-dialect.contract.md) | Writing or changing SQL strings |
| [Stub / exclusion contract](../../../../docs/contracts/stub-and-exclusion.contract.md) | Capability missing from Xugu docs |
| [XuguDB docs map](../../../../docs/references/xugudb-docs-map.md) | Locating official dialect pages |
| [ADO driver contract](../../../../docs/contracts/ado-driver-contract.md) | Reader/writer / driver quirks |

## Spec index

| Spec | Topic |
|------|-------|
| [directory-structure.md](./directory-structure.md) | Folder ownership under `src/EFCore.Xugu/` |
| [naming-and-namespaces.md](./naming-and-namespaces.md) | Namespaces, prefixes, where extensions live |
| [service-registration.md](./service-registration.md) | `UseXugu` → options → DI registration |
| [query-guidelines.md](./query-guidelines.md) | Translators, visitors, SQL generator |
| [storage-and-type-mapping.md](./storage-and-type-mapping.md) | Type mappings, connection, retry |
| [update-guidelines.md](./update-guidelines.md) | DML SQL, batches, identity retrieval |
| [migrations-guidelines.md](./migrations-guidelines.md) | DDL generation, unsupported ops |
| [scaffolding-and-design.md](./scaffolding-and-design.md) | Design-time + reverse engineering |
| [error-handling.md](./error-handling.md) | `XuguStrings.resx`, XGCI hints |
| [testing-guidelines.md](./testing-guidelines.md) | Unit / integration / functional layout |
| [quality-guidelines.md](./quality-guidelines.md) | Verify gates, contracts, anti-patterns |
| [logging-guidelines.md](./logging-guidelines.md) | Diagnostics definitions |

## Local verification

```powershell
scripts/verify.ps1
scripts/verify-module.ps1
```

Do not edit `external/` (Pomelo, csharp-driver).
