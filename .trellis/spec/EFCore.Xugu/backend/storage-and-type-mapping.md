# Storage and type mapping

## When this applies

Work under `src/EFCore.Xugu/Storage/` — CLR↔store mappings, connection open behavior, database creator, execution strategy / transient errors.

## Type mapping pattern

Hub: `Storage/Internal/XuguTypeMappingSource.cs`

- Maintain store-type and CLR-type lookup tables (`INTEGER`, `BIGINT`, `VARCHAR`, `JSON`, `GUID`, `BLOB`, `DATETIME WITH TIME ZONE`, …).
- Each mapping is a `Xugu*TypeMapping` with a `Default` singleton.
- Driver quirks are adapted **inside the provider** (do not patch `external/csharp-driver`).

Representative files:

| File | Pattern |
|------|---------|
| `XuguByteArrayTypeMapping.cs` | `XGBlob` ↔ `byte[]` |
| `XuguStringTypeMapping.cs` | Avoid wrong FixedLength DbType |
| `XuguUIntTypeMapping.cs` / `XuguULongTypeMapping.cs` / `XuguSByteTypeMapping.cs` / `XuguFloatTypeMapping.cs` | Binding workarounds when ADO DbType is incomplete |
| `XuguDateOnlyTypeMapping.cs` / `XuguTimeOnlyTypeMapping.cs` | Temporal via driver-friendly binding |
| `XuguTemporalValueConverters.cs` | Shared temporal converters |
| `XuguGuidTypeMapping.cs` | GUID store handling |
| `XuguPrimitiveCollectionTypeMapping.cs` | Parameter primitive collections as JSON text; DateTime ISO normalize before DATETIME |

Driver behavior authority: [docs/contracts/ado-driver-contract.md](../../../../docs/contracts/ado-driver-contract.md), [docs/references/csharp-driver-analysis.md](../../../../docs/references/csharp-driver-analysis.md).

## Connection and retry

- `XuguRelationalConnection` — open path may issue `SET compatible_mode` based on options.
- `XuguRetryingExecutionStrategy` + `XuguTransientExceptionDetector` — retry classification; notes in [docs/references/retrying-execution-strategy.md](../../../../docs/references/retrying-execution-strategy.md).
- `XuguDatabaseCreator` — Create/Delete databases throw `NotSupportedException` via `XuguStrings` (server-managed DB lifecycle).

## Tests

- `test/EFCore.Xugu.Tests.Unit/TypeMappingSourceTests.cs`
- Integration: `QueryTests.cs`, `StoreGeneratedTests.cs` under `Tests.Integration`

## Anti-patterns

- Assuming MySQL `TINYINT(1)` / `DATETIME(6)` store names.
- Encoding absolute machine paths to `csharp-driver-v3.3.4-cyj` in new code; use ProjectReference / `UseLocalXuguDriver` / NuGet as documented in csproj.
- Changing native DLL discovery without updating `NativeAssets.props` behavior.
