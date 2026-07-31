# Naming and namespaces

## Actual conventions (source of truth)

| Kind | Convention | Example |
|------|------------|---------|
| Public API | `Microsoft.EntityFrameworkCore` | `UseXugu` in `Extensions/XuguDbContextOptionsBuilderExtensions.cs` |
| DI extensions | `Microsoft.Extensions.DependencyInjection` | `AddEntityFrameworkXugu` |
| Implementation | `Microsoft.EntityFrameworkCore.Xugu.{Layer}[.Internal]` | `Microsoft.EntityFrameworkCore.Xugu.Query.Internal` |
| Conventions (some) | `Microsoft.EntityFrameworkCore.Metadata.Conventions` | `XuguConventionSetBuilder` |
| Class prefix | `Xugu` | `XuguQuerySqlGenerator` |
| Package / assembly | `Microsoft.EntityFrameworkCore.Xugu` | `EFCore.Xugu.csproj` |

`RootNamespace` in csproj is `Microsoft.EntityFrameworkCore.Xugu`.

## Outdated docs to ignore

Do **not** use `Xugu.EntityFrameworkCore.Xugu` as an implementation namespace — it does not appear in source. Older agent docs that said so are wrong.

## Placement

- User-facing Fluent methods → `Extensions/`, public EF namespace.
- Provider services registered in DI → `*Internal` folders, `Xugu*` class names.
- Exception message resources → `Properties/XuguStrings.resx` only.
