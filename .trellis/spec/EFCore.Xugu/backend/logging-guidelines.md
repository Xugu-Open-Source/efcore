# Logging guidelines

## Local pattern

Provider logging definitions live in:

- `src/EFCore.Xugu/Diagnostics/Internal/XuguLoggingDefinitions.cs`
- Registered as `LoggingDefinitions` → `XuguLoggingDefinitions` in `XuguServiceCollectionExtensions`

Extend this type when adding provider-specific event definitions following EF Core Relational patterns. Prefer EF Core's logging infrastructure over `Console.WriteLine` in provider code.

## Anti-patterns

- Logging connection strings or passwords.
- Introducing a parallel logging framework inside `src/EFCore.Xugu/`.
