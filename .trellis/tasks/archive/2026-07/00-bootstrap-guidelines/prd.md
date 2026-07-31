# Bootstrap Task: Fill Project Development Guidelines

Populate `.trellis/spec/` with real conventions for this repository so future Trellis sessions follow Xugu EF Core patterns.

## Status

- [x] Fill guidelines for **EFCore.Xugu** (`src/EFCore.Xugu`) — package of record
- [x] Document hard constraints (dialect authority, contracts, verify gates)
- [x] Add code examples / file-path references in backend specs
- [x] Correct Trellis package config (default = `EFCore.Xugu`, not Pomelo submodule)
- [x] Mark `external/*` as **read-only reference** — do **not** maintain frontend/backend template specs for Pomelo or csharp-driver

## Out of scope

- Filling `.trellis/spec/external/Pomelo*` or `external/csharp-driver` template trees (deleted; submodules stay read-only)
- Product dialect feature work
- NuGet.org push (separate release task)

## Acceptance criteria

- [x] `.trellis/spec/EFCore.Xugu/backend/` has practical guidelines with real paths
- [x] `.trellis/spec/guides/xugu-provider-constraints.md` matches source namespaces
- [x] No template placeholders remain in project-owned specs
- [x] Spec changes committed on the development branch

## Related

Release / version republish is a separate task after this bootstrap is archived.
