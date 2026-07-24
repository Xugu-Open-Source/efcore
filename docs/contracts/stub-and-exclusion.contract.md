# Stub 与排除策略契约（Living Document）

> **关联**：`sql-dialect.contract.md`、`docs/LIMITATIONS.md`、`docs/RELEASE-SCOPE.md`  
> **权威**：SQL/功能是否存在�`E:\BaiduSyncdisk\docs\content\` 为准：*Pomelo/MySQL 不是行为参�*  
> **完全体门�*：`harness/tasks/phase-11-xugu-native-release/PHASE11-CLOSURE-CRITERIA.md`（Adjusted 100%）

---

## 1. 目的

XuguDB �MySQL/Pomelo **不等�*。当官方文档无对应能力、或能力�Xugu 语义下不可实现时，Provider **不得**静默照搬 MySQL 行为；必�**stub**（显式拒�跳过/空实现）�**登记文档**�

本契约定义：**何时 stub**�*如何登记**�*实现流程**�*与完全体 exclusion 的关�*�

---

## 2. 权威与禁止事�

| 优先�| 来源 | 用�|
|--------|------|------|
| 1 | `E:\BaiduSyncdisk\docs\content\` | SQL、类型、函数、DDL/DML **唯一权威** |
| 2 | `docs/contracts/sql-dialect.contract.md` | 已确认方言规则 |
| 3 | 本文 + `docs/LIMITATIONS.md` | stub / skip / exclusion 产品承诺 |
| 4 | `external/Pomelo.EntityFrameworkCore.MySql` | **�* C# 架构、DI、Translator 模式 |
| —| MySQL 文档 / Pomelo 测试预期 | **禁止**作为 SQL 或运行时行为依据 |

**禁止**）

- �Pomelo 测试通过即认�Xugu 应相同行�
- �`COMPATIBLE_MODE=MYSQL` 下偶然兼容当作产品保�
- stub 后不�LIMITATIONS / contract / parity 矩阵

---

## 3. 标准流程（check doc �implement �stub + record）

```
┌─────────────────────────────────────────────────────────────�
�1. 打开 xugudb-docs-map.md，定位官方文档章�                 �
└───────────────────────────┬─────────────────────────────────�
                            �
              ┌─────────────────────────────�
              �文档明确支持（              �
              └─────────────┬───────────────�
                    �     �     �
                    �      �      �
         �Xugu 文档实现    �  进入 stub 决策（�?）
         更新 sql-dialect    �
         contract           �
                            �
              ┌─────────────▼───────────────�
              �文档未提�/ �MySQL 有？   �
              └─────────────┬───────────────�
                            �
                    stub + 登记（�?）
                    不实�MySQL 等价
```

**Handoff 要求**：任�SQL 相关 PR/任务必须�Handoff 中注�**Xugu 文档路径**；若�stub，注�**disposition ID**（见 §6）�

---

## 4. 何时 stub —三种处置

### 4.1 运行�Stub —`NotSupportedException`

**适用**：用户调�EF/Fluent API 会触达不支持�DB 能力；应 **快速失�* 并给�`.resx` 消息�

| 场景 | 示例 | 登记位置 |
|------|------|----------|
| DDL 不支�| 过滤索引、FULLTEXT/RTREE 迁移、`IDENTITY` PK 类型变更 | `XuguMigrationsSqlGenerator` + LIMITATIONS |
| 运维边界 | `CREATE DATABASE` / `DROP DATABASE` | `XuguDatabaseCreator` |
| 迁移能力 | 幂等脚本生成 | `XuguHistoryRepository` |
| 文档�FULLTEXT | `IsFullText()` 注解存在但迁�`NotSupported` | LIMITATIONS + 本文 §7 |

**要求**）

- 使用 `Properties/XuguStrings.resx`，禁止裸字符�
- �`sql-dialect.contract.md`「已知差异」或 DDL 表增加一�
- 单元/迁移测试断言 `NotSupportedException`（若 API 可触达）

### 4.2 测试 Stub —Skip / Category / �port

**适用**：Pomelo 测试覆盖的能力在 Xugu **永久不可�* �**无测试宿�*�

| 机制 | 何时使用 | 示例 |
|------|----------|------|
| `[SkippableFact(Skip = "...")]` | 单方�defer/blocked，有明确解除条件 | ROW_COUNT、WithConstructors |
| �port Pomelo 测试�| 整模块无 Xugu 文档依据 | NTS/Spatial、FULLTEXT Match、Collation |
| `Category=NativeDialect` | �native 语义相关 | �compat 对照分离 |
| `XuguTestConnection.SkipIfUnavailable()` | 无实�—**�*能力 stub | 基础设施 |

**要求**）

- Skip 字符串含 **原因 + ID**（如 `E10049`、`EF #31376`）
- 记入 `test-parity-matrix.md`：*ported | Xugu-adapted | excluded-with-evidence**
- 完全体前：Skip �**0** 或移�**OUT OF SCOPE �*（W14.1109）

### 4.3 API Stub —No-op / 注解存储 / 客户端求�

**适用**：Fluent API 需�Pomelo **表面对齐**，但 Xugu 无运行时等价�

| 模式 | 行为 | 示例 |
|------|------|------|
| **No-op 扩展** | 方法存在但不改变模型语义 | Pomelo `HasCharSet` —Xugu 用连接串 `CHAR_SET`（文档注释说�skip：|
| **注解 only** | 存储元数据，迁移/查询不生�MySQL SQL | 索引前缀长度 `HasPrefixLength` |
| **不翻�* | LINQ 回退客户端求�| `DateTimeOffset.LocalDateTime`（无 `CONVERT_TZ`：|
| **替代实现** | �Xugu 文档函数达成相近目标 | `REGEXP_LIKE` 替代 `MATCH —AGAINST` |

**要求**）

- 公共 API 须有 XML 文档说明 **Xugu 实际行为**
- �Pomelo 同名 API 存在，在 `LIMITATIONS.md` 对照表列�**skip / 替代**

---

## 5. 文档登记要求（缺一不可）

每次 stub �exclusion 必须同步 **至少三处**）

| 文档 | 登记内容 |
|------|----------|
| **`docs/LIMITATIONS.md`** | 用户可见：能力名、状态（skip/defer/blocked/done）、原因、变�|
| **`sql-dialect.contract.md`** | 方言级：SQL 差异、函数映射表一行、DDL �|
| **本文 §6 �parity 矩阵** | 项目级：disposition ID、Pomelo 对照、Wave 任务 |

**完全体（3.0.0）额外要�*（`PHASE11-CLOSURE-CRITERIA.md` §B）：

- **XuguDB 官方文档链接**（证明不可实现或不在产品范围）
- **用户 approved OUT OF SCOPE �*（`W14.1109`）—表为空或每项�evidence = Adjusted 100%

---

## 6. Disposition 分类（与 Phase 11 对齐）

|  disposition | 含义 | 完全体要�|
|-------------|------|-----------|
| **implemented** | �Xugu 文档实现 | 测试 PASS |
| **Xugu-adapted** | 测试/断言改写�Xugu 语义 | 0 FAIL |
| **excluded-with-evidence** | 文档证明不可实现 �stub + OUT OF SCOPE | doc link + approval |
| **blocked** | 依赖驱动/DB vendor（ROW_COUNT、Linux RID：| ticket �signed-off exclusion |
| **defer** | 有解路径但未排期（DateOnly SC、FOR UPDATE：| W12 resolved �reclassified |

**禁止第四�*：未分类�silent gap（既无实现也�exclusion 记录）�

---

## 7. 当前已登�stub 清单（审计快�2026-07-09）

> 扫描结论：*�*发现 `AUTO_INCREMENT` / `INFORMATION_SCHEMA` �MySQL 硬编�SQL（`verify-source-lineage.ps1` 门禁）� 
> 下列�**应维�stub** �**需 W14 正式 exclusion** 的项�

### 7.1 Provider 运行�NotSupported（已正确 stub）

| 能力 | 代码位置 | 文档依据 |
|------|----------|----------|
| CREATE/DROP DATABASE | `XuguDatabaseCreator` | 运维边界；无 EF 产品承诺 |
| 幂等迁移脚本 | `XuguHistoryRepository` | Xugu 迁移模型 |
| 过滤索引 DDL | `XuguMigrationsSqlGenerator` | 文档�filtered index |
| FULLTEXT/RTREE 索引迁移 | 同上 | `indexes.md` 无对�FULLTEXT tail |
| IDENTITY PK 类型变更 | 同上 | Xugu IDENTITY 限制 |

### 7.2 No-op / 注解 / 不翻译（MySQL API 表面，Xugu 无等价）

| 能力 | 处置 | 建议 |
|------|------|------|
| `HasCharSet` / `HasCollation` | **未暴�*（正确） | W14 doc exclusion |
| `ConvertTimeZone` / `CONVERT_TZ` | **不翻�* | 保持；W14 exclusion |
| `IsFullText()` Fluent | 注解可设；迁�**NotSupported** | 文档注明「仅注解；DDL 拒绝�|
| `DateTimeOffset.LocalDateTime` | 客户端求�| contract 已登�|

### 7.3 测试 Skip（W4 收口 —0 open defer）

| 测试 | 原因 | disposition |
|------|------|-------------|
| `OptimisticConcurrencyTests.Stale_*` | E10049 ROW_COUNT | **signed-off blocked**：2.509/PLAT-01）| W5 |
| `WithConstructorsTests` graph insert | constructor insert �`DbUpdateException`（可执行断言，非 Skip：| **excluded-with-evidence**：2.312）|
| `ComplexTypesTrackingTests` optional null | EF #31376；模�`IsNullable=false` + SaveChanges 拒绝 null | **excluded-with-evidence**：2.313）|
| `LazyLoadTests` proxy | �proxy 宿主；断言未加载直�Explicit Load（OOS-08）| **excluded-with-evidence**：2.410）|
| `SeedingTests.EnsureCreated_*` | 共享 SYSTEM �`EnsureCreated` 返回 false、不应用 HasData | **excluded-with-evidence**：2.410）|
| Sequence `RESTART WITH` / HiLo | Alter 文档�RESTART；HiLo 未做 | **NotSupported / defer**：026-07-21）|
| Spec Functional W2+ | Northwind Spec 全量替换、Interception 全家桶等 | **defer**（W1 已落地，�`2026-07-21-spec-matrix-alignment-design.md`：|

### 7.4 永久 skip 模块（W4 formal —不计�Adjusted 分母）

> **权威�*：`docs/references/out-of-scope-approved-12.409.md`：*approved** @ 12.409）

| 模块 | Pomelo 估算测试 | disposition | OOS ID |
|------|----------------|-------------|--------|
| NTS / Spatial | 32 | **excluded-with-evidence** | OOS-01 |
| FULLTEXT / Match | 15 | **excluded-with-evidence**；`REGEXP_LIKE` 替代 | OOS-02 |
| Collation / HasCharSet | 10 | **excluded-with-evidence** | OOS-03 |
| CONVERT_TZ | 5 | **excluded-with-evidence** | OOS-04 |
| Scaffolding Baselines 全量 | 20 | **excluded-with-evidence** | OOS-05 |
| IntegrationTests | 15 | **excluded-with-evidence** | OOS-06 |
| TwoDatabases | 6 | **excluded-with-evidence** | OOS-07 |
| Lazy proxy 余量 | 4 | **excluded-with-evidence** | OOS-08 |

**Adjusted 分母**：050 �98 = **952**；见 `adjusted-denominator-12.411.md`�

### 7.5 需关注�**�* 立即改代�

| 观察 | 说明 |
|------|------|
| 大量测试注释引用 `*MySqlTest` | **可接�* —追溯 Pomelo 源；断言�Xugu �|
| `GearsOfWarQueryMySqlTest` �port | defer —需独立模型；非 MySQL 假设 |
| compat 测试�`XUGU_DIALECT_MODE=compat` 运行 | 开发对照；**产品 SQL �native + 文档为准** |

---

## 8. Agent Checklist

**开工前**）

- [ ] 读本�+ `sql-dialect.contract.md`
- [ ] �Xugu 官方文档（非 Pomelo �SQL）
- [ ] �`LIMITATIONS.md` 是否已有 stub 登记

**实现�*）

- [ ] 文档��Xugu 原生实现 + contract 更新
- [ ] 文档���§4 一�stub + §5 三处登记

**完工�*）

- [ ] `verify-module.ps1` PASS
- [ ] 更新 `test-parity-matrix.md` disposition（若涉及测试）
- [ ] Handoff 注明文档路径�exclusion ID

---

## 变更日志

| 日期 | 变更 |
|------|------|
| 2026-07-09 | W4 收口：OUT OF SCOPE approved（`out-of-scope-approved-12.409.md`）；Adjusted 952；Skip 0 open defer |
| 2026-07-09 | 初稿：stub 三态、登记流程� 节审计快照、与 Phase 11 Adjusted 100% 对齐 |
