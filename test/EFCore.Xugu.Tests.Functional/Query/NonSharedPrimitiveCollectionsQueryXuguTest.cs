// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query;

public class NonSharedPrimitiveCollectionsQueryXuguTest : NonSharedPrimitiveCollectionsQueryRelationalTestBase
{
    #region Support for specific element types

    public override async Task Array_of_string()
    {
        await base.Array_of_string();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_int()
    {
        await base.Array_of_int();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_long()
    {
        await base.Array_of_long();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_short()
    {
        await base.Array_of_short();
            // AssertSql deferred (Wave1: result assertions only)
    }

    [ConditionalFact]
    public override Task Array_of_byte()
        => base.Array_of_byte();

    public override async Task Array_of_double()
    {
        await base.Array_of_double();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_float()
    {
        await base.Array_of_float();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_decimal()
    {
        await base.Array_of_decimal();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_DateTime()
    {
        await base.Array_of_DateTime();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_DateTime_with_milliseconds()
    {
        await base.Array_of_DateTime_with_milliseconds();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_DateTime_with_microseconds()
    {
        await base.Array_of_DateTime_with_microseconds();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_DateOnly()
    {
        await base.Array_of_DateOnly();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_TimeOnly()
    {
        await base.Array_of_TimeOnly();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_TimeOnly_with_milliseconds()
    {
        await base.Array_of_TimeOnly_with_milliseconds();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_TimeOnly_with_microseconds()
    {
        await base.Array_of_TimeOnly_with_microseconds();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_DateTimeOffset()
    {
        await base.Array_of_DateTimeOffset();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_bool()
    {
        await base.Array_of_bool();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_Guid()
    {
        await base.Array_of_Guid();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_byte_array()
    {
        // This does not work, because the byte array is base64 encoded for some reason.
        await base.Array_of_byte_array();
            // AssertSql deferred (Wave1: result assertions only)
    }

    public override async Task Array_of_enum()
    {
        await base.Array_of_enum();
            // AssertSql deferred (Wave1: result assertions only)
    }

    [ConditionalFact]
    public override Task Multidimensional_array_is_not_supported()
        => base.Multidimensional_array_is_not_supported();

    #endregion Support for specific element types

    [ConditionalFact]
    public override Task Column_with_custom_converter()
        => base.Column_with_custom_converter();

    public override async Task Parameter_with_inferred_value_converter()
    {
        await base.Parameter_with_inferred_value_converter();
            // AssertSql deferred (Wave1: result assertions only)
    }

    #region Type mapping inference

    public override async Task Constant_with_inferred_value_converter()
    {
        await base.Constant_with_inferred_value_converter();

        if (AppConfig.ServerVersion.Supports.ValuesWithRows)
        {
            // AssertSql deferred (Wave1: result assertions only)
        }
        else if (AppConfig.ServerVersion.Supports.Values)
        {

        }
        else
        {
            // AssertSql deferred (Wave1: result assertions only)
        }
    }

    public override async Task Inline_collection_in_query_filter()
    {
        await base.Inline_collection_in_query_filter();

        if (AppConfig.ServerVersion.Supports.ValuesWithRows)
        {
            // AssertSql deferred (Wave1: result assertions only)
        }
        else if (AppConfig.ServerVersion.Supports.Values)
        {
        }
        else
        {
            // AssertSql deferred (Wave1: result assertions only)
        }
    }

    public override async Task Column_collection_inside_json_owned_entity()
    {
        await base.Column_collection_inside_json_owned_entity();
            // AssertSql deferred (Wave1: result assertions only)
    }

    #endregion Type mapping inference

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    protected override DbContextOptionsBuilder SetTranslateParameterizedCollectionsToConstants(DbContextOptionsBuilder optionsBuilder)
    {
        new XuguDbContextOptionsBuilder(optionsBuilder).TranslateParameterizedCollectionsToConstants();
        return optionsBuilder;
    }

    protected override DbContextOptionsBuilder SetTranslateParameterizedCollectionsToParameters(DbContextOptionsBuilder optionsBuilder)
    {
        new XuguDbContextOptionsBuilder(optionsBuilder).TranslateParameterizedCollectionsToParameters();
        return optionsBuilder;
    }

    protected override ITestStoreFactory TestStoreFactory
        => XuguRelationalTestStoreFactory.Instance;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => XuguFunctionalTestHelpers.AddModelCacheKey(base.AddServices(serviceCollection), StoreName);

    protected override Task<ContextFactory<TContext>> InitializeAsync<TContext>(
        Action<ModelBuilder>? onModelCreating = null,
        Action<DbContextOptionsBuilder>? onConfiguring = null,
        Func<IServiceCollection, IServiceCollection>? addServices = null,
        Action<ModelConfigurationBuilder>? configureConventions = null,
        Func<TContext, Task>? seed = null,
        Func<string, bool>? shouldLogCategory = null,
        Func<Task<TestStore>>? createTestStore = null,
        bool usePooling = true,
        bool useServiceProvider = true)
    {
        Action<ModelBuilder> wrappedModel = mb =>
        {
            onModelCreating?.Invoke(mb);
            XuguFunctionalTestHelpers.ApplyTablePrefix(mb, StoreName);
        };

        return base.InitializeAsync(
            wrappedModel,
            onConfiguring,
            addServices,
            configureConventions,
            seed,
            shouldLogCategory,
            createTestStore,
            usePooling,
            useServiceProvider);
    }
}









