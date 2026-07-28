# Journal - LPAG (Part 1)

> AI development session journal
> Started: 2026-07-21

---



## Session 1: Wave A release acceptance for 9.0.0

**Date**: 2026-07-23
**Task**: Wave A release acceptance for 9.0.0
**Package**: EFCore.Xugu
**Branch**: `phase-13-production-hardening`

### Summary

Closed Wave A gaps: TimeOnly Unit, ALL_* catalog, UTF-8 Northwind, APPLY skips, pack/docs; independent suite PASS; committed b04fbc4; overwritten GitHub Release v9.0.0.

### Main Changes

- Detailed change bullets were not supplied; see the summary above.

### Git Commits

| Hash | Message |
|------|---------|
| `b04fbc4` | (see git log) |

### Testing

- Validation was not recorded for this session.

### Status

[OK] **Completed**

### Next Steps

- None - task complete

## 2026-07-28 — Wave5 workspace freeze + docs/Trellis sync

- Cleaned scratch (`.tmp/*`), gitignore `.tmp/` + `.trellis/spec/external/`.
- Kept Wave5 provider/test working tree (primitive collections, translators, Functional Skips).
- Aligned user docs: RELEASE-SCOPE delivery bar, LIMITATIONS primitive/residual, CHANGELOG Unreleased.
- Repaired `sql-dialect.contract.md` / stub contract **headers** (historical U+FFFD remains in some table bodies — full rewrite needs clean source).
- Trellis backend specs: query / storage / testing Wave5 rules; task implement Wave5e; omp hooks ported.
- Public claim remains **9.0.0 Wave A Windows trialable**, not full Functional 0 FAIL.

## 2026-07-28 — Garbled docs: rewrite or delete

- git history for sql-dialect / stub-and-exclusion / test-parity-matrix / pomelo-file-map: **no clean UTF-8 ancestor** (corrupt since harness→docs migrate).
- **Rewrote** `docs/contracts/sql-dialect.contract.md` and `stub-and-exclusion.contract.md` clean (9.0.0 + Wave5 facts).
- **Deleted** `docs/references/test-parity-matrix.md`, `pomelo-file-map.md`; retargeted links; fixed verify scripts.
- Wave6 still deferred.

## 2026-07-28 — Merge to release/9.0.0 and republish v9.0.0

- Fast-forwarded `release/9.0.0` to tip `f2f4628` (from phase-13).
- Force-moved tag `v9.0.0`; recreated GitHub Release assets (nupkg/snupkg).
- **Primary development branch is now `release/9.0.0` only.**
- nuget.org not pushed.

