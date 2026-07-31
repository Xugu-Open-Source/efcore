# Quality guidelines

## Before finishing a change

1. Dialect/docs check — [xugu-provider-constraints.md](../../guides/xugu-provider-constraints.md).
2. Update [docs/contracts/](../../../../docs/contracts/) when behavior or SQL changes.
3. Run:

```powershell
scripts/verify-source-lineage.ps1
scripts/verify.ps1
```

4. Add or extend Unit tests for SQL / mapping / messages; Integration when runtime semantics change.

## Anti-patterns (must document in review)

| Anti-pattern | Why |
|--------------|-----|
| MySQL dialect leakage (`AUTO_INCREMENT`, MySQL function names, `INFORMATION_SCHEMA` scaffolding) | Xugu docs are authority; lineage script fails |
| Using deleted `harness/` paths as authority | Contracts/scripts live in `docs/` and `scripts/` |
| Hardcoding developer-machine driver paths | Use `UseLocalXuguDriver` / NuGet / submodule |
| Editing `external/Pomelo*` or `external/csharp-driver` | Read-only references |
| Inline user-visible exception strings | Use `XuguStrings.resx` |
| Wrong implementation namespace `Xugu.EntityFrameworkCore.Xugu` | Actual is `Microsoft.EntityFrameworkCore.Xugu.*` |
| Treating `COMPATIBLE_MODE=MYSQL` as full MySQL dialect | Identifier folding only |

## Cross-layer thinking

When a change spans Query + Storage + Update (e.g. new CLR type), update mapping, translators, migrations, scaffolding, and contracts together. See [cross-layer-thinking-guide.md](../../guides/cross-layer-thinking-guide.md).
