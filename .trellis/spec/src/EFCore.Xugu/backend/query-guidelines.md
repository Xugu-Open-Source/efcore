# Query guidelines

## When this applies

Any change under `src/EFCore.Xugu/Query/` — method/member translators, SQL translating visitors, `XuguQuerySqlGenerator`, query compilation hooks.

## Local pattern

1. Confirm the function/operator in Xugu official docs via [docs/references/xugudb-docs-map.md](../../../../docs/references/xugudb-docs-map.md).
2. Record or update the mapping in [docs/contracts/sql-dialect.contract.md](../../../../docs/contracts/sql-dialect.contract.md).
3. Implement translation in the matching translator under `Query/ExpressionTranslators/Internal/` (or visitor under `Query/ExpressionVisitors/Internal/`).
4. Emit SQL only through `XuguQuerySqlGenerator` / `ISqlExpressionFactory` — no stringly MySQL leftovers.
5. Cover with unit SQL baselines and/or Functional `*XuguTest` overrides.

## Hub files

| Concern | Path |
|---------|------|
| SQL rendering | `Query/Internal/XuguQuerySqlGenerator.cs` |
| Method calls | `Query/Internal/XuguMethodCallTranslatorProvider.cs` |
| Members | `Query/Internal/XuguMemberTranslatorProvider.cs` |
| DateTime / byte[] / etc. | `Query/ExpressionTranslators/Internal/Xugu*Translator.cs` |
| APPLY / unsupported shapes | throw via `XuguStrings` (e.g. `ApplyNotSupported`) |

## Tests to mirror

- `test/EFCore.Xugu.Tests.Unit/TranslatorSqlTests.cs`
- `test/EFCore.Xugu.Tests.Unit/NativeSqlBaselineTests.cs`
- `test/EFCore.Xugu.Tests.Unit/CompatibleModeSqlUnitTests.cs`
- Functional: `test/EFCore.Xugu.Tests.Functional/Query/NullSemanticsQueryXuguTest.cs`, `GearsOfWarQueryXuguTest.cs`

## Anti-patterns

- Copying Pomelo translator bodies that emit MySQL functions (`DATE_ADD` quirks, backticks, etc.) without Xugu doc proof.
- Silent client eval for functions that should stub/skip per stub-and-exclusion contract.
- Hardcoding `LIMIT`/`OFFSET` or MySQL-only paging idioms without checking Xugu dialect contract.

## Primitive collections & paging (Wave5)

| Concern | Rule |
|---------|------|
| Parameter `Contains` / membership | Prefer guarded JSON scalar predicates in `XuguQuerySqlGenerator.GenerateIn`; serialize via `XuguPrimitiveCollectionTypeMapping`. |
| Row-set expand (`JSON_TABLE`) | **Do not invent**. Official docs lack JSON_TABLE; probe → E19132. Skip Functional cases with evidence. |
| Tree type | `Query/Expressions/Internal/XuguPrimitiveCollectionTableExpression.cs` when a collection table shape is required. |
| `LIMIT`/`OFFSET` | Integerize constants; `CAST` non-constants (`GenerateIntegerLimitOffsetValue`). |
| `TimeSpan.Milliseconds` | Extract as high-precision then divide; **Convert to int** before materialization (driver E34412). |
| `string` First/LastOrDefault | `SUBSTRING` — treat string as char sequence. |
| Inline VALUES | `SELECT … UNION ALL SELECT …` only (not `UNION ALL VALUES`). |

## Anti-patterns (Wave5)

- Emitting `IN (SELECT …)` over JSON parameter collections that Xugu evaluates incorrectly.
- AssertSql baselines hard-coding unprefixed table names under shared SYSTEM + `FormatTablePrefix` isolation.
- Closing Wave5 with residual server codes APPLY/E17010/E19132 still as FAIL (must be fix or evidenced Skip).
