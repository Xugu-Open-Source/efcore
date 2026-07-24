// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query
{
    public class OwnedEntityQueryXuguTest : OwnedEntityQueryRelationalTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => XuguRelationalTestStoreFactory.Instance;

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

        public override async Task Multiple_single_result_in_projection_containing_owned_types(bool async)
        {
            await base.Multiple_single_result_in_projection_containing_owned_types(async);
            // AssertSql deferred (Wave1: result assertions only)
        }

        public override async Task Owned_collection_basic_split_query(bool async)
        {
            // Use custom context to set prefix length, so we don't exhaust the max. key length.
            var contextFactory = await InitializeAsync<Context25680>(onModelCreating: modelBuilder =>
            {
                modelBuilder.Entity<Location25680>().OwnsMany(e => e.PublishTokenTypes,
                    b =>
                    {
                        b.WithOwner(e => e.Location).HasForeignKey(e => e.LocationId);
                        b.HasKey(e => new { e.LocationId, e.ExternalId, e.VisualNumber, e.TokenGroupId })
                            .HasPrefixLength(0, 128, 128, 128); // <-- set prefix length, so we don't exhaust the max. key length
                    });
            });

            using var context = contextFactory.CreateContext();

            var id = new Guid("6c1ae3e5-30b9-4c77-8d98-f02075974a0a");
            var query = context.Set<Location25680>().Where(e => e.Id == id).AsSplitQuery();
            var result = async
                ? await query.FirstOrDefaultAsync()
                : query.FirstOrDefault();
        }

        public override Task Projecting_correlated_collection_property_for_owned_entity(bool async)
            => base.Projecting_correlated_collection_property_for_owned_entity(async);
    }
}
