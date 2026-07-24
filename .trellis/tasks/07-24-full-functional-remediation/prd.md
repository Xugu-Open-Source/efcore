# Full Functional release acceptance remediation

## Goal

在 Wave A 已通过基础上，按 2026-07-24 复验报告解决全部剩余问题，使 Windows 双模式 Functional 达到 **0 FAIL**（硬限制权威 Skip）。包版本 **9.0.0**；中间只本地 commit、不 push；**仅最终** push 并覆写 GitHub `v9.0.0`。

## Decisions（已锁定）

| 主题 | 选择 |
|------|------|
| Done | A：0 FAIL + 权威 Skip（APPLY/LATERAL；文档确认的 E17010 等） |
| 版本 | A：最终覆写 `v9.0.0` / 9.0.0 |
| 分波 | A：卫生 → E9016 → E17010 → E19132 → LINQ/语义 → 全量复验+覆写 |
| 中间发布 | A：本地 commit only，不 push；不中途覆写 Release |
| Linux / 列测发现基建 | A：不进本任务硬门禁（可文档说明；另开任务） |

## Background / confirmed facts

- 复验：`TEST-REPORT.md`（2026-07-24 REJECT）；tag `b04fbc4`
- Wave A 已绿：Unit 283；Integration native/compat；核心用户路径；Description
- Functional：native **473** / compat **487** FAIL；183 Skip（APPLY 大部分已 Skip，仍残留 ~12）
- 证据：`test-output/revalidation-20260724-isolated-functional`

| 类别 | native | 处置 |
|------|-------:|------|
| LINQ 翻译 | 122 | 修复 |
| 结果/语义 | 110 | 修复 |
| E19132 | 81 | 优先修复；文档硬限制才 Skip |
| E9016 | 52 | 测试基建 |
| E17010 | 52 | 文档确认后 Skip 或改写 |
| SQL/字符串基线 | 25 | 修复 |
| APPLY 未 Skip | 12 | Skip hygiene |
| E5021/物化/其他 | ~19 | 修或分类 |

## Requirements

| ID | 要求 |
|----|------|
| R1 | Functional native+compat **0 FAIL**（Skip ⊆ LIMITATIONS 硬限制） |
| R2 | Wave1：APPLY 残留 Skip；Provider 包 native 统一；contracts UTF-8 |
| R3 | Wave2：E9016 共享状态/隔离/清理契约 |
| R4 | Wave3：E17010 — Xugu docs 确认后 Skip 或改写 SQL |
| R5 | Wave4：E19132 SQL 生成簇修复 |
| R6 | Wave5：LINQ 翻译 + 结果/语义 + 基线 + E5021/物化/其他 |
| R7 | Wave6：独立套件全量复验 → push → 覆写 `v9.0.0`；升级 RELEASE-SCOPE |
| R8 | 每波结束：相关 Functional filter + Unit/Integration 绿；仅本地 commit |
| R9 | 不宣称 Linux；列测 8412 vs TRX 不一致不阻塞本任务 Done |

## Acceptance criteria

- [ ] 独立套件 Functional native+compat：**0 FAIL**
- [ ] Unit / Integration：**0 FAIL**
- [ ] APPLY/LATERAL 无未 Skip 的 `ApplyNotSupported` FAIL
- [ ] LIMITATIONS 列出全部权威 Skip；E17010/E19132 硬限制有文档锚点
- [ ] nupkg 无冲突自带 native（或文档+构建策略统一为声明依赖）
- [ ] `RELEASE-SCOPE` 从 Wave A 升级为全能力（定义 A）
- [ ] 最终才 remote push + 覆写 GitHub Release `v9.0.0`
- [ ] 中间不 push

## Out of scope

- nuget.org
- Linux 生产实库认证
- Functional 发现/计数基础设施专项（duplicate ID / 8412 vs 5977）
- 性能/长稳/灾备 SLA
- 中间远程推送或中途覆写 tag

## Technical notes

- 方言权威：Xugu 官方 docs；禁止用 MySQL/Pomelo 定义 SQL
- 不改 `external/`
- 错误串走 `XuguStrings.resx`
- 独立套件：`E:\Work\Tests\entityframeworkcore-xugu-release-test`；Functional 宜按类隔离（复验已证明必要）
