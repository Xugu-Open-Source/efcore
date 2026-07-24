# XuguDB SQL 方言契约（Living Document）

> **SQL 唯一权威**：`E:\BaiduSyncdisk\docs\content\`（XuguDB 官方文档（ 
> 所�Agent 实现 SQL 相关代码前必须阅读本文档 + 对应官方文档� 
> 发现新差异时更新本文档并注明文档路径�

> **⚠️ 非权威来源（禁止作为 SQL 依据：*  
> - Pomelo / MySQL 语法与行�—**�* C# 架构、DI、Translator 模式参� 
> - `COMPATIBLE_MODE=MYSQL` 下的偶然兼容 —**不是**产品语义（见下文（ 
> - 本文「与 MySQL/Pomelo 差异」列 —帮助对照：*�*定义 Xugu 应实现的 SQL

## 参考源优先�

```
1. E:\BaiduSyncdisk\docs\content\              �SQL 方言、类型、函数（唯一权威）
2. docs/contracts/sql-dialect.contract.md   �项目内已登记规则
3. docs/contracts/stub-and-exclusion.contract.md �无文档能力时�stub/skip 策略
4. docs/LIMITATIONS.md / RELEASE-SCOPE.md      �产品范围
5. external/Pomelo.EntityFrameworkCore.MySql     �C# 架构参考（MySqlJson* 等命名可借鉴，SQL 不可照搬）
```

## 数据库信�

| �| �|
|----|-----|
| 数据库 | XuguDB（虚谷数据库：|
| EF Core 包名 | `Microsoft.EntityFrameworkCore.Xugu` |
| 连接 API | `UseXugu(connectionString)` |
| 连接串示例 | `IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8` |
| ADO.NET 驱动 | `XuguClient.dll` + 原生 `xugusql.dll`（见 `external/csharp-driver/`） |
| 兼容模式 | `SET compatible_mode TO 'MYSQL';`；**可选开发对照便利**；产品 SQL 以 Xugu 原生文档为准，见 `docs/RELEASE-SCOPE.md`） |
| 文档根目录 | `E:\BaiduSyncdisk\docs\content\` |

## 兼容模式行为

> 文档：`reference/system-configuration-parameter/session-parameter/compatible_mode.md`

**产品定位**：`COMPATIBLE_MODE` �**可�* 会话参数（`NONE`/`MYSQL`/`ORACLE`/`POSTGRESQL`），用于标识符折叠与开发对照；**不是** Provider 的异�SQL 方言目标�*2.1.0 起默认关闭*；`EnableCompatibleModeOnOpen(XuguCompatibleMode)` 启用。交叉：`ado-driver-contract.md`、Phase 13 W4�

| COMPATIBLE_MODE | 标识符处理| Provider 默认（.1.0+：|
|-----------------|-----------|-------------------------|
| NONE / ORACLE | 词法阶段转大写| **默认（compat off：* |
| **MYSQL** | **不做大小写转�* | opt-in via `EnableCompatibleModeOnOpen()` |
| POSTGRESQL | 词法阶段转小�| opt-in |

Provider 生成�SQL 与验收标准仍�**Xugu 官方文档** 为准，不以「零改动 MySQL 迁移」为设计目标�

## 标识�

> 文档：`reference/sql/identifier.md`

| �| XuguDB 规则 |
|----|------------|
| 引号 | 双引�`"` 或反引号 `` ` `` |
| 非引号标识符 | 不区分大小写（MYSQL 模式下不做转换） |
| 加引号标识符 | 区分大小�|
| 最大长�| 1—27 字节 |
| Schema | 支持，`schema.object` 形式 |

**Provider 实现**）

- `RelationalMaxIdentifierLengthConvention(127)`（与文档一致）
- `SqlGenerationHelper.DelimitIdentifier()` 使用反引�`` ` ``
- 表名/列名映射遵循 MYSQL 模式不做自动大小写转�

## ExecuteDelete / ExecuteUpdate（批�DML）

> 文档：`reference/sql/dml/delete.md`、`reference/sql/dml/update.md`

| 场景 | 生成 SQL 形�| 文档依据 | Provider |
|------|--------------|---------|----------|
| 单表带谓�DELETE | `DELETE FROM {table} WHERE …` | delete.md §语法 | `XuguQuerySqlGenerator.VisitDelete` |
| 单表 DELETE + ORDER BY / LIMIT | `DELETE FROM {table} WHERE —ORDER BY —LIMIT …`（无表别名） | delete.md + resultset-restricted.md | 同上（`_removeTableAlias`：|
| 多表 DELETE（JOIN：| `DELETE FROM {target} FROM {joins…} WHERE …` | delete.md §`opt_from_clause` 示例 | `IsValidSelectExpressionForExecuteDelete` + `VisitDelete` |
| 单表 UPDATE | `UPDATE {table} SET col = val WHERE …` | update.md §`update_filter_clause` | `XuguQuerySqlGenerator.VisitUpdate` |
| 多表 UPDATE（JOIN：| `UPDATE {target}, {joins…} SET —WHERE …` | update.md §`base_table_refs` 多表 | 同上 |
| UPDATE + LIMIT | 单表支持 `LIMIT`：*多表不支�* ORDER BY / LIMIT | update.md §提示 4 | `GenerateLimitOffset` |

**�MySQL/Pomelo 差异**：Xugu 单表 DELETE 使用 `DELETE FROM table`（非 MySQL `DELETE alias FROM table`）；多表使用�`FROM` 子句（`DELETE FROM t1 FROM t2`），�MySQL 逗号 JOIN 列表。多�UPDATE **不支�* LIMIT（`update.md` §提示 4）�

## HAVING / 布尔优化

> 文档：`reference/sql/select/group-by.md`

| 场景 | XuguDB | Provider |
|------|--------|----------|
| HAVING 聚合函数 | `HAVING COUNT(*) > N` | 标准翻译 |
| HAVING SELECT 别名 | `HAVING emp_count > 2`（文档示例） | `XuguHavingExpressionVisitor` 子查�pushdown + `XuguColumnAliasReferenceExpression` |
| 布尔列谓词优�| `WHERE col = TRUE` 利于索引 | `XuguBoolOptimizingExpressionVisitor`（`XuguParameterBasedSqlProcessor`：|

**�MySQL 差异**：Xugu 文档明确支持 HAVING 引用 SELECT 别名；Pomelo �MySQL bug #103961 workaround 仍保�pushdown 路径以兼�EF Core 复杂 HAVING 表达式�

## Sequential GUID

> 文档：`reference/sql/datatype/guid.md`、`reference/function/uuid-functions/sys_guid.md`

| 场景 | XuguDB | Provider |
|------|--------|----------|
| 查询�NewGuid | `SYS_GUID()` | `XuguNewGuidTranslator` |
| 插入�Guid PK | 客户端顺�GUID（ticks + random：| `XuguSequentialGuidValueGenerator` + `XuguValueGeneratorSelector` |

**�MySQL/Pomelo 差异**：无 `GuidFormat` 连接选项；统一 RFC4122 大端序客户端生成�

## Migrations Differ 过滤

| 场景 | 行为 |
|------|------|
| 过滤索引 `HasFilter` | Differ 剥离 `Filter`（DDL 不支持，�`FilteredIndexesNotSupported`：|
| �Collation 变更 | 忽略（连接级 `CHAR_SET`：|

## 外键 ReferentialAction

> 文档：`reference/object/constraints.md` §key_actions

| ReferentialAction | XuguDB DDL | Provider |
|-------------------|-----------|----------|
| Cascade | `ON DELETE/UPDATE CASCADE` | `XuguMigrationsSqlGenerator.ForeignKeyAction` |
| SetNull | `SET NULL` | 同上 |
| SetDefault | `SET DEFAULT` | 同上 |
| Restrict | `RESTRICT` | 同上 |
| NoAction | 省略子句（默�NO ACTION：| 同上 |

**�MySQL/Pomelo**：EF Core 关系模型仅映�`DeleteBehavior` �`OnDelete`；`OnUpdate` 由迁移操作显式指定（DDL 支持 `ON UPDATE`）�

## 字符�Fluent API

| MySQL/Pomelo | XuguDB | Provider |
|--------------|--------|----------|
| `ModelBuilder.HasCharSet` | 无表/模型�charset | **skip** —使用连接�`CHAR_SET` |
| `EntityTypeBuilder.HasCharSet` | �| **skip** |
| `XuguTableBuilderExtensions.HasXuguComment` | `COMMENT ON TABLE` | **done** Wave 5 |

## 分页

> 文档：`reference/sql/select/resultset-restricted.md`

XuguDB 同时支持 **LIMIT** �**TOP**）

```sql
-- 形式 1（MySQL 风格）
SELECT * FROM t LIMIT {count};
SELECT * FROM t LIMIT {offset}, {count};
SELECT * FROM t LIMIT {count} OFFSET {offset};

-- 形式 2（SQL Server 风格）
SELECT TOP {n} * FROM t ORDER BY ...;
```

**EF Core Provider 映射**）

| LINQ | 生成 SQL | 文档依据 |
|------|---------|---------|
| `.Take(n)` | `LIMIT n` | resultset-restricted.md |
| `.Skip(o).Take(n)` | `LIMIT o, n` �`LIMIT n OFFSET o` | resultset-restricted.md |
| `.Take(n)` + OrderBy (SQL Server 模式) | 可�`TOP n` | resultset-restricted.md §TOP |

**参数内联：0.201）*：`Skip` 生成�`OFFSET` 子句在参数值已知时内联为字面量（`XuguParameterInliningExpressionVisitor`）；JSON 动态路径使�`CONCAT`/`JSON_EXTRACT`：1.109b）�

## JSON 列（Phase 11 —11.109 done）

> **权威文档**（ 
> - 类型：`reference/sql/datatype/json.md`  
> - 运算符：`reference/sql/operators/json-operators/column_path.md`（`->`）、`inline_path.md`（`->>`（ 
> - 函数：`reference/function/json-functions/`：8+ 函数）

| �| XuguDB（官方文档） | Provider 2.0.x | Provider 2.1.0 目标：1.109）|
|----|-------------------|----------------|------------------------------|
| 原生 `JSON` 列类�| **支持**（LOB，最�2GB；Java `String` 绑定：| **未映�* | **`XuguJsonTypeMapping` + DDL `JSON`：1.109a done：* |
| `->` / `->>` 路径运算�| **支持**（JSONPath，`$` 前缀；含 `last`、`**`、`[M to N]` 扩展：| 未翻�| **`XuguJsonTraversalExpression` + `VisitJsonScalar`：1.109b done：* |
| `JSON_EXTRACT` / `JSON_VALUE` �| **28+ 函数**（见 `json.md` §预览表） | 未实�| **`XuguJsonDbFunctionsExtensions` + Translator：1.109b done：* |
| `JSON_ARRAYAGG` / `JSON_OBJECTAGG` | 支持 | 未实�| P2 / 按需 |
| EF `ToJson()` / owned JSON �| —| **不实�* | **不承�* Pomelo 全矩阵；基础 JSON 列映射优�|
| Pomelo `Json*MySqlTest` | —| **skip**（.0.x：| 手写 Xugu 子集：1.109d done：|
| Fluent `HasXuguJsonColumn()` | —| —| **done**：1.109c：|

**�MySQL/Pomelo 差异（实现时�Xugu 文档为准，非 MySQL 字节级兼容）**）

- XuguDB JSON 比较/排序有独立类型优先级（BOOL > ARRAY > OBJECT > STRING > NUMBER > NULL；见 `json.md` §JSON比较与排序）�
- 路径语法支持 `last`、`**` 深度查找、`[M to N]` 切片�Xugu 扩展（见 `json.md` §JSONPath）�
- `->` 返回 JSON 文本；`->>` 取消 JSON 类型引用（见 `column_path.md` / `inline_path.md`）�
- ADO.NET 驱动映射�`java.sql.String`（`json.md` §特性表）；EF 需确认 `XuguClient` 参数绑定与反序列化策略�

### 11.109 实现脚手架（Wave 2 入口）

| 子任�| Provider 模块 | Pomelo 架构参考（�C#：| Xugu 文档锚点 |
|--------|--------------|-------------------------|--------------|
| 11.109a | `Storage/Internal/XuguJsonTypeMapping.cs` | `MySqlJsonTypeMapping` | `json.md` §JSON存储类型、DDL 示例 | **done** |
| 11.109b | Query Translators（`JsonScalarExpression` 遍历：| `MySqlJson*` translators | `json-operators/`、`json-functions/json_extract.md` | **done** |
| 11.109c | Fluent API（`HasXuguJsonColumn`：| `MySqlEntityTypeBuilderExtensions` | �Xugu 文档为准 | **done** |
| 11.109d | 实库测试 | `JsonQueryMySqlTest` 可跑子集 | 手写断言 | **done** |

**2.1.0 状�*：Wave 2 done：75 列测；`JsonIntegrationTests` SkippableFact）�

## 自增主键（IDENTITY）

> 文档：`reference/system-configuration-parameter/xugu.ini/compatible/def_identity_mode.md`

XuguDB 使用 `IDENTITY(seed, increment)` 而非 MySQL `AUTO_INCREMENT`）

```sql
CREATE TABLE t1(c1 INTEGER IDENTITY(1, 1));
```

| identity_mode | INSERT NULL 行为 |
|---------------|-----------------|
| 0 (DEFAULT) | 报错 E16005 |
| 1 (NULL_AS_AUTO_INCREMENT) | NULL 替换为自增�|
| 2 (ZERO_AS_AUTO_INCREMENT) | NULL �0 替换为自增�|

**Provider 实现要点**）

- 模型约定：映射为 `IDENTITY(1,1)`，不�`AUTO_INCREMENT`
- **SaveChanges 回读（.1.0）*）
  - **默认�compat 运行�SQL**：`INSERT` + `SELECT —WHERE {identity_col} = LAST_INSERT_ID()`（Xugu 原生函数，见 `last_insert_id.md`）
  - **差异**：compat 模式连接打开时额�`SET compatible_mode TO 'MYSQL'`
  - **RETURNING**：数据库�`insert.md` 支持：*XuguClient ADO 暂不可读** —Provider 不使�`AppendInsertReturningOperation` 直至驱动修复：1.506）
- �Pomelo 差异：*必须�MigrationsSqlGenerator �Convention 中单独实�*
- **ROW_COUNT**：*signed-off blocked**：2.509/PLAT-01 / E10049）；RETURNING 路径 **�* 依赖 `ROW_COUNT()`

## 数据类型映射（CLR �XuguDB）

> 文档：`reference/sql/datatype/`

| CLR 类型 | XuguDB 类型 | 文档 |
|----------|------------|------|
| `bool` | `BOOLEAN` | `datatype/bool.md` |
| `byte` | `TINYINT` | `datatype/numerical.md` |
| `short` | `SMALLINT` | `datatype/numerical.md` |
| `int` | `INTEGER` | `datatype/numerical.md` |
| `long` | `BIGINT` | `datatype/numerical.md` |
| `uint` | `BIGINT`（无 unsigned 类型：| `datatype/numerical.md` |
| `ulong` | `NUMERIC(20,0)`（无 unsigned 类型：| `datatype/numerical.md` |
| `decimal` | `NUMERIC/DECIMAL/NUMBER` | `datatype/numerical.md` |
| `float` | `FLOAT` | `datatype/numerical.md` |
| `double` | `DOUBLE` | `datatype/numerical.md` |
| `string` | `VARCHAR(n)` / `CHAR(n)` | `datatype/character.md` |
| `DateTime` | `DATETIME` | `datatype/datetime.md` |
| `DateOnly` | `DATE` | `datatype/datetime.md` |
| `TimeOnly` | `TIME(3)`（CLR 默认）；显式 `TIME` / `TIME(n)` 精度 `n∈[0,3]` | `datatype/datetime.md` |
| `DateTimeOffset` | `DATETIME WITH TIME ZONE` / `TIMESTAMP WITH TIME ZONE`：*�*生成未经文档支持�`DATETIME WITH TIME ZONE(n)` 后缀：| `datatype/datetime.md` |

### ADO / 驱动适配（Post-GA · Provider 侧，2026-07-13）

> 官方驱动 `external/csharp-driver` **只读**；下列规则在 Provider 层闭环，**�*修改驱动�

| 场景 | XuguDB / 驱动事实 | Provider 处理 | 文档 |
|------|-------------------|---------------|------|
| `COUNT` �CLR `int`/`long` | 聚合结果常以高精度数值返回；驱动 `GetInt32` 遇高精度�`E34412` | `XuguQuerySqlGenerator.VisitSqlFunction`：`CAST(COUNT(— AS INTEGER\|BIGINT)` | `type_conversion.md`（显�`CAST`：|
| `DateDiff*` / `TIMESTAMPDIFF` | 官方返回 **`BIGINT`** | Translator �`BIGINT`/`long` 生成 `TIMESTAMPDIFF`，单位换算后 `Convert` �CLR `int`（末�`CAST —AS INTEGER`：| `timestampdiff.md` §输出结果 |
| `DateOnly` / `TimeOnly` / `DateTimeOffset` 参数与物�| 驱动缺完整原�CLR 绑定；读回多为字符串/`DateTime` | `XuguTemporalValueConverters`：invariant 字符串往返；DTO 字面量含偏移（`yyyy-MM-dd HH:mm:ss[.fff] zzz`）；读回兼容驱动 `—8` / `—08:00` | `datatype/datetime.md` |
| `Guid` | `GUID`：6 字节原生类型：| `datatype/guid.md` |
| `TimeSpan` | `TIME` | `datatype/datetime.md` |
| `byte[]` | 无长��`BLOB`；有 `MaxLength`/size（≤64KB）→ `BINARY`：*均不**生成 `BLOB(n)`/`BINARY(n)`（文�DDL �size 后缀：| `datatype/binary.md`、`datatype/large-object.md` |

## INSERT 语句

> 文档：`reference/sql/dml/insert.md`

支持）

- 标准 INSERT（单�多行）
- REPLACE INTO
- INSERT IGNORE INTO
- RETURNING 子句（XuguDB 特有，见 insert.md）

**Provider 实现（.1.0）*）

| 模式 | INSERT identity 回读 SQL（.1.0 运行时） |
|------|----------------------------------------|
| Native（默认） | `INSERT …`; `SELECT id FROM t WHERE id = LAST_INSERT_ID()` |
| Compat（opt-in：| 同上 + 连接�`SET compatible_mode TO 'MYSQL'` |

**理想路径（驱动修复后：*：`INSERT INTO t (— VALUES (— RETURNING id`（`insert.md`）。当�XuguClient 不暴�RETURNING 结果集�

## 函数映射�

> 文档：`reference/sql/expression/function.md` �`reference/function/` 目录  
> **实现每个 Translator 前必须打开对应函数文档**

| C# 表达�| SQL（XuguDB：| Pomelo 参�| 负责 Agent | 状�|
|-----------|--------------|------------|-----------|------|
| `string.Contains(s)` | `LIKE CONCAT('%', s, '%')` | LIKE / LOCATE | QueryTranslators | done |
| `string.StartsWith(s)` | `LIKE CONCAT(s, '%')` | LIKE | QueryTranslators | done |
| `string.EndsWith(s)` | `LIKE CONCAT('%', s)` | LIKE | QueryTranslators | done |
| `string.Length` | `LENGTH()` | CHAR_LENGTH | QueryTranslators | done |
| `DateTime.Year` | `YEAR()` | `EXTRACT(year FROM —` / `YEAR()` | QueryTranslators | done |
| `DateTime.Month/Day/Hour/Minute/Second` | `MONTH()` �| `EXTRACT(—FROM —` | QueryTranslators | done |
| `DateTime.Millisecond` | `MICROSECOND()/1000` | `EXTRACT(microsecond FROM —/1000` | QueryTranslators | done |
| `DateTime.Date` | `DATE()` | `CONVERT(— date)` | QueryTranslators | done |
| `DateTime.Now/UtcNow/Today` | `CURRENT_TIMESTAMP()` / `UTC_TIMESTAMP()` / `CURDATE()` | 同左（MySQL 风格：| QueryTranslators | done |
| `DateTime.AddDays(n)` �| `TIMESTAMPADD(unit, n, dt)` | `DATE_ADD(dt, INTERVAL n unit)` | QueryTranslators | done |
| `DateTime.DayOfWeek` | `DAYOFWEEK()-1` | 同左（ODBC 索引 1=Sunday：| QueryTranslators | done |
| `Math.Abs(x)` | `ABS()` | ABS() | QueryTranslators | done |
| `Guid.NewGuid()` | `SYS_GUID()` | `UUID()` | QueryTranslators | done |
| `Convert.To*(x)` | `CAST(x AS type)` | `CONVERT(x, type)` / `CAST` | QueryTranslators | done |
| `DateTimeOffset.Now` | `SYSTIMESTAMP()` | `UTC_TIMESTAMP()` | QueryTranslators | done |
| `DateTimeOffset.UtcNow` | `UTC_TIMESTAMP()` | `UTC_TIMESTAMP()` | QueryTranslators | done |
| `DateTimeOffset.ToUnixTime*` | `TIMESTAMPDIFF(...)` | 同左 | QueryTranslators | done |
| `TimeOnly.FromDateTime(dt)` | `TIME(dt)` | `TIME(dt)` | QueryTranslators | done |
| `TimeOnly.AddHours/Minutes` | `ADDTIME(CAST(t AS TIME), INTERVAL n unit)` | `DATE_ADD` / `ADDTIME` | QueryTranslators | done |
| `EF.Functions.Degrees/Radians` | `DEGREES()` / `RADIANS()` | 同左 | QueryTranslators | done |
| `double.RadiansToDegrees/DegreesToRadians` | `DEGREES()` / `RADIANS()` | 同左 | QueryTranslators | done |
| `DateOnly.ToDateTime(time)` | `MAKE_TIMESTAMP(...)` | `ADDTIME(CAST(...), time)` | QueryTranslators | done |
| `DateOnly.DayNumber` | `TO_DAYS(d) - 366` | 同左 | QueryTranslators | done |
| `XuguDbFunctionsExtensions.DateDiff*` | `TIMESTAMPDIFF`→`BIGINT`，再 `CAST`/`Convert`→`INTEGER`（公�API �`int`：| 同左 | QueryTranslators | done |
| `Queryable.Count` / `LongCount` | `CAST(COUNT(— AS INTEGER\|BIGINT)` | 直接 `COUNT` | QueryCore | done |
| `byte[].Contains(b)` | `LOCATE(LPAD(HEX(b),2,'0'), HEX(arr)) > 0`（BLOB 无直�LOCATE：| 旧：`LOCATE(b, arr)` | QueryTranslators | done |
| `Enumerable.First(byte[])` | `ASCII(arr)` | 同左 | QueryTranslators | done |
| `XuguDbFunctionsExtensions.Like` | `LIKE` | 同左 | QueryTranslators | done |
| `XuguDbFunctionsExtensions.Hex` | `HEX(expr)` | 同左 | QueryTranslators | done |
| `XuguDbFunctionsExtensions.Unhex` | `UNHEX(expr)` | 同左 | QueryTranslators | done |
| `object.ToString()` | `CAST(expr AS VARCHAR)` | 同左 | QueryTranslators | done |
| `Regex.IsMatch(s, pattern)` | `REGEXP_LIKE(expr, pattern)` | `expr REGEXP pattern` | QueryTranslators | done |
| `string.Equals(s, StringComparison)` | `LCASE(a)=LCASE(b)`（IgnoreCase（ 直接 `=` | COLLATE utf8mb4_bin | QueryTranslators | done |
| `string.Trim/TrimStart/TrimEnd` | `TRIM([LEADING\|TRAILING] —FROM —` | 同左 | QueryTranslators | done |
| `string.Replace` | `REPLACE()` | 同左 | QueryTranslators | done |
| `string.ToLower/ToUpper` | `LCASE()` / `UCASE()` | `LOWER()` / `UPPER()` | QueryTranslators | done |
| `string.PadLeft/PadRight` | `LPAD()` / `RPAD()`（常量参数） | 同左 | QueryTranslators | done |
| `string.IndexOf` | `LOCATE(sub, str) - 1` | 同左 | QueryTranslators | done |
| `string.Substring` | `SUBSTRING(str, start+1, len)` | 同左 | QueryTranslators | done |
| `Math.Floor/Ceiling/Round/Truncate` | `FLOOR/CEILING/ROUND/TRUNCATE` | 同左 | QueryTranslators | done |
| `Math.Sin/Cos/Tan/Sqrt/Pow/Exp` | `SIN/COS/TAN/SQRT/POWER/EXP` | 同左 | QueryTranslators | done |
| `Math.Log(x)` | `LN(x)` | `LOG(x)` | QueryTranslators | done |
| `Math.Log(x, base)` | `LOG(base, x)`（参数反转，�log.md：| `LOG(x, base)` | QueryTranslators | done |
| `TimeSpan.Hours/Minutes/Seconds/Milliseconds` | `HOUR/MINUTE/SECOND/MICROSECOND` | `EXTRACT(part FROM —` | QueryTranslators | done |
| `string.Split` | —| —| —| **defer**（无简�LINQ→SQL 映射；`SPLIT_PART` 仅常量） |

## 索引 DDL

> 文档：`reference/object/indexes.md`

| �| XuguDB | MySQL/Pomelo | Provider |
|----|--------|-------------|----------|
| 创建 | `CREATE [UNIQUE] INDEX name ON table (cols) [INDEXTYPE IS BTREE\|BITMAP]` | `CREATE INDEX —USING BTREE` / FULLTEXT | `XuguMigrationsSqlGenerator.IndexOptions` |
| 删除 | `DROP INDEX table.index_name` | `ALTER TABLE —DROP INDEX …` | `Generate(DropIndexOperation)` |
| 重命�| `ALTER INDEX table.old RENAME TO new` | `ALTER TABLE —RENAME INDEX …` | `Generate(RenameIndexOperation)` |
| 位图索引 | `INDEXTYPE IS BITMAP` | 不支�| `HasIndexType(Bitmap)` |
| 全文/RTREE | 文档未对外发�FULLTEXT tail opt | FULLTEXT / SPATIAL | Migration **NotSupported** |

## 已知 XuguDB vs MySQL 差异（必读）

| 差异�| XuguDB | MySQL/Pomelo | 处理 |
|--------|--------|-------------|------|
| 自增�| `IDENTITY(1,1)` | `AUTO_INCREMENT` | Migrations + Convention |
| 兼容模式 | 需 SET compatible_mode | 不需�| Connection 初始�|
| 分页 | LIMIT + TOP 都支�| 主要 LIMIT | QuerySqlGenerator |
| 标识符（MYSQL模式：| 不转换大小写 | 不转�| 一�|
| RETURNING | 支持 | MySQL 8.0.21+ 部分支持 | UpdateSqlGenerator 可利�|
| DateTime.Date | `DATE(expr)` | `CONVERT(expr, date)` | DateTimeMemberTranslator |
| DateTime.Add* | `TIMESTAMPADD(unit, n, dt)` | `DATE_ADD(dt, INTERVAL n unit)` | DateTimeMethodTranslator |
| DateTime 部分提取 | `YEAR()`/`MONTH()` 等独立函�| `EXTRACT(part FROM —` | DateTimeMemberTranslator |
| Guid 生成 | `SYS_GUID()` | `UUID()` | NewGuidTranslator |
| 类型转换 | `CAST(expr AS type)` 标准 SQL | MySQL CAST 映射�| QuerySqlGenerator |
| DateTimeOffset.LocalDateTime | **�CONVERT_TZ** | `CONVERT_TZ(..., @@session.time_zone)` | 不翻译（客户端求值） |
| DateOnly.ToDateTime | `MAKE_TIMESTAMP(Y,M,D,h,m,s)` | `ADDTIME(CAST(date AS datetime), time)` | DateTimeMethodTranslator |
| TimeOnly.Add* | `ADDTIME(CAST(t AS TIME), INTERVAL n unit)` | `DATE_ADD` / `ADDTIME` | DateTimeMethodTranslator |
| EF.Functions.Degrees/Radians | `DEGREES()` / `RADIANS()` | 同左 | DbFunctionsExtensionsMethodTranslator |
| double.RadiansToDegrees | `DEGREES()` | 同左 | MathMethodTranslator |
| DateTimeOffset.Now | `SYSTIMESTAMP()` | `UTC_TIMESTAMP()` | DateTimeMemberTranslator |
| DateDiff (DbFunctions) | `TIMESTAMPDIFF`→`BIGINT`，公�`int` API 末端 `CAST AS INTEGER` | 同左 | XuguDateDiffFunctionsTranslator |
| Count / LongCount 物化 | `CAST(COUNT(— AS INTEGER\|BIGINT)`（规避驱�E34412）| 直接 `COUNT` | XuguQuerySqlGenerator |
| DateOnly/TimeOnly/DTO ADO | Provider string converter 适配官方驱动；DTO 字面量带偏移；`+H`/`+HH:mm` 读回 | 原生 CLR 绑定 | XuguTemporalValueConverters |
| byte[] Contains | `LOCATE(LPAD(HEX(b),2,'0'), HEX(src)) > 0` | 同左（BLOB 旁路：| XuguByteArrayMethodTranslator |
| byte[] First / indexer | `CONV(SUBSTRING(HEX(src),—,16,10)` | MySQL: `ASCII(SUBSTRING—` | XuguByteArrayMethodTranslator / SqlTranslatingExpressionVisitor |
| string `+`（ExecuteUpdate：| `CONCAT(a, b)`（禁�`+`，E17003）| 同左 | XuguQuerySqlGenerator.VisitSqlBinary |
| byte[] First | `ASCII(blob)` | 同左 | XuguByteArrayMethodTranslator |
| byte[] Length | `LENGTH(blob)` | 同左 | XuguSqlTranslatingExpressionVisitor |
| byte[] indexer / ElementAt | `ASCII(SUBSTRING(blob, index+1, 1))` | 同左 | XuguSqlTranslatingExpressionVisitor |
| Math.Max / Math.Min | `GREATEST(—` / `LEAST(—` | 同左 | XuguSqlTranslatingExpressionVisitor |
| TimeOnly subtract | `subtract(left, right)` �TIME | 同左 | XuguSqlTranslatingExpressionVisitor |
| DbFunctions.Like | `LIKE —[ESCAPE …]` | 同左 | XuguDbFunctionsExtensionsMethodTranslator |
| DbFunctions.Hex | `HEX(expr)` | 同左 | XuguDbFunctionsExtensionsMethodTranslator |
| DbFunctions.Unhex | `UNHEX(expr)` | 同左 | XuguDbFunctionsExtensionsMethodTranslator |
| object.ToString() | `CAST(expr AS VARCHAR)` | 同左 | XuguObjectToStringTranslator |
| Regex.IsMatch | `REGEXP_LIKE(expr, pattern)` | `expr REGEXP pattern` | XuguRegexIsMatchTranslator |
| StringComparison.Equals | `LCASE` 双端（IgnoreCase：| `COLLATE utf8mb4_bin` | XuguStringComparisonMethodTranslator |
| Math.Log (1-arg) | `LN()` | `LOG()` | XuguMathMethodTranslator |
| Math.Log (2-arg) | `LOG(base, value)` 参数序与 CLR 相反 | `LOG(value, base)` | XuguMathMethodTranslator |
| TimeSpan members | `HOUR()` 等独立函�| `EXTRACT(hour FROM —` | XuguTimeSpanMemberTranslator |
| TimeSpan/TimeOnly 物化 | `GetString` + `CustomizeDataReaderExpression`（无 ValueConverter 叠层：| converter-only shaper | XuguTimeSpan/TimeOnlyTypeMapping |
| DateTimeOffset.DayOfYear | `DAYOFYEAR(DATE(timestamptz))`（直�DAYOFYEAR(timestamptz)→E10049/E17007）| —| XuguDateTimeMemberTranslator |
| DML CROSS JOIN | 实库 E19132；SELECT CROSS JOIN 有文�| MySQL multi-table UPDATE | Northwind Bulk 负向 |
| MySQL YEAR 类型 | **�YEAR 列类�* | `YEAR` | **skip** |
| ConvertTimeZone | **�CONVERT_TZ** | `CONVERT_TZ(dt, from, to)` | **不实�*（defer：|
| FULLTEXT IsMatch | **�MATCH AGAINST** | `MATCH —AGAINST` | **不实�* |
| HasTables | `ALL_TABLES`（`VALID='T'`, `IS_SYS='F'`：| `information_schema.tables` | XuguDatabaseCreator |

## FOR UPDATE / 位运算（Phase 12 W3 收口）

> 文档：`reference/sql/select/select.md` §FOR UPDATE；`reference/sql/datatype/bit.md`；`reference/sql/operators/bit-operators/`

| 场景 | XuguDB | Provider | 状�|
|------|--------|----------|------|
| `SELECT —FOR UPDATE` | 支持行排他锁 | —| **excluded-with-evidence**（.Q12；EF Core 无标�Tag 翻译入口 �W4 formal：|
| 窗口函数 | 文档子集支持 | —| **excluded-with-evidence**（.Q12；无 EF 标准 API �W4 formal：|
| 整数位运�`& \| ^ << >>` | BIGINT 返回；BIT 类型独立 | `BitwiseOperationReturnTypeCorrectingExpressionVisitor` | **done**：2.302）|
| `BitwiseOperationReturnTypeCorrecting` | Pomelo 用于 MySQL 返回类型修正 | 同上 | **done**：2.302）|

## DDL 差异

| 操作 | 文档路径 | 负责 Agent | 状�|
|------|---------|-----------|------|
| CREATE TABLE | `reference/object/table/create.md` | Migrations | done |
| IDENTITY �| `reference/object/table/create.md#4-opt_serial` | Migrations | done |
| ALTER COLUMN | `reference/object/table/alter.md` | Migrations | done |
| CREATE INDEX | `reference/object/indexes.md` | Migrations | done |
| 迁移�| `reference/object/table/lock.md` | Migrations | done |
| Schema diff（ModelDiffer：| EF Core #25899 字符�NOT NULL | Migrations | done |
| Scaffolding 元数�| `reference/system-view/all/all_tables.md`, `reference/system-view/all/all_columns.md` | Migrations | partial |
| 列重命名 | **�RENAME COLUMN**；`ADD + UPDATE + DROP`  workaround | MySQL 8 `RENAME COLUMN` / `CHANGE` | `XuguMigrationsSqlGenerator` |
| �列备�| `COMMENT ON TABLE/COLUMN —IS …`；CREATE 内联 `COMMENT '—` | MySQL `COMMENT=` | `XuguMigrationsSqlGenerator` |
| Identity PK 类型变更 | **不支持自�ALTER**；需手工重建�| Pomelo DropPrimaryKey+recreate | throws `NotSupportedException` |
| 索引前缀长度 | **�INDEX(col(N)) 语法** | MySQL `HasPrefixLength` | 注解存储 only（.E2）|
| 视图 Scaffolding | `ALL_VIEWS` + `ALL_VIEW_COLUMNS` | `INFORMATION_SCHEMA.VIEWS` | `XuguDatabaseModelFactory` |
| Convert 扩展 | `CAST(expr AS type)` | �| `XuguConvertTranslator` (+ unsigned/float) |
| SqlTranslatingVisitor | GREATEST/LEAST、byte[]、TimeOnly、string[] Concat/Join | Pomelo 对齐（无 JSON：| `XuguSqlTranslatingExpressionVisitor` |

## Scaffolding 元数据（DBA 视图）

> 文档：`reference/system-view/all/all_tables.md`, `reference/system-view/all/all_columns.md`

| �| XuguDB | MySQL/Pomelo | Provider |
|----|--------|-------------|----------|
| 表列�| `ALL_TABLES`（`VALID='T'`, `IS_SYS='F'`：| `INFORMATION_SCHEMA.TABLES` | `XuguDatabaseModelFactory` |
| 列信�| `ALL_COLUMNS`（`IS_SERIAL` �IDENTITY：| `INFORMATION_SCHEMA.COLUMNS` | 同上 |
| CHAR/VARCHAR | `VARYING` + `SCALE` | `DATA_TYPE` + `CHARACTER_MAXIMUM_LENGTH` | `BuildStoreType()` |
| NUMERIC 精度 | `SCALE/65536`, `SCALE%65536` | 直接�| `BuildStoreType()` |
| 主键/索引/FK | `ALL_INDEXES` + `ALL_CONSTRAINTS` | `INFORMATION_SCHEMA` | `XuguDatabaseModelFactory` |
| 视图 | `ALL_VIEWS` + `ALL_VIEW_COLUMNS` | `INFORMATION_SCHEMA.VIEWS` | `XuguDatabaseModelFactory`（`DatabaseView`：|
| 外键动作 | `DELETE_ACTION`/`UPDATE_ACTION` 单字�(n/c/u/d/r) | RESTRICT/CASCADE 等字符串 | `MapReferentialAction()` |
| 索引类型 | `INDEX_TYPE` 0— (BTREE/RTREE/FULLTEXT/BITMAP) | FULLTEXT/SPATIAL 注解 | `XuguIndexType` Fluent API + Migration DDL |
| Collation/Charset | **不适用**（连接级 `CHAR_SET`：| �列级 HasCharSet | 不实�Pomelo Collation |

## 变更日志

| 日期 | 变更 | 作�|
|------|------|------|
| 2026-07-23 | Wave A Task 4: scaffolding/HasTables/fixture catalog uses ALL_* (E18012) | Provider |
| 2026-07-19 | Phase 13：ado-driver-contract v1；CompatibleMode 枚举 ORACLE/MYSQL/POSTGRESQL（仅会话 SET）；并发决策 C；BUSINESS-SQL-BACKLOG frozen；AppCapabilityMatrix 门禁 | Provider / Testing / Docs |
| 2026-07-13 | Post-GA 运行时缺口：`CAST(COUNT AS INTEGER\|BIGINT)`；`TIMESTAMPDIFF`→`BIGINT` 再转 `int`；DateOnly/TimeOnly/DTO string converter（`TIME(3)` 默认、DTO 无未文档 precision 后缀、`+H` 读回）；证据 `RuntimeGap` native/compat 9/9 | Provider |
| 2026-07-09 | Phase 12 W4：NTS/FULLTEXT/Collation/CONVERT_TZ/Scaffolding Baselines formal exclusion（`out-of-scope-approved-12.409.md`：| W4 |
| 2026-07-06 | Phase 8 W4：FOR UPDATE/位运�defer 登记；Translator/TypeMapping/Migration/Scaffolding 测试扩展 | Orchestrator |
| 2026-07-06 | Phase 8 W3：Having/BoolOptimizing/Postprocessor visitors；ExecuteUpdate 多表 LIMIT 守卫；SequentialGuid；MigrationsModelDiffer 索引/Collation 过滤 | Orchestrator |
| 2026-07-06 | Phase 8 W2：SqlTranslating/Convert 扩展；Migration 列重命名/备注；视�Scaffolding；Extensions E1–E3 | Orchestrator |
| 2026-07-06 | Phase 8 W1：StringComparison/Math/TimeSpan Translators；专�TypeMapping 注册�| Orchestrator |
| 2026-07-06 | Phase 7 W1：TypeMapping 专用类（GUID/BOOL/TIME/uint/ulong）；Retry defer 文档�| Storage |
| 2026-07-06 | 批次 B：Unhex/ObjectToString Translator；NorthwindDbFunctions + DateOnly/TimeOnly 测试；TypeMapping NUMERIC/BINARY | Orchestrator |
| 2026-07-06 | 波次 7：DateDiff/ByteArray/DbFunctions.Like Translator；HasTables via ALL_TABLES | Orchestrator |
| 2026-07-06 | 波次 6：实库索�create/rename/drop 验收；`ALL_INDEXES.VALID=1` 用于集成测试断言 | Orchestrator |
| 2026-07-06 | 波次 5：Git 追踪 + Index DDL + Scaffolding 集成测试 + CI 打包脚本 | Orchestrator |
| 2026-07-06 | Phase 4：IDENTITY(1,1) MigrationsSqlGenerator、LOCK TABLE 迁移锁、HistoryRepository | Agent-Migrations |
| 2026-07-06 | Phase 3 扩展：Convert/DateTimeOffset/TimeOnly/DateOnly Translator + AssertSql 测试 | Agent-QueryExtensions |
| 2026-07-06 | Phase 3：DateTime/TimestampAdd/CAST/SYS_GUID 函数映射；DateTimeQueryTests | Agent-Query |
| 2026-07-06 | Phase 3：LENGTH/ABS/CONCAT/LIKE 函数映射；Query DI 注册 | Agent-Query |
| 2026-07-06 | 初稿，基�XuguDB 官方文档整理 | Orchestrator |
| 2026-07-07 | 来源血缘校验脚�`scripts/verify-source-lineage.ps1`（禁�AUTO_INCREMENT/INFORMATION_SCHEMA 等） | Orchestrator |
| 2026-07-07 | Phase 9 M3 关闭：`XuguTestStore` 全量 adoption、Northwind seed、`XuguQueryTestBase`、`AssertSql` 基线�0+ Collection fixtures：76 列测（.0.0 发版 | Testing / Orchestrator |
| 2026-07-07 | Phase 10 Wave 1：CI 实库矩阵（GitHub + GitLab（ `verify.ps1 -RunTests` 全量门禁；`docs/GETTING-STARTED.md` �2.0.0；`docs/XUGU-VS-MYSQL.md` 用户对比文档；`docs/references/phase-10-test-triage.md` 剩余 ~374 测试 triage | Infra / Docs / Testing |
| 2026-07-07 | Phase 10 Wave 2：Query 深覆�+119（FromSql / TPH / Deep nested / DbFunctions / ComplexNav）对�Pomelo `NorthwindQueryMySqlTest` + `AdHocQueryMySqlTest` 子集（.T defer 补全（SaveChangesInterception +6 / ConvertToProviderTypes +10 / Seeding +3 / WithConstructors insert ×2）；795 列测：0.M2 �| Testing |
| 2026-07-08 | Phase 10 Wave 3：`MonsterFixupXuguTests` + `StoreGeneratedFixupXuguTests`（手�Xugu 兼容模型，对�Pomelo `MonsterFixup*MySqlTest`）；`DesignTimeXuguTest` + `KeysWithConverters` + `TransactionBasics` 子集（对�`EFCore.Specification.Tests` 数据库相关）：50 列测：0.M4 ✅；~81% Pomelo 覆盖 | Testing |
| 2026-07-08 | Phase 10 Wave 5：OFFSET 参数内联（`XuguInlinedParameterExpression`）；Linux RID blocked 登记 | QueryCore |
| 2026-07-08 | Phase 10 Wave 4：`XuguRetryingExecutionStrategy` + `XuguTransientExceptionDetector`：0.106 ✅）：0.105 ROW_COUNT **blocked**（实�E10049：`ROW_COUNT()` 不存在）：60 列测 | Storage / Testing |
| 2026-07-09 | Phase 12 W5：ROW_COUNT **signed-off blocked**：2.509/PLAT-01：2.501 复验 E10049）；Linux RID **signed-off**（PLAT-02）；`PlatformLimitationProbeTests` | Platform / Testing |
| 2026-07-08 | Phase 11 W1：方言权威声明强化；JSON § 扩展�11.109 实现脚手架；COMPATIBLE_MODE 标注为可选开发便�| Docs |
| 2026-07-08 | Phase 10 Wave 6：0.108）：JSON 原生类型 + 函数已确认；Provider defer 10.109 �Phase 11 | Orchestrator |
