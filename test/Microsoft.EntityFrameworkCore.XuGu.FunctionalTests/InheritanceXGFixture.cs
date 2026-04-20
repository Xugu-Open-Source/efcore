// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Inheritance;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class InheritanceXGFixture : InheritanceRelationalFixture, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly XGTestStore _testStore;

        public InheritanceXGFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXG()
                .AddSingleton(TestXGModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _testStore = XGTestStore.CreateScratch();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseXG(_testStore.Connection)
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using (var context = CreateContext())
            {
                context.Database.EnsureCreated();
                SeedData(context);
            }
        }

        public override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plant>().HasDiscriminator(p => p.Genus)
                .HasValue<Rose>(PlantGenus.Rose)
                .HasValue<Daisy>(PlantGenus.Daisy);

            modelBuilder.Entity<Country>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Eagle>().HasMany(e => e.Prey).WithOne().HasForeignKey(e => e.EagleId).IsRequired(false);

            // #2455
            modelBuilder.Entity<Animal>().Property(e => e.Species).HasColumnType("varchar(100)");
            modelBuilder.Entity<Bird>().Property(e => e.EagleId).HasColumnType("varchar(100)");
        }

        public override InheritanceContext CreateContext() => new InheritanceContext(_options);
        public void Dispose() => _testStore.Dispose();
    }
}
