using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;

/// <summary>
/// Functional-test helpers for options not exposed as first-class Xugu product APIs.
/// </summary>
public static class XuguDbContextOptionsBuilderTestExtensions
{
    public static XuguDbContextOptionsBuilder EnableIndexOptimizedBooleanColumns(
        this XuguDbContextOptionsBuilder builder,
        bool enable = true)
        => builder;

    public static XuguDbContextOptionsBuilder TranslateParameterizedCollectionsToConstants(
        this XuguDbContextOptionsBuilder builder)
        => builder;

    public static XuguDbContextOptionsBuilder TranslateParameterizedCollectionsToParameters(
        this XuguDbContextOptionsBuilder builder)
        => builder;
}
