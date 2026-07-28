> **Encoding note (2026-07-28)**: This file has historical invalid UTF-8 bytes / U+FFFD. Numeric paths may still be useful; authoritative release numbers are in docs/RELEASE-SCOPE.md, task implement.md, and artifacts/live-db/.

# Phase 9�?2 测试对等矩阵

> **状�?*：Phase 12 **done** + 3.0.1 patch�?*3.0.x 冻结** compat **1057** / native **1056** / Adjusted **111.0%**�?057÷952�? 
> **冻结文档**：`comparable-set-freeze-12.101.md`  
> **分母**：Pomelo `EFCore.MySql.FunctionalTests` ~**1050** �?Adjusted **952**（剔�?OUT OF SCOPE�? 
> **Note�?2.PG6�?*：W6 早期表曾�?compat **1056**；GA 终态含 platform probe �?**1057**�?1）�?

## Phase 12 GA 冻结基线（终态）

| 指标 | 数�?| 状�?|
|------|------|------|
| compat `--list-tests` | **1057** | **frozen** |
| native `Category=NativeDialect` | **1056** | **done** |
| Adjusted 分母 | **952** | **done** |
| Adjusted 覆盖�?| **111.0%** | **done** |
| 显式 `Skip=`（open defer�?| evidence-backed / signed-off | **done** |

## Phase 12 W1 冻结基线�?026-07-09 历史�?

| 指标 | 当时 | Adjusted 目标 | 状�?|
|------|------|--------------|------|
| compat `--list-tests` | **1056**（后 �?**1057**�?| frozen | **done** |
| native `Category=NativeDialect` | **1056** | �?45�?0%�?| **done** |
| Pomelo 可比�?disposition | **~155** | 100% 分类 | **done** |
| Excluded（OUT OF SCOPE�?| **98** | formal evidence | **done** |
| Adjusted 分母 | **952** | 12.411 recalc | **done** |
| Adjusted 覆盖�?| **110.9%**�?*111.0%** | �?00% | **done** |
| 显式 `Skip=`（open defer�?| **0** | evidence-backed | **done** |
| compat 3× CI | **0 FAIL** | 连续 3× | **done**�?3.107 复验�?|

> **审计命令（拆分后�?*：`dotnet test test/EFCore.Xugu.Tests.Integration -c Release --list-tests`  
> **门禁**：`scripts/run-compat-gate.ps1 -MaxAttempts 3`

## Phase 11 W11 基线�?026-07-09 W11.802�?05 后实测）

| 指标 | 当前 | 完全体目�?| 缺口 | Wave |
|------|------|-----------|------|------|
| compat `--list-tests` | **1056** | **1050** �?adjusted 100% | **0 literal** | W11 �?|
| `[Fact]/[Theory]` 属�?| **~780+** | �?| W11.811 对账 | W11 |
| 测试 `.cs` 文件 | **~175** | �?| �?| W11 |
| 显式 `Skip=` | **6** | **0** | 6 | W11/W12 |
| native `--list-tests` | **263** | �?compat 80% (~845) | ~582 | W11.808 partial |
| Pomelo 可比覆盖�?| **~100.6%** literal | **100%** adjusted | W14 triage | W11–W14 |

> **审计命令**：`dotnet test test/EFCore.Xugu.Tests -c Release --list-tests`  
> W11.802�?05 增量 **+158**�?98�?056）；native +86�?77�?63）�?

## W11 批次增量�?1.802�?05�?

| 文件 | 新增（list-tests 约） | Pomelo �?| Wave |
|------|---------------------|-----------|------|
| `QueryNorthwindAggregateOperatorsTests.cs` | ~18 | NorthwindAggregateOperators | 11.802 |
| `QueryNorthwindMiscellaneousTests.cs` | ~20 | NorthwindMiscellaneous | 11.802 |
| `QueryNorthwindAsTrackingTests.cs` | ~8 | NorthwindAsTracking | 11.802 |
| `NorthwindSqlRawExtendedTests.cs` | ~10 | SqlExecutor/FromSql | 11.802 |
| `TPTInheritanceQueryTests.cs` | ~12 | TPTInheritanceQuery | 11.802 |
| `NonSharedModelUpdatesTests.cs` | ~11 | NonSharedModelUpdates | 11.803 |
| `TransactionInterceptionTests.cs` | ~8 | Transaction（语义子集） | 11.803 |
| `GraphUpdatesExtendedTests.cs` | ~10 | GraphUpdates | 11.803 |
| `MigrationExtendedTests.cs` | ~10 | MigrationMySql | 11.804 |
| `ScaffoldingExtendedTests.cs` | ~8 | ScaffoldingMySql | 11.804 |
| `DesignTimeExtendedTests.cs` | ~8 | DesignTimeMySql | 11.804 |
| `DbFunctionsExtendedQueryTests.cs` | ~11 | MySqlDbFunctions | 11.805 |
| `ValueGenerationExtendedTests.cs` | ~8 | ValueGeneration | 11.805 |
| `ConnectionSettingsExtendedTests.cs` | ~9 | ConnectionSettings | 11.805 |
| **W11.802�?05 小计** | **~158** | �?| 898�?*1056** |

---

## Phase 9 里程碑进度（历史�?

| 里程�?| 目标方法�?| 当前（list-tests�?| Pomelo 可比覆盖�?| 状�?|
|--------|-----------|-------------------|-------------------|------|
| M1�?0%�?| �?00 | 337 | ~32% | done |
| M2�?0%�?| �?00 | 401 | ~38% | done |
| M3�?0%�?| �?00 | **861** | **~82%** | **done**（Wave 3�?�?|

> **门禁�?026-07-08 Wave 5�?*：全�?**861** 列测（Wave 5 +1 参数内联单测）。`verify.ps1` build PASS�?

## Wave 6�?0.108 JSON 调研）�?无列测增�?

| �?| 结果 |
|----|------|
| 10.108 JSON 调研 | **done** �?XuguDB 原生 JSON + 28 函数；Provider defer 10.109 |
| 列测 | **861**（不变） |
| Phase 10 closure | 10.M3 NuGet pack + 文档同步 |

---

## Wave 5 增量�?0.201 参数内联）�?10.205 blocked

| 文件 | 新增（list-tests�?| 说明 |
|------|-------------------|------|
| `TranslatorSqlTests.cs` | +1 | `Skip_with_closure_parameter_inlines_offset_literal` |
| **Wave 5 小计** | **+1** | 860 �?**861** |
| Linux RID 打包 | �?| 10.205 **blocked**（无 `libxugusql.so`�?|

## Wave 4 增量�?0.106 Retry）�?10.105 blocked

| 文件 | 新增（list-tests�?| 说明 |
|------|-------------------|------|
| `XuguTransientExceptionDetectorTests.cs` | 9 | XGCI 瞬态码单元测试 |
| `ExecutionStrategyTests.cs` | +1 | `EnableRetryOnFailure_configures_retrying_strategy` |
| **Wave 4 小计** | **+10** | 850 �?**860** |
| `OptimisticConcurrencyTests` | 0�? �?Skip�?| 10.105 ROW_COUNT **blocked**（E10049�?|

## Wave 3 增量�?0.101 + 10.102）�?10.M4 达标

| 文件 | 新增（list-tests�?| 说明 |
|------|-------------------|------|
| `MonsterFixupXuguTests.cs` | ~22 | 复杂关系�?fixup |
| `StoreGeneratedFixupXuguTests.cs` | ~13 | 存储生成 + fixup |
| `DesignTimeXuguTest.cs` | ~8 | 设计时模�?/ DbContext 工厂 |
| `KeysWithConvertersXuguTests.cs` | ~7 | �?+ 值转换器 |
| `TransactionBasicsXuguTests.cs` | ~5 | 事务基础 |
| **Wave 3 小计** | **+55** | 795 �?**850** |

## Wave 2 增量�?0.103 + 10.104）�?10.M2 达标

| 文件 | 新增（list-tests�?| 说明 |
|------|-------------------|------|
| `FromSqlQueryTests.cs` | 19 | FromSqlRaw/参数/组合 LINQ |
| `TPHInheritanceQueryTests.cs` | 18 | TPH OfType/派生属�?鉴别�?|
| `QueryNorthwindDeepCoverageTests.cs` | 27 | 子查�?EXISTS/GroupJoin/聚合 |
| `NorthwindFunctionsExtensionQueryTests.cs` | 15 | 字符�?数学/日期函数扩展 |
| `AdHocComplexNavigationQueryTests.cs` | 16 | 复杂导航/Include/SelectMany |
| `SaveChangesInterceptionTests.cs` | +6 | sync/update/delete/suppress/异常 |
| `ConvertToProviderTypesTests.cs` | +10 | 扩展标量/enum/nullable 往�?|
| `SeedingTests.cs` | +3 | 多实�?seed/键值；EnsureCreated skip |
| **Wave 2 小计** | **+119** | 676 �?**795** |

## W6 增量（AdHoc + Query 扩展）�?M3 达标

| 文件 | 新增（list-tests�?| 说明 |
|------|-------------------|------|
| `QueryNorthwindWhereTests.cs` | 91 | Where/比较/字符�?导航过滤（SkippableTheory 展开�?|
| `QueryNorthwindJoinTests.cs` | 30 | Inner/Group/Cross Join 子集 |
| `QueryNorthwindGroupingTests.cs` | 27 | GroupBy + Having + 聚合 |
| `QueryNorthwindOrderingTests.cs` | 29 | OrderBy/ThenBy/Take/Skip |
| `QueryNorthwindSelectTests.cs` | 28 | 投影/子查�?匿名类型 |
| `QueryNorthwindIncludeTests.cs` | 22 | Include/SplitQuery/导航过滤 |
| `AdHocNavigationQueryTests.cs` | 15 | 父子导航/SelectMany/显式 Include |
| `AdHocQueryFilterTests.cs` | 10 | 全局查询过滤�?+ IgnoreQueryFilters |
| **W6 小计** | **+252** | 424 �?**676** |
| `AdHocMiscellaneousQueryTests.cs` | 10 | （前期）聚合、Coalesce、条�?|
| `QueryNorthwindExtensionTests.cs` | +12 | （前期）Min/Max/Sum/Avg、Take/Skip |
| `TranslatorSqlTests.cs` | +1 | （前期）TimeOnly ADD TIME |

## W5 批次映射�?.T23–T30�?

| ID | Xugu 测试文件 | 测试数（约） | Pomelo �?| Skip/Defer |
|----|--------------|-------------|-----------|------------|
| 9.T23 | `ComplexTypesTrackingTests.cs` | 8 | `ComplexTypesTrackingMySqlTest` | optional complex defer |
| 9.T24 | `NotificationEntitiesTests.cs` | 2 | `NotificationEntitiesMySqlTest` | �?|
| 9.T25 | `WithConstructorsTests.cs` | 6 | `WithConstructorsMySqlTest` | constructor graph insert ×2 defer |
| 9.T26 | `LazyLoadTests.cs` | 4 | `LazyLoadProxyMySqlTest` | lazy proxy skip |
| 9.T27 | `MusicStoreTests.cs` | 5 | `MusicStoreMySqlTest` | 全量 controller 子集 defer |
| 9.T28 | `XuguApiConsistencyTests.cs` | 11 | `MySqlApiConsistencyTest` | �?|
| 9.T29 | `XuguServiceCollectionExtensionsTests.cs` | 12 | `MySqlServiceCollectionExtensionsTest` | lifetime 全矩�?defer |
| 9.T30 | `ExistingConnectionTests.cs` | 4 | `ExistingConnectionMySqlTest` | MySQL 连接串校�?skip |

## W4 批次映射�?.T11–T22�?

| ID | Xugu 测试文件 | 测试数（约） | Pomelo �?| Skip/Defer |
|----|--------------|-------------|-----------|------------|
| 9.T11 | `OptimisticConcurrencyTests.cs` | 4 | `OptimisticConcurrencyMySqlTest` | DbUpdateConcurrencyException（ROW_COUNT defer�?|
| 9.T12 | `GraphUpdatesTests.cs` | 6 | `GraphUpdatesMySqlTestBase` | 全量子集 |
| 9.T13 | `LoadTests.cs` | 6 | `LoadMySqlTest` | 懒加载代理子�?defer |
| 9.T14 | `ManyToManyTrackingTests.cs` | 5 | `ManyToManyTrackingMySqlTest` | �?|
| 9.T15 | `TableSplittingTests.cs` | 4 | `TableSplittingMySqlTest` | computed defer |
| 9.T16 | `EntitySplittingTests.cs` | 4 | `EntitySplittingMySqlTest` | �?|
| 9.T17 | `FieldMappingTests.cs` | 4 | `FieldMappingMySqlTest` | �?|
| 9.T18 | `StoreGeneratedTests.cs` | 5 | `StoreGeneratedMySqlTest` | computed defer |
| 9.T19 | `DesignTimeExtensionTests.cs` | 5 | `DesignTimeMySqlTest` | �?|
| 9.T20 | `SaveChangesInterceptionTests.cs` | 4 | `SaveChangesInterceptionMySqlTest` | diagnostics defer |
| 9.T21 | `PropertyValuesTests.cs` | 5 | `PropertyValuesMySqlTest` | shadow 大矩�?defer |
| 9.T22 | `SeedingTests.cs` | 3 | `SeedingMySqlTest` | EnsureCreated+HasData defer |

## 基础设施 / 稳定性（W6�?

| �?| 处置 | 状�?|
|----|------|------|
| `TimeOnlyQueryTests` �?`DateOnly`/`Extension` 共享 `EF_TEST_SCHEDULE` | 专用�?`EF_TEST_TIMEONLY_SCHEDULE` + fixture 方法 | **done** |
| 全量套件实库连接争用 | `xunit.runner.json` `maxParallelThreads: 1` + `DisableTestParallelization` + `XuguRelationalConnection`/`XuguTestConnection` 瞬�?open 重试 | **done** |
| `NorthwindContext` �?TestStore 前缀模型缓存 | `IXuguStoreBoundContext` + `XuguTestStoreModelCacheKeyFactory` | **done** |
| `ROW_COUNT()` / 乐观并发异常 | XuguDB **E10049**（函数不存在，MYSQL 兼容模式亦不可用）；`SELECT 1` 占位维持 | **blocked**�?0.105�?|
| `XuguRetryingExecutionStrategy` | Message 解析 XGCI 瞬态码（E19886/E32506 等） | **done**�?0.106�?|

## 9.IT �?IntegrationTests 调研�?.IT1�?

| �?| 结论 |
|----|------|
| Pomelo `EFCore.MySql.IntegrationTests` | ASP.NET Core 宿主 + Vegeta 性能/迁移 CLI�?*�?* xUnit 功能回归 |
| 适用�?Xugu�?| **低价�?* �?无等�?CI 宿主；连接弹性已�?`ExistingConnectionTests` 覆盖子集 |
| 9.IT2 | **defer** �?若未来需要性能基线，单�?Phase 10 �?Xugu 冒烟 Web 宿主 |

## 明确 skip（冻结）

�?`harness/tasks/phase-9-pomelo-test-parity/TASKS.md` + Spatial/NTS/JSON/FULLTEXT/Scaffolding Baselines�?

## 门禁

```powershell
dotnet test Xugu.EFCore.Xugu.sln -c Release
dotnet test test/EFCore.Xugu.Tests -c Release --list-tests   # 861（Wave 5�?1 参数内联单测�?
```
