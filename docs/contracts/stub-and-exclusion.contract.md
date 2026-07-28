# Stub 与 Exclusion 契约

> **关联**：[sql-dialect.contract.md](sql-dialect.contract.md) · [LIMITATIONS.md](../LIMITATIONS.md) · [RELEASE-SCOPE.md](../RELEASE-SCOPE.md)  
> **权威**：能力是否存在以 `E:\BaiduSyncdisk\docs\content\` 为准；Pomelo/MySQL **不是**行为参考。  
> **原则**：无官方文档（或实库否定性证据）时，**禁止 silent 实现**；应 `NotSupported` / translator 拒绝 / Functional `Skip`，并在 LIMITATIONS 或本文登记。

## 处置等级

| 等级 | 含义 | 典型动作 |
|------|------|----------|
| **implement** | 文档支持且实库可验证 | 翻译 + 测试 |
| **provider-workaround** | 引擎/文档可做，驱动有缺口 | Provider 旁路（见 ado-driver-contract） |
| **skip / not-supported** | 无 API、实库稳定失败、或产品排除 | `XuguStrings.*` / `[Skip]` + 原因 |
| **excluded** | 产品永久不纳入（有证据） | LIMITATIONS OUT OF SCOPE |
| **defer** | 可做但本版本不做 | LIMITATIONS + 任务 backlog |

## 决策流程

1. 查 [xugudb-docs-map.md](../references/xugudb-docs-map.md) → 打开官方页。  
2. 有语法/函数 → 登入 [sql-dialect.contract.md](sql-dialect.contract.md) 再实现。  
3. 无文档 → **不要**从 MySQL 脑补；记入本文 + LIMITATIONS。  
4. 有文档但实库拒绝 → 记录 XGCI 码与最小 SQL 探针；Skip 或改写生成。  
5. Functional Skip **必须**带具体原因（错误码 / 文档缺口 / 语义 residual），禁止空 Skip。

## 已登记 exclusion / skip（9.0.0）

| 能力 | 处置 | 证据 / 去向 |
|------|------|-------------|
| `CROSS APPLY` / `OUTER APPLY` / `LATERAL` | **not-supported** | `from.md` 无；`ApplyNotSupported`；实库 E19132 |
| `FROM` 子查询引用上级表达式 | **skip** | E17010；`subquery.md` |
| `JSON_TABLE` / 原始集合行集展开 | **skip** | 官方 JSON 函数无 JSON_TABLE；实库 E19132 |
| 参数集合 `Contains` 标量 membership | **implement** | Wave5 `GenerateIn` JSON 标量谓词（非 exclusion） |
| `FULLTEXT` / `MATCH…AGAINST` | **excluded** | `indexes.md` 无对外全文 |
| NetTopologySuite / Spatial ORM | **excluded** | 无 EF NTS 集成包 |
| 列/表级 Collation Fluent | **excluded** | 连接级 `CHAR_SET` |
| `CONVERT_TZ` / `ConvertTimeZone` | **excluded** | 无等价函数 |
| `CREATE/DROP DATABASE`（EF） | **excluded** | 运维边界 |
| Sequence `RESTART WITH` / HiLo | **not-supported** / **defer** | `sequence.md`；HiLo 未做 |
| Linux x64 生产 | **blocked**（signed-off） | 无稳定验收；Wave A 仅 Windows |
| DateTimeOffset 相等 / `Contains` 过滤 | **skip**（驱动） | ado-driver DRV-04/05/06 |
| EF `ToJson()` owned JSON | **defer / 不承诺** | LIMITATIONS |
| Scaffolding Baselines 全量快照 | **excluded** | 维护成本；主路径 Integration 覆盖 |
| 复杂导航 / TPC M2M 序等 Functional residual | **skip**（语义边角） | LIMITATIONS「Functional residual」；**不**阻塞 Wave A |

## 测试期望

| 层 | Skip / NotSupported 时 |
|----|------------------------|
| Unit | 可断言生成抛错或固定 SQL 金标 |
| Integration | 负向路径或 Skippable 事实；勿当静默绿 |
| Functional | `[ConditionalTheory(Skip = "…证据…")]` 或等价；override 面不全用 Skip 标 `Check_all_*` |

## 变更日志

| 日期 | 摘要 |
|------|------|
| 2026-07-28 | **全文重写**（历史 UTF-8 损坏，git 无干净祖先）。与 9.0.0 LIMITATIONS / Wave5 对齐。 |
