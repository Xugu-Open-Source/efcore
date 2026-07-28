# Microsoft.EntityFrameworkCore.Xugu — Release Guide

> **Current release**: **9.0.0** (`v9.0.0`) — EF Core **9.0.x** aligned（Wave A + Wave5 同版覆盖）  
> **Development / release branch**: **`release/9.0.0`**（后续只在此分支开发；历史 `phase-13-production-hardening` 已并入）  
> **Package ID**: `Microsoft.EntityFrameworkCore.Xugu`

---

## Versioning policy（与 EF Core 对齐）

| 规则 | 说明 |
|------|------|
| **主.次 = EF Core 主.次** | `Directory.Packages.props` 中 `EFCoreVersion` 为 `9.0.0` 时，包版本为 `9.0.x` |
| **修订号** | 可仅因 Provider 修复递增（如 `9.0.1`），仍绑定同一 EF Core 9.0 带 |
| **升级 EF** | 升到 EF Core `9.1.x` / `10.0.x` 时，同步升包版本主.次 |
| **历史** | GitHub 上 `v1.x`–`v3.x` 为旧编号；自 **`v9.0.0`** 起采用本策略（对齐 Pomelo 惯例） |

`Version.props` 的 `VersionPrefix` 必须与当前目标 EF 带一致；发版 checklist 须核对 `EFCoreVersion`。

---

## Quick reference

| Item | Value |
|------|-------|
| Target framework | .NET 9.0 (`net9.0`) |
| EF Core | **9.0.0** (`Directory.Packages.props` → `EFCoreVersion`) |
| Package version | **9.0.0**（与 EF 主.次对齐） |
| Driver (published) | NuGet [Xuguclient](https://www.nuget.org/packages/Xuguclient) (`UseLocalXuguDriver=false`) |
| Platform GA | **Windows** (Linux RID signed-off blocked — see [LIMITATIONS.md](docs/LIMITATIONS.md)) |

---

## 1. Verify before release

From repository root (Windows, .NET SDK 9.0+):

```powershell
$env:PATH = "C:\Program Files\dotnet;$env:PATH"

dotnet build Xugu.EFCore.Xugu.sln -c Release
dotnet test Xugu.EFCore.Xugu.sln -c Release --no-build
```

With XuguDB instance and harness (development monorepo):

```powershell
scripts/verify.ps1
scripts/test-nuget-pack.ps1
scripts/run-integration-smoke.ps1   # requires DB
```

---

## 2. Pack NuGet (local)

```powershell
# Development monorepo (uses harness script)
scripts/publish-nuget.ps1 -Pack

# Public mirror / minimal checkout (no harness)
dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release `
  -o artifacts/ `
  -p:UseLocalXuguDriver=false
```

Output: `artifacts/Microsoft.EntityFrameworkCore.Xugu.9.0.0.nupkg` (+ `.snupkg` if symbols enabled).

---

## 3. Push to NuGet feed

**Dry-run by default** — `publish-nuget.ps1` never pushes unless `-Push` is explicit.

```powershell
$env:GITLAB_NUGET_FEED_URL = "https://your-feed/v3/index.json"
$env:GITLAB_NUGET_API_KEY = "<api-key>"

scripts/publish-nuget.ps1 -Pack -Push
```

Or manually:

```powershell
dotnet nuget push artifacts/Microsoft.EntityFrameworkCore.Xugu.9.0.0.nupkg `
  --source $env:GITLAB_NUGET_FEED_URL `
  --api-key $env:GITLAB_NUGET_API_KEY
```

For [nuget.org](https://www.nuget.org):

```powershell
dotnet nuget push artifacts/Microsoft.EntityFrameworkCore.Xugu.9.0.0.nupkg `
  --source https://api.nuget.org/v3/index.json `
  --api-key <NUGET_API_KEY>
```

---

## 4. Tag workflow

Tags are created on the **development** branch after Release Gate passes:

```powershell
git tag -a v9.0.0 -m "Microsoft.EntityFrameworkCore.Xugu 9.0.0 (EF Core 9.0 aligned)"
git push origin v9.0.0
```

Create or refresh release branch from tag:

```powershell
scripts/prepare-release-branch.ps1 -Version 9.0.0
```

Historical tags (pre–EF-alignment numbering):

```powershell
git tag -a v3.0.1 -m "Microsoft.EntityFrameworkCore.Xugu 3.0.1"
git tag -a v3.0.0 -m "Microsoft.EntityFrameworkCore.Xugu 3.0.0 GA"
```

---

## 5. Push release branch to remote

### Internal GitLab (full monorepo branch)

```powershell
git push -u origin release/9.0.0
```

### Public GitHub / GitLab (minimal mirror — **recommended**)

Do **not** push the full monorepo. Use the mirror script:

```powershell
scripts/prepare-release-branch.ps1 -Version 9.0.0 -Mirror -Force -OutputDir ..\xuguefcore-public-staging
# sync into payAgain/EntityFrameworkCore.Xugu clone, then:
git push -u origin main
git push origin v9.0.0
```

See [docs/RELEASE-BRANCH-STRATEGY.md](docs/RELEASE-BRANCH-STRATEGY.md) for path include/exclude tables.

---

## 6. GitHub Actions (tag pack)

On push of `v*` tags, `.github/workflows/ci.yml` / `release-pack.yml` runs pack and uploads `artifacts/*.nupkg`. Configure repository secrets:

- `XUGU_NATIVE_DLL_PATH` — optional, for native RID validation during pack

To publish from CI, add feed secrets and extend workflow (not enabled by default).

---

## 7. Release checklist

### 9.0.0 (2026-07-28 overwrite) — Wave A + Wave5

- [x] Merge `phase-13-production-hardening` → `release/9.0.0`
- [x] Docs/contracts cleaned; Functional maturity documented（非 full 0 FAIL）
- [x] Retag `v9.0.0` + GitHub Release assets overwrite
- [ ] NuGet.org push（仍按需）

### 9.0.0 (2026-07-21) — EF Core aligned / dialect iteration baseline

- [x] `Version.props` → `9.0.0`（与 `EFCoreVersion=9.0.0` 主.次对齐）
- [x] `docs/CHANGELOG.md` — 9.0.0 + Post-GA hardening 收口 + 版本策略说明
- [x] README / USER-GUIDE / GETTING-STARTED / RELEASE-SCOPE / LIMITATIONS 同步为 **9.0.0**
- [x] `publish-nuget.ps1 -Pack` → `Microsoft.EntityFrameworkCore.Xugu.9.0.0.nupkg`
- [x] Public GitHub mirror `payAgain/EntityFrameworkCore.Xugu`（历史 `v9.0.0`；本收尾后请用户自行确认是否重打/附注 tag）
- [ ] NuGet push to public feed (deferred — feed configuration)

### 3.0.1 (2026-07-14) — completed（旧编号）

- [x] `Version.props` → `3.0.1`
- [x] `docs/CHANGELOG.md` — 3.0.1 section
- [x] Runtime gap Provider fixes + `RuntimeGap` gate
- [x] `git tag v3.0.1`
- [x] Public GitHub mirror `payAgain/EntityFrameworkCore.Xugu` (`main` + `v3.0.1`)
- [ ] NuGet push to public feed (deferred — feed configuration)

### 3.0.0 GA — completed（旧编号）

- [x] `Version.props` → `3.0.0`
- [x] `docs/CHANGELOG.md` — 3.0.0 section
- [x] `docs/LIMITATIONS.md` — frozen for 3.0.0
- [x] `docs/RELEASE-SCOPE.md` — GA scope
- [x] Compat 1057 + native 1056 — 0 FAIL
- [x] `git tag v3.0.0`
- [x] `release/3.0.0` branch + `RELEASE.md` + `LICENSE`
- [ ] NuGet push to public feed (deferred — feed configuration)
- [x] Public GitHub mirror (`payAgain/EntityFrameworkCore.Xugu`)

---

## 8. User documentation

| Document | Purpose |
|----------|---------|
| [docs/GETTING-STARTED.md](docs/GETTING-STARTED.md) | Connection string, `UseXugu`, migrations |
| [docs/USER-GUIDE.md](docs/USER-GUIDE.md) | End-user guide (models, LINQ, FAQ) |
| [docs/LIMITATIONS.md](docs/LIMITATIONS.md) | Known limits, platform exclusions |
| [docs/CHANGELOG.md](docs/CHANGELOG.md) | Version history |
| [docs/RELEASE-SCOPE.md](docs/RELEASE-SCOPE.md) | Product positioning |
| [docs/xuguclient-dependency-strategy.md](docs/xuguclient-dependency-strategy.md) | Driver packaging |
