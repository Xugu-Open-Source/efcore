# Service registration

## Call chain

```text
UseXugu(connectionString [, ServerVersion] [, Action<XuguDbContextOptionsBuilder>])
  → validate connection string (XuguConnectionStringOptionsValidator)
  → XuguOptionsExtension (WithConnectionString / WithServerVersion)
  → optional XuguDbContextOptionsBuilder (retry, compatible mode)
  → AddEntityFrameworkXugu() via XuguOptionsExtension.ApplyServices
```

Entry: `src/EFCore.Xugu/Extensions/XuguDbContextOptionsBuilderExtensions.cs`  
DI table: `src/EFCore.Xugu/Extensions/XuguServiceCollectionExtensions.cs`  
Options fluent: `src/EFCore.Xugu/Infrastructure/XuguDbContextOptionsBuilder.cs`

## Registration pattern

Use `EntityFrameworkRelationalServicesBuilder` + `TryAdd` / `TryAddProviderSpecificServices`. Register:

- Storage: `IRelationalTypeMappingSource` → `XuguTypeMappingSource`, `IXuguRelationalConnection`
- Query: `IQuerySqlGeneratorFactory`, translator providers, SQL translating visitors
- Update: `IUpdateSqlGenerator`, modification command batch factory
- Migrations: `IMigrationsSqlGenerator`, `IHistoryRepository`, `IMigrator`, `IMigrationsModelDiffer`
- Other: `IExecutionStrategyFactory`, `IRelationalDatabaseCreator`, `IXuguOptions`

Design-time: `Design/Internal/XuguDesignTimeServices` calls `AddEntityFrameworkXugu()` then adds scaffolding services.

## Options knobs

- `EnableCompatibleModeOnOpen` / `DisableCompatibleModeOnOpen` — session `compatible_mode` (identifier folding), **not** a MySQL dialect rewrite.
- `EnableRetryOnFailure` / `UseXuguExecutionStrategy` — see [docs/references/retrying-execution-strategy.md](../../../../docs/references/retrying-execution-strategy.md).

## Tests

- `test/EFCore.Xugu.Tests.Unit/XuguServiceCollectionExtensionsTests.cs`
- `test/EFCore.Xugu.Tests.Unit/FluentApiExtensionTests.cs`

## Anti-patterns

- Registering Pomelo or MySqlConnector types.
- Skipping connection-string validation in new `UseXugu` overloads.
- Treating `COMPATIBLE_MODE=MYSQL` as permission to emit MySQL SQL.
