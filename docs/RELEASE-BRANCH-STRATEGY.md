# Release Branch Strategy — XuguDB EF Core Provider

> **当前公开版本**：**9.0.0**（`v9.0.0` — 对齐 EF Core 9.0.x；方言迭代基线）  
> **历史示例**：3.0.0 GA（`v3.0.0` @ `6ab8184`）  
> **更新**：2026-07-28（**主开发线 = `release/9.0.0`**；`phase-13-production-hardening` 已并入）

本文档对比 [Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) 的公开发布布局与当前 **xuguefcore** 开发单体仓库，并定义 **release 分支** 与 **公开远程** 的推送策略。当前发布线建议使用 `release/9.0.0`（见 [RELEASE.md](../RELEASE.md)）。

---

## 1. 仓库角色

| 角色 | 分支 | 远程 | 用途 |
|------|------|------|------|
| **开发 + 发布线** | **`release/9.0.0`**（当前唯一主线） | 公开 GitHub | 9.0.0 开发与发版；含 Trellis / tests / docs |
| **历史开发** | `phase-13-production-hardening`（已并入 9.0.0，不再作为主线） | — | 仅历史参考 |
| **更早发布线** | `release/3.0.0` / `release/3.0.1` | — | 旧编号归档 |
| **公开镜像**（推荐） | `main` on **独立公开仓库** 或同仓 `main` | 例如 `github.com/xugudb/EntityFrameworkCore.Xugu` | 仅含可发布路径；无 `.trellis/workspace`、无 external 子模块 |

**原则**：开发分支 **保留** Trellis 与 external；公开发布 **不推送** 整个单体仓库（或用 `-Mirror` 剥离）。

---

## 2. Pomelo vs Xugu 布局对比

### 2.1 Pomelo（公开发布仓库）

根目录典型内容（`external/Pomelo.EntityFrameworkCore.MySql/` @ 9.0.0）：

| 路径 | 说明 |
|------|------|
| `src/EFCore.MySql/` (+ NTS, Json.*) | 多包 Provider 实现 |
| `test/EFCore.MySql.FunctionalTests/` 等 | 公开功能测试 |
| `tools/` | 维护脚本 |
| `.github/workflows/` | CI / 发布 |
| `Directory.Build.props`, `Directory.Packages.props`, `Version.props` | 中央构建属性 |
| `Pomelo.EFCore.MySql.sln` | 解决方案 |
| `LICENSE`, `README.md`, `icon.png` | 开源标配 |
| `global.json`, `NuGet.config` | SDK / 源配置 |

**不包含**：Trellis workspace / 内部任务痕迹、外部参考源码子模块、本地驱动 vendoring。

### 2.2 Xugu 开发单体（当前仓库）

| 路径 | 发布相关 | 说明 |
|------|----------|------|
| `src/EFCore.Xugu/` | **INCLUDE** | NuGet 包 `Microsoft.EntityFrameworkCore.Xugu` 源码 |
| `test/EFCore.Xugu.Tests/` | **INCLUDE** | 1057 列测；公开仓库可保留（需实库 secrets 才跑全量） |
| `samples/EfDesignSample/` | **INCLUDE** | 设计时 / 集成示例 |
| `docs/` | **INCLUDE**（用户向） | GETTING-STARTED, LIMITATIONS, CHANGELOG, RELEASE-SCOPE, xuguclient-dependency-strategy |
| `docs/XUGU-VS-MYSQL.md`, `docs/TESTING.md` | **INCLUDE** | 用户与贡献者文档 |
| 根构建文件 | **INCLUDE** | `Xugu.EFCore.Xugu.sln`, `Directory.Build.props`, `Directory.Packages.props`, `NativeAssets.props`, `Version.props`, `global.json`, `NuGet.config`, `.gitignore` |
| `LICENSE`, `README.md`, `RELEASE.md` | **INCLUDE** | 发布分支新增或更新 |
| `docs/contracts/`, `docs/references/` | **INCLUDE** | 方言契约与参考地图（自原 harness 迁出） |
| `scripts/` | **OPTIONAL** | 本地 verify / 发版脚本；公开镜像可保留 |
| `.trellis/` | **EXCLUDE**（公开镜像） | Trellis 工作流；开发仓保留，`workspace` 多为本地 |
| `external/Pomelo.EntityFrameworkCore.MySql/` | **EXCLUDE** | Git 子模块；架构参考 only |
| `external/csharp-driver/` | **EXCLUDE** | Git 子模块；内部 GitLab；发布包依赖 NuGet `Xuguclient` |
| `artifacts/` | **EXCLUDE** | 本地 nupkg 输出（`.gitignore` 已忽略） |
| `.gitmodules` | **EXCLUDE**（公开镜像） | 公开仓库不需要子模块 |

### 2.3 关键差异

| 维度 | Pomelo | Xugu 开发单体 | Xugu 公开镜像 |
|------|--------|---------------|---------------|
| 包数量 | 4（Core + Json + NTS） | 1（`Microsoft.EntityFrameworkCore.Xugu`） | 1 |
| 驱动依赖 | NuGet `MySqlConnector` | 开发：ProjectReference；发布：NuGet `Xuguclient` | NuGet `Xuguclient` only |
| 测试规模 | EF 官方 FunctionalTests 子集 | 1057 compat + native 双矩阵 | 同左（可选裁剪 list-tests 门禁） |
| 协作模型 | 开源 PR | Trellis + Cursor | Issue / PR 常规流程 |
| 参考源码 | 无 vendored Pomelo | `external/Pomelo` 子模块 | 无 |

---

## 3. 分支策略

### 3.1 开发分支 — `phase-8/feature-parity`

- 承载 Phase 13+ 规划与 Trellis 工作流演进。
- **保留** `.trellis/`、`scripts/`、`external/`、内部路径引用（如 `E:\BaiduSyncdisk\...`）。
- README / `AGENTS.md` 指向 Trellis；面向内部协作者。
- 不打乱已发布的 `v3.0.0` / `v9.0.0` tag。

与 `master` 的关系（截至 2026-07-09）：

- `phase-8/feature-parity` 比 `master` **超前 44 commits**（Phase 8–12 全量）。
- `master` 比 `phase-8` **超前 1 commit**（`fb84a04` 验证维度文档）；发布以 **tag `v3.0.0`** 为准，不依赖该 commit。

### 3.2 发布分支 — `release/3.0.0`

- **起点**：annotated tag `v3.0.0` → commit `6ab8184`。
- **追加**：`RELEASE.md`、`docs/RELEASE-BRANCH-STRATEGY.md`、`LICENSE`、公开向 `README.md`、`scripts/prepare-release-branch.ps1`、可选 `.github/workflows/release-pack.yml`。
- **不删除** `.trellis/` / external（同仓多分支模型）；公开推送时用 **镜像 / subtree / filter-repo** 剥离。
- 未来补丁：`release/3.0.x` cherry-pick 自 develop；新 minor：`release/3.1.0` from 对应 tag。

### 3.3 公开仓库 `main`（推荐镜像）

使用 `scripts/prepare-release-branch.ps1 -Mirror` 生成仅含发布路径的历史，推送到 **独立公开 remote**。开发单体 remote 可保持私有。

---

## 4. 推送到远程：包含 vs 排除

### 4.1 应推送（公开镜像最小集）

```
.github/
.gitignore
Directory.Build.props
Directory.Packages.props
NativeAssets.props
NuGet.config
Version.props
global.json
Xugu.EFCore.Xugu.sln
LICENSE
README.md
RELEASE.md
src/
test/
samples/
docs/
```

### 4.2 不应推送（公开远程）

```
.trellis/
external/
.gitmodules
artifacts/
**/test-run*.txt
**/test_diag*.txt
tmp_cols.cs
```

### 4.3 NuGet 包内容

`Microsoft.EntityFrameworkCore.Xugu.nupkg` 仅含编译后的 Provider 程序集与 XML 文档，**从不**包含 `.trellis/` 或 external 源码。本地打包：

```powershell
scripts/publish-nuget.ps1 -Pack   # 开发仓；UseLocalXuguDriver=false
# 或公开镜像根目录：
dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release -o artifacts/ -p:UseLocalXuguDriver=false
```

---

## 5. 公开镜像工作流（三种选项）

### 选项 A — 同仓 release 分支（当前默认）

- 分支 `release/3.0.0` 在内部 GitLab 存在，文档说明公开推送范围。
- 适合：仅 NuGet 发布、仓库仍私有。

### 选项 B — `git subtree split`（单路径历史）

```powershell
git subtree split -P src -b mirror/src-3.0.0
# 需多次 split 再合并，较繁琐；更推荐 filter-repo。
```

### 选项 C — `git filter-repo`（推荐用于独立公开仓）

```powershell
git clone --branch release/3.0.0 . ../xuguefcore-public
cd ../xuguefcore-public
git filter-repo --force `
  --path .github/ `
  --path src/ `
  --path test/ `
  --path samples/ `
  --path docs/ `
  --path Directory.Build.props `
  --path Directory.Packages.props `
  --path NativeAssets.props `
  --path NuGet.config `
  --path Version.props `
  --path global.json `
  --path Xugu.EFCore.Xugu.sln `
  --path .gitignore `
  --path LICENSE `
  --path README.md `
  --path RELEASE.md
git remote add public https://github.com/YOUR_ORG/EntityFrameworkCore.Xugu.git
git push -u public HEAD:main
```

或使用 `scripts/prepare-release-branch.ps1 -Mirror -OutputDir ...`。

---

## 6. 版本与 Tag 对照

| Tag | Commit | 说明 |
|-----|--------|------|
| `v1.0.0` | — | Phase 7 首发 |
| `v2.0.0` | — | Phase 9–10 测试对等里程碑 |
| `v2.1.0` | `6dc0c72` | Phase 11 GA-preview（Xugu 原生方言） |
| `v3.0.0` | `6ab8184` | Phase 12 GA（Pomelo Comparable Parity 110.9%） |

---

## 7. CI 与发布门禁

| 检查项 | 开发仓 | 公开镜像 |
|--------|--------|----------|
| `dotnet build` | ✅ | ✅ |
| `dotnet test`（无 DB） | ✅ SkippableFact | ✅ |
| 实库 integration | `vars.XUGU_CI_INTEGRATION=true` | 同左 + secrets |
| NuGet pack on tag | `.github/workflows/ci.yml` `pack` job | `release-pack.yml` 或同等 |
| Harness verify | `scripts/verify.ps1` | **不需要** |

---

## 8. 相关文档

- [RELEASE.md](../RELEASE.md) — 发布操作清单
- [RELEASE-SCOPE.md](RELEASE-SCOPE.md) — 产品范围与 GA 定义
- [GETTING-STARTED.md](GETTING-STARTED.md) — 用户快速开始
- [xuguclient-dependency-strategy.md](xuguclient-dependency-strategy.md) — 驱动 NuGet vs 本地引用
