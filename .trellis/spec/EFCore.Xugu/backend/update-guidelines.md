# Update guidelines

## When this applies

Changes under `src/EFCore.Xugu/Update/` — INSERT/UPDATE/DELETE SQL, batching, store-generated values.

## Local pattern

- SQL generation: `Update/Internal/XuguUpdateSqlGenerator.cs` (`IUpdateSqlGenerator` / `IXuguUpdateSqlGenerator`).
- Batching: `XuguModificationCommandBatch` + factory — respect provider batch limits and Xugu command semantics.
- Identity / returning: prefer patterns already validated against the driver contract. Xugu supports `RETURNING` in SQL, but current XuguClient often does not expose returning rows via `DbDataReader` — provider uses `LAST_INSERT_ID()` style retrieval where needed (see [docs/contracts/ado-driver-contract.md](../../../../docs/contracts/ado-driver-contract.md) and [docs/LIMITATIONS.md](../../../../docs/LIMITATIONS.md)).
- Bulk `ExecuteUpdate` / `ExecuteDelete` must stay aligned with Query SQL generator and dialect contract.

## Tests

- `test/EFCore.Xugu.Tests.Unit/XuguUpdateSqlGeneratorTests.cs`
- Integration: `ExecuteDeleteTests.cs`, `ExecuteUpdateTests.cs`, `CrudTests.cs`, `UpdatesMatrixTests.cs`

## Anti-patterns

- Assuming MySQL `ON DUPLICATE KEY` or `LAST_INSERT_ID` semantics without Xugu docs + contract confirmation.
- Ignoring `RecordsAffected` / concurrency token behavior documented in LIMITATIONS and decision notes.
- Emitting `RETURNING` and reading it as a result set without verifying driver `FieldCount` behavior.
