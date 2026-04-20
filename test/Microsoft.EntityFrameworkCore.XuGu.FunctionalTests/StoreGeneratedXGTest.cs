// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.EntityFrameworkCore.Extensions;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class StoreGeneratedXGTest
        : StoreGeneratedTestBase<XGTestStore, StoreGeneratedXGTest.StoreGeneratedXGFixture>
    {
        public StoreGeneratedXGTest(StoreGeneratedXGFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public virtual void Exception_in_SaveChanges_causes_store_values_to_be_reverted()
        {
            using (var context = CreateContext())
            {
                var entities = new List<Darwin>();
                for (var i = 0; i < 1000; i++)
                {
                    entities.Add(new Darwin());
                }
                entities.Add(new Darwin { Id = 1777 });

                context.AddRange(entities);

                var identityMap = entities.ToDictionary(e => e.Id, e => e);

                var stateManager = context.GetService<IStateManager>();
                var key = context.Model.FindEntityType(typeof(Darwin)).FindPrimaryKey();

                foreach (var entity in entities)
                {
                    Assert.Same(
                        entity,
                        stateManager.TryGetEntry(key, new ValueBuffer(new object[] { entity.Id, entity.Name }), false).Entity);
                }

                Assert.Throws<DbUpdateException>(() => context.SaveChanges());

                foreach (var entity in entities)
                {
                    Assert.Same(entity, identityMap[entity.Id]);
                }

                foreach (var entity in entities)
                {
                    Assert.Same(
                        entity,
                        stateManager.TryGetEntry(key, new ValueBuffer(new object[] { entity.Id, entity.Name }), false).Entity);
                }
            }
        }

        public class StoreGeneratedXGFixture : StoreGeneratedFixtureBase
        {
            private const string DatabaseName = "StoreGeneratedTest";

            private readonly IServiceProvider _serviceProvider;

            public StoreGeneratedXGFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkXG()
                    .AddSingleton(TestXGModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override XGTestStore CreateTestStore()
            {
                return XGTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseXG(XGTestStore.CreateConnectionString(DatabaseName), b => b.MaxBatchSize(1))
                            .UseInternalServiceProvider(_serviceProvider);

                        using (var context = new StoreGeneratedContext(optionsBuilder.Options))
                        {
                            context.Database.EnsureDeleted();
                            context.Database.EnsureCreated();
                        }
                    });
            }

            public override DbContext CreateContext(XGTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseXG(testStore.Connection)
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new StoreGeneratedContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);

                return context;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Gumball>(b =>
                    {
                        b.Property(e => e.Id)
                            .UseXGIdentityColumn();

                        b.Property(e => e.Identity)
                            .HasDefaultValue("Banana Joe");

                        b.Property(e => e.IdentityReadOnlyBeforeSave)
                            .HasDefaultValue("Doughnut Sheriff");

                        b.Property(e => e.IdentityReadOnlyAfterSave)
                            .HasDefaultValue("Anton");

                        b.Property(e => e.AlwaysIdentity)
                            .HasDefaultValue("Banana Joe");

                        b.Property(e => e.AlwaysIdentityReadOnlyBeforeSave)
                            .HasDefaultValue("Doughnut Sheriff");

                        b.Property(e => e.AlwaysIdentityReadOnlyAfterSave)
                            .HasDefaultValue("Anton");

                        b.Property(e => e.Computed)
                            .HasDefaultValue("Alan");

                        b.Property(e => e.ComputedReadOnlyBeforeSave)
                            .HasDefaultValue("Carmen");

                        b.Property(e => e.ComputedReadOnlyAfterSave)
                            .HasDefaultValue("Tina Rex");

                        b.Property(e => e.AlwaysComputed)
                            .HasDefaultValue("Alan");

                        b.Property(e => e.AlwaysComputedReadOnlyBeforeSave)
                            .HasDefaultValue("Carmen");

                        b.Property(e => e.AlwaysComputedReadOnlyAfterSave)
                            .HasDefaultValue("Tina Rex");
                    });
            }
        }
    }
}
