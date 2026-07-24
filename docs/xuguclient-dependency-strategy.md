# Xuguclient 依赖策略

> Phase 7 任务 7.R4  
> 最后更新：2026-07-06

本文说明 `Microsoft.EntityFrameworkCore.Xugu` 如何引用 XuguDB ADO.NET 驱动（`Xuguclient`）：本地开发 `ProjectReference` 与发布/CI 的 NuGet `PackageReference` 切换、版本锁定与原生库部署。

---

## 背景

EF Core Provider 底层依赖标准 ADO.NET 栈：

```
应用 / EF Core Provider (Microsoft.EntityFrameworkCore.Xugu)
        ↓
XuguClient.dll（C# 托管层，NuGet: Xuguclient）
        ↓ P/Invoke
xugusql.dll（C++ 原生驱动）
        ↓
XuguDB 服务端
```

驱动分析详见 `docs/references/csharp-driver-analysis.md`。

---

## 两种引用模式

由 MSBuild 属性 **`UseLocalXuguDriver`** 控制（定义于 `src/EFCore.Xugu/EFCore.Xugu.csproj`）。

### 模式 A — 本地 ProjectReference（默认，开发推荐）

```xml
<!-- UseLocalXuguDriver 未设置或为 true 时生效 -->
<ProjectReference Include="..\..\external\csharp-driver\XGCSClient\XGCSClient.csproj" />
```

| 项 | 值 |
|----|-----|
| 触发条件 | `UseLocalXuguDriver` **≠** `false`（默认） |
| 驱动源码 | `external/csharp-driver/`（只读子模块/clone） |
| 程序集 | `XuguClient.dll` |
| 适用场景 | 日常开发、调试驱动、离线构建 |

```powershell
dotnet build Xugu.EFCore.Xugu.sln
# 等价于显式：
dotnet build Xugu.EFCore.Xugu.sln -p:UseLocalXuguDriver=true
```

### 模式 B — NuGet PackageReference（发布 / CI pack）

```xml
<!-- UseLocalXuguDriver=false 时生效 -->
<PackageReference Include="Xuguclient" VersionOverride="3.3.6-bionic" />
```

| 项 | 值 |
|----|-----|
| 触发条件 | `-p:UseLocalXuguDriver=false` |
| NuGet 包 ID | `Xuguclient` |
| 适用场景 | `dotnet pack`、消费方还原、CI 产物与仓库解耦 |

```powershell
dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release -o artifacts -p:UseLocalXuguDriver=false
```

---

## 版本锁定

中央包管理：`Directory.Packages.props`

```xml
<PropertyGroup Label="Common Versions">
  <XuguClientVersion>3.3.5</XuguClientVersion>
</PropertyGroup>
<PackageVersion Include="Xuguclient" Version="$(XuguClientVersion)" />
```

| 来源 | 版本 | 说明 |
|------|------|------|
| `external/csharp-driver` csproj | **3.3.5** | 本地 ProjectReference 实际版本 |
| `Directory.Packages.props` | **3.3.5** | 中央锁定（消费方一致） |
| `EFCore.Xugu.csproj` `VersionOverride` | **3.3.6-bionic** | **仅** `UseLocalXuguDriver=false` 时；nuget.org 暂无稳定 3.3.5 |

### 版本策略原则

1. **开发默认** 跟 `external/csharp-driver` 子模块版本（当前 3.3.5）。
2. **发布 pack** 使用 `VersionOverride` 指向 nuget.org 上可还原的预发布包，确保 `dotnet pack` 在无本地驱动源码的环境通过。
3. **升级驱动** 时同步更新：
   - `Directory.Packages.props` → `XuguClientVersion`
   - `EFCore.Xugu.csproj` → `VersionOverride`（若公共 feed 有新稳定版可移除 override）
   - `external/csharp-driver` 子模块指针
   - 本文档版本表

### 内部 GitLab feed（7.R3）

`scripts/publish-nuget.ps1` 提供 pack / 可选 push 流程：

```powershell
# 默认 dry-run：打印版本、输出路径与命令，不写包
scripts/publish-nuget.ps1

# 产出 artifacts/Microsoft.EntityFrameworkCore.Xugu.{version}.nupkg
scripts/publish-nuget.ps1 -Pack

# 构建 + 测试 + verify + pack（与 CI 一致）
scripts/publish-nuget.ps1 -Pack -UseCiBuild

# 推送到内部 feed（需环境变量；本地发版默认不 push）
$env:GITLAB_NUGET_FEED_URL = "https://gitlab.example.com/api/v4/projects/.../packages/nuget/index.json"
$env:GITLAB_NUGET_API_KEY = "<token>"
scripts/publish-nuget.ps1 -Pack -Push
```

| 参数 | 说明 |
|------|------|
| 默认 | Dry-run，不写入 `artifacts/` |
| `-Pack` | `dotnet pack` 且 `-p:UseLocalXuguDriver=false` |
| `-UseCiBuild` | 改调 `ci-build.ps1 -Pack`（含 build/test/verify） |
| `-Push` | 需 `GITLAB_NUGET_FEED_URL` + `GITLAB_NUGET_API_KEY` |

稳定版 `Xuguclient` 若发布到内部 GitLab NuGet feed，消费方应：

```xml
<!-- nuget.config 添加 GitLab 源后 -->
<PackageReference Include="Xuguclient" Version="3.3.5" />
```

届时可去掉 `VersionOverride`，与 `XuguClientVersion` 对齐。

---

## 原生库（xugusql.dll）

| 项 | 说明 |
|----|------|
| 文件 | `xugusql.dll`（Windows x64） |
| 本地调试 | `XuguNativeDllPath` / `XUGU_NATIVE_DLL_PATH`：存在时复制到输出目录（`CopyToOutputDirectory`） |
| 发布 pack | `UseLocalXuguDriver=false` 时 **`Pack=false`**：Provider nupkg **不**嵌入仓库本地 `xugusql.dll`，避免与 `Xuguclient` 包内 native 双源冲突 |
| 消费方运行时 | 使用 `Xuguclient` 传递依赖中的 `runtimes/win-x64/native/xugusql.dll`（或手动部署匹配版本） |

`Directory.Build.props` 通过 `NativeAssets.props` 检测 DLL 是否存在；缺失时跳过本地 native 复制（本地无 DLL 仍可编译，但实库测试可能失败）。

---

## CI 用法

`scripts/ci-build.ps1` 标准流程：

```powershell
# 构建 + 测试 + verify（默认本地驱动）
scripts/ci-build.ps1 -Configuration Release

# 额外验证 NuGet 打包（强制 NuGet 驱动）
scripts/ci-build.ps1 -Configuration Release -Pack

# 指定原生 DLL 路径
$env:XUGU_NATIVE_DLL_PATH = "C:\path\to\xugusql.dll"
scripts/ci-build.ps1 -Configuration Release -Pack
```

| 步骤 | `UseLocalXuguDriver` | 说明 |
|------|---------------------|------|
| `dotnet build` / `dotnet test` | 默认 `true` | 使用 `external/csharp-driver` |
| `dotnet pack`（`-Pack`） | **`false`** | 验证发布包结构与 NuGet 驱动还原 |

CI 实库测试通过环境变量 `XUGU_CONNECTION_STRING` 配置连接；无数据库时 Skippable 测试跳过。

---

## 消费方建议

| 场景 | 建议 |
|------|------|
| 应用项目引用 `Microsoft.EntityFrameworkCore.Xugu` NuGet | 无需直接引用 `Xuguclient`；传递依赖由 Provider 包带入 |
| 需要直接操作 `XGConnection` | 单独添加 `Xuguclient` 包，版本 ≥ Provider 所依赖版本 |
| 仅 Windows x64 部署 | 确认 `runtimes/win-x64/native/xugusql.dll` 随应用发布 |
| Linux 部署 | **当前仅 win-x64**；跨平台见 Phase 8（8.N1–N3） |

---

## 决策记录

| 日期 | 决策 |
|------|------|
| Phase 0 | 引入 `UseLocalXuguDriver` 双模式 |
| Phase 6 | CI `-Pack` 强制 `UseLocalXuguDriver=false`；`3.3.6-bionic` override |
| Phase 7.R4 | 本文档正式化策略；7.R3 `publish-nuget.ps1` |

---

## 相关文件

| 路径 | 角色 |
|------|------|
| `src/EFCore.Xugu/EFCore.Xugu.csproj` | 条件引用逻辑 |
| `Directory.Packages.props` | `XuguClientVersion` |
| `Directory.Build.props` / `NativeAssets.props` | 原生 DLL 检测 |
| `scripts/ci-build.ps1` | CI build/test/pack |
| `scripts/publish-nuget.ps1` | NuGet dry-run / pack / optional push |
| `external/csharp-driver/XGCSClient/XGCSClient.csproj` | 本地驱动工程 |
