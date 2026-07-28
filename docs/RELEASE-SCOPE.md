# Microsoft.EntityFrameworkCore.Xugu — 发布范围声明

> **当前稳定版**：**9.0.0**（`v9.0.0` — **与 EF Core 9.0.x 版本对齐**；**方言迭代基线**）  
> **上一公开 tag（旧编号）**：**3.0.1**（`v3.0.1`）  
> **更新**：2026-07-28（Wave A 门禁仍有效；补充 Wave5 Functional 成熟度与交付口径，**非** full Functional 0 FAIL）

> **版本策略**：包 `主.次` = 目标 EF Core `主.次`（见 `Directory.Packages.props` / `Version.props`）。旧 `3.x` 编号不再用于新公开发布。

> **⚠️ 产品立场（必读）**  
> 本 Provider 是 **Xugu 原生方言** 产品：SQL 语法、函数、DDL/DML 以 **XuguDB 官方文档** 为唯一权威。  
> **Pomelo / MySQL 仅作 C# 架构与目录结构参考**，不得作为 SQL 方言定义或迁移验收标准。  
> `COMPATIBLE_MODE=MYSQL` 为可选开发/对照便利，**不是**产品语义，**不是**「零改动 MySQL 迁移」承诺。

---

## 产品定位

**Microsoft.EntityFrameworkCore.Xugu** 是面向 **XuguDB（虚谷数据库）** 的 Entity Framework Core 关系型 Provider。本产品：

- **以 XuguDB 官方文档为 SQL 方言唯一权威**（`E:\BaiduSyncdisk\docs\content\`）
- **架构对齐** Pomelo.EntityFrameworkCore.MySql 9.0.0（目录、DI、扩展模式 — **仅 C# 实现模式**）
- **不是** MySQL 或 Pomelo 的即插即用替代品
- **2.1.0 GA-preview** ✅ — Phase 11 Xugu 原生方言首发（`v2.1.0` @ 6dc0c72）
- **GA（3.0.0）** ✅ — Phase 12：**Adjusted 110.9%** Pomelo Comparable Parity（`v3.0.0`）

`COMPATIBLE_MODE=MYSQL` 可在连接会话中启用，便于与 MySQL 语法对照或遗留脚本调试；**这不构成**「迁移到 Xugu 无需改 SQL」的产品承诺。新应用应编写 **Xugu 原生 SQL** 与 EF 映射；对照文档见 [XUGU-VS-MYSQL.md](XUGU-VS-MYSQL.md)（**对照参考，非迁移目标，非虚谷方言定义**）。

---

## 版本策略

| 版本 | 性质 | 内容 |
|------|------|------|
| **2.0.0** | 稳定维护基线 | Phase 9–10：~861 列测、CRUD/LINQ/迁移/Scaffolding 主路径、Retry、参数内联 |
| **2.0.1** | 补丁（按需） | 仅严重缺陷修复；**不**承载新功能 |
| **2.1.0** | **GA-preview** ✅ | Phase 11 W1–W10：JSON、native-first、dual CI、**1056** 列测 |
| **2.1.x** | 补丁（按需） | 仅严重缺陷；**不**承载 Phase 12 功能 |
| **3.0.0** | **GA** ✅ | Phase 12 W1–W6：Adjusted 110.9%；1057 列测；194 disposition 100%；`v3.0.0` tag |
| **3.0.1** | **补丁** ✅ | COUNT/DateDiff `E34412`、DateTimeOffset、DateOnly/TimeOnly 物化 Provider 闭环；`v3.0.1` |
| **3.0.2** | Phase 13 W1 | 应用矩阵门禁、ado-driver 契约、三类绿、文档漂移清零 |
| **3.1.0** | Phase 13 W2 | 并发决策 C、RETURNING 探测、XGCI 提示 |
| **3.2.0** | Phase 13 W3 | BUSINESS-SQL-BACKLOG frozen |
| **3.3.0** | Phase 13 W4（归档编号） | `XuguCompatibleMode` 会话 API；**并入公开发布 9.0.0** |
| **9.0.0** | **EF 对齐 / 方言迭代基线** ✅ | 主.次 = EF Core 9.0；功能 = 3.0.1 + Phase 13 + Post-GA hardening + **Wave A 验收修复**；`v9.0.0`（**Wave A 门禁**，见下节） |

---

## 9.0.0 Wave A 验收（Release Acceptance Wave A）

> **定位**：**Windows 可试用** 发布门槛（2026-07-23 收口）。**不是** Pomelo FunctionalTests Comparable Set 全矩阵 0 FAIL 承诺，**不是** Linux 生产认证。

### 验收标准（Wave A gate）

| 层 | 标准 | 状态 |
|----|------|------|
| **Unit** | Release 全量 **0 FAIL**（含 TimeOnly / temporal 契约） | ✅ Wave A |
| **Integration** | 独立验收报告原 **13 FAIL → 0 FAIL**（EF_TS 前缀断言、E18012、`ALL_*` 元数据、Northwind UTF-8 重音隔离） | ✅ Wave A |
| **Functional** | **仅** APPLY / LATERAL（`ApplyNotSupported`）集群标记 Skip（~120 方法 / ~240 theory）；**不**宣称 Comparable Set 双矩阵 0 FAIL | ⚠️ 部分 Skip |
| **Packaging** | NuGet `<Description>` UTF-8 正确（虚谷数据库）；`dotnet pack -p:UseLocalXuguDriver=false` 可还原 | ✅ Wave A |
| **核心用户路径** | CRUD、LINQ、迁移、`dotnet ef`、Scaffolding（`ALL_*`）、ExecuteDelete/Update 主路径 | ✅ Wave A |

### 明确不在 Wave A 范围

| 项 | 处置 |
|----|------|
| Pomelo **Comparable Set** 全量 Functional（~8500+ 列测）双模式 **0 FAIL** | **未宣称** — Wave5 后 native 矩阵约 7524 pass / ~105 fail / 509 skip；经典方言炸码 FAIL 已清零，剩余多为复杂导航语义与测试金标；后续 full Functional 波次 |
| **Linux x64** 生产可用 | **未验收** — `Xuguclient` 3.3.6-bionic 可能含 `.so` 资产，Wave A **仅签 Windows x64 试用** |
| nuget.org 公开发布 | 不在本波次 |
| 版本号 bump（9.0.1） | 不在本波次 — 仍为 **9.0.0 覆盖式** 发布叙事 |

### Wave A 本地门禁（参考命令）

```powershell
dotnet test test/EFCore.Xugu.Tests.Unit/EFCore.Xugu.Tests.Unit.csproj -c Release
dotnet test test/EFCore.Xugu.Tests.Integration/EFCore.Xugu.Tests.Integration.csproj -c Release
# Functional：APPLY/LATERAL 已 Skip；其余 FAIL 不阻塞 Wave A
dotnet pack src/EFCore.Xugu/EFCore.Xugu.csproj -c Release -p:UseLocalXuguDriver=false -o artifacts
```

证据与修复明细见 `.superpowers/sdd/task-*-report.md` 与 `docs/superpowers/specs/2026-07-23-release-acceptance-wave-a-design.md`。

### 9.0.0 交付口径（2026-07-28）

| 口径 | 是否达到 | 说明 |
|------|----------|------|
| **Wave A / Windows 可试用** | **是** | Unit/Integration 门禁 + 核心用户路径；可交付试用 |
| **Functional 方言硬失败清零** | **是（工作区矩阵）** | APPLY / E17010 / E19132 / E19196 无 residual FAIL 行 |
| **Primitive collection 参数 membership** | **是** | JSON 标量谓词路径；行集 `JSON_TABLE` 仍不支持 |
| **Full Functional definition A（0 FAIL + 仅权威 Skip）** | **否** | 仍有复杂导航/继承边角 residual；见 [LIMITATIONS.md](LIMITATIONS.md) |
| **Linux 生产 / nuget.org** | **否** | 未验收 / 非本波次默认动作 |

**对用户一句话**：9.0.0 可在 **Windows x64** 作为 EF Core 9 Xugu 原生 Provider **认真试用/试投产**；请阅读 LIMITATIONS（APPLY、复杂导航边角、DateTimeOffset 驱动边界）。**不要**按 Pomelo Functional 全绿或 Linux 生产认证理解本版。

独立全量套件复跑与 tag 覆盖属于发版操作，不改变 Wave A 门禁定义。

---

## 完全体定义（3.0.0 = Pomelo 完全体）

**完全体** 指在 **XuguDB 官方文档权威** 前提下，对 Pomelo.EntityFrameworkCore.MySql 9.0.0 达成 **100% Comparable Parity**：

| 维度 | 标准 |
|------|------|
| **测试** | Pomelo FunctionalTests **Comparable Set** 100% ported 或 Xugu-adapted；compat + native **0 FAIL** |
| **源码** | Pomelo 194 `.cs` **Comparable Files** 100% 对等或 per-file justification |
| **Skip 模块** | NTS / FULLTEXT / Collation / Scaffolding baselines 等 — **implement** 或 **XuguDB doc evidence + user approved exclusion**（OUT OF SCOPE 表趋于空） |
| **Blocked** | ROW_COUNT、Linux RID — **unblocked** 或 **vendor ticket + signed-off** |
| **Defer** | DateOnly SC、net8.0、11.202–204 等 — **resolved** 或 reclassified |

### Literal 100% vs Adjusted 100%

| 模式 | 含义 | 推荐 |
|------|------|------|
| **Literal 100%** | 1050 测试 + 194 文件 + 全部 Pomelo 特性物理实现 | Stretch goal；可能不可达 |
| **Adjusted 100%** | 对 proven Xugu-impossible 项 recalc 分母后 **100%**；每项须 doc link + approval | **推荐** — 满足「无 silent gap」意图 |

权威门禁：见 [RELEASE.md](../RELEASE.md) 与 `scripts/verify.ps1`（原 Phase 12 GOALS 已随 harness 移除）

---

## 2.1.0 包含（IN SCOPE）

| 能力 | 说明 |
|------|------|
| **Xugu 原生 JSON 列** | EF 映射 `JSON` 类型；路径运算符与 JSON 函数按官方文档 |
| **核心 ORM 路径** | CRUD、LINQ 翻译、迁移、`dotnet ef`、Scaffolding（`DBA_*` 元数据） |
| **ExecuteDelete / ExecuteUpdate** | 单表及文档支持的多表路径 |
| **乐观并发 token 列** | UPDATE 含 concurrency token；**9.0+ / 当前树** 另支持 `DbUpdateConcurrencyException`（Path A：`RecordsAffected`；见 LIMITATIONS） |
| **自动重试** | `EnableRetryOnFailure` + XGCI 码解析 |
| **连接串校验** | Xugu 键值对格式（`IP`、`DB`、`USER`、`PWD`、`PORT` 等） |
| **NuGet 包** | `Microsoft.EntityFrameworkCore.Xugu` + 依赖 `Xuguclient`（Windows x64 当前支持） |
| **文档** | GETTING-STARTED、LIMITATIONS（frozen）、本文件、sql-dialect.contract |

---

## 2.1.0 不包含（OUT OF SCOPE — GA-preview 时期；完全体在 Phase 12 重新打开）

> 下列在 2.1.0 **不阻塞发布**。完全体（3.0.0）要求每项 Path A 实现或 Path B evidence-backed exclusion。

| 能力 | 处置 | 原因 |
|------|------|------|
| MySQL 迁移零改动 / Pomelo 即插即用 | **非目标** | 产品定位为 Xugu 原生方言 |
| `ROW_COUNT()` SQL（非并发路径依赖） | **仍不可用** E10049 | 并发已改 Path A；VT-XUGU-ROWCOUNT-001 可作简化 |
| Linux x64 原生 RID | **signed-off blocked** PLAT-02 | 驱动无 `libxugusql.so`；VT-XUGU-LINUXRID-001 |
| NetTopologySuite / Spatial | **永久 skip** | 无 NTS 生态 |
| FULLTEXT / `MATCH … AGAINST` | **永久 skip** | 文档无对外 FULLTEXT；用 `REGEXP_LIKE` |
| `CONVERT_TZ` / `ConvertTimeZone` | **永久 skip** | XuguDB 无等价函数 |
| 列/表级 Collation / `HasCharSet` Fluent | **永久 skip** | 连接级 `CHAR_SET` |
| Scaffolding Baselines 全量快照 | **永久 skip** | 维护成本过高 |
| Lazy loading proxies 测试宿主 | **永久 skip** | 无测试宿主 |
| `CREATE DATABASE` / `DROP DATABASE`（EF API） | **永久 defer** | 运维边界 |
| DateOnly/TimeOnly **SaveChanges** | **defer** | 驱动参数绑定 |
| net8.0 多 TFM | **可选** | 2.1.0 默认 net9.0 only |
| Pomelo FunctionalTests 100% | **2.1.0 非目标** / **3.0.0 Phase 12** | 1056/~1050 literal ✅；Adjusted 100% 待 Phase 12 |
| Pomelo IntegrationTests（Vegeta/ASP.NET 性能） | **永久 defer** | 低 ROI；见 PACKAGING-AND-INTEGRATION |
| 全量 EF.Specification.Tests | **分阶段** | 见 `PACKAGING-AND-INTEGRATION.md` |
| FOR UPDATE / 窗口函数 Tag / 位运算返回类型修正 | **defer** | EF 无标准 API 或低 ROI |
| RelationalCommand/Database 表面 API | **defer** | P2 补齐 |

完整技术对照见 [XUGU-VS-MYSQL.md](XUGU-VS-MYSQL.md)（**对照参考，非迁移目标，非虚谷方言定义**）与 [LIMITATIONS.md](LIMITATIONS.md)。

---

## 2.1.0 发布门禁（Release Gate — **closed** @ 6dc0c72）

> Phase 11 **done**（GA-preview @ 6dc0c72）。GA 见 `phase-12-pomelo-full-parity/PHASE12-GOALS.md`。

### 构建与测试

- [x] `dotnet build Xugu.EFCore.Xugu.sln -c Release` — PASS
- [x] `scripts/verify.ps1` — PASS
- [x] `dotnet test Xugu.EFCore.Xugu.sln -c Release` — **0 FAIL**
- [x] 列测基线 **1056** compat（literal ≥ ~1050）
- [x] **native** CI 子集 **177** 列测 0 FAIL

### 功能

- [x] JSON 列：迁移 DDL + 基础 LINQ/查询 — 实库验证（11.109）
- [x] 连接串校验器 — 单元测试覆盖非法/合法键值对（11.208）
- [x] 永久 OUT OF SCOPE 项在 `LIMITATIONS.md` 中明确列出且未静默回退

### 打包与文档

- [x] `scripts/test-nuget-pack.ps1` — 干净项目安装 + 编译 PASS
- [x] `scripts/publish-nuget.ps1 -Pack` — 产出 `Microsoft.EntityFrameworkCore.Xugu.2.1.0.nupkg`
- [x] `docs/RELEASE-SCOPE.md`、`docs/GETTING-STARTED.md`、`CHANGELOG.md` — 2.1.0 同步
- [x] `docs/XUGU-VS-MYSQL.md` — 已标注「对照参考，非迁移目标」
- [x] `LIMITATIONS.md` — **frozen** for 2.1.0

### 明确不纳入 2.1.0 门禁

- ROW_COUNT / Linux RID / DateOnly SaveChanges / net8.0 TFM
- 全量 `EFCore.Specification.Tests` / Pomelo FunctionalTests 100%
- NTS / FULLTEXT / Scaffolding Baselines / MySQL 迁移承诺

### 2.0.1 补丁线（若需要）

仅用于 **2.0.0 严重缺陷**（安全、数据损坏、构建破坏），**不**承载 Phase 11 功能。功能发布统一走 **2.1.0**。

---

## 生产就绪定义

### 2.1.0 GA-preview（已达成）

在 2.1.0 OUT OF SCOPE 前提下：实库 CI 0 FAIL、verify + test-nuget-pack PASS、集成样本冒烟、LIMITATIONS 2.1.0 frozen、`v2.1.0` tag。

### GA 3.0.0（Phase 12 — **done**）

在 **Comparable Parity Adjusted 110.9%** 前提下：Phase 12 W1–W6 全绿 + `PHASE12-GOALS.md` 全部门禁 + `v3.0.0` tag。平台限制 PLAT-01/02 **signed-off blocked**（vendor tickets filed）；**Windows-only GA**。

### 9.0.0 Wave A（**done** @ 2026-07-23）

在 **Windows 可试用** 前提下：Unit **0 FAIL**；Integration 独立验收原 13 项 **0 FAIL**；Functional **仅** APPLY/LATERAL Skip 卫生；NuGet 描述 UTF-8 修复。**不**宣称 Functional Comparable Set 全绿或 Linux 生产就绪。详见上文 [9.0.0 Wave A 验收](#900-wave-a-验收release-acceptance-wave-a)。

### 9.0.0 Wave5 能力增强（工作区，2026-07-28）

在 **不改变 Wave A 门禁** 前提下：PrimitiveCollections 参数 membership、string First/LastOrDefault、`TimeSpan.Milliseconds` E34412 规避、FromSql 测试前缀、大量方言硬限制 Skip 证据化；Functional native 成熟度见上文交付口径。**完全体 / definition A 仍未关闭**。

---

## 方言权威声明

实现与文档冲突时，优先级：

```
1. E:\BaiduSyncdisk\docs\content\          ← XuguDB 官方文档（SQL 唯一权威）
2. docs/contracts/sql-dialect.contract.md ← 项目内已确认方言规则
3. docs/LIMITATIONS.md / RELEASE-SCOPE.md    ← 产品范围与已知限制
4. external/Pomelo.EntityFrameworkCore.MySql  ← 仅 C# 架构/DI/目录参考；禁止作为 SQL 依据
5. MySQL 文档 / 社区习惯                     ← 不得作为实现依据
```

Agent 与贡献者：**禁止**仅凭 MySQL 或 Pomelo 行为推断 Xugu SQL；**禁止**将 `COMPATIBLE_MODE=MYSQL` 下的偶然兼容当作产品保证。

`COMPATIBLE_MODE=MYSQL` 定位：

| 用途 | 允许 | 禁止 |
|------|------|------|
| 开发时 LINQ→SQL 与 Pomelo 对照 | ✅ | — |
| 遗留脚本临时调试 | ✅ | — |
| 产品 SQL 方言定义 | — | ❌ |
| 迁移验收标准 | — | ❌ |
| 新功能实现依据 | — | ❌ |

---

## 相关文档

- [RELEASE.md](../RELEASE.md) — 发版与本地门禁
- [docs/contracts/sql-dialect.contract.md](contracts/sql-dialect.contract.md)
- [.trellis/spec/guides/xugu-provider-constraints.md](../.trellis/spec/guides/xugu-provider-constraints.md)
- 历史 Phase 任务 / ROADMAP：原 `harness/tasks/*`（已移除；见 git 历史）
