# XuguDB EF Core Provider 与 MySQL/Pomelo 对照参考

## 对照参考 · 非迁移目标 · 非虚谷方言定义

> **版本**：Microsoft.EntityFrameworkCore.Xugu **9.0.0**（对齐 EF Core 9.0.x；方言迭代基线）
> **对照基准**：Pomelo.EntityFrameworkCore.MySql **9.0.0**（EF Core 9）— **仅 C# 架构参考**
> **更新**：2026-07-19（关闭 Post-GA 文档漂移 12.PG4；口径对齐 3.0.x）

> **⚠️ 本文档定位（必读）**
>
> | 本文 **是** | 本文 **不是** |
> |------------|--------------|
> | 熟悉 MySQL/Pomelo 的开发者的 **技术对照参考** | XuguDB SQL 方言的 **权威定义** |
> | 能力差异与选型边界的 **说明** | 「零改动迁移到 Xugu」的 **目标清单** |
> | Pomelo 测试覆盖差距的 **速查** | 应以 MySQL 语法 **首要编写** 应用的指南 |
>
> **SQL 方言唯一权威**：`E:\BaiduSyncdisk\docs\content\` → [RELEASE-SCOPE.md](RELEASE-SCOPE.md) → `docs/contracts/sql-dialect.contract.md`  
> `COMPATIBLE_MODE=MYSQL` 仅为开发/对照便利，**不是**产品承诺。新应用请编写 **Xugu 原生 SQL** 与 EF 映射。

本文面向并列评估 XuguDB 与 MySQL/Pomelo 的开发者，说明能力差异、SQL 方言、测试覆盖与选型建议。技术细节以 [LIMITATIONS.md](LIMITATIONS.md)、[RELEASE-SCOPE.md](RELEASE-SCOPE.md) 与 `docs/contracts/sql-dialect.contract.md` 为准。
---

## 执行摘要

| 维度 | Xugu 3.0.x | Pomelo / MySQL |
|------|------------|----------------|
| 架构对齐 | 目录与服务注册对齐 Pomelo 9.0.0 | 参考实现 |
| 源码文件 | 物理 **~140** `.cs`；disposition **194/194**（含 EF-base / excluded） | 194 .cs |
| FunctionalTests | compat **1057** / native **1056**；Adjusted **111.0%**（分母 952） | ~1050 |
| 核心 CRUD / LINQ / 迁移 | **支持** | 支持 |
| ExecuteDelete / ExecuteUpdate | **核心路径支持**（Owned/多表 ORDER·LIMIT 见 LIMITATIONS） | 支持 |
| JSON / Spatial / FULLTEXT | JSON **✅**；Spatial/FULLTEXT **excluded** | 支持 |
| 自动重试（Retry） | **支持**（Message 解析 XGCI 码） | `EnableRetryOnFailure` |
| 乐观并发异常检测 | **支持**（Path A：`RecordsAffected`；不依赖 `ROW_COUNT()`；见 13.201 / LIMITATIONS） | `ROW_COUNT()` |
| 连接串 | Xugu 键值对（`IP=…; DB=…`） | MySQL 标准 URI/键值 |
| 自增主键 DDL | `IDENTITY(1,1)` | `AUTO_INCREMENT` |
| GUID 存储 | 原生 `GUID`（16 字节） | 常映射 `CHAR(36)` |

**结论**：Xugu Provider **9.0.x / 3.0.x** 已具备生产级 CRUD、查询翻译、迁移与 Scaffolding **主路径**；乐观并发走驱动 `RecordsAffected`。与 Pomelo 差距主要在 **扩展生态**（NTS/FULLTEXT）、**驱动 workaround 依赖**（COUNT/时态/RETURNING）及 **平台**（Linux RID）。详见 [LIMITATIONS.md](LIMITATIONS.md) 与 [USER-GUIDE.md](USER-GUIDE.md)。

---

## MySQL / Pomelo 支持、Xugu 不支持

> 含永久 **skip**、**blocked**（实库验证不可用）及 **defer**（计划后续）项。

| 类别 | MySQL / Pomelo | Xugu 处置 | 原因 / 文档依据 |
|------|----------------|-----------|----------------|
| JSON 列 / `Json*` Fluent API | 原生 JSON 类型 + 序列化 | **2.1.0 目标**（11.109） | XuguDB **支持**原生 JSON + 28 函数（`json.md`）；EF Provider **2.0.x 未实现** |
| NetTopologySuite / Spatial | `geometry` / `geography` | **skip** | 无 NTS 集成 |
| FULLTEXT 索引 / `Match` / `IsMatch` | `MATCH … AGAINST` | **skip** | 文档无对外 FULLTEXT；用 `REGEXP_LIKE` 替代文本匹配 |
| 列级 / 表级 Collation / `HasCharSet` | Fluent + 迁移 ALTER | **skip** | 连接级 `CHAR_SET`；见 `compatible_mode.md` |
| `CONVERT_TZ` / `ConvertTimeZone` | `CONVERT_TZ(dt, from, to)` | **skip** | XuguDB 无等价函数；时区为库级配置 |
| MySQL `YEAR` 列类型 | `YEAR` | **skip** | XuguDB 无 YEAR 类型 |
| `AUTO_INCREMENT` DDL | 标准语法 | **不同** — 用 `IDENTITY(1,1)` | `reference/object/table/create.md` |
| `INFORMATION_SCHEMA` Scaffolding | 标准元数据视图 | **不同** — `DBA_*` / `ALL_*` | `reference/system-view/` |
| `ROW_COUNT()` SQL 函数 | `SELECT ROW_COUNT()` / `WHERE ROW_COUNT() = n` | **不可用**（E10049） | 不依赖；并发改走 `RecordsAffected` |
| `DbUpdateConcurrencyException` 自动检测 | Pomelo `MySqlUpdateSqlGenerator` | **支持**（Path A） | `XuguModificationCommandBatch` + `RecordsAffected` |
| CREATE/DROP DATABASE（EF API） | 部分支持 | **defer** | 运维手工建库 |
| 过滤索引 `HasFilter` | 迁移生成 WHERE 子句 | **不支持** — Differ 剥离 | DDL 不支持 |
| Identity PK **类型变更**（自动迁移） | DropPrimaryKey + 重建 | **NotSupported** | `XuguMigrationsSqlGenerator` 抛异常 |
| 列 `RENAME COLUMN`（单语句） | MySQL 8+ | **workaround** — ADD+UPDATE+DROP | 无 `RENAME COLUMN` |
| Scaffolding Baselines 全量快照 | Pomelo baseline 文件 | **skip** | 维护成本过高 |
| Lazy loading proxies 测试宿主 | Proxies 包 | **skip** | 无测试宿主 |
| Linux x64 原生 RID 打包 | 多平台 `libmysqlclient` 等 | **defer**（10.205） | 当前 Windows x64 |
| 参数内联（查询性能） | Pomelo 部分路径 | **done**（10.201） | OFFSET 闭包参数内联 |
| FOR UPDATE / 窗口函数（EF Tag） | 部分场景 | **defer**（10.202） | EF 无标准翻译入口 |
| 位运算返回类型修正 | `BitwiseOperationReturnTypeCorrecting` | **defer**（10.203） | 8.Q11 |
| DateOnly / TimeOnly **SaveChanges** | 原生参数绑定 | **defer** | 驱动无原生 `DateOnly`/`TimeOnly` 参数 |
| Pomelo IntegrationTests（Vegeta/ASP.NET） | 性能宿主 | **defer**（10.206） | 低优先级 |
| `errorNumbersToAdd`（Retry 参数） | MySqlConnector 数字错误码 | **忽略** | Xugu 用 XGCI 字符串码，非数字 |

---

## Xugu 支持、MySQL / Pomelo 不支持（或行为不同）

| 类别 | XuguDB / Provider | MySQL / Pomelo | 文档 / 实现 |
|------|-------------------|----------------|-------------|
| `INSERT … RETURNING` | 支持（可利用回读） | MySQL 8.0.21+ 部分支持 | `reference/sql/dml/insert.md` |
| 原生 `GUID` 类型（16 字节） | `GUID` CLR 映射 | 常 `CHAR(36)` 字符串 | `reference/sql/datatype/guid.md` |
| `SYS_GUID()` | LINQ `Guid.NewGuid()` | `UUID()` | `reference/function/uuid-functions/sys_guid.md` |
| `IDENTITY(seed, increment)` | 迁移 / 约定 | `AUTO_INCREMENT` | `create.md#4-opt_serial` |
| `identity_mode` 0/1/2 | NULL/0 插入行为可配 | `sql_mode` 相关 | `def_identity_mode.md` |
| BITMAP 索引 | `INDEXTYPE IS BITMAP` | 不支持 | `reference/object/indexes.md` |
| `COMMENT ON TABLE/COLUMN` | 标准 + CREATE 内联 | `COMMENT=` 子句 | `alter.md` / Migrations |
| `DROP INDEX table.index` | 独立语句 | `ALTER TABLE … DROP INDEX` | `indexes.md` |
| `LOCK TABLE` 迁移锁 | `XuguHistoryRepository` | Pomelo 类似 | `reference/object/table/lock.md` |
| 分页 `TOP n` | LIMIT **与** TOP 均支持 | 主要 LIMIT | `resultset-restricted.md` |
| HAVING 引用 SELECT 别名 | 文档明确支持 | MySQL 部分场景需 workaround | `group-by.md` |
| `REGEXP_LIKE(expr, pattern)` | `Regex.IsMatch` | `expr REGEXP pattern` | `regexp_like.md` |
| `Math.Log(x, base)` 参数序 | `LOG(base, value)` **与 CLR 相反** | `LOG(value, base)` | `log.md` |
| `DateTimeOffset.Now` | `SYSTIMESTAMP()` | `UTC_TIMESTAMP()` 等 | 函数映射表 |
| `DateOnly.ToDateTime` | `MAKE_TIMESTAMP(…)` | `ADDTIME(CAST(…), time)` | Translator |
| 无符号整数 | `uint`→`BIGINT`，`ulong`→`NUMERIC(20,0)` | `UNSIGNED` 类型 | `datatype/numerical.md` |
| Scaffolding `DBA_TABLES` | `VALID='T'`, `IS_SYS='F'` | `information_schema.tables` | `dba_tables.md` |
| XGCI Message 瞬态重试 | `XuguTransientExceptionDetector` | `MySqlException.IsTransient` | `development/xgci/error.md` |
| 连接串格式 | `IP=…; DB=…; USER=…` | `Server=…;Database=…` | 驱动文档 |
| 默认 `SET compatible_mode TO 'MYSQL'` | **2.1.0 起默认关闭**；`EnableCompatibleModeOnOpen()` opt-in | 不需要 | `compatible_mode.md` |

---

## 兼容模式（`compatible_mode=MYSQL`）下的行为差异

> 文档：`reference/system-configuration-parameter/session-parameter/compatible_mode.md`  
> **2.1.0 起默认使用 Xugu 原生方言**（连接打开时 **不** 执行 `SET compatible_mode`）。需 MySQL 对照时调用 `EnableCompatibleModeOnOpen()`。

| 项 | MYSQL 模式下 XuguDB | 仍与 MySQL 不同 |
|----|---------------------|-----------------|
| 标识符大小写 | **不做**词法大小写转换 | 与 MySQL 一致 |
| 反引号标识符 | 支持 | 一致 |
| `LIMIT` / `LAST_INSERT_ID()` | 一般可用 | 一致 |
| `ROW_COUNT()` | **不可用**（E10049） | MySQL 核心并发检测依赖此函数 |
| 自增列语法 | 仍用 `IDENTITY`，非 `AUTO_INCREMENT` | DDL 不同 |
| GUID 存储 | 仍用原生 `GUID` 二进制 | 非 `CHAR(36)` |
| 元数据视图 | 仍用 `DBA_*` / `ALL_*` | 非 `INFORMATION_SCHEMA` |
| 列 Collation Fluent | 仍不支持 | 连接级 `CHAR_SET` |
| 多表 UPDATE + `LIMIT` | **不支持** | MySQL 部分支持 |
| 单表 DELETE 语法 | `DELETE FROM table`（无别名前缀） | MySQL 常用 `DELETE alias FROM` |
| 多表 DELETE | `DELETE FROM t1 FROM t2 …`（双 FROM） | `DELETE t1 FROM t1 JOIN …` |
| `RETURNING` 子句 | Xugu 原生支持 | MySQL 有限支持 |
| FULLTEXT / Spatial / JSON EF 映射 | 仍不可用 / defer | MySQL 原生 JSON Fluent |

**要点**：`COMPATIBLE_MODE=MYSQL` 便于 LINQ→SQL 与 Pomelo 对照开发，**不能**假设 MySQL 专有函数（如 `ROW_COUNT()`、`CONVERT_TZ`）在 XuguDB 上存在；以实库验证与官方文档为准。

---

## 永久 skip 的功能及原因

| Pomelo 源 / 能力 | 原因 | Phase 处置 |
|------------------|------|------------|
| `SpatialMySqlTest` / NTS | 无空间类型生态 | 永久 skip |
| `MatchQueryMySqlTest` / FULLTEXT | 无 `MATCH AGAINST` | 永久 skip |
| `BadDataJsonDeserializationMySqlTest` | 无 EF JSON Provider | skip（2.0.x）；**2.1.0 解锁**（11.109d） |
| `MySqlJson*` 全套 | 无 JSON Fluent / TypeMapping | skip（2.0.x）；**2.1.0 目标**（11.109） |
| Collation / Charset DataAnnotations | 连接级 `CHAR_SET` | 永久 skip（8.E4/DA2） |
| Scaffolding Baselines 快照 | 维护成本 | 永久 skip（10.209） |
| Lazy loading proxies 宿主 | 无 Proxies 测试基础设施 | 永久 skip |

永久 skip 的产品口径以 [LIMITATIONS.md](LIMITATIONS.md) 与 [stub-and-exclusion.contract.md](contracts/stub-and-exclusion.contract.md) 为准；Pomelo 源码树仅作 C# 结构参考：`external/Pomelo.EntityFrameworkCore.MySql/`（原 `pomelo-file-map.md` 无干净历史，已删除）。

---

## Wave 4 相关：ROW_COUNT 与 Retry

| 项 | Pomelo / MySQL | Xugu 2.0.0（Wave 4） | 状态 |
|----|----------------|----------------------|------|
| **10.106 Retry** | `MySqlRetryingExecutionStrategy` + `MySqlException.IsTransient` | `XuguRetryingExecutionStrategy` + `XuguTransientExceptionDetector`（`[E19886]`/`[E19887]`/`[E32506]`；**不含** E34304/E34305） | **done** |
| `EnableRetryOnFailure()` | 三重重载 | 三重重载，注册重试策略 | **done** |
| 默认执行策略 | 可配置 | `XuguExecutionStrategy`（`RetriesOnFailure => false`） | **done** |
| Retry 单元测试 | Pomelo 对等 | +10（`XuguTransientExceptionDetectorTests` + `ExecutionStrategyTests`） | **done** |
| 故障注入 / 策略重试证明 | 有 | **有**（`ExecutionStrategyTests` 注入 `[E19886]`；非实库 idle disconnect） | **done**（残余：无真实断连注入） |
| **10.105 ROW_COUNT** | `SELECT ROW_COUNT()` | 仍 E10049；**不使用** | 函数不可用（与并发解耦） |
| `DbUpdateConcurrencyException` | Pomelo 全通过 | Path A：`RecordsAffected`；`OptimisticConcurrencyTests.Stale_*` | **done** |
| 并发 token 列映射 / UPDATE 含 token | 支持 | 支持 | **done** |

### 代码锚点

- Retry：`src/EFCore.Xugu/Storage/Internal/XuguRetryingExecutionStrategy.cs`、`XuguTransientExceptionDetector.cs`
- 并发：`XuguModificationCommandBatch`（`RecordsAffected`）；`ado-driver-contract.md` ROW_COUNT 行
- 测试：`OptimisticConcurrencyTests`、`AffectedRowsProbeTests`、`ExecutionStrategyTests`

### XuguDB 文档依据

| 能力 | 文档路径 |
|------|----------|
| XGCI 瞬态错误码 | `development/xgci/error.md` |
| ROW_COUNT 不可用 | 实库 E10049（函数不在 XuguDB 函数目录）；非文档假设 |
| LAST_INSERT_ID | MYSQL 兼容模式下用于 identity 回读 |

---

## 功能对等表（速查）

| 能力 | Xugu 2.0.0 | 说明 |
|------|------------|------|
| `UseXugu` / DI 注册 | ✅ supported | 对齐 `UseMySql` 模式 |
| 基础 LINQ（Where/Select/Join/GroupBy/Include） | ✅ supported | Northwind + AdHoc 大矩阵已测 |
| ExecuteDelete / ExecuteUpdate | ✅ supported | 单表 + 多表 JOIN；TPC/TPT/Owned 批量 DML ❌ |
| 迁移（Create/Alter/Drop、索引、FK） | ✅ supported | 列重命名：ADD+UPDATE+DROP workaround |
| Scaffolding | ⚠️ partial | DBA/ALL 视图；无 Collation/Charset 列级还原 |
| SequentialGuid | ✅ supported | 客户端 ticks+random；查询 `SYS_GUID()` |
| 乐观并发 token 列 | ✅ supported | 异常检测 ✅ Path A（`RecordsAffected`） |
| DateOnly / TimeOnly 查询 | ✅ supported | SaveChanges / 物化 ✅（Post-GA converter） |
| `EnableRetryOnFailure` | ✅ supported | Wave 4 — 10.106 |
| Monster / Specification 子集 | ✅ supported | Wave 3 — 10.101/10.102 |
| Collation / `HasCharSet` Fluent | ❌ skip | 连接串 `CHAR_SET` |
| JSON / Spatial / FULLTEXT | JSON ⚠️ **2.1.0**；Spatial/FULLTEXT ❌ skip | — |

图例：**✅ supported** · **⚠️ partial** · **❌ skip/defer/blocked**

---

## SQL 方言差异（节选）

> 完整映射：`docs/contracts/sql-dialect.contract.md` §函数映射表

### 连接与兼容模式

| 项 | XuguDB | MySQL/Pomelo |
|----|--------|-------------|
| 连接串 | `IP=…; DB=…; USER=…; PWD=…; PORT=5138` | `Server=…;Database=…;` 等 |
| MySQL 兼容 | `EnableCompatibleModeOnOpen()` opt-in | 原生 MySQL |
| 字符集 | 连接级 `CHAR_SET=UTF8` | 表/列级 `HasCharSet` |

### 自增与 Identity

| 项 | XuguDB | MySQL |
|----|--------|-------|
| DDL | `INTEGER IDENTITY(1,1)` | `INT AUTO_INCREMENT` |
| NULL/0 插入 | `identity_mode` 0/1/2 | `sql_mode` 相关 |

### 类型映射（节选）

| CLR | XuguDB | MySQL/Pomelo |
|-----|--------|-------------|
| `Guid` | `GUID`（二进制） | 常 `CHAR(36)` |
| `uint` | `BIGINT` | `INT UNSIGNED` |
| `ulong` | `NUMERIC(20,0)` | `BIGINT UNSIGNED` |
| `bool` | `BOOLEAN` | `TINYINT(1)` |

### DML / 查询生成差异

| 场景 | XuguDB | MySQL/Pomelo |
|------|--------|-------------|
| 分页 | `LIMIT` + 可选 `TOP` | 主要 `LIMIT` |
| 多表 UPDATE + LIMIT | **不支持** | 部分支持 |
| INSERT 回读 | **RETURNING**（Xugu 特有） | 有限支持 |
| 索引删除 | `DROP INDEX table.index` | `ALTER TABLE … DROP INDEX` |

### 函数与表达式（LINQ → SQL）

| C# | XuguDB | MySQL/Pomelo |
|----|--------|-------------|
| `Guid.NewGuid()` | `SYS_GUID()` | `UUID()` |
| `DateTime.AddDays` | `TIMESTAMPADD(day, n, dt)` | `DATE_ADD(… INTERVAL …)` |
| `Math.Log(x, base)` | `LOG(base, x)` **参数序相反** | `LOG(x, base)` |
| `Regex.IsMatch` | `REGEXP_LIKE(…)` | `REGEXP` 运算符 |

---

## 测试覆盖差距

| 指标 | Xugu 2.0.0 | Pomelo 9.0.0 |
|------|------------|--------------|
| FunctionalTests 列测 | **861** | ~**1050** |
| 可比覆盖率 | **~82%** | 100% |
| 剩余差距 | **~190** 方法 | — |
| 全量实库门禁 | **0 FAIL**（显式 Skip 含 ROW_COUNT） | — |

### 已覆盖（Phase 9–10）

- Northwind Query 六套件、AdHoc、FromSql/TPH/Deep/Functions
- CRUD、GraphUpdates、ManyToMany、OptimisticConcurrency（除异常检测）
- Monster Fixup + StoreGenerated Fixup（Wave 3）
- DesignTime + Specification 子集（Wave 3）
- Retry 单元测试 +10（Wave 4）
- OFFSET 参数内联（Wave 5 — 10.201）

### 未覆盖 / Phase 11 候选

| Pomelo 类别 | 处置 |
|-------------|------|
| Spatial / Match | 永久 skip |
| JSON EF Provider（`Json*MySqlTest`） | **2.1.0 目标**（11.109） |
| ROW_COUNT() SQL / 旧并发路径 | 函数仍 E10049；异常检测已 Path A |
| IntegrationTests（Vegeta/ASP.NET） | defer（10.206） |
| Linux RID | blocked（10.205） |
| FOR UPDATE / 位运算 / RelationalCommand | defer（10.202–10.204） |

详见 [TESTING.md](TESTING.md)（Phase 10 closure handoff 已随 harness 移除，见 git 历史）。

---

## 连接与配置

### Xugu 连接串示例

```
IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8
```

### 启用自动重试（Wave 4）

```csharp
options.UseXugu(connectionString, xugu => xugu.EnableRetryOnFailure(
    maxRetryCount: 5,
    maxRetryDelay: TimeSpan.FromSeconds(30),
    errorNumbersToAdd: null)); // Xugu 忽略 errorNumbersToAdd，使用 XGCI Message 码
```

### 执行策略对比

| API | Pomelo | Xugu 2.0.0 |
|-----|--------|------------|
| 默认 `CreateExecutionStrategy()` | 可配置 | `XuguExecutionStrategy`（不重试） |
| `EnableRetryOnFailure()` | `MySqlRetryingExecutionStrategy` | `XuguRetryingExecutionStrategy` ✅ |
| 瞬态判定 | `MySqlException.IsTransient` + 错误号 | `XuguTransientExceptionDetector`（Message 解析） |
| 自定义 `IExecutionStrategyFactory` | 支持 | 支持 |

---

## 迁移与 Scaffolding 差异

| 能力 | Xugu | Pomelo/MySQL |
|------|------|-------------|
| `dotnet ef migrations add` | ✅ | ✅ |
| `dotnet ef database update` | ✅（LOCK TABLE 迁移锁） | ✅ |
| Identity 列 | `IDENTITY(1,1)` | `AUTO_INCREMENT` |
| 过滤索引 `HasFilter` | Differ 剥离 | 支持 |
| 索引类型 | BTREE + **BITMAP** | BTREE/FULLTEXT/SPATIAL |
| Scaffolding 元数据 | `DBA_TABLES` / `ALL_COLUMNS` | `INFORMATION_SCHEMA` |

---

## 已知限制与变通

| 限制 | 变通 |
|------|------|
| `ROW_COUNT()` SQL | 勿使用；乐观并发用 Provider Path A（`RecordsAffected`） |
| DateOnly/TimeOnly SaveChanges | **已支持**（converter）；见 LIMITATIONS |
| JSON 整列大 LOB | 优先 `JsonValue`/`JsonExtract` 投影；见 LIMITATIONS 13.206 |
| 无 FULLTEXT | 应用层搜索；或 `LIKE` / `REGEXP_LIKE` |
| Identity PK 类型变更 | 手工迁移 |
| 仅 Windows x64 原生包 | Linux 待 10.205 |
| Retry Message 格式变更风险 | 驱动提供结构化 `IsTransient`（长期） |

完整列表：[LIMITATIONS.md](LIMITATIONS.md)

---

## 何时选 Xugu vs Pomelo

### 选择 Xugu Provider

- 目标数据库为 **XuguDB**
- 需要 EF Core 9 + CRUD/LINQ/迁移主路径
- 可接受连接串差异、连接级字符集、Windows-only 原生包
- 不依赖 Spatial、FULLTEXT、列级 Collation；JSON 按文档边界使用

### 选择 Pomelo（MySQL）

- 目标库为 **MySQL / MariaDB**
- 需要 Spatial、FULLTEXT、MySQL 原生 `ROW_COUNT()` 语义、多平台生态
- 依赖 MySQL 特有 DDL

### 从 Pomelo 迁移检查清单

> **注意**：以下清单帮助评估迁移工作量；**不是**承诺逐项可零改动完成。每项须对照 Xugu 官方文档与实库验证。

1. 改写连接串与 `UseXugu`
2. 确认 `COMPATIBLE_MODE=MYSQL` 与 `CHAR_SET`（**对照便利**，非产品语义）
3. `Guid` 存储（二进制 vs 字符串）
4. `AUTO_INCREMENT` → `IDENTITY`
5. 移除 `HasCollation` / Spatial；JSON 按 11.109 或 VARCHAR 变通
6. 评估 Retry（已支持）、乐观并发 Path A、DateOnly/TimeOnly 物化边界
7. 集成测试 — [TESTING.md](TESTING.md)

### 请勿以 MySQL 语法作为首要依据

| 场景 | 错误做法 | 正确做法 |
|------|---------|---------|
| 新功能 SQL 实现 | 复制 Pomelo 生成的 MySQL SQL | 查 `E:\BaiduSyncdisk\docs\content\` + 更新 contract |
| 迁移验收 | 「MYSQL 模式下能跑就算完成」 | 以 Xugu 原生文档 + 实库测试为准 |
| JSON 路径 | 假设与 MySQL JSONPath 100% 一致 | 查 `json.md` §JSONPath（含 `last`、`**` 等 Xugu 扩展） |
| 函数映射 | 沿用 MySQL 函数名 | 查 `reference/function/` 目录 |

---

## 参考文档

| 文档 | 路径 |
|------|------|
| 已知限制 | [LIMITATIONS.md](LIMITATIONS.md) |
| 快速开始 | [GETTING-STARTED.md](GETTING-STARTED.md) |
| 测试说明 | [TESTING.md](TESTING.md) |
| SQL 方言契约 | `docs/contracts/sql-dialect.contract.md` |
| Retry 调研 | `docs/references/retrying-execution-strategy.md` |
| Wave 6 / Phase 10 Closure | 原 `harness/handoffs/*`（已移除；见 git 历史） |
| 测试说明 | [TESTING.md](TESTING.md) |
| XuguDB 官方文档 | `E:\BaiduSyncdisk\docs\content\` |
| Pomelo 参考源码 | `external/Pomelo.EntityFrameworkCore.MySql/` |
