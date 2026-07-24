# Implement: Full Functional release acceptance remediation

## Preconditions

- Read `prd.md` + `design.md` in this task.
- Before coding each wave: `trellis-before-dev` for EFCore.Xugu layers touched.
- Dialect: Xugu docs only; no `external/` edits.
- **Local commit after each wave; do not `git push` until Wave 6.**

## Wave checklist

### Wave 1 — Hygiene

- [x] Enumerate remaining APPLY/LATERAL fails from revalidation TRX (expect ~12).
- [x] Add Skip / AssertApplyNotSupported on those overrides only.
- [x] Change pack story: do not embed conflicting local `xugusql.dll` for `UseLocalXuguDriver=false` packs (or equivalent unified policy).
- [x] UTF-8 cleanup on `docs/contracts/*.md` mojibake.
- [x] Validate: APPLY filter 0 FAIL; `dotnet pack -p:UseLocalXuguDriver=false`; Unit green.
- [x] **Local commit** (no push).

### Wave 2 — E9016

- [x] Reproduce `E9016` (e.g. NonSharedModelBulkUpdates `BlogsPart1`) under isolation.
- [x] Fix store naming / cleanup / Dispose / Ensure path.
- [x] Validate: offline model assert (`BlogsPart1` → `EF_*_BLOGSPART1`); Shared/Functional build green; Unit 283 green. Live E9016 filter deferred (local XuguDB :5138 down this session).
- [ ] **Local commit**.

### Wave 3 — E17010

- [x] Confirm dialect rules via docs map (`from.md` etc.).
- [x] For each cluster: rewrite SQL **or** Skip + LIMITATIONS anchor.
- [x] Validate: E17010 methods Skip-attributed + LIMITATIONS/contract anchors; Functional build green (live filter deferred, DB down).
- [ ] **Local commit**.

### Wave 4 — E19132

- [x] Cluster SQL from logs/TRX; fix generators per cluster.
- [x] Skip only with doc proof.
- [x] Validate: GenerateValues fix builds; residual E19132 clusters Skip+LIMITATIONS (live filter deferred, DB down).
- [ ] **Local commit**.

### Wave 5 — LINQ / semantics / residuals

- [ ] Drive from remaining FAIL taxonomy (LINQ, result/exception, baselines, E5021, materialization, other).
- [ ] Fix translators / mappings / baselines; keep Unit/Integration green.
- [ ] Validate: residual Functional FAIL → 0 on focused then broadening filters.
- [ ] **Local commit**.

### Wave 6 — Full suite + overwrite

- [ ] Independent suite Functional native+compat class-isolated full run → **0 FAIL**.
- [ ] Update RELEASE-SCOPE / LIMITATIONS / CHANGELOG to full capability (definition A).
- [ ] Pack 9.0.0; verify description + native policy.
- [ ] **First push** of all local commits; force-move `v9.0.0`; recreate GitHub Release assets.
- [ ] Do not publish nuget.org unless user asks.

## Validation commands (templates)

```powershell
# Unit
dotnet test test/EFCore.Xugu.Tests.Unit -c Release

# Integration (live DB)
dotnet test test/EFCore.Xugu.Tests.Integration -c Release

# Wave-scoped Functional filters — refine per wave from TRX class names
dotnet test test/EFCore.Xugu.Tests.Functional -c Release --filter "..."

# Final independent suite
cd E:\Work\Tests\entityframeworkcore-xugu-release-test
.\run-tests.ps1   # full; use class-isolated Functional path as in revalidation-20260724
```

## Risky files

- `src/EFCore.Xugu/Query/**` (E19132 / E17010 / LINQ)
- `src/EFCore.Xugu/EFCore.Xugu.csproj` (native pack)
- `test/EFCore.Xugu.Tests.Shared/**` (E9016)
- Functional override surface area (Skip hygiene)

## Rollback

- Local commits can be reset before Wave 6 push.
- After overwrite, recover previous `v9.0.0` only via prior tag object / Release assets if retained.

## Before `task.py start`

- [ ] User reviewed `prd.md` / `design.md` / `implement.md`
- [ ] `implement.jsonl` / `check.jsonl` curated (non-seed)
- [ ] PRD convergence pass done
