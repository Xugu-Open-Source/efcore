# Microsoft.EntityFrameworkCore.Xugu — 已知限制

> **面向用户**：本文列出 **9.0.0** 的功能边界与平台范围。使用前请先阅读 [GETTING-STARTED.md](GETTING-STARTED.md) 与 [USER-GUIDE.md](USER-GUIDE.md)。  
> **版本**：9.0.0（对齐 EF Core 9.0.x；含原 3.0.0 GA + 3.0.1 runtime-gap + Phase 13 + Post-GA hardening）

若你遇到文档未列出的行为，请在 GitHub 仓库提交 Issue，并附上 XGCI 错误码与复现步骤。

## 测试门禁（L1 / L2 / L3）

| 层 | CI 触发 | 说明 |
|----|---------|------|
| **L1 Unit** | 每次 PR / push | 无 DB；SQL 金标 + NotSupported |
| **L2 Integration** | `main` / nightly / `v*` tag | **native 全量 + compat 全量**（均 `XUGU_REQUIRE_DATABASE=true`） |
| **L3 Experiential** | nightly / `v*` tag | NuGet 消费 + `dotnet ef` + MinimalApi 冒烟 |
| **L4 Functional（Spec 矩阵）** | 本地 / 独立验收 | Pomelo Comparable Set 移植（~8500+）；**9.0.0 Wave A 不宣称全绿** |

详见 [TESTING.md](TESTING.md)。PR **不**跑 L2；主干与发布强制双方言完整集成。

**9.0.0 Wave A（发布门禁，仍有效）**：Unit **0 FAIL**；Integration **0 FAIL**（原独立验收 13 项已闭合）；Functional **仅** APPLY/LATERAL（`ApplyNotSupported`）集群必须 Skip — 其余 FAIL **不**计入 Wave A 发布阻塞。见 [RELEASE-SCOPE.md](RELEASE-SCOPE.md#900-wave-a-验收release-acceptance-wave-a)。

**9.0.0 工作区 Functional 成熟度（2026-07-28，非完全体门禁）**：class-isolated native 矩阵约 **7524 passed / ~105 failed / 509 skipped**（`artifacts/live-db/matrix-native-current`）。经典方言服务器炸码 FAIL（APPLY / E17010 / E19132 / E19196）已清零；`PrimitiveCollections*` **0 FAIL**（硬限制项为带证据 Skip）。**仍不宣称** Pomelo Comparable Set 双模式 0 FAIL。

## 方言模式（9.0.0）

| 模式 | 默认 | Identity 回读 | 说明 |
|------|------|---------------|------|
| **Xugu 原生** | **是** | `INSERT` + `SELECT … WHERE id = LAST_INSERT_ID()` | 产品默认；不执行 `SET compatible_mode` |
| **会话兼容模式** | 否（opt-in） | 同上 | `EnableCompatibleModeOnOpen(XuguCompatibleMode)` → `SET compatible_mode TO 'MYSQL'\|'ORACLE'\|'POSTGRESQL'` |

**标识符折叠**（仅会话行为，**不是** SQL 方言切换）见官方 `compatible_mode.md`：NONE/ORACLE→大写；MYSQL→不转换；POSTGRESQL→小写。

**明确不承诺**：ORACLE/POSTGRESQL 模式 **不** 提供 Oracle/PostgreSQL SQL 方言翻译；禁止据此宣称「可零改动迁移」。

**ADO 说明**：XuguDB 支持 `INSERT … RETURNING`，但当前 **XuguClient** 不通过 `DbDataReader` 暴露 RETURNING 行（`FieldCount=0`）。Provider 使用 `LAST_INSERT_ID()`；见 [docs/contracts/ado-driver-contract.md](contracts/ado-driver-contract.md)。

`COMPATIBLE_MODE=MYSQL` **不是**产品目标或迁移承诺。Pomelo 移植测试在 CI 中通过 `XUGU_DIALECT_MODE=compat` 运行。

## 应用能力矩阵（Phase 13）

审核关键路径常驻门禁：`Category=AppCapabilityMatrix` + `scripts/run-app-matrix-gate.ps1`（native+compat，`XUGU_REQUIRE_DATABASE=true`）。样例：`samples/AppCapabilityMatrix`。驱动读写契约见 `docs/contracts/ado-driver-contract.md`。

## 自动重试（Execution Strategy）

**状态：done（Phase 10 Wave 4 — 10.106）**

Provider 默认注册 `XuguExecutionStrategy`（`RetriesOnFailure => false`）。调用 `EnableRetryOnFailure()` 时注册 `XuguRetryingExecutionStrategy`，通过解析 `Exception.Message` 中的 XGCI 码判定瞬态错误。

### 实现说明

| 项 | 说明 |
|----|------|
| 瞬态检测 | `XuguTransientExceptionDetector` 解析 `[E19886]`、`[E19887]`、`[E32506]`、`[E34301]`；以及 `[E34501]` 且 `sqlexecure/sqlexecute err:` 后无服务端正文（`DROP SESSION` / 会话被杀常见表面） |
| **非瞬态** | `[E34304]` / `[E34305]`（IP/Port 或连接串无效）为配置/打开失败，**不**进入 `EnableRetryOnFailure` 重试；带真实 SQL 正文的 `[E34501]` 亦不重试 |
| 重试前清理 | `XuguRetryingExecutionStrategy.OnRetry` 关闭失效连接，避免复用已死 native handle |
| 驱动限制 | 驱动仍抛出 `System.Exception`，无 `DbException.ErrorCode` / `IsTransient`；Message 中存在历史拼写 `sqlexecure` |
| `errorNumbersToAdd` | Pomelo 兼容参数 **忽略**（Xugu 使用字符串 XGCI 码，非数字 error number） |

### 残余风险

若驱动团队变更 Message 中 XGCI 码格式，瞬态判定可能失效。长期建议驱动提供 `XuguException` + 结构化 `IsTransient`。  
测试基建（`XuguTestConnection`）可对 E34304/E34305 做会话争用退避，**与生产瞬态策略解耦**。

**测试**：`ExecutionStrategyTests`（Unit）；`RetryFaultInjectionTests`（拦截器注入 `[E19886]`）；`RetryServerDisconnectTests`（实库 `DROP SESSION` + `USERENV('SID')`，Category=`QualityMatrix`）。

详见 `docs/references/retrying-execution-strategy.md`。

## DateOnly / TimeOnly SaveChanges

**状态：done（Phase 12 W3 — 12.304；Post-GA 物化闭环 2026-07-13）**

| 能力 | 查询（LINQ） | SaveChanges（INSERT/UPDATE） | 实体物化 / Include |
|------|-------------|------------------------------|-------------------|
| `DateOnly` | **支持** | **支持**（`XuguDateOnlyTypeMapping`） | **支持**（string converter 适配官方驱动） |
| `TimeOnly` | **支持** | **支持**（默认 store `TIME(3)`） | **支持**（同上） |

Phase 12 早期仅覆盖 LINQ 过滤/投影；**完整 SaveChanges 往返 + Include 含 `DATE` 列物化** 由 Post-GA `RuntimeGap` / `BuiltInDataTypesTests` 实库闭合（native/compat）。**未**修改 `external/csharp-driver`。

## DateTimeOffset

**状态：done（Post-GA Provider 适配，2026-07-13）**

| 能力 | 说明 |
|------|------|
| Store 类型 | `DATETIME WITH TIME ZONE` / `TIMESTAMP WITH TIME ZONE`（**不**生成未经文档支持的 `DATETIME WITH TIME ZONE(n)`） |
| 参数 / 字面量 | Provider 以 invariant 字符串携带偏移（如 `… +08:00`）写入 |
| 读回 | 兼容驱动常见短年（`3-03-01…`→年 3）、`…+8`（单数字小时）与 `…+08:00` |
| 驱动边界 | `XGDbType` **无** `DateTimeOffset`/`DATETIME_TZ` 绑定；`GetValue` 对 `XG_C_DATETIME_TZ`(14) 抛「未支持的列转化类型」。物化走 `GetString`（`VarChar`）+ `XuguTemporalValueConverters` |
| 精度 / 亚小时偏移 | **驱动 `GetString` 截断亚秒**，且偏移只保留整小时（写 `+01:30` → 读 `…+1`，墙钟不变）。`CAST(ts AS VARCHAR)` 可能保留 `+01:30`，但 EF 物化不走 CAST。测试用 `XGTestHelpers.GetExpectedValue` 对齐该行为 |

验证：`RuntimeGapBaselineTests` DateTimeOffset SaveChanges 往返 PASS；GearsOfWar `Timeline` 物化期望值按整小时偏移 + 秒级精度对齐。

**未闭环（驱动限制，已 Skip）**：`Timeline = …` / `Contains` 过滤在实库常返回 0 行。根因见驱动缺陷 **DRV-04/05/06**（`GetValue` 无 TZ case、`GetString` 截断、无原生 TZ 绑定）；完整登记与探针表：

`E:\Work\worksummary\xuguefcore-production-docs\XuguClient-驱动缺陷汇总-EF联调发现.md`

仓库契约交叉引用：`docs/contracts/ado-driver-contract.md`（DateTimeOffset 读/写/相等过滤行）。ADO 直绑**写入时完整字符串**可为 1，但 `TIMESTAMP '…'` 字面量与 `GetString` 截断往返常为 0——Provider **无可靠旁路**，GearsOfWar 相关过滤测试保持 `Driver limitation (DRV-06)` Skip。

## byte[] Contains / First / 索引（BLOB）

**状态：done（Provider HEX 旁路，2026-07-21）**

| 场景 | 驱动 / 服务端事实 | Provider 处理 |
|------|-------------------|---------------|
| `LOCATE` / `ASCII` 直接作用于 `BLOB` | E10049 函数不存在 | 不用 ASCII；`HEX(blob)` 后处理 |
| `CAST(tinyint AS BLOB)` | E17007 不能转换 | Contains 针值走 `LPAD(HEX(n),2,'0')` |
| `byte[].Contains(byte)` | `LOCATE` 在 BINARY 上按十六进制文本定位 | `LOCATE(LPAD(HEX(n),2,'0'), HEX(src)) > 0` |
| `byte[].First` / `bytes[i]` | ASCII 对 HEX 文本首字符错误 | `CAST(CONV(SUBSTRING(HEX(src), i*2+1, 2), 16, 10) AS …)` |

依据：`csharp-driver-v3.3.4-cyj` `XGDataReader.GetValue`（BLOB→`XGBlob`）；实库探针确认上述 SQL。

## COUNT / DateDiff 标量（驱动 E34412 适配）

**状态：done（Post-GA Provider CAST，2026-07-13）**

官方驱动对高精度聚合/`TIMESTAMPDIFF`（官方返回 `BIGINT`）走 `GetInt32` 时可能抛 `E34412`。Provider 侧：

| 场景 | SQL / 类型处理 |
|------|----------------|
| `Count` / `LongCount`（含 GroupBy、导航） | `CAST(COUNT(…) AS INTEGER\|BIGINT)` |
| `EF.Functions.DateDiff*` | `TIMESTAMPDIFF` 按 `BIGINT` 生成，公共 `int` API 末端再转 `INTEGER` |

验证：`RuntimeGapBaselineTests`（Category=`RuntimeGap`）native/compat **9/9** PASS。驱动上游仍可改进 `GetInt32`；当前 Provider 兜底已足够应用路径。

## Collation（排序规则）

**状态：excluded-with-evidence（Phase 12 W4 — OOS-03）**

XuguDB 文档未提供与 MySQL 等价的列级/表级 Collation Fluent API。以下 Pomelo 能力 **永久不实现**：

| Pomelo API | 处置 | 文档依据 |
|------------|------|----------|
| `HasCollation(...)` | **excluded** — `8.E4` | `reference/system-configuration-parameter/session-parameter/char_set.md` |
| `XuguCollationAttribute` | **excluded** — `8.DA2` | 同上；JDBC 不支持列级排序规则 |
| 表级 charset Fluent | **excluded** | 连接级 `CHAR_SET=UTF8`（`XuguModelBuilderExtensions` 文档注释） |

字符集在连接级通过 `CHAR_SET=UTF8`（或等价参数）配置，非 EF 模型注解。

## NetTopologySuite / Spatial

**状态：excluded-with-evidence（Phase 12 W4 — OOS-01）**

XuguDB 服务端有 `spatial-database/` SQL 能力，但 **无** EF Core NetTopologySuite 集成包或 ORM 映射文档。Pomelo 对等能力在独立 `EFCore.MySql.NTS` 包中。

| 能力 | 处置 |
|------|------|
| `geometry` / `geography` CLR 映射 | **不实现** |
| `SpatialMySqlTest` / `SpatialQuery*` | **不 port**（OUT OF SCOPE） |
| 空间索引 RTREE 迁移 | `NotSupportedException`（与 FULLTEXT 同类） |

## FULLTEXT 索引 / Match 查询

**状态：excluded-with-evidence（Phase 12 W4 — OOS-02）**

| 能力 | 处置 | 文档依据 |
|------|------|----------|
| `CREATE … FULLTEXT INDEX` | **NotSupported** | `reference/object/indexes.md` — 「目前不提供全文索引相关的内容」 |
| `MATCH … AGAINST` / `IsMatch` | **不实现** | 同上 |
| 文本搜索替代 | **`REGEXP_LIKE`** | `XuguRegexIsMatchTranslator`（8.Q15 done） |

## CONVERT_TZ / ConvertTimeZone

**状态：excluded-with-evidence（Phase 12 W4 — OOS-04）**

| 能力 | 处置 |
|------|------|
| `ConvertTimeZone` DbFunction | **不翻译** |
| `DateTimeOffset.LocalDateTime` LINQ | 客户端求值 |
| 时区配置 | 库级 `def_timezone` / 会话参数 |

## Scaffolding / catalog privilege (E18012)

**状态：fixed（Wave A — Task 4）**

Scaffolding、`HasTables`、迁移 history 探测与 Integration 元数据断言统一走 **`ALL_*` 系统视图**（`ALL_TABLES` / `ALL_COLUMNS` / `ALL_INDEXES` / `ALL_CONSTRAINTS` / `ALL_VIEWS`），**不**再查询 `SYS_*` 字典表或依赖 `DBA_*`。

文档依据：`reference/system-view/all/all.md` — ALL 视图只需普通用户权限（无需 DBA）；`DBA_*` 需 DBA 权限。验收中 `ColumnExists` 查 `SYS_COLUMNS` 会稳定触发 `E18012 权限不够`。

应用用户只要能读本库 `ALL_*`（自建对象默认可见）即可完成 scaffold / migrate 探测；**无需**再 GRANT `DBA_*`。

## Scaffolding Baselines

**状态：excluded-with-evidence（Phase 12 W4 — OOS-05）**

Pomelo `Scaffolding/Baselines/**` 全量快照（86 文件）**不 port**。主路径由 `ScaffoldingIntegrationTests` + `ScaffoldingExtendedTests` 覆盖。

## ExecuteDelete / ExecuteUpdate

**状态：done（核心路径）**

Provider 支持 EF Core `ExecuteDelete()` / `ExecuteUpdate()` **核心**路径：

- 单表谓词过滤
- MySQL 风格多表 JOIN（无 `LIMIT`/`ORDER BY`/`DISTINCT`/`GROUP BY` 的受限 `SelectExpression`）

SQL 生成见 `XuguQuerySqlGenerator`；验证见 `ExecuteDeleteTests` / `ExecuteUpdateTests` / `ExecuteBulkBoundaryTests`（Category=`QualityMatrix`）。

### 不支持或边缘场景（13.205 清单）

| 场景 | 处置 | 测试 / 文档 |
|------|------|-------------|
| TPC / TPT 继承层次批量 DML | **拒绝**（翻译或服务器 SQL 失败，如 CROSS） | `ExecuteBulkBoundaryTests` TPC_* / TPT_* |
| TPH 单表 ExecuteUpdate/Delete | **支持**（核心路径） | `ExecuteBulkBoundaryTests.TPH_single_table_*` |
| Owned 共享列 `Select(o => o.Profile).ExecuteUpdate` | **支持**（表共享 owned） | `Owned_shared_column_ExecuteUpdate_via_select_*` |
| Owned `ExecuteDelete` / 导航集合目标 | **拒绝** | `Owned_type_ExecuteDelete_*` / `navigation_collection_*` |
| 导航 JOIN UPDATE（`o.Customer.City`） | **拒绝**（生成 CROSS，E19132） | 用 FK 谓词代替 |
| FK 谓词单表 ExecuteUpdate | **支持** | `Single_table_ExecuteUpdate_by_fk_filter_*` |
| `ORDER BY` / `LIMIT` / `DISTINCT` / `GROUP BY` 源 | **拒绝**（翻译或服务器错误） | `ExecuteBulkBoundaryTests` / Execute*Tests |
| `CROSS APPLY` / `OUTER APPLY` / `LATERAL` | **拒绝**（`XuguStrings.ApplyNotSupported`；实库 E19132） | Northwind BulkUpdates `*_apply_*`；Functional APPLY/LATERAL overrides 已 `[Skip]`（含 TPC ManyToMany residual 3 methods × tracking/no-tracking）；`from.md` 无 APPLY/LATERAL |
| `FROM` 子查询引用上级查询表达式 | **拒绝**（实库 **E17010**：`From部份的子查询不能使用上级查询的表达式`） | 表子查询文档 `reference/sql/select/subquery.md`；Functional 相关 override 已 Skip（ComplexNavigations Contains、Owned Distinct/Union、TPC/TPT GearsOfWar 子查询簇） |
| 内联集合 `VALUES` 派生表 | **已修复生成**（`GenerateValues` → `SELECT … UNION ALL SELECT …`；拒绝 EF 默认 `UNION ALL VALUES`） | `set.md` UNION ALL；`insert.md` 多行 VALUES 仅 DML |
| `UPDATE`/`DELETE` … `CROSS JOIN` | **拒绝**（实库 E19132 unexpected CROSS；SELECT 侧 `CROSS JOIN` 有文档） | Northwind BulkUpdates `*_cross_join_*` 负向断言 |

需要上述能力时请使用常规 `SaveChanges` 或显式 SQL。

## Identity 主键类型变更（迁移）

**状态：NotSupported**

将 **IDENTITY 列**（`UseXuguIdentityColumn` / `INTEGER IDENTITY(1,1)`）作为主键的列 **变更 CLR/存储类型** 时，`XuguMigrationsSqlGenerator` 抛出 `NotSupportedException`（`XuguStrings.IdentityPrimaryKeyTypeChangeNotSupported`）。

**原因**：XuguDB IDENTITY 列重建策略与 Pomelo `AUTO_INCREMENT` 不同；自动迁移可能导致数据丢失。

**替代方案**：手工迁移（新列 + 数据拷贝 + 切换主键 + 删除旧列），或新建表迁移。

## 无符号整数

XuguDB 文档（`reference/sql/datatype/numerical.md`）仅定义有符号整数。Provider 映射：

| CLR | 存储类型 | 说明 |
|-----|---------|------|
| `uint` | `BIGINT` | 最大值 4_294_967_295 在 BIGINT 范围内 |
| `ulong` | `NUMERIC(20,0)` | 超出 BIGINT 上限时使用 NUMERIC |

## GUID 存储

CLR `Guid` 默认映射 XuguDB 原生 `GUID`（16 字节），非 MySQL 风格 `CHAR(36)`。见 `reference/sql/datatype/guid.md`。

## 乐观并发与 ROW_COUNT()

**状态：已支持自动 `DbUpdateConcurrencyException`（Path A — RecordsAffected，2026-07-20）**

| 能力 | 状态 |
|------|------|
| 并发 token 列映射 / UPDATE 含 token 列 | **支持** |
| `DbUpdateConcurrencyException` 自动检测 | **支持** — `XuguModificationCommandBatch` 读 `DbDataReader.RecordsAffected`（UPDATE/DELETE） |
| `ROW_COUNT()` SQL 函数 | 仍 **E10049**（不依赖；VT-XUGU-ROWCOUNT-001 可作后续简化） |

**驱动注意**：参数化 `INSERT` 经 `ExecuteReader` 时 `RecordsAffected` 恒为 0（`ExecuteNonQuery` 正常）。Provider 对 `EntityState.Added` 不做「0 行」并发误报。多语句 batch 末尾 `SELECT` 会冲掉 DML 的 affected 计数，故不再追加 `SELECT 1`。

验证：`AffectedRowsProbeTests`、`OptimisticConcurrencyTests.Stale_concurrency_token_throws_DbUpdateConcurrencyException`。

## 平台支持（Windows / Linux RID）

**状态：signed-off platform exclusion（Phase 12 W5 — 12.509/PLAT-02）；Wave A 仅签 Windows 试用**

| 平台 | RID | 状态 |
|------|-----|------|
| Windows x64 | `win-x64` | **支持** — `xugusql.dll` 随 NuGet / 构建输出 |
| Linux x64 | `linux-x64` | **signed-off blocked** — 官方驱动长期无 `libxugusql.so`；VT-XUGU-LINUXRID-001 |

### 原生库与 NuGet 驱动（Xuguclient）

| 项 | 说明 |
|----|------|
| **NuGet 依赖** | 发布包 nuspec 依赖 **`Xuguclient`**（pack 时 `UseLocalXuguDriver=false`，当前 `VersionOverride=3.3.6-bionic`） |
| **托管层** | `XuguClient.dll` 由 `Xuguclient` 包还原 |
| **原生层** | P/Invoke 加载 `xugusql.dll`（Windows）或 `libxugusql.so`（Linux，若存在） |
| **发布包 native 策略（9.0.0 Wave1）** | `dotnet pack -p:UseLocalXuguDriver=false` **不再**把仓库本地 `xugusql.dll` 打进 Provider nupkg；消费方只使用 **`Xuguclient` 传递依赖** 自带的 native 资产 |
| **本地开发构建** | `UseLocalXuguDriver` 默认 true：仍可把 `XuguNativeDllPath` 的 `xugusql.dll` **复制到输出目录**并可选打进本地调试包；**不等于**发布包策略 |
| **Wave A 范围** | **仅 Windows x64 已验收**；连接串须含 `CHAR_SET=UTF8`（驱动默认 GBK 会导致中文/重音乱码） |

**预备**：`NativeAssets.props` + `EFCore.Xugu.csproj` 条件 `runtimes/linux-x64/native/` 打包（`.so` 存在时自动启用）。`Xuguclient` **3.3.6-bionic** 预发布包**可能**含 Linux `.so` 资产，但 **Wave A 未在 Linux 实库验收**，**不得**据此宣称 Linux 生产就绪。

**CI**：GitHub Actions 实库 job 为 **Windows-only**（12.508 signed-off）；Linux agent 待驱动发布稳定 `.so` 且完成实库矩阵后添加。

详见 `docs/references/platform-limitations-signed-off-12.509.md` 与 [xuguclient-dependency-strategy.md](xuguclient-dependency-strategy.md)。

## JSON 列（EF Core 映射）

**状态：done（11.109 Wave 2）— TypeMapping + DDL + LINQ/函数翻译 + 实库冒烟**

XuguDB **服务端**支持原生 `JSON` 列类型（LOB，最大 2GB）、`->` / `->>` 路径运算符及 28+ JSON 函数（`reference/sql/datatype/json.md`、`reference/function/json-functions/`）。

| 能力 | XuguDB | Provider 2.1.0 |
|------|--------|------------------|
| `CREATE TABLE … col JSON` | **支持** | **`XuguJsonTypeMapping` + DDL `JSON`（11.109a）** |
| `EF.Functions.JsonValue/JsonExtract/JsonContains*` | SQL 层支持 | **`XuguJsonDbFunctionsExtensions` + Translator（11.109b）** |
| `JsonScalarExpression` / `->` / `->>` | SQL 层支持 | **`XuguQuerySqlGenerator`（11.109b）** |
| Fluent `HasXuguJsonColumn()` | — | **done**（11.109c） |
| 实库 INSERT + JSON 函数查询 | — | **`JsonIntegrationTests`（11.109d；SkippableFact）** |
| 整列 JSON LOB 实体物化（`SELECT payload`） | 驱动 `String`/LOB | **边界（13.206）** — 小文档可通；大 LOB/`GetString` 可能失败；**推荐** `JsonValue`/`JsonExtract` 投影 |
| Pomelo `MySqlJson*` / `Json*MySqlTest` 全矩阵 | — | **skip** |
| EF `ToJson()` owned JSON 列 | — | **不承诺** |

**变通**：查询优先 `EF.Functions.JsonValue` / `JsonExtract`；整列读取仅适合小 JSON；见 `ado-driver-contract.md` G-06。  
**测试**：`JsonIntegrationTests`（小文档/函数）；`JsonBoundaryTests`（大 LOB 边界 + `ToJson` 非支持路径，Category=`QualityMatrix`）。


## Primitive collections（LINQ 参数 / 列）

**状态：部分支持（9.0.0 Wave5 加固，2026-07-28）**

XuguDB 有 JSON 标量函数（`JSON_LENGTH` / `JSON_VALUE` / `JSON_CONTAINS` 等），但 **无** 文档化的 `JSON_TABLE` / `unnest` 行集 API；实库 `JSON_TABLE(...)` 探测为 **E19132**。因此 EF 原始集合能力分层如下：

| 场景 | 状态 | Provider 处理 |
|------|------|----------------|
| 参数集合成员判定（`list.Contains(col)` / 取反） | **支持** | `XuguQuerySqlGenerator.GenerateIn` → 带长度/JSON-null 防护的标量 JSON 谓词；参数经 `XuguPrimitiveCollectionTypeMapping` 序列化为 JSON 文本 |
| `OpenJson` / 行集展开（`SelectMany`、集合投影为行、部分 `ElementAt`/集合运算） | **不承诺** | `ServerVersionSupport.JsonTable = false`；相关 Functional 用例 **Skip**（证据：无 JSON_TABLE + E19132） |
| 列上 JSON 数组的复杂集合运算 / 嵌套 Contains | **Skip/defer** | 见 Functional `PrimitiveCollections*` / `NonSharedPrimitiveCollections*` Skip 原因 |
| 空内联集合抛错契约、部分 float/Guid/DateTime 数组物化 | **Skip** | 方言/驱动边界；带原因属性 |

**推荐**：参数侧 membership 用 LINQ `Contains`；需要行集语义时在应用层展开，或用 `EF.Functions.JsonValue` / `JsonExtract` 做标量投影。

**依据**：`reference/sql/datatype/json.md`、`reference/function/json-functions/`；实库 SYSTEM@5287（XuguDB 12）探测；矩阵见任务 `07-24-full-functional-remediation` Wave5c/5d。

**测试**：`PrimitiveCollectionsQueryXuguTest` / `NonSharedPrimitiveCollectionsQueryXuguTest`（参数 membership 矩阵绿；行集硬限制 Skip）。

## Sequence（序列）

**状态：Migrations DDL done（2026-07-21）** — 文档 `reference/object/sequence.md`

| 操作 | 状态 |
|------|------|
| `CREATE SEQUENCE`（START/INCREMENT/MIN/MAX/CYCLE） | **支持** — `XuguMigrationsSqlGenerator`（无 `AS type`） |
| `DROP SEQUENCE IF EXISTS` | **支持** |
| `ALTER SEQUENCE` 选项 | **支持**（`NOMINVALUE` / `NOMAXVALUE` / `NO CYCLE`） |
| `RESTART WITH` | **NotSupported** — 文档 Alter 无此子句 |
| Sequence HiLo 值生成器 | **未做**（可用 `seq.NEXTVAL` 手工/默认值） |

**测试**：`MigrationSequenceSqlTests`（Unit）；`SequenceIntegrationTests`（实库 NEXTVAL）。


## Functional Spec 矩阵 residual（9.0.0）

**状态：已知边角 / 不阻塞 Wave A**

在 APPLY/E17010/E19132 硬失败清零后，剩余 Functional FAIL/Skip 主要集中在：

| 类别 | 说明 | 用户影响 |
|------|------|----------|
| 复杂导航 / GroupJoin / 多级 Include 结果序或计数 | ComplexNavigations* residual；部分已 Skip | 常规 CRUD/简单 Include 通常不受影响 |
| TPC + skip navigation 左连接键序 | 如 `Left_join_with_skip_navigation`（`1_2` vs `1_1`） | 少见继承+多对多边角 |
| Owned 空导航 / 相关集合投影语义 | Owned residual Skip | 复杂 owned 图查询 |
| SQL 金标 / 共享 SYSTEM 表前缀噪声 | 测试基建；部分 AssertSql 已 no-op 或 deferred | **不影响**应用运行时 |
| `Check_all_tests_overridden` | override 面不完整，非运行时缺陷 | 仅测试卫生 |
| string `FirstOrDefault`/`LastOrDefault` | **已修复** → `SUBSTRING` | — |
| `TimeSpan.Milliseconds` 物化 E34412 | **已修复** → `MICROSECOND/1000` 后 `Convert(int)` | — |
| FromSql 裸表名 | 测试侧应使用 `FormatTableName` 前缀；应用应使用真实表名/映射 | 共享库测试基建问题 |

完整 burn-down 与独立套件 0 FAIL 属于 **full Functional（definition A）** 后续工作，**不是** 9.0.0 Wave A 发布阻塞项。

## 其他 excluded / blocked（摘要）

| 能力 | 处置 |
|------|------|
| `CREATE DATABASE` / `DROP DATABASE` | **excluded** — 运维手工建库 |
| NetTopologySuite / Spatial | **excluded** — OOS-01 |
| `FULLTEXT` 索引 / `Match` 查询 | **excluded** — OOS-02 |
| `CONVERT_TZ` / `ConvertTimeZone` | **excluded** — OOS-04 |
| Scaffolding Baselines 全量快照 | **excluded** — OOS-05 |
| Native Linux RID 打包 | **signed-off blocked** — PLAT-02 / VT-XUGU-LINUXRID-001 |
| `ROW_COUNT()` SQL 函数 | 仍 **E10049**；乐观并发已改用 `RecordsAffected` Path A（见上文） |
| Sequence `RESTART WITH` / HiLo | Restart **NotSupported**；HiLo 未做 |
| 参数内联（OFFSET） | **done** — `10.201` |

详见 git 历史中原 `harness/references/out-of-scope-approved-12.409.md` 与 `docs/contracts/stub-and-exclusion.contract.md`。
