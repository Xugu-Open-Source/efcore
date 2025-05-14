// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Tests;

//ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities
{
    public class XGTestHelpers : RelationalTestHelpers
    {
        protected XGTestHelpers()
        {
        }

        public static XGTestHelpers Instance { get; } = new XGTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkXG();

        public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseXG("ip=127.0.0.1;db=DummyDatabase;user=SYSDBA;pwd=SYSDBA;port=5138;auto_commit=off;char_set=UTF8", AppConfig.ServerVersion);

        public IServiceProvider CreateContextServices(ServerVersion serverVersion)
            => ((IInfrastructure<IServiceProvider>)new DbContext(CreateOptions(serverVersion))).Instance;

        public IServiceProvider CreateContextServices(Action<XGDbContextOptionsBuilder> builder)
            => ((IInfrastructure<IServiceProvider>)new DbContext(CreateOptions(builder))).Instance;

        public ModelBuilder CreateConventionBuilder(IServiceProvider contextServices)
        {
            var conventionSet = contextServices.GetRequiredService<IConventionSetBuilder>()
                .CreateConventionSet();

            return new ModelBuilder(conventionSet);
        }

        public DbContextOptions CreateOptions(ServerVersion serverVersion)
            => CreateOptions(b => {});

        public DbContextOptions CreateOptions(Action<XGDbContextOptionsBuilder> builder)
            => new DbContextOptionsBuilder()
                .UseXG("ip=127.0.0.1;db=DummyDatabase;user=SYSDBA;pwd=SYSDBA;port=5138;auto_commit=off;char_set=UTF8", AppConfig.ServerVersion, builder)
                .Options;

        public void EnsureSufficientKeySpace(IMutableModel model, TestStore testStore = null, bool limitColumnSize = false)
        {
            if (!AppConfig.ServerVersion.Supports.LargerKeyLength)
            {
                var databaseCharSet = CharSet.GetCharSetFromName((testStore as XGTestStore)?.DatabaseCharSet) ?? CharSet.Utf8Mb4;

                foreach (var entityType in model.GetEntityTypes())
                {
                    var indexes = entityType.GetIndexes();
                    var indexedStringProperties = entityType.GetProperties()
                        .Where(p => p.ClrType == typeof(string) &&
                                    indexes.Any(i => i.Properties.Contains(p)))
                        .ToList();

                    if (indexedStringProperties.Count > 0)
                    {
                        var safePropertyLength = AppConfig.ServerVersion.MaxKeyLength /
                                                 databaseCharSet.MaxBytesPerChar /
                                                 indexedStringProperties.Count;

                        if (limitColumnSize)
                        {
                            foreach (var property in indexedStringProperties)
                            {
                                property.SetMaxLength(safePropertyLength);
                            }
                        }
                        else
                        {
                            foreach (var index in indexes)
                            {
                                index.SetPrefixLength(
                                    index.Properties.Select(
                                            p => indexedStringProperties.Contains(p) && p.GetMaxLength() > safePropertyLength
                                                ? safePropertyLength
                                                : 0)
                                        .ToArray());
                            }
                        }
                    }
                }
            }
        }

        public int GetIndexedStringPropertyDefaultLength
            => 255;

        public static DateTimeOffset GetExpectedValue(DateTimeOffset value)
        {
            const int xgMaxMillisecondDecimalPlaces = 6;
            var decimalPlacesFactor = (decimal)Math.Pow(10, 7 - xgMaxMillisecondDecimalPlaces);

            // Change DateTimeOffset values, because XG does not preserve offsets and has a maximum of 6 decimal places, in contrast to
            // .NET which has 7.
            return new DateTimeOffset(
                (long)(Math.Truncate(value.UtcTicks / decimalPlacesFactor) * decimalPlacesFactor),
                TimeSpan.Zero);
        }

        public static string CastAsDouble(string innerSql)
            => AppConfig.ServerVersion.Supports.DoubleCast
                ? $@"CAST({innerSql} AS double)"
                : $@"(CAST({innerSql} AS decimal(65,30)) + 0e0)";

        public static string XGBug96947Workaround(string innerSql, string type = "char")
            => AppConfig.ServerVersion.Supports.XGBug96947Workaround
                ? $@"CAST({innerSql} AS {type})"
                : innerSql;

        public static bool HasPrimitiveCollectionsSupport<TContext>(SharedStoreFixtureBase<TContext> fixture)
            where TContext : DbContext
        {
            return AppConfig.ServerVersion.Supports.JsonTable &&
                   fixture.CreateOptions().GetExtension<XGOptionsExtension>().PrimitiveCollectionsSupport;
        }
    }
}
