# XuguDB SQL 方言契约（Living Document）

> **SQL 唯一权威**：`E:\BaiduSyncdisk\docs\content\`（XuguDB 官方文档）。  
> 实现或修改 SQL 前必须阅读本文 + 对应官方文档页；发现新差异时更新本文并注明文档路径。  
> **禁止**把 Pomelo / MySQL 语法或 `COMPATIBLE_MODE=MYSQL` 偶然兼容当作产品方言。

交叉：[LIMITATIONS.md](../LIMITATIONS.md) · [RELEASE-SCOPE.md](../RELEASE-SCOPE.md) · [ado-driver-contract.md](ado-driver-contract.md) · [stub-and-exclusion.contract.md](stub-and-exclusion.contract.md) · [xugudb-docs-map.md](../references/xugudb-docs-map.md)

## 参考源优先级

```
1. E:\BaiduSyncdisk\docs\content\                 ← SQL / 类型 / 函数（唯一权威）
2. docs/contracts/sql-dialect.contract.md         ← 项目内已登记规则（本文）
3. docs/contracts/stub-and-exclusion.contract.md  ← 无文档能力时的 stub / Skip
4. docs/LIMITATIONS.md / RELEASE-SCOPE.md         ← 产品范围与发布口径
5. external/Pomelo.EntityFrameworkCore.MySql      ← 仅 C# 架构参考（SQL 不可照搬）
```

## 数据库与连接

| 项 | 值 |
|----|-----|
| 数据库 | XuguDB（虚谷数据库） |
| EF Core 包 | `Microsoft.EntityFrameworkCore.Xugu`（对齐 EF Core 9.0.x → 包 9.0.0） |
| 连接 API | `UseXugu(connectionString)` |
| 连接串示例 | `IP=127.0.0.1;DB=SYSTEM;USER=SYSDBA;PWD=SYSDBA;PORT=5138;AUTO_COMMIT=on;CHAR_SET=UTF8` |
| ADO.NET | `XuguClient` + 原生 `xugusql.dll`（Windows）；见 [xuguclient-dependency-strategy.md](../xuguclient-dependency-strategy.md) |
| 文档根 | `E:\BaiduSyncdisk\docs\content\` |

**必填**：生产连接串使用 `CHAR_SET=UTF8`（驱动默认可能为 GBK）。

## 兼容模式（会话，非方言产品）

文档：`reference/system-configuration-parameter/session-parameter/compatible_mode.md`

| COMPATIBLE_MODE | 标识符折叠 | Provider |
|-----------------|------------|----------|
| NONE / ORACLE | 词法转大写 | **默认**（不发 `SET compatible_mode`） |
| MYSQL | 不转换大小写 | opt-in：`EnableCompatibleModeOnOpen(XuguCompatibleMode.Mysql)` |
| POSTGRESQL | 词法转小写 | opt-in |

**不承诺**：ORACLE/POSTGRESQL/MYSQL 模式提供对应引擎的 SQL 方言翻译或「零改动迁移」。

## 标识符

文档：`reference/sql/identifier.md`

| 项 | 规则 |
|----|------|
| 定界 | 反引号 `` ` ``（`SqlGenerationHelper.DelimitIdentifier`）或双引号 |
| 最大长度 | 127 字节 → `RelationalMaxIdentifierLengthConvention(127)` |
| Schema | 支持 `schema.object` |

## 分页

| 项 | SQL |
|----|-----|
| 语法 | `LIMIT {count} [OFFSET {offset}]`（亦支持部分 TOP 场景，生成侧以 LIMIT 为主） |
| 值 | 常量整数化；非常量 `CAST(… AS INTEGER)`（`GenerateIntegerLimitOffsetValue`） |

## 自增与 identity 回读

| 项 | 规则 |
|----|------|
| DDL | `IDENTITY(seed, increment)`（非 MySQL `AUTO_INCREMENT`） |
| 回读 | **不**依赖驱动暴露 `INSERT … RETURNING` 行；使用 `INSERT` + `SELECT … WHERE id = LAST_INSERT_ID()` |
| IDENTITY 主键改类型 | Migrations **NotSupported**（见 LIMITATIONS） |

## 数据类型（摘要）

| CLR | 典型 store | 备注 |
|-----|------------|------|
| `int` / `long` / `short` / `byte` | `INTEGER` / `BIGINT` / … | 有符号；`uint`→`BIGINT`，`ulong`→`NUMERIC(20,0)` |
| `string` | `VARCHAR` / `CLOB` | 避免错误 FixedLength DbType |
| `bool` | 文档布尔 / 整型映射 | 布尔优化见查询侧 |
| `DateTime` | `DATETIME` / `TIMESTAMP` | |
| `DateOnly` / `TimeOnly` | `DATE` / `TIME(n)` | converter 物化（驱动绑定不完整） |
| `DateTimeOffset` | `DATETIME WITH TIME ZONE` 等 | 字符串读写；相等过滤不可靠（驱动） |
| `Guid` | 原生 `GUID` | 非 `CHAR(36)` |
| `byte[]` | `BINARY` / `BLOB` | Contains/索引走 HEX 旁路 |
| JSON 文档 | `JSON` | 标量函数路径优先；整列 LOB 有边界 |
| 参数原始集合 | JSON **文本**参数 | `XuguPrimitiveCollectionTypeMapping` |

完整驱动读写行为见 [ado-driver-contract.md](ado-driver-contract.md)。

## 函数与 LINQ 翻译（已实现要点）

| C# / EF | SQL（Xugu） | 状态 |
|---------|-------------|------|
| `string.Contains/StartsWith/EndsWith` | `LIKE` + `CONCAT` | done |
| `string.Length` | `LENGTH` | done |
| `string.Substring` / `IndexOf` / `Replace` / `Trim*` / `ToLower`/`ToUpper` / `Pad*` | `SUBSTRING` / `LOCATE-1` / `REPLACE` / `TRIM` / `LCASE`/`UCASE` / `LPAD`/`RPAD` | done |
| `Enumerable.FirstOrDefault/LastOrDefault(string)` | `SUBSTRING(s,1,1)` / `SUBSTRING(s,LENGTH(s),1)` | done（Wave5） |
| `DateTime` 部件 / `Add*` | `YEAR`/`MONTH`/… · `TIMESTAMPADD` | done |
| `DateTime.Millisecond` | `MICROSECOND()/1000` | done |
| `TimeSpan.Hours/Minutes/Seconds/Milliseconds` | `HOUR`/`MINUTE`/`SECOND`/`MICROSECOND`；**Milliseconds 再 `/1000` 并 `Convert`→`int`**（避 E34412） | done |
| `Guid.NewGuid()` | `SYS_GUID()` | done |
| `Count` / `LongCount` | `CAST(COUNT(…) AS INTEGER\|BIGINT)` | done（避 E34412） |
| `EF.Functions.DateDiff*` | `TIMESTAMPDIFF`→BIGINT，公共 `int` 再转 INTEGER | done |
| `Regex.IsMatch` | `REGEXP_LIKE` | done |
| `byte[].Contains` / 索引 | `LOCATE(LPAD(HEX(…),2,'0'), HEX(src))` 等 | done |
| `Math.*` 常用 | `ABS`/`FLOOR`/`SIN`/… · `LN` · `LOG(base,x)` 注意参数序 | done |
| 参数集合 `list.Contains(column)` | 防护后的 JSON 标量谓词（`JSON_LENGTH` / `JSON_VALUE` 等） | done（Wave5） |
| 集合行集 / `JSON_TABLE` | — | **skip**（无文档 API；实库 E19132） |
| `CROSS/OUTER APPLY` / `LATERAL` | — | **NotSupported** → `XuguStrings.ApplyNotSupported` |
| `FROM` 子查询引用上级列 | — | **skip**（E17010） |
| 内联 `VALUES` 派生表 | `SELECT … UNION ALL SELECT …`（禁止 `UNION ALL VALUES`） | done（Wave4，避 E19132） |

未列出的函数：先查官方 `reference/function/**`，再补行；无文档则走 [stub-and-exclusion.contract.md](stub-and-exclusion.contract.md)。

## DML：ExecuteDelete / ExecuteUpdate

文档：`reference/sql/dml/delete.md`、`update.md`

| 场景 | 形状 | 状态 |
|------|------|------|
| 单表 DELETE/UPDATE + 谓词 | 标准 `DELETE FROM` / `UPDATE … SET` | **支持** |
| 多表 JOIN 形态 | 按 Xugu `FROM` / 多表 UPDATE 语法 | **受限支持** |
| 源带 `ORDER BY`/`LIMIT`/`DISTINCT`/`GROUP BY` | — | **拒绝** |
| TPC/TPT 继承批量、部分 owned/导航目标 | — | **拒绝**（见 LIMITATIONS） |
| `CROSS JOIN` 于 UPDATE/DELETE | — | **拒绝**（E19132） |

## 内联集合 / 分页 / 子查询硬限制

| 主题 | 规则 | 证据 |
|------|------|------|
| E17010 | 子查询不得引用上级查询表达式 | `subquery.md`；Functional Skip |
| E19132 `VALUES` 派生表 | 使用 `UNION ALL` 的 `SELECT` 列表 | Wave4 `GenerateValues` |
| E19132 / 无 APPLY | 相关集合部分形状不可翻译 | `from.md` 无 APPLY/LATERAL |
| JSON 参数 `IN (SELECT…)` | **不要**生成；改标量 JSON 谓词 | Wave5 实库错误/错误结果 |

## JSON

| 能力 | 状态 |
|------|------|
| 列类型 `JSON` + DDL | done |
| `JsonValue` / `JsonExtract` / 路径 | done（标量） |
| 整列 LOB 物化 | 边界；推荐标量投影 |
| `ToJson()` owned | **不承诺** |
| `JSON_TABLE` 行集 | **不支持** |

## Migrations / Scaffolding（摘要）

| 主题 | 规则 |
|------|------|
| Sequence | `CREATE/DROP/ALTER SEQUENCE` 按 `sequence.md`；`RESTART WITH` NotSupported；HiLo 未做 |
| Index | `CREATE/DROP/ALTER INDEX`；FULLTEXT/RTREE Migration NotSupported |
| Scaffold / HasTables | **`ALL_*` 视图**（勿用需 DBA 的 `SYS_*`/`DBA_*` 作主路径） |
| `CREATE/DROP DATABASE` | EF API **不支持**（运维建库） |

## 与 MySQL/Pomelo 的关键差异（对照，非实现依据）

| 点 | Xugu | 勿照搬 |
|----|------|--------|
| 自增 | `IDENTITY` | `AUTO_INCREMENT` |
| 新 Guid | `SYS_GUID()` | `UUID()` |
| 日期加减 | `TIMESTAMPADD` | `DATE_ADD` |
| 单表 DELETE | `DELETE FROM t` | `DELETE alias FROM t` |
| Guid 存储 | 原生 `GUID` | `CHAR(36)` |
| 兼容模式 | 可选会话折叠 | 当作方言开关 |

## 变更日志

| 日期 | 摘要 |
|------|------|
| 2026-07-28 | **全文重写**（历史 UTF-8 损坏，git 无干净祖先可恢复）。内容对齐 9.0.0 Wave A + Wave5 已实现行为与 LIMITATIONS。 |
| 2026-07-28 | Wave5：参数集合 membership、string First/LastOrDefault、TimeSpan.Milliseconds int、LIMIT 整数化。 |
| 2026-07-24 | Wave4：VALUES→UNION ALL SELECT；E17010/E19132 Skip 证据化。 |
