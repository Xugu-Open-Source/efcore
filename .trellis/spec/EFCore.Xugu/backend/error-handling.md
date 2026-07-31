# Error handling

## User-visible messages

All user-facing provider messages go through:

- `src/EFCore.Xugu/Properties/XuguStrings.resx`
- Generated accessor: `Properties/XuguStrings.Designer.cs`

Usage pattern in code:

```csharp
throw new NotSupportedException(XuguStrings.FilteredIndexesNotSupported);
throw new InvalidOperationException(XuguStrings.ApplyNotSupported);
```

Examples: `XuguMigrationsSqlGenerator`, `XuguQuerySqlGenerator`, `XuguDatabaseCreator`.

## Driver / XGCI hints

`src/EFCore.Xugu/XuguExceptionHints.cs` maps known XuguClient / server codes (e.g. E34412, E19230) to `XuguStrings.XgciHint*` guidance.

## Tests

- `test/EFCore.Xugu.Tests.Unit/NotSupportedMessageTests.cs`
- `XuguExceptionHintsTests.cs`

## Anti-patterns

- Inline English/Chinese exception strings in provider code.
- Swallowing driver errors without preserving server code when retry classification needs it (`XuguTransientExceptionDetector`).
