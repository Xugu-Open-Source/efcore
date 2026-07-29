# Changelog

All notable changes to **Microsoft.EntityFrameworkCore.Xugu** are documented here.

Format loosely follows [Keep a Changelog](https://keepachangelog.com/).  
Known limitations and deferred features: [LIMITATIONS.md](LIMITATIONS.md).

---

## [Unreleased]

- **Wave5f** — `ORDER BY` null ordering: `ASC NULLS FIRST` / `DESC NULLS LAST`（对齐 SQL Server/EF；Xugu 默认 ASC 空值在后）。ComplexNavigations / TPC M2M / correlated OrderBy 主 residual 簇 live 转绿；少数 collection-count / Sum+Include / Owned 仍 Skip。
- 全量 Functional 0 FAIL + 覆写 `v9.0.0` 仍属 Wave6。

---

## [9.0.0] — 2026-07-28 (EF Core 9 aligned — Wave A + Wave5 overwrite)

**版本策略**：自本版起，包版本 **主.次与目标 EF Core 对齐**（`EFCoreVersion=9.0.0` → `9.0.0`）。历史 `1.x`–`3.x` 编号仅作归档。本 tag 为 **覆盖式** 发布（Wave A 修复仍标 9.0.0，不 bump 9.0.1）。

**基线声明**：本版为 **方言迭代基线** + **Wave A 可试用验收**。功能 = 原 3.0.0 GA + 3.0.1 runtime-gap + Phase 13（原工作口径 3.0.2–3.3.0）+ Post-GA hardening + **Wave A 发布验收修复**（2026-07-23）。

**Wave A 验收门槛**（详见 [RELEASE-SCOPE.md](RELEASE-SCOPE.md#900-wave-a-验收release-acceptance-wave-a)）：

| 层 | 结果 |
|----|------|
| Unit | **0 FAIL**（修复 TimeOnly / temporal 契约 6 项） |
| Integration | 独立验收原 **13 FAIL → 0 FAIL** |
| Functional | **仅** APPLY/LATERAL Skip；**不**宣称 Comparable Set 全矩阵 0 FAIL |
| 平台 | **Windows x64 可试用**；Linux **未验收** |

### Wave5 Functional 加固（同版覆盖，2026-07-28）

- **Primitive collection 参数 membership** — JSON 标量谓词 + `XuguPrimitiveCollectionTypeMapping`；无 `JSON_TABLE` 行集仍 Skip。
- **string `FirstOrDefault` / `LastOrDefault`** — `SUBSTRING`（+ `LENGTH`）。
- **`TimeSpan.Milliseconds`** — `MICROSECOND/1000` 后投影 `INTEGER`（避 E34412）。
- **LIMIT/OFFSET** 整数化；内联 VALUES → `UNION ALL SELECT`（Wave4，并入本覆盖）。
- **Functional 卫生** — FromSql 表前缀、AssertSql 降噪、复杂导航/TPC M2M 等 residual 带原因 Skip。
- **契约文档** — `sql-dialect` / `stub-and-exclusion` 全文重写；删除不可恢复的 `test-parity-matrix` / `pomelo-file-map`。
- **矩阵** — native class-isolated 约 7524 passed / ~105 failed / 509 skipped；经典方言炸码 FAIL 清零；**仍非** full Functional 0 FAIL。
- **口径** — 仍为 **Wave A Windows 可试用**；见 RELEASE-SCOPE 2026-07-28 交付表。

### Wave A fixes（2026-07-23）

- **TimeOnly Unit 契约** — 恢复 converter 路径（`XuguTemporalValueConverters.TimeOnlyToString`）、`TIME(n)` 精度、`DbType.Time` 绑定；`TypeMappingSourceTests` 全绿。
- **NuGet Description UTF-8** — 修正 `<Description>` 乱码为「虚谷数据库」；`dotnet pack -p:UseLocalXuguDriver=false` 验证 nuspec。
- **Integration EF_TS 前缀** — 断言对齐 SHA1 短前缀 `EF_{8-hex}_`（`XuguTestStoreFactory.FormatTablePrefix`）。
- **Integration E18012 / ALL_*** — Scaffolding、迁移 history、`HasTables` 与 fixture 元数据统一走 `ALL_*` 视图；普通用户无需 `DBA_*`/`SYS_*` GRANT。
- **Integration Northwind UTF-8** — 测试连接强制 `CHAR_SET=UTF8`；Northwind seed 重音 canary（`Bräcke` / `Folk och Fä HB`）防共享库污染。
- **Functional APPLY Skip 卫生** — ~120 方法 / ~240 theory 标记 Skip（`ApplyNotSupported`）；BulkUpdates 负向断言保留 PASS。
- **文档** — RELEASE-SCOPE / LIMITATIONS / USER-GUIDE / CHANGELOG 对齐 Wave A 口径。

### Tooling（同版覆盖）

- **Agent 工作流** — 移除 `harness/`；用 Trellis（Cursor）接管：`.trellis/` + `.cursor/`。
- **活文档迁出** — 方言/驱动契约 → `docs/contracts/`；关键参考 → `docs/references/`；门禁/发版脚本 → `scripts/`。
- **Trellis specs** — `.trellis/spec/src/EFCore.Xugu/backend/`；硬约束见 `guides/xugu-provider-constraints.md`。
- **CI** — GitHub Actions / GitLab CI 工作流已移除（门禁改本地 `scripts/`）。

### Added / Fixed（相对公开 `v3.0.1`；含 Post-GA hardening）

- Phase 13：应用能力矩阵门禁、`ado-driver-contract`、RuntimeGap 强化、RETURNING 探测、业务 SQL 清单、`XuguCompatibleMode` 会话 API。
- 3.0.1：COUNT/DateDiff CAST、时态 converter、严格实库模式。
- **乐观并发 Path A** — `ConsumeResultSetWithRowsAffectedOnly` 使用 `DbDataReader.RecordsAffected`（UPDATE/DELETE）；去掉占位 `SELECT 1`；启用 `DbUpdateConcurrencyException`（`OptimisticConcurrencyTests.Stale_*` PASS）。INSERT 经 reader 的 `RecordsAffected` 恒 0 时对 `Added` 不做误报。（**取代** Phase 13 W2 归档决策 C。）
- **TIME 物化** — `TimeSpan`/`TimeOnly` 经 converter / `GetString` 物化（Wave A：TimeOnly 以 Unit 契约为准，见 `XuguTimeOnlyTypeMapping`）。
- **BLOB 物化** — `byte[]` 从驱动 `XGBlob` 适配为 `byte[]`（不改驱动）。
- **DateTimeOffset 读回** — 兼容短年（`3-03-01…`）与小时级偏移（`…-5`）；LIMITATIONS 注明亚小时偏移/亚秒截断边界。
- **DateTimeOffset.Date / DayOfYear** — LINQ 翻译为文档函数 `DATE` / `DAYOFYEAR`。
- **二进制 DDL** — `byte[]` 有长度时映射为无后缀 `BINARY`（非 `BLOB(n)`）；文档无 size 后缀（`binary.md` / `large-object.md`）。
- **标识符长度** — `RelationalMaxIdentifierLengthConvention(127)`（`identifier.md`）；Spec 表前缀改为短哈希，避免 FK 名超长。
- **byte / 未映射 DbType 参数绑定与物化** — TinyInt/`UInt*`/`Single`/`Guid`/`SByte`/`StringFixedLength` 等旁路驱动 Binary 默认；TIME/DATE 按驱动要求绑定为字符串。
- **APPLY/LATERAL 明确拒绝** — 实库确认无 `CROSS/OUTER APPLY`、`LATERAL`；抛 `XuguStrings.ApplyNotSupported`；Functional 相关用例 Skip（Wave A）。
- **瞬态检测 / Retry** — `[E34304]`/`[E34305]` 不再视为可重试；识别空正文 `[E34501]`/`[E34301]`，`OnRetry` 关闭失效连接后再重试。
- **Sequence Migrations DDL** — 按 `reference/object/sequence.md`；`RESTART WITH` NotSupported。
- **Spec Functional 工程** — `test/EFCore.Xugu.Tests.Functional`（NullSemantics / GearsOfWar(+TPT/TPC) / ComplexNavigations / Owned / PrimitiveCollections / BulkUpdates 等，`--list-tests` ~8500+）；**Wave A 不宣称全矩阵 0 FAIL**。
- **质量补强** — QualityMatrix / Cluster Integration / Affected-rows 探针；Integration Skip 闭环；测试默认方言 **native**。

### Docs

- `RELEASE.md` / README / USER-GUIDE / RELEASE-SCOPE / LIMITATIONS / CHANGELOG：公开口径统一为 **9.0.0 Wave A**（对齐 EF Core 9.0.x）。

---

## [3.3.0] — 2026-07-19 (Phase 13 W4 — 归档编号；并入 9.0.0)

### Added

- **`XuguCompatibleMode`** 枚举与 `EnableCompatibleModeOnOpen(XuguCompatibleMode)`（MYSQL / ORACLE / POSTGRESQL / None）。
- 会话 `SET compatible_mode` + `CompatibleModeSessionTests`（标识符/SHOW 行为；**不**实现 Oracle/PG SQL 方言）。

### Docs

- LIMITATIONS：compat ≠ 异构 SQL 方言；W4 门控签字记录。

---

## [3.2.0] — 2026-07-19 (Phase 13 W3 — business SQL backlog)

### Added

- **BUSINESS-SQL-BACKLOG** 冻结（20 项：implement/defer/excluded，无 open）。
- **MYSQL-COMPAT-DIFFS** 验收清单（13.306）。

### Docs

- sql-dialect / LIMITATIONS 与清单交叉引用。

---

## [3.1.0] — 2026-07-19 (Phase 13 W2 — production gaps)

### Changed

- **乐观并发**：产品决策 **(C)** — 不支持自动 `DbUpdateConcurrencyException`（ROW_COUNT E10049）；token 列仍映射。
- **RETURNING**：探测脚本/测试确认驱动不可读 → 保持 `LAST_INSERT_ID`。
- **ExecuteUpdate 边缘 / JSON 整列**：LIMITATIONS 清单化。
- **XGCI 提示**：`XuguExceptionHints` + resx（E34412 / E19230 / E10049）。

### Deferred

- NuGet 公开 push（无 feed）→ 本地 pack 验证。
- Linux RID：无 `.so` → signed-off 续期。

---

## [3.0.2] — 2026-07-19 (Phase 13 W1 — app matrix / contracts)

### Added

- **应用能力矩阵** — `samples/AppCapabilityMatrix` + `AppCapabilityMatrixTests`（Category=`AppCapabilityMatrix`）。
- **门禁** — `run-app-matrix-gate.ps1`；CI L2 并入 RuntimeGap + AppMatrix。
- **驱动契约** — `harness/contracts/ado-driver-contract.md`。
- **三类绿** — `docs/TESTING.md`（SQL 形状 / 服务端执行 / 客户端物化）。

### Docs

- Post-GA PG2–PG7 文档漂移清零；LIMITATIONS/CHANGELOG 指向应用矩阵与契约。

---

## [3.0.1] — 2026-07-14 (Post-GA runtime gap patch)

Provider 侧闭环应用审核暴露的 6 个独立运行时缺口（保留官方 `Xuguclient` / `external/csharp-driver` 只读基线，不改驱动）。

### Fixed

- **Count / LongCount / GroupBy Count / 导航 Count** — `CAST(COUNT(…) AS INTEGER|BIGINT)`，规避驱动 `E34412`（高精度→Int32）。
- **DateDiff\*** — `TIMESTAMPDIFF` 按官方 **BIGINT** 生成，公共 `int` API 末端再转 `INTEGER`。
- **DateTimeOffset SaveChanges** — string converter + 带偏移字面量；读回兼容驱动 `…+8` / `…+08:00`。
- **DateOnly / TimeOnly 物化** — Provider string converter；默认 `TIME(3)`；Include DATE 覆盖。

### Added

- **RuntimeGap 门禁** — `RuntimeGapBaselineTests` + `harness/scripts/run-runtime-gap-gate.ps1`（Category=`RuntimeGap`）。
- **严格实库模式** — `XUGU_REQUIRE_DATABASE=true` 时库不可用即失败（非 Skip）。

### Evidence

- 审核库 RuntimeGap：**native 9/9**、**compat 9/9**（0 skip）。

---

## [3.0.0] — 2026-07-09 (Phase 12 — Pomelo 完全体 GA)

Phase 12 完成：**首次生产 GA**。Adjusted **110.9%** Pomelo Comparable Parity（分母 **952** / 列测 **1057**）；compat + native 双矩阵 **0 FAIL**；Pomelo comparable 文件 disposition **194/194**（历史；原 map 文档已删）**100%**。

### Added

- **Platform limitation 探针** — `PlatformLimitationProbeTests` + `probe-platform-limitations.ps1`（ROW_COUNT E10049 / Linux `.so` 自动化复验）。
- **Vendor ticket 登记** — VT-XUGU-ROWCOUNT-001、VT-XUGU-LINUXRID-001（Path B signed-off）。
- **DateOnly/TimeOnly SaveChanges** — `XuguDateOnlyTypeMapping` / `XuguTimeOnlyTypeMapping`（12.304）。
- **位运算返回类型修正** — `BitwiseOperationReturnTypeCorrectingExpressionVisitor`（12.302）。
- **OUT OF SCOPE 正式排除表** — NTS / FULLTEXT / Collation / CONVERT_TZ / Scaffolding baselines（`out-of-scope-approved-12.409.md`）。
- **Native 矩阵 100%** — `Category=NativeDialect` 扩展至 **1056** 列测（W2）。

### Changed

- **版本** — `2.1.0` GA-preview → **`3.0.0` GA**。
- **测试基线** — compat **1057**（+1 platform probe）；native **1056** = compat 100%。
- **Adjusted 分母** — Pomelo literal 1050 → 剔除 98 OUT OF SCOPE → **952**；覆盖率 **110.9%**。
- **显式 Skip** — 5 方法全 evidence（1 signed-off blocked PLAT-01）。
- **LIMITATIONS** — **frozen for 3.0.0**；DateOnly SC done；平台限制 PLAT-01/02 签收。
- **平台 GA 范围** — **Windows-only GA**（Linux RID signed-off PLAT-02）。

### Signed-off blocked（非 GA 阻塞）

- **PLAT-01** — `ROW_COUNT()` / `DbUpdateConcurrencyException`（E10049）。
- **PLAT-02** — Linux x64 `libxugusql.so` / RID 打包。

### Deferred（documented，非 3.0.0 阻塞）

- Specification Tests Phase 3（12.103 defer）。
- net8.0 多 TFM（net9.0 only）。
- NuGet 公开发布 push（pack 验证 PASS；push 待 feed 配置）。

---

## [2.1.0] — 2026-07-09 (Phase 11 — Xugu 原生方言 GA-preview)

Phase 11 完成（**GA-preview**）：**Xugu 原生方言优先**、JSON Provider、RETURNING identity 回读、双 CI 矩阵、NuGet 2.1.0 门禁。完全体 GA → Phase 12 `v3.0.0`。

### Added

- **JSON 列 Provider** — `XuguJsonTypeMapping`、`HasXuguJsonColumn()`、JSON 路径/函数 LINQ 翻译与实库测试。
- **Native INSERT … RETURNING** — 原生模式（默认）identity 回读主路径；`LAST_INSERT_ID()` 仅 compat 回退。
- **连接串校验器** — `XuguConnectionStringOptionsValidator`；`UseXugu` 在配置时校验 `IP`/`DB`/`USER`/`PWD`/`PORT`。
- **双 CI 矩阵** — `XUGU_DIALECT_MODE=compat`（全量回归）与 `native`（`Category=NativeDialect` 核心子集）。
- **集成样本** — `test/integration-sample/MinimalApi` Web API CRUD 冒烟。
- **测试扩展** — `NativeDialectIdentityTests`、`SpecificationPhase2XuguTests` 等。

### Changed

- **默认方言** — 连接打开时 **不再** 默认执行 `SET compatible_mode TO 'MYSQL'`；需显式 `EnableCompatibleModeOnOpen()`。
- **版本** — `2.0.0` → **`2.1.0`**。
- **测试套件** — **1056** 列测（compat 矩阵）；native **263** 列测。

### Breaking

- 依赖隐式 MySQL compat 的应用须调用 `options.UseXugu(cs, x => x.EnableCompatibleModeOnOpen())` 或连接串会话级设置。

### Deferred (documented)

- ROW_COUNT / Linux RID / DateOnly SaveChanges / net8.0 TFM — 驱动解锁后 2.1.x / 2.2。

---

## [2.0.x] — 2026-07-07 / 2026-07-08 (Phase 10 Wave 1/2/3)

Phase 10 **维护与剩余对等**前三个 Wave 落地，稳固 2.0.0 维护线并扩展 Pomelo 测试对等到 **850 列测**（~81% 覆盖）。skip/defer 项见 [LIMITATIONS.md](LIMITATIONS.md)。

### Wave 1 — 2026-07-07（10.001–10.005，CI + 文档 + triage）

#### Added

- **CI/CD 实库矩阵** — `.github/workflows/ci.yml` + `.gitlab-ci.yml`；secrets 与运行说明见 `docs/TESTING.md`。
- **全量门禁回归** — `harness/scripts/verify.ps1 -RunTests` 模式；CI integration job 调用。
- **NuGet 发布流水线 2.0.0** — `publish-nuget.ps1` dry-run 验证 **2.0.0**；GitLab `publish-nuget` manual job。
- **用户文档刷新** — `docs/GETTING-STARTED.md` → 2.0.0；链到新增 `docs/XUGU-VS-MYSQL.md`。
- **剩余测试 triage** — `harness/references/phase-10-test-triage.md`；Wave 2–6 计划。

### Wave 2 — 2026-07-07（10.103 / 10.104，795 列测）

#### Added

- **Query 深覆盖 +119 列测** — FromSql / TPH / Deep nested / DbFunctions / ComplexNav 全量补齐；Pomelo `NorthwindQueryMySqlTest`、`AdHocQueryMySqlTest` 子集对等。
- **9.T defer 补全** — `SaveChangesInterceptionTests` +6、`ConvertToProviderTypesMySqlTest` +10、`SeedingTests` +3；`WithConstructorsTests` insert ×2。

#### Changed

- **测试套件** — 676 → **795**（Phase 10 Wave 2 达标 10.M2）。
- **Pomelo 可比覆盖率** — ~64% → ~76%。

### Wave 3 — 2026-07-08（10.101 / 10.102，850 列测）

#### Added

- **Monster Fixup 子集** — `MonsterFixupXuguTests` + `StoreGeneratedFixupXuguTests`（手写 Xugu 兼容模型；对齐 Pomelo `MonsterFixup*MySqlTest`）。
- **Specification Tests 子集** — `DesignTimeXuguTest` + `KeysWithConverters` + `TransactionBasics` 子集（对齐 `EFCore.Specification.Tests` 数据库相关条目）。

#### Changed

- **测试套件** — 795 → **850**（Phase 10 Wave 3 达标 10.M4）。
- **Pomelo 可比覆盖率** — ~76% → **~81%**（850 ÷ 1050 测试方法）。

### Wave 4 — 2026-07-08（10.106 done / 10.105 blocked）

#### Added

- **`XuguRetryingExecutionStrategy`** — `EnableRetryOnFailure()` 实装；`XuguTransientExceptionDetector` 解析 Message 中 XGCI 瞬态码（E19886/E32506/E34304/E34305 等）。
- **Retry 单测 +10** — `XuguTransientExceptionDetectorTests`、`ExecutionStrategyTests.EnableRetryOnFailure_configures_retrying_strategy`。

#### Changed

- **测试套件** — 850 → **860**（+10 Retry 单测）。
- **Pomelo 可比覆盖率** — ~81% → **~82%**（860 ÷ 1050）。

### Deferred (documented, Phase 10 Wave 5–6)

- **10.105 ROW_COUNT 乐观并发** — **blocked**（E10049）；需驱动 `RecordsAffected` 或 XuguDB 等价 API。
- **10.107 EF 版本矩阵** — **assessed**；2.0.x 维持 net9.0 only。
- **10.205 Linux x64 RID 打包** — **blocked**（无 `libxugusql.so`）。
- **10.202 FOR UPDATE / 窗口函数**、**10.203 位运算返回类型**、**10.204 RelationalCommand 表面** — defer Phase 11。

### Wave 5 — 2026-07-08（10.201 done / 10.205 blocked）

#### Added

- **OFFSET 参数内联** — `XuguInlinedParameterExpression` + `XuguParameterInliningExpressionVisitor`；`TranslatorSqlTests.Skip_with_closure_parameter_inlines_offset_literal`。

#### Changed

- **测试套件** — 860 → **861**（+1 参数内联单测）。

#### Blocked

- **10.205 Linux x64 RID** — 驱动仓库无预编译 `libxugusql.so`。

### Wave 6 — 2026-07-08（10.108 JSON 调研 / Phase 10 closure）

#### Research（10.108）

- **XuguDB 原生 JSON** — `reference/sql/datatype/json.md`：LOB 类型、JSONPath、`->`/`->>` 运算符、28+ JSON 函数。
- **Provider 结论** — EF JSON 列映射 **未实现**；**10.109 defer Phase 11**（对标 Pomelo `EFCore.MySql.Json.*`）。
- **Pomelo `Json*MySqlTest`** — 永久 skip（2.0.x）。

#### Closure（10.M3）

- **NuGet pack** — `publish-nuget.ps1 -Pack` 验证 **2.0.0**。
- **文档同步** — `LIMITATIONS.md`、`XUGU-VS-MYSQL.md`、`sql-dialect.contract.md`、ROADMAP/TASKS closure。

---

## [2.0.0] — 2026-07-07

Phase 9 **Pomelo 9.0.0 测试对等**里程碑（M3 达标；skip/defer 项见 [LIMITATIONS.md](LIMITATIONS.md)）。

### Added

- **676 列测** — 自 Phase 8 的 207 扩展；W6 Northwind Query/AdHoc **+252** 批次。
- **测试基础设施** — `XuguTestStore` 全量 adoption、Northwind seed、`XuguQueryTestBase`、`AssertSql`、20+ Collection fixtures。
- **连接稳定性** — `XuguRelationalConnection` 瞬态 open 重试；测试 `OpenConnection` 串行化；`DisableTestParallelization`。

### Changed

- **版本** — `1.1.0-preview` → **`2.0.0`**。
- **Pomelo 可比覆盖率** — **~64%**（676 ÷ 1050 测试方法）。
- **测试套件** — M1/M2/M3 里程碑（≥200 / ≥400 / ≥600）全部达标。

### Deferred (documented)

- ~374 剩余 Pomelo 测试（Monster、Specification、JSON/NTS）→ Phase 10。
- ROW_COUNT 乐观并发异常、LazyLoad 代理、optional complex — 见 [LIMITATIONS.md](LIMITATIONS.md)。

---

## [1.1.0-preview] — 2026-07-06

Phase 8 **Pomelo 9.0.0 功能对等**里程碑（skip/defer 项见 [LIMITATIONS.md](LIMITATIONS.md)）。

### Added

- **Query Translators 全量 P0/P1** — StringComparison、TimeSpan、Math 全量、Convert、Regex `IsMatch`（`REGEXP_LIKE`）等。
- **ExpressionVisitors** — SqlTranslating、Having、BoolOptimizing、QueryableMethodNormalizing、QueryTranslationPostprocessor。
- **Storage TypeMapping** — 专用 Bool/Decimal/Double/Float/String/DateTime/DateOnly/TimeOnly/ByteArray/Guid 映射与 `XuguRelationalTypeMappingSource` 注册表模式。
- **Extensions** — MigrationBuilder、Key/Entity/Table/Model Builder 增量；`ServerVersion.AutoDetect`；`EnableRetryOnFailure` API 入口（实现 defer）。
- **Migrations** — Identity PK 类型变更 NotSupported、FK 全动作、列重命名/类型变更、Scaffolding CodeGenerator 对齐 Pomelo。
- **SequentialGuid** — `XuguSequentialGuidValueGenerator` + Selector 注册。
- **Phase 9 测试基础设施（起步）** — `XuguTestStore`、`XuguTestStoreFactory`、`XuguSharedStoreFixture`；`docs/TESTING.md`。

### Changed

- **版本** — `1.0.0` → **`1.1.0-preview`**。
- **Provider 规模** — **120** `.cs`（~62% Pomelo 9.0.0 文件数；JSON/NTS/Collation intentionally skip）。
- **测试套件** — **207** 测试（Phase 7: 141 → Phase 8 Wave 5: 207）。

### Deferred (documented)

- `XuguRetryingExecutionStrategy`、`ConvertTimeZone`、Native Linux RID、参数内联（8.Q14）、DateOnly/TimeOnly SaveChanges 驱动绑定 — 见 [LIMITATIONS.md](LIMITATIONS.md)。

---

## [1.0.0] — 2026-07-06

First **production-ready** release. Builds on `0.1.0-preview` (Phase 0–6) with Phase 7 query pipeline, storage mappings, documentation, and release tooling.

### Added

- **ExecuteDelete / ExecuteUpdate** — Core bulk DML via `XuguQuerySqlGenerator` (single-table and MySQL-style multi-table JOIN). Smoke tests: `ExecuteDeleteTests`, `ExecuteUpdateTests`.
- **Query compilation pipeline** — `XuguQueryCompilationContext`, translation pre/postprocessors, `XuguEvaluatableExpressionFilter`, `XuguSqlTranslatingExpressionVisitor`, `XuguQueryableMethodTranslatingExpressionVisitor`, `XuguCompiledQueryCacheKeyGenerator`.
- **Type mappings** — Dedicated mappings for `int`, `long`, `decimal`, `bool`, `DateTime`, `Guid`, `uint`, `ulong`, `TimeSpan` (see [LIMITATIONS.md](LIMITATIONS.md) for unsigned/GUID notes).
- **Compiled queries** — `CompiledQueryTests` smoke coverage.
- **User documentation** — [GETTING-STARTED.md](GETTING-STARTED.md), [LIMITATIONS.md](LIMITATIONS.md), [xuguclient-dependency-strategy.md](xuguclient-dependency-strategy.md).
- **Release tooling** — `harness/scripts/publish-nuget.ps1` (dry-run default; `-Pack` for `artifacts/` output).

### Changed

- **Version** — `0.1.0-preview` → **`1.0.0`** (no prerelease suffix).
- **Test suite** — **141** tests (from 116 at Phase 6 close).
- **Provider scale** — ~105 `.cs` files vs Pomelo 9.0.0 ~194 (~54% file count; feature parity targeted in Phase 8).

### Deferred (documented, not in 1.0.0)

- **`XuguRetryingExecutionStrategy`** — Driver lacks structured transient error API; see [LIMITATIONS.md](LIMITATIONS.md#自动重试execution-strategy).
- CREATE/DROP DATABASE, column collation, FULLTEXT, `CONVERT_TZ`, JSON/NTS extensions — see [LIMITATIONS.md](LIMITATIONS.md) and Phase 8/9 roadmap.

---

## [0.1.0-preview] — 2026-07-06

Initial preview spanning Phase 0–6.

### Phase 0 — Harness & skeleton

- Solution layout, Pomelo reference clone, `UseXugu()` entry point, harness contracts and agent workflow.

### Phase 1 — Infrastructure & Storage

- `XuguConnection`, `XuguDatabaseCreator`, core type mappings, `CanConnect()`.

### Phase 2 — Metadata & Update

- Model metadata, INSERT/UPDATE/DELETE command generation, CRUD integration tests.

### Phase 3 — Query

- LINQ translation baseline, method/member translators, `XuguQuerySqlGenerator` (SELECT/WHERE/JOIN/LIMIT).

### Phase 4 — Migrations & Design

- `dotnet ef` migrations, scaffolding hooks, design-time factory support.

### Phase 5 — Extensions

- Fluent API extensions (`HasCharSet`, index prefixes, etc.) aligned with Pomelo subset.

### Phase 6 — Testing & packaging

- **116** tests, `.resx` error strings, NuGet pack with `UseLocalXuguDriver` dual mode, `ci-build.ps1 -Pack`, native `xugusql.dll` in `runtimes/win-x64/native/` when available.

---

## Links

| Document | Description |
|----------|-------------|
| [GETTING-STARTED.md](GETTING-STARTED.md) | Install, connection string, migrations |
| [LIMITATIONS.md](LIMITATIONS.md) | Known limits and defer/skip list |
| [xuguclient-dependency-strategy.md](xuguclient-dependency-strategy.md) | Local driver vs NuGet `Xuguclient` |

[2.0.x]: https://github.com/your-org/xuguefcore/compare/v2.0.0...v2.0.x
[2.0.0]: https://github.com/your-org/xuguefcore/compare/v1.1.0-preview...v2.0.0
[1.0.0]: https://github.com/your-org/xuguefcore/compare/v0.1.0-preview...v1.0.0
[0.1.0-preview]: https://github.com/your-org/xuguefcore/releases/tag/v0.1.0-preview
