# XuguDB EF Core Provider — 用户指南

> **版本**：`Microsoft.EntityFrameworkCore.Xugu` **9.0.0**（对齐 EF Core 9.0.x；方言迭代基线）  
> **目标框架**：.NET 9.0 · EF Core 9.0.x  
> **适用读者**：在 Windows 上使用 XuguDB 的应用开发者

本文是面向最终用户的完整使用指南。快速上手见 [GETTING-STARTED.md](GETTING-STARTED.md)；已知限制见 [LIMITATIONS.md](LIMITATIONS.md)；与 MySQL/Pomelo 的对照见 [XUGU-VS-MYSQL.md](XUGU-VS-MYSQL.md)（**对照参考，非迁移指南**）。

---

## 目录

1. [安装与系统要求](#1-安装与系统要求)
2. [连接配置](#2-连接配置)
3. [模型与约定](#3-模型与约定)
4. [查询与 LINQ](#4-查询与-linq)
5. [迁移工作流](#5-迁移工作流)
6. [反向工程（Scaffolding）](#6-反向工程scaffolding)
7. [ExecuteUpdate / ExecuteDelete](#7-executeupdate--executedelete)
8. [重试与弹性（EnableRetryOnFailure）](#8-重试与弹性enableretryonfailure)
9. [已知限制摘要](#9-已知限制摘要)
10. [常见问题（FAQ）](#10-常见问题faq)

---

## 1. 安装与系统要求

### 1.1 软件要求

| 项 | 要求 |
|----|------|
| .NET SDK | **9.0+**（见仓库 `global.json`） |
| 操作系统 | **Windows x64**（9.0.0 生产平台） |
| XuguDB 服务端 | 可网络访问的实例（默认端口 `5138`） |
| ADO.NET 驱动 | NuGet 包 [`Xuguclient`](https://www.nuget.org/packages/Xuguclient)（Provider 包传递依赖；pack 时当前 `3.3.6-bionic`） |
| 原生库 | `xugusql.dll`（Windows x64；须能被进程加载） |

> **Linux**：`Xuguclient` 3.3.6-bionic **可能**含 `libxugusql.so`，但 **9.0.0 Wave A 未在 Linux 实库验收** — 仍为 **signed-off blocked**（见 [LIMITATIONS.md](LIMITATIONS.md)）。请勿在生产环境假设跨平台可用。

### 1.2 安装 NuGet 包

```powershell
dotnet add package Microsoft.EntityFrameworkCore.Xugu --version 9.0.0
dotnet add package Xuguclient
```

设计时工具（迁移、Scaffolding）还需：

```powershell
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef
```

### 1.3 原生库部署

Provider 通过 `Xuguclient` P/Invoke 调用原生驱动（Windows：`xugusql.dll`）。

**依赖链**：

```text
Microsoft.EntityFrameworkCore.Xugu  →  Xuguclient (XuguClient.dll)  →  xugusql.dll  →  XuguDB
```

常见部署方式：

1. **NuGet 还原（推荐）**：安装 Provider 后，`Xuguclient` 作为传递依赖还原；`runtimes/win-x64/native/xugusql.dll` 随应用输出目录复制。
2. **手动复制**：若运行时提示找不到 DLL，将与 `Xuguclient` 版本匹配的 `xugusql.dll` 复制到 `.exe` 同目录。

**发布包 native 策略**：`dotnet pack -p:UseLocalXuguDriver=false` **不**把仓库本地 `xugusql.dll` 打进 Provider nupkg；NuGet 消费方只使用 **`Xuguclient` 传递依赖** 中的 native 资产。本地源码调试仍可把 `XuguNativeDllPath` 的 DLL 复制到输出目录。详见 [xuguclient-dependency-strategy.md](xuguclient-dependency-strategy.md)。

**字符集**：连接串务必含 `CHAR_SET=UTF8`（或 `CHARSET=UTF8`）。驱动在省略时默认 GBK，会导致中文与欧洲重音字符乱码。

---

## 2. 连接配置

### 2.1 连接串格式

XuguDB 使用 **键值对** 连接串（**不是** MySQL 的 `Server=...;Database=...` 格式）：

```
IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=SYSDBA; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8
```

| 键 | 必填 | 说明 |
|----|------|------|
| `IP` | 是 | 服务端地址 |
| `DB` | 是 | 数据库名 |
| `USER` / `PWD` | 是 | 认证凭据 |
| `PORT` | 否 | 默认 `5138` |
| `AUTO_COMMIT` | 建议 | 建议 `on` |
| `CHAR_SET` | 建议 | 建议 `UTF8`，避免中文乱码 |

### 2.2 在应用中配置

**ASP.NET Core（依赖注入）：**

```csharp
using Microsoft.EntityFrameworkCore;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseXugu(builder.Configuration.GetConnectionString("XuguDb")!));
```

**appsettings.json：**

```json
{
  "ConnectionStrings": {
    "XuguDb": "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=***; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"
  }
}
```

**环境变量**（设计时 / CI 常用）：

```powershell
$env:XUGU_CONNECTION = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=***; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"
```

### 2.3 兼容模式（可选，非默认）

**9.0.0 默认使用 Xugu 原生方言**，连接打开时 **不会** 自动设置 `compatible_mode`。

若需与 MySQL 脚本对照或运行遗留 compat 测试，显式启用：

```csharp
options.UseXugu(connectionString, xugu => xugu.EnableCompatibleModeOnOpen());
```

等价于会话级：

```sql
SET compatible_mode TO 'MYSQL'
```

> **重要**：兼容模式是开发/对照便利，**不是**产品目标。新应用应编写 **Xugu 原生 SQL** 与 EF 映射。

### 2.4 设计时工厂（dotnet ef）

`dotnet ef` 需要 `IDesignTimeDbContextFactory<T>`：

```csharp
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("XUGU_CONNECTION")
            ?? "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=***; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8";

        return new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseXugu(cs).Options);
    }
}
```

完整示例见 `samples/EfDesignSample/`。

---

## 3. 模型与约定

### 3.1 自增主键（IDENTITY）

XuguDB 使用 `INTEGER IDENTITY(1,1)`，**不是** MySQL 的 `AUTO_INCREMENT`。

**方式 A — 全局约定（推荐）：**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.UseIdentityColumns(); // EF Core 内置，映射到 Xugu IDENTITY
}
```

**方式 B — Fluent API：**

```csharp
modelBuilder.Entity<Blog>(entity =>
{
    entity.Property(e => e.Id).UseXuguIdentityColumn();
    // 或实体级：entity.UseXuguIdentityColumns();
});
```

Identity 值回读使用 `INSERT` + `SELECT … WHERE id = LAST_INSERT_ID()`（Xugu 原生函数）。

> **迁移限制**：已定义为 IDENTITY 的主键列 **不能** 通过迁移变更 CLR/存储类型，会抛出 `NotSupportedException`。需手工迁移。

### 3.2 GUID 主键

CLR `Guid` 默认映射 XuguDB 原生 **`GUID`**（16 字节），**不是** MySQL 常见的 `CHAR(36)`：

```csharp
public class Order
{
    public Guid Id { get; set; }  // 存储类型：GUID
    public string Name { get; set; } = null!;
}
```

Provider 提供 `XuguSequentialGuidValueGenerator` 用于有序 GUID 生成（与 Pomelo 模式类似）。

### 3.3 JSON 列

XuguDB 支持原生 `JSON` 列类型。EF 映射示例：

```csharp
public class EventLog
{
    public int Id { get; set; }
    public string Payload { get; set; } = null!; // 或 JsonDocument
}

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<EventLog>(entity =>
    {
        entity.Property(e => e.Payload).HasXuguJsonColumn();
    });
}
```

迁移 DDL 会生成 `JSON` 存储类型。

> **注意**：整列 JSON LOB 的 `GetString` 物化在部分场景下可能未完全验证；查询时优先使用 JSON 函数投影（见下一节）。

### 3.4 DateOnly / TimeOnly

| CLR 类型 | LINQ 查询 | SaveChanges |
|----------|-----------|-------------|
| `DateOnly` | ✅ 支持 | ✅ 支持 |
| `TimeOnly` | ✅ 支持 | ✅ 支持 |

无需额外 Fluent 配置，Provider 自动映射。

### 3.5 无符号整数

XuguDB 仅有符号整数类型：

| CLR | 存储类型 |
|-----|---------|
| `uint` | `BIGINT` |
| `ulong` | `NUMERIC(20,0)` |

### 3.6 不支持的模型 API

以下 Pomelo/MySQL Fluent API **不会** 在 Xugu Provider 中实现：

| API | 原因 |
|-----|------|
| `HasCollation(...)` | XuguDB 无列级 Collation；字符集在连接级 `CHAR_SET` 配置 |
| `HasCharSet(...)` | 同上 |
| NetTopologySuite / Spatial | 无 EF 集成包 |
| FULLTEXT 索引 | XuguDB 文档未提供全文索引 |

---

## 4. 查询与 LINQ

### 4.1 基本 LINQ

标准 EF Core LINQ 操作在 Xugu Provider 上可用：

- `Where` / `Select` / `OrderBy` / `GroupBy` / `Join`
- `Include` / 导航属性
- 聚合：`Count` / `Sum` / `Average` / `Min` / `Max`
- 分页：`Skip` / `Take`
- 字符串：`Contains` / `StartsWith` / `EndsWith`
- 正则：`EF.Functions.Like`、Xugu `REGEXP_LIKE` 翻译

### 4.2 JSON 函数（EF.Functions）

```csharp
// 按 JSON 路径过滤
var active = await db.Events
    .Where(e => EF.Functions.JsonValue<string>(e.Payload, "$.name") == "Alice")
    .ToListAsync();

// 提取 JSON 片段
var tags = await db.Events
    .Select(e => EF.Functions.JsonExtract<string>(e.Payload, "$.tags"))
    .ToListAsync();

// 包含判断
var matched = await db.Events
    .Where(e => EF.Functions.JsonContains(e.Payload, "{\"active\":true}"))
    .ToListAsync();
```

生成的 SQL 使用 XuguDB 原生 `JSON_VALUE` / `JSON_EXTRACT` / `JSON_CONTAINS` 等函数。

### 4.3 查询限制

| 场景 | 状态 |
|------|------|
| `CONVERT_TZ` / 时区转换 DbFunction | ❌ 不翻译；时区在库级配置 |
| FULLTEXT / `MATCH … AGAINST` | ❌ 不实现；可用 `REGEXP_LIKE` 替代 |
| Spatial / NetTopologySuite | ❌ 不实现 |
| 过滤索引 `HasFilter` | ❌ 迁移不支持 |

更多细节见 [LIMITATIONS.md](LIMITATIONS.md)。

---

## 5. 迁移工作流

### 5.1 标准流程

**前提**：数据库已由 DBA/运维创建（Provider **不支持** `EnsureCreated()` / `EnsureDeleted()`）。

```powershell
$env:XUGU_CONNECTION = "IP=127.0.0.1; DB=SYSTEM; USER=SYSDBA; PWD=***; PORT=5138; AUTO_COMMIT=on; CHAR_SET=UTF8"

dotnet ef migrations add InitialCreate --project YourApp --startup-project YourApp
dotnet ef database update --project YourApp --startup-project YourApp
```

### 5.2 支持与不支持的迁移能力

| 能力 | 状态 |
|------|------|
| `migrations add` / `database update` | ✅ |
| `migrations remove` / `script` | ✅ |
| 表/列/索引/FK 创建与变更 | ✅（主路径） |
| 过滤索引（`HasFilter`） | ❌ |
| 幂等迁移脚本 API | ❌ |
| `EnsureCreated()` / `EnsureDeleted()` | ❌ 抛 `NotSupportedException` |
| IDENTITY 主键类型变更 | ❌ 抛 `NotSupportedException` |

### 5.3 常见迁移陷阱

1. **未先建库**：`database update` 前确认 `DB=` 指向的数据库已存在。
2. **字符集**：连接串使用 `CHAR_SET=UTF8`，避免迁移脚本中中文注释/默认值乱码。
3. **compat 模式混用**：生产环境不要用 compat 模式生成迁移又在原生模式执行（或反之）。

---

## 6. 反向工程（Scaffolding）

从现有 XuguDB 表生成实体：

```powershell
dotnet ef dbcontext scaffold $env:XUGU_CONNECTION Microsoft.EntityFrameworkCore.Xugu `
  --project YourApp --startup-project YourApp `
  --context ScaffoldedDbContext --table YOUR_TABLE
```

Provider 读取 `ALL_TABLES` / `ALL_COLUMNS` 等系统视图（普通用户可读；**不是** MySQL 的 `INFORMATION_SCHEMA`，也**不**依赖 `DBA_*` / `SYS_*`）。

生成的 `OnConfiguring` 会调用 `UseXugu(...)`。

> Scaffolding 覆盖主路径（表/列/类型）；复杂 FK/索引命名可能需要手工调整。

---

## 7. ExecuteUpdate / ExecuteDelete

EF Core 7+ 批量 DML 在 Xugu Provider **核心路径**上可用：

```csharp
// 批量更新（不加载实体到内存）
await db.Blogs
    .Where(b => b.IsArchived)
    .ExecuteUpdateAsync(s => s
        .SetProperty(b => b.Title, "Archived")
        .SetProperty(b => b.UpdatedAt, DateTime.UtcNow));

// 批量删除
await db.Blogs
    .Where(b => b.CreatedAt < cutoff)
    .ExecuteDeleteAsync();
```

### 支持范围

| 场景 | 状态 |
|------|------|
| 单表 + 谓词过滤 | ✅ |
| 多表 JOIN（受限 SelectExpression） | ✅ |
| `ExecuteUpdateAsync` / `ExecuteDeleteAsync` | ✅ |

### 不支持

| 场景 | 处置 |
|------|------|
| TPC / TPT 继承层次批量 DML | 未实现 |
| Owned 类型上的 ExecuteUpdate/Delete | 未实现 |
| 带 `ORDER BY` / `LIMIT` / `DISTINCT` / `GROUP BY` 的源查询 | 拒绝翻译 |
| `CROSS APPLY` / `OUTER APPLY` / `LATERAL` | **不支持** — Provider 抛 `ApplyNotSupported`；请用常规 JOIN 或 `SaveChanges` |

需要上述能力时使用常规 `SaveChanges` 或显式 SQL。

---

## 8. 重试与弹性（EnableRetryOnFailure）

Provider 默认注册 `XuguExecutionStrategy`（**不重试**）。启用瞬态错误重试：

```csharp
options.UseXugu(connectionString, xugu => xugu.EnableRetryOnFailure());

// 自定义重试次数与间隔
options.UseXugu(connectionString, xugu => xugu.EnableRetryOnFailure(
    maxRetryCount: 5,
    maxRetryDelay: TimeSpan.FromSeconds(10)));
```

实现通过解析异常 `Message` 中的 XGCI 码（如 `[E19886]`、`[E32506]`）判定瞬态错误。`[E34304]` / `[E34305]`（IP/连接串无效）**不是**瞬态错误，不会无限/多次重试掩盖配置问题。

> **注意**：Pomelo 的 `errorNumbersToAdd` 参数在 Xugu 上 **被忽略**（Xugu 使用字符串 XGCI 码，非 MySQL 数字 error number）。

在重试策略内执行操作：

```csharp
var strategy = db.Database.CreateExecutionStrategy();
await strategy.ExecuteAsync(async () =>
{
    await using var tx = await db.Database.BeginTransactionAsync();
    // ... 业务逻辑 ...
    await tx.CommitAsync();
});
```

---

## 9. 已知限制摘要

以下为 9.0.0 最常见限制。**完整清单**见 [LIMITATIONS.md](LIMITATIONS.md)。

| 类别 | 限制 | 影响 |
|------|------|------|
| **PLAT-01 乐观并发** | 不依赖 `ROW_COUNT()`（仍 E10049） | **`DbUpdateConcurrencyException` 已支持**（`RecordsAffected` Path A） |
| **PLAT-02 平台** | Linux 无稳定实库验收 | **9.0.0 Wave A 仅 Windows x64 可试用**；3.3.6-bionic 或含 `.so` 但未验 Linux |
| **建库 API** | 无 `CREATE/DROP DATABASE` | 须 DBA 手工建库 |
| **EnsureCreated** | 不支持 | 使用 Migrations |
| **过滤索引** | `HasFilter` 不支持 | 迁移会剥离 filter |
| **Spatial / FULLTEXT** | 永久不实现 | 见 LIMITATIONS |
| **Collation Fluent** | 永久不实现 | 连接级 `CHAR_SET=UTF8` |
| **JSON 整列 LOB 读取** | 部分场景未验证 | 优先 JSON 函数投影 |
| **APPLY / LATERAL** | XuguDB 无 APPLY/LATERAL | ExecuteDelete/Update 与部分 LINQ 形状不可用 |

---

## 10. 常见问题（FAQ）

### Q1：能从 Pomelo/MySQL 零改动迁移吗？

**不能作为目标。** Xugu Provider 与 Pomelo 在架构上对齐，但 SQL 方言、连接串、DDL（`IDENTITY` vs `AUTO_INCREMENT`）、元数据视图（`DBA_*` vs `INFORMATION_SCHEMA`）均不同。请参阅 [XUGU-VS-MYSQL.md](XUGU-VS-MYSQL.md) 作 **对照参考**，而非迁移 checklist。

### Q2：为什么连接失败 `[E34301]`？

连接串错误、XuguDB 服务未启动、或 `IP`/`PORT`/凭据不正确。确认 `xugusql.dll` 已部署且服务监听 `5138`。

### Q3：为什么 SQL 执行失败 `[E34501]`？

SQL 方言不兼容。确认未误用 MySQL 专有语法；若需对照可临时启用 `EnableCompatibleModeOnOpen()`，但生产应使用 Xugu 原生 SQL。

### Q4：中文乱码怎么办？

连接串设置 `CHAR_SET=UTF8`（或 `CHARSET=UTF8`，视驱动文档而定）。应用与数据库字符集保持一致。

### Q5：`EnsureCreated()` 为什么抛异常？

Xugu Provider 不支持 EF 的 `Database.EnsureCreated()` / `EnsureDeleted()`。请由 DBA 建库后使用 `dotnet ef database update`。

### Q6：乐观并发 token 能用吗？

**可以。** 并发 token 列映射、UPDATE WHERE、以及陈旧 token 时抛出 `DbUpdateConcurrencyException` 均已支持（Provider 读 `DbDataReader.RecordsAffected`，不依赖 `ROW_COUNT()`）。详见 `LIMITATIONS.md`。

### Q7：能在 Linux 上跑吗？

9.0.0 **Wave A 仅签 Windows x64 可试用**。`Xuguclient` 3.3.6-bionic 预发布包**可能**携带 Linux `.so`，但 **未在 Linux 实库验收** — 与 Phase 12 PLAT-02 signed-off blocked 一致。待驱动稳定发布并完成实库矩阵后再解锁。

### Q8：JSON 列怎么查？

优先 `EF.Functions.JsonValue` / `JsonExtract` / `JsonContains`；避免直接 `SELECT` 整列大 JSON 再客户端解析（驱动 LOB 绑定限制）。

### Q9：与 MySQL 的 GUID 存储有何不同？

Xugu 原生 `GUID`（16 字节）；MySQL/Pomelo 常用 `CHAR(36)`。跨库同步 GUID 时注意格式转换。

### Q10：如何报告 Bug 或提功能？

在公开 GitHub 仓库提交 Issue（见根目录 README）。附复现步骤、连接串（脱敏）、XuguDB 版本与完整异常 Message（含 XGCI 码）。

---

## 相关文档

| 文档 | 说明 |
|------|------|
| [GETTING-STARTED.md](GETTING-STARTED.md) | 5 分钟快速开始 |
| [LIMITATIONS.md](LIMITATIONS.md) | 完整已知限制 |
| [XUGU-VS-MYSQL.md](XUGU-VS-MYSQL.md) | 与 Pomelo/MySQL 对照 |
| [CHANGELOG.md](CHANGELOG.md) | 版本变更 |
| [xuguclient-dependency-strategy.md](xuguclient-dependency-strategy.md) | 驱动与原生库 |
| [RELEASE-SCOPE.md](RELEASE-SCOPE.md) | 产品范围 |
