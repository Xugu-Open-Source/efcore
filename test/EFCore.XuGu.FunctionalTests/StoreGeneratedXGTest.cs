using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using EntityFrameworkCore.XuGu.Tests;
using Xunit;
using System.Reflection;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public class StoreGeneratedXGTest : StoreGeneratedTestBase<StoreGeneratedXGTest.StoreGeneratedXGFixture>
    {
        public StoreGeneratedXGTest(StoreGeneratedXGFixture fixture)
            : base(fixture)
        {
        }
        [ConditionalTheory]
        [InlineData(nameof(Anais.NeverThrowBeforeUseAfter))]
        [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter))]
        [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter))]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter))]
        public override void Before_save_throw_always_throws_if_value_set(string propertyName)
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Add(WithValue(propertyName));

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave(propertyName, "Anais"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.NeverThrowBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter), "Rabbit")]
        public override void Before_save_throw_ignores_value_if_not_set(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Anais()).Entity;

                    context.SaveChanges();

                    id = entity.Id;
                },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.Never))]
        [InlineData(nameof(Anais.OnAdd))]
        [InlineData(nameof(Anais.OnUpdate))]
        [InlineData(nameof(Anais.NeverUseBeforeUseAfter))]
        [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.NeverUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddUseBeforeUseAfter))]
        [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter))]
        [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter))]
        [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter))]
        public override void Before_save_use_always_uses_value_if_set(string propertyName)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(WithValue(propertyName)).Entity;

                    context.SaveChanges();

                    id = entity.Id;
                },
                context => Assert.Equal("Pink", GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.Never), null)]
        [InlineData(nameof(Anais.OnAdd), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdate), null)]
        [InlineData(nameof(Anais.NeverUseBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverUseBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnAddUseBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter), null)]
        [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter), null)]
        public override void Before_save_use_ignores_value_if_not_set(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Anais()).Entity;

                    context.SaveChanges();

                    id = entity.Id;
                },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
        [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter), "Rabbit")]
        public override void Before_save_ignore_ignores_value_if_not_set(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Anais()).Entity;

                    context.SaveChanges();

                    id = entity.Id;
                },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
        [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter), "Rabbit")]
        public override void Before_save_ignore_ignores_value_even_if_set(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(WithValue(propertyName)).Entity;

                    context.SaveChanges();

                    id = entity.Id;
                },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.NeverUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter))]
        [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter))]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter))]
        public override void After_save_throw_always_throws_if_value_modified(string propertyName)
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Attach(WithValue(propertyName, 1)).Property(propertyName).IsModified = true;

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyAfterSave(propertyName, "Anais"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.NeverUseBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter), null)]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter), "Rabbit")]
        public override void After_save_throw_ignores_value_if_not_modified(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Anais()).Entity;

                    context.SaveChanges();

                    id = entity.Id;
                },
                context =>
                {
                    var entry = context.Entry(context.Set<Anais>().Find(id));
                    entry.State = EntityState.Modified;
                    entry.Property(propertyName).CurrentValue = "Daisy";
                    entry.Property(propertyName).IsModified = false;

                    context.SaveChanges();
                },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdate), null)]
        [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter), "Rabbit")]
        public override void After_save_ignore_ignores_value_if_not_modified(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Anais()).Entity;

                    context.SaveChanges();

                    id = entity.Id;
                },
                context =>
                {
                    var entry = context.Entry(context.Set<Anais>().Find(id));
                    entry.State = EntityState.Modified;
                    entry.Property(propertyName).CurrentValue = "Daisy";
                    entry.Property(propertyName).IsModified = false;

                    context.SaveChanges();
                },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdate), null)]
        [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter), null)]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter), "Rabbit")]
        public override void After_save_ignore_ignores_value_even_if_modified(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Anais()).Entity;

                    context.SaveChanges();

                    id = entity.Id;
                },
                context =>
                {
                    var entry = context.Entry(context.Set<Anais>().Find(id));
                    entry.State = EntityState.Modified;
                    entry.Property(propertyName).CurrentValue = "Daisy";
                    entry.Property(propertyName).IsModified = true;

                    context.SaveChanges();
                },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.Never), null)]
        [InlineData(nameof(Anais.OnAdd), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdate), null)]
        [InlineData(nameof(Anais.NeverUseBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), null)]
        [InlineData(nameof(Anais.NeverThrowBeforeUseAfter), null)]
        [InlineData(nameof(Anais.OnAddUseBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter), null)]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Rabbit")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter), "Rabbit")]
        public override void After_save_use_ignores_value_if_not_modified(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Anais()).Entity;

                    context.SaveChanges();

                    id = entity.Id;
                },
                context =>
                {
                    var entry = context.Entry(context.Set<Anais>().Find(id));
                    entry.State = EntityState.Modified;
                    entry.Property(propertyName).CurrentValue = "Daisy";
                    entry.Property(propertyName).IsModified = false;

                    context.SaveChanges();
                },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [ConditionalTheory]
        [InlineData(nameof(Anais.Never), "Daisy")]
        [InlineData(nameof(Anais.OnAdd), "Daisy")]
        [InlineData(nameof(Anais.NeverUseBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.NeverThrowBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddUseBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Daisy")]
        [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter), "Daisy")]
        public override void After_save_use_uses_value_if_modified(string propertyName, string expectedValue)
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Anais()).Entity;

                    context.SaveChanges();

                    id = entity.Id;
                },
                context =>
                {
                    var entry = context.Entry(context.Set<Anais>().Find(id));
                    entry.State = EntityState.Modified;
                    entry.Property(propertyName).CurrentValue = "Daisy";

                    context.SaveChanges();
                },
                context => Assert.Equal(expectedValue, GetValue(context.Set<Anais>().Find(id), propertyName)));
        }

        [ConditionalFact]
        public override void Identity_key_with_read_only_before_save_throws_if_explicit_values_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Add(new Gumball { Id = 1 });

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("Id", "Gumball"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public override void Identity_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entry = context.Add(new Gumball { Identity = "Masami" });
                    entry.Property(e => e.Identity).IsTemporary = true;

                    context.SaveChanges();
                    id = entry.Entity.Id;

                    Assert.Equal("Banana Joe", entry.Entity.Identity);
                    Assert.False(entry.Property(e => e.Identity).IsTemporary);
                },
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).Identity));
        }

        [ConditionalTheory] // Issue #22027 #14192
        [InlineData(EntityState.Modified)]
        [InlineData(EntityState.Deleted)]
        public new void Change_state_of_entity_with_temp_non_key_does_not_throw(EntityState targetState)
        {
            int id = new Random().Next();
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var dependent = new NonStoreGenDependent
                    {
                        Id = id,
                    };

                    context.Add(dependent);

                    Assert.True(context.Entry(dependent).Property(e => e.HasTemp).IsTemporary);

                    context.SaveChanges();

                    Assert.False(context.Entry(dependent).Property(e => e.HasTemp).IsTemporary);
                    Assert.Equal(777, dependent.HasTemp);
                },
                context =>
                {
                    var principal = new StoreGenPrincipal() { Id = id };
                    var dependent = new NonStoreGenDependent
                    {
                        Id = id,
                        StoreGenPrincipal = principal
                    };

                    context.Add(dependent);

                    context.Entry(dependent).State = targetState;

                    Assert.Equal(EntityState.Added, context.Entry(principal).State);
                    //Assert.True(context.Entry(principal).Property(e => e.Id).IsTemporary);
                    Assert.True(context.Entry(dependent).Property(e => e.HasTemp).IsTemporary);
                    //Assert.True(context.Entry(dependent).Property(e => e.StoreGenPrincipalId).IsTemporary);

                    context.SaveChanges();

                    Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);

                    Assert.Equal(
                        targetState == EntityState.Modified ? EntityState.Unchanged : EntityState.Detached,
                        context.Entry(dependent).State);

                    Assert.False(context.Entry(principal).Property(e => e.Id).IsTemporary);
                    Assert.False(context.Entry(dependent).Property(e => e.HasTemp).IsTemporary);
                    Assert.False(context.Entry(dependent).Property(e => e.StoreGenPrincipalId).IsTemporary);
                });
        }

        [ConditionalFact] // Issue #19137
        public new void Clearing_optional_FK_does_not_leave_temporary_value()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var product = new OptionalProduct();
                    context.Add(product);

                    Assert.True(context.ChangeTracker.HasChanges());

                    var productEntry = context.Entry(product);
                    Assert.Equal(EntityState.Added, productEntry.State);

                    Assert.Equal(0, product.Id);
                    Assert.True(productEntry.Property(e => e.Id).CurrentValue < 0);
                    Assert.True(productEntry.Property(e => e.Id).IsTemporary);

                    Assert.Null(product.CategoryId);
                    Assert.Null(productEntry.Property(e => e.CategoryId).CurrentValue);
                    Assert.False(productEntry.Property(e => e.CategoryId).IsTemporary);

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    productEntry = context.Entry(product);
                    Assert.Equal(EntityState.Unchanged, productEntry.State);

                    Assert.Equal(1, product.Id);
                    Assert.Equal(1, productEntry.Property(e => e.Id).CurrentValue);
                    Assert.False(productEntry.Property(e => e.Id).IsTemporary);

                    Assert.Null(product.CategoryId);
                    Assert.Null(productEntry.Property(e => e.CategoryId).CurrentValue);
                    Assert.False(productEntry.Property(e => e.CategoryId).IsTemporary);

                    var category = new OptionalCategory();
                    product.Category = category;

                    Assert.True(context.ChangeTracker.HasChanges());

                    productEntry = context.Entry(product);
                    Assert.Equal(EntityState.Modified, productEntry.State);

                    Assert.Equal(1, product.Id);
                    Assert.Equal(1, productEntry.Property(e => e.Id).CurrentValue);
                    Assert.False(productEntry.Property(e => e.Id).IsTemporary);

                    Assert.Null(product.CategoryId);
                    Assert.True(productEntry.Property(e => e.CategoryId).CurrentValue < 0);
                    Assert.True(productEntry.Property(e => e.CategoryId).IsTemporary);

                    var categoryEntry = context.Entry(category);
                    Assert.Equal(EntityState.Added, categoryEntry.State);
                    Assert.Equal(0, category.Id);
                    Assert.True(categoryEntry.Property(e => e.Id).CurrentValue < 0);
                    Assert.True(categoryEntry.Property(e => e.Id).IsTemporary);

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    productEntry = context.Entry(product);
                    Assert.Equal(EntityState.Unchanged, productEntry.State);

                    Assert.Equal(1, product.Id);
                    Assert.Equal(1, productEntry.Property(e => e.Id).CurrentValue);
                    Assert.False(productEntry.Property(e => e.Id).IsTemporary);

                    Assert.Equal(1, product.CategoryId);
                    Assert.Equal(1, productEntry.Property(e => e.CategoryId).CurrentValue);
                    Assert.False(productEntry.Property(e => e.CategoryId).IsTemporary);

                    categoryEntry = context.Entry(category);
                    Assert.Equal(EntityState.Unchanged, categoryEntry.State);
                    Assert.Equal(1, category.Id);
                    Assert.Equal(1, categoryEntry.Property(e => e.Id).CurrentValue);
                    Assert.False(categoryEntry.Property(e => e.Id).IsTemporary);

                    product.Category = null;

                    productEntry = context.Entry(product);
                    Assert.Equal(EntityState.Modified, productEntry.State);

                    Assert.Equal(1, product.Id);
                    Assert.Equal(1, productEntry.Property(e => e.Id).CurrentValue);
                    Assert.False(productEntry.Property(e => e.Id).IsTemporary);

                    Assert.Null(product.CategoryId);
                    Assert.Null(productEntry.Property(e => e.CategoryId).CurrentValue);
                    Assert.False(productEntry.Property(e => e.CategoryId).IsTemporary);

                    categoryEntry = context.Entry(category);
                    Assert.Equal(EntityState.Unchanged, categoryEntry.State);
                    Assert.Equal(1, category.Id);
                    Assert.Equal(1, categoryEntry.Property(e => e.Id).CurrentValue);

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    productEntry = context.Entry(product);
                    Assert.Equal(EntityState.Unchanged, productEntry.State);

                    Assert.Equal(1, product.Id);
                    Assert.Null(product.CategoryId);
                    Assert.False(productEntry.Property(e => e.Id).IsTemporary);

                    Assert.Equal(1, productEntry.Property(e => e.Id).CurrentValue);
                    Assert.Null(productEntry.Property(e => e.CategoryId).CurrentValue);
                    Assert.False(productEntry.Property(e => e.CategoryId).IsTemporary);

                    categoryEntry = context.Entry(category);
                    Assert.Equal(EntityState.Unchanged, categoryEntry.State);
                    Assert.Equal(1, category.Id);
                    Assert.Equal(1, categoryEntry.Property(e => e.Id).CurrentValue);
                    Assert.False(categoryEntry.Property(e => e.Id).IsTemporary);
                });
        }

        [ConditionalFact]
        public override void Identity_property_on_Added_entity_with_temporary_value_gets_value_from_store_even_if_same()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entry = context.Add(new Gumball { Identity = "Banana Joe" });
                    entry.Property(e => e.Identity).IsTemporary = true;

                    context.SaveChanges();
                    id = entry.Entity.Id;

                    Assert.Equal("Banana Joe", entry.Entity.Identity);
                    Assert.False(entry.Property(e => e.Identity).IsTemporary);
                },
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).Identity));
        }

        [ConditionalFact]
        public override void Identity_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;

                    Assert.Equal("Banana Joe", entity.Identity);
                },
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).Identity));
        }

        [ConditionalFact]
        public override void Identity_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Add(new Gumball { IdentityReadOnlyBeforeSave = "Masami" });

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("IdentityReadOnlyBeforeSave", "Gumball"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public override void Identity_property_on_Added_entity_can_have_value_set_explicitly()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball { Identity = "Masami" }).Entity;

                    context.SaveChanges();
                    id = entity.Id;

                    Assert.Equal("Masami", entity.Identity);
                },
                context => Assert.Equal("Masami", context.Set<Gumball>().Single(e => e.Id == id).Identity));
        }

        [ConditionalFact]
        public override void Identity_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                    Assert.Equal("Anton", gumball.IdentityReadOnlyAfterSave);

                    gumball.IdentityReadOnlyAfterSave = "Masami";
                    gumball.NotStoreGenerated = "Larry Needlemeye";

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyAfterSave("IdentityReadOnlyAfterSave", "Gumball"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public override void Identity_property_on_Modified_entity_is_included_in_update_when_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                    Assert.Equal("Banana Joe", gumball.Identity);

                    gumball.Identity = "Masami";
                    gumball.NotStoreGenerated = "Larry Needlemeye";

                    context.SaveChanges();

                    Assert.Equal("Masami", gumball.Identity);
                },
                context => Assert.Equal("Masami", context.Set<Gumball>().Single(e => e.Id == id).Identity));
        }

        [ConditionalFact]
        public override void Identity_property_on_Modified_entity_is_not_included_in_update_when_not_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                    Assert.Equal("Banana Joe", gumball.Identity);

                    gumball.Identity = "Masami";
                    gumball.NotStoreGenerated = "Larry Needlemeye";

                    context.Entry(gumball).Property(e => e.Identity).OriginalValue = "Masami";
                    context.Entry(gumball).Property(e => e.Identity).IsModified = false;

                    context.SaveChanges();

                    Assert.Equal("Masami", gumball.Identity);
                },
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).Identity));
        }

        [ConditionalFact]
        public override void Always_identity_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entry = context.Add(new Gumball { AlwaysIdentity = "Masami" });
                    entry.Property(e => e.AlwaysIdentity).IsTemporary = true;

                    context.SaveChanges();
                    id = entry.Entity.Id;

                    Assert.Equal("Banana Joe", entry.Entity.AlwaysIdentity);
                },
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).AlwaysIdentity));
        }

        [ConditionalFact]
        public override void Always_identity_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;

                    Assert.Equal("Banana Joe", entity.AlwaysIdentity);
                },
                context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).AlwaysIdentity));
        }

        [ConditionalFact]
        public override void Always_identity_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Add(new Gumball { AlwaysIdentityReadOnlyBeforeSave = "Masami" });

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("AlwaysIdentityReadOnlyBeforeSave", "Gumball"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public override void Always_identity_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                    Assert.Equal("Anton", gumball.AlwaysIdentityReadOnlyAfterSave);

                    gumball.AlwaysIdentityReadOnlyAfterSave = "Masami";
                    gumball.NotStoreGenerated = "Larry Needlemeye";

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyAfterSave("AlwaysIdentityReadOnlyAfterSave", "Gumball"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public override void Always_identity_property_on_Modified_entity_is_not_included_in_the_update_when_not_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                    Assert.Equal("Banana Joe", gumball.AlwaysIdentity);

                    gumball.AlwaysIdentity = "Masami";
                    gumball.NotStoreGenerated = "Larry Needlemeye";

                    context.Entry(gumball).Property(e => e.AlwaysIdentity).OriginalValue = "Masami";
                    context.Entry(gumball).Property(e => e.AlwaysIdentity).IsModified = false;

                    context.SaveChanges();

                    Assert.Equal("Masami", gumball.AlwaysIdentity);
                }, context => Assert.Equal("Banana Joe", context.Set<Gumball>().Single(e => e.Id == id).AlwaysIdentity));
        }

        [ConditionalFact]
        public override void Computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entry = context.Add(new Gumball { Computed = "Masami" });
                    entry.Property(e => e.Computed).IsTemporary = true;

                    context.SaveChanges();
                    id = entry.Entity.Id;

                    Assert.Equal("Alan", entry.Entity.Computed);
                },
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).Computed));
        }

        [ConditionalFact]
        public override void Computed_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;

                    Assert.Equal("Alan", entity.Computed);
                },
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).Computed));
        }

        [ConditionalFact]
        public override void Computed_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Add(new Gumball { ComputedReadOnlyBeforeSave = "Masami" });

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("ComputedReadOnlyBeforeSave", "Gumball"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public override void Computed_property_on_Added_entity_can_have_value_set_explicitly()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball { Computed = "Masami" }).Entity;

                    context.SaveChanges();
                    id = entity.Id;

                    Assert.Equal("Masami", entity.Computed);
                },
                context => Assert.Equal("Masami", context.Set<Gumball>().Single(e => e.Id == id).Computed));
        }

        [ConditionalFact]
        public override void Computed_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                    Assert.Equal("Tina Rex", gumball.ComputedReadOnlyAfterSave);

                    gumball.ComputedReadOnlyAfterSave = "Masami";
                    gumball.NotStoreGenerated = "Larry Needlemeye";

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyAfterSave("ComputedReadOnlyAfterSave", "Gumball"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public override void Computed_property_on_Modified_entity_is_included_in_update_when_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                    Assert.Equal("Alan", gumball.Computed);

                    gumball.Computed = "Masami";
                    gumball.NotStoreGenerated = "Larry Needlemeye";

                    context.SaveChanges();

                    Assert.Equal("Masami", gumball.Computed);
                },
                context => Assert.Equal("Masami", context.Set<Gumball>().Single(e => e.Id == id).Computed));
        }

        [ConditionalFact]
        public override void Computed_property_on_Modified_entity_is_read_from_store_when_not_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                    Assert.Equal("Alan", gumball.Computed);

                    gumball.Computed = "Masami";
                    gumball.NotStoreGenerated = "Larry Needlemeye";

                    context.Entry(gumball).Property(e => e.Computed).OriginalValue = "Masami";
                    context.Entry(gumball).Property(e => e.Computed).IsModified = false;

                    context.SaveChanges();

                    Assert.Equal("Alan", gumball.Computed);
                },
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).Computed));
        }

        [ConditionalFact]
        public override void Always_computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entry = context.Add(new Gumball { AlwaysComputed = "Masami" });
                    entry.Property(e => e.AlwaysComputed).IsTemporary = true;

                    context.SaveChanges();
                    id = entry.Entity.Id;

                    Assert.Equal("Alan", entry.Entity.AlwaysComputed);
                },
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).AlwaysComputed));
        }

        [ConditionalFact]
        public override void Always_computed_property_on_Added_entity_with_default_value_gets_value_from_store()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;

                    Assert.Equal("Alan", entity.AlwaysComputed);
                },
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).AlwaysComputed));
        }

        [ConditionalFact]
        public override void Always_computed_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.Add(new Gumball { AlwaysComputedReadOnlyBeforeSave = "Masami" });

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyBeforeSave("AlwaysComputedReadOnlyBeforeSave", "Gumball"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public override void Always_computed_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                    Assert.Equal("Tina Rex", gumball.AlwaysComputedReadOnlyAfterSave);

                    gumball.AlwaysComputedReadOnlyAfterSave = "Masami";
                    gumball.NotStoreGenerated = "Larry Needlemeye";

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyAfterSave("AlwaysComputedReadOnlyAfterSave", "Gumball"),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public override void Always_computed_property_on_Modified_entity_is_read_from_store_when_not_modified()
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new Gumball()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var gumball = context.Set<Gumball>().Single(e => e.Id == id);

                    Assert.Equal("Alan", gumball.AlwaysComputed);

                    gumball.AlwaysComputed = "Masami";
                    gumball.NotStoreGenerated = "Larry Needlemeye";

                    context.Entry(gumball).Property(e => e.AlwaysComputed).OriginalValue = "Masami";
                    context.Entry(gumball).Property(e => e.AlwaysComputed).IsModified = false;

                    context.SaveChanges();

                    Assert.Equal("Alan", gumball.AlwaysComputed);
                },
                context => Assert.Equal("Alan", context.Set<Gumball>().Single(e => e.Id == id).AlwaysComputed));
        }

        [ConditionalFact]
        public override void Fields_used_correctly_for_store_generated_values()
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new WithBackingFields()).Entity;

                    context.SaveChanges();
                    id = entity.Id;
                },
                context =>
                {
                    var entity = context.Set<WithBackingFields>().Single(e => e.Id.Equals(id));
                    Assert.Equal(1, entity.NullableAsNonNullable);
                    Assert.Equal(1, entity.NonNullableAsNullable);
                });
        }

        [ConditionalFact]
        public override void Nullable_fields_get_defaults_when_not_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new WithNullableBackingFields()).Entity;

                    context.SaveChanges();

                    Assert.NotEqual(0, entity.Id);
                    Assert.True(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(-1, entity.NullableBackedIntNonZeroDefault);
                    Assert.False(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(0, entity.NullableBackedIntZeroDefault);
                },
                context =>
                {
                    var entity = context.Set<WithNullableBackingFields>().Single();
                    Assert.True(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(-1, entity.NullableBackedIntNonZeroDefault);
                    Assert.False(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(0, entity.NullableBackedIntZeroDefault);
                });
        }

        [ConditionalFact]
        public override void Nullable_fields_store_non_defaults_when_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(
                        new WithNullableBackingFields
                        {
                            NullableBackedBoolTrueDefault = false,
                            NullableBackedIntNonZeroDefault = 0,
                            NullableBackedBoolFalseDefault = true,
                            NullableBackedIntZeroDefault = -1
                        }).Entity;

                    context.SaveChanges();

                    Assert.NotEqual(0, entity.Id);
                    Assert.False(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(0, entity.NullableBackedIntNonZeroDefault);
                    Assert.True(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(-1, entity.NullableBackedIntZeroDefault);
                },
                context =>
                {
                    var entity = context.Set<WithNullableBackingFields>().Single();
                    Assert.False(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(0, entity.NullableBackedIntNonZeroDefault);
                    Assert.True(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(-1, entity.NullableBackedIntZeroDefault);
                });
        }

        [ConditionalFact]
        public override void Nullable_fields_store_any_value_when_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(
                        new WithNullableBackingFields
                        {
                            NullableBackedBoolTrueDefault = true,
                            NullableBackedIntNonZeroDefault = 3,
                            NullableBackedBoolFalseDefault = true,
                            NullableBackedIntZeroDefault = 5
                        }).Entity;

                    context.SaveChanges();

                    Assert.NotEqual(0, entity.Id);
                    Assert.True(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(3, entity.NullableBackedIntNonZeroDefault);
                    Assert.True(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(5, entity.NullableBackedIntZeroDefault);
                },
                context =>
                {
                    var entity = context.Set<WithNullableBackingFields>().Single();
                    Assert.True(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(3, entity.NullableBackedIntNonZeroDefault);
                    Assert.True(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(5, entity.NullableBackedIntZeroDefault);
                });
        }

        [ConditionalFact]
        public override void Object_fields_get_defaults_when_not_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(new WithObjectBackingFields()).Entity;

                    context.SaveChanges();

                    Assert.NotEqual(0, entity.Id);
                    Assert.True(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(-1, entity.NullableBackedIntNonZeroDefault);
                    Assert.False(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(0, entity.NullableBackedIntZeroDefault);
                },
                context =>
                {
                    var entity = context.Set<WithObjectBackingFields>().Single();
                    Assert.True(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(-1, entity.NullableBackedIntNonZeroDefault);
                    Assert.False(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(0, entity.NullableBackedIntZeroDefault);
                });
        }

        [ConditionalFact]
        public override void Object_fields_store_non_defaults_when_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(
                        new WithObjectBackingFields
                        {
                            NullableBackedBoolTrueDefault = false,
                            NullableBackedIntNonZeroDefault = 0,
                            NullableBackedBoolFalseDefault = true,
                            NullableBackedIntZeroDefault = -1
                        }).Entity;

                    context.SaveChanges();

                    Assert.NotEqual(0, entity.Id);
                    Assert.False(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(0, entity.NullableBackedIntNonZeroDefault);
                    Assert.True(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(-1, entity.NullableBackedIntZeroDefault);
                },
                context =>
                {
                    var entity = context.Set<WithObjectBackingFields>().Single();
                    Assert.False(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(0, entity.NullableBackedIntNonZeroDefault);
                    Assert.True(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(-1, entity.NullableBackedIntZeroDefault);
                });
        }

        [ConditionalFact]
        public override void Object_fields_store_any_value_when_set()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Add(
                        new WithObjectBackingFields
                        {
                            NullableBackedBoolTrueDefault = true,
                            NullableBackedIntNonZeroDefault = 3,
                            NullableBackedBoolFalseDefault = true,
                            NullableBackedIntZeroDefault = 5
                        }).Entity;

                    context.SaveChanges();

                    Assert.NotEqual(0, entity.Id);
                    Assert.True(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(3, entity.NullableBackedIntNonZeroDefault);
                    Assert.True(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(5, entity.NullableBackedIntZeroDefault);
                },
                context =>
                {
                    var entity = context.Set<WithObjectBackingFields>().Single();
                    Assert.True(entity.NullableBackedBoolTrueDefault);
                    Assert.Equal(3, entity.NullableBackedIntNonZeroDefault);
                    Assert.True(entity.NullableBackedBoolFalseDefault);
                    Assert.Equal(5, entity.NullableBackedIntZeroDefault);
                });
        }

        private static Anais WithValue(string propertyName, int id = 0)
            => SetValue(new Anais { Id = id }, propertyName);

        private static Anais SetValue(Anais entity, string propertyName)
        {
            entity.GetType().GetTypeInfo().GetDeclaredProperty(propertyName).SetValue(entity, "Pink");
            return entity;
        }

        private static string GetValue(Anais entity, string propertyName)
            => (string)entity.GetType().GetTypeInfo().GetDeclaredProperty(propertyName).GetValue(entity);
        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class StoreGeneratedXGFixture : StoreGeneratedFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => builder
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(
                        b => b.Default(WarningBehavior.Throw)
                            .Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning)
                            .Ignore(RelationalEventId.BoolWithDefaultWarning));

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<Gumball>(
                    b =>
                    {
                        b.Property(e => e.Id).UseXGIdentityColumn();
                        b.Property(e => e.Identity).HasMaxLength(500).HasDefaultValue("Banana Joe");
                        b.Property(e => e.IdentityReadOnlyBeforeSave).HasMaxLength(500).HasDefaultValue("Doughnut Sheriff");
                        b.Property(e => e.IdentityReadOnlyAfterSave).HasMaxLength(500).HasDefaultValue("Anton");
                        b.Property(e => e.AlwaysIdentity).HasMaxLength(500).HasDefaultValue("Banana Joe");
                        b.Property(e => e.AlwaysIdentityReadOnlyBeforeSave).HasMaxLength(500).HasDefaultValue("Doughnut Sheriff");
                        b.Property(e => e.AlwaysIdentityReadOnlyAfterSave).HasMaxLength(500).HasDefaultValue("Anton");
                        b.Property(e => e.Computed).HasMaxLength(500).HasDefaultValue("Alan");
                        b.Property(e => e.ComputedReadOnlyBeforeSave).HasMaxLength(500).HasDefaultValue("Carmen");
                        b.Property(e => e.ComputedReadOnlyAfterSave).HasMaxLength(500).HasDefaultValue("Tina Rex");
                        b.Property(e => e.AlwaysComputed).HasMaxLength(500).HasDefaultValue("Alan");
                        b.Property(e => e.AlwaysComputedReadOnlyBeforeSave).HasMaxLength(500).HasDefaultValue("Carmen");
                        b.Property(e => e.AlwaysComputedReadOnlyAfterSave).HasMaxLength(500).HasDefaultValue("Tina Rex");
                    });

                modelBuilder.Entity<Anais>(
                    b =>
                    {
                        b.Property(e => e.OnAdd).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddUseBeforeUseAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddIgnoreBeforeUseAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddThrowBeforeUseAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddUseBeforeIgnoreAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddIgnoreBeforeIgnoreAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddThrowBeforeIgnoreAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddUseBeforeThrowAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddIgnoreBeforeThrowAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddThrowBeforeThrowAfter).HasMaxLength(500).HasDefaultValue("Rabbit");

                        b.Property(e => e.OnAddOrUpdate).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddOrUpdateUseBeforeUseAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddOrUpdateIgnoreBeforeUseAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddOrUpdateThrowBeforeUseAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddOrUpdateUseBeforeIgnoreAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddOrUpdateIgnoreBeforeIgnoreAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddOrUpdateThrowBeforeIgnoreAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddOrUpdateUseBeforeThrowAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddOrUpdateIgnoreBeforeThrowAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnAddOrUpdateThrowBeforeThrowAfter).HasMaxLength(500).HasDefaultValue("Rabbit");

                        b.Property(e => e.OnUpdate).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnUpdateUseBeforeUseAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnUpdateIgnoreBeforeUseAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnUpdateThrowBeforeUseAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnUpdateUseBeforeIgnoreAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnUpdateIgnoreBeforeIgnoreAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnUpdateThrowBeforeIgnoreAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnUpdateUseBeforeThrowAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnUpdateIgnoreBeforeThrowAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                        b.Property(e => e.OnUpdateThrowBeforeThrowAfter).HasMaxLength(500).HasDefaultValue("Rabbit");
                    });

                modelBuilder.Entity<WithBackingFields>(
                    b =>
                    {
                        if (AppConfig.ServerVersion.Supports.GeneratedColumns)
                        {
                            b.Property(e => e.NullableAsNonNullable).HasComputedColumnSql("1");
                            b.Property(e => e.NonNullableAsNullable).HasComputedColumnSql("1");
                        }
                        else
                        {
                            b.Property(e => e.NullableAsNonNullable).HasDefaultValue(1);
                            b.Property(e => e.NonNullableAsNullable).HasDefaultValue(1);
                        }
                    });

                modelBuilder.Entity<WithNullableBackingFields>(
                    b =>
                    {
                        b.Property(e => e.NullableBackedBoolTrueDefault).HasDefaultValue(true);
                        b.Property(e => e.NullableBackedIntNonZeroDefault).HasDefaultValue(-1);
                        b.Property(e => e.NullableBackedBoolFalseDefault).HasDefaultValue(false);
                        b.Property(e => e.NullableBackedIntZeroDefault).HasDefaultValue(0);
                    });

                modelBuilder.Entity<WithObjectBackingFields>(
                    b =>
                    {
                        b.Property(e => e.NullableBackedBoolTrueDefault).HasDefaultValue(true);
                        b.Property(e => e.NullableBackedIntNonZeroDefault).HasDefaultValue(-1);
                        b.Property(e => e.NullableBackedBoolFalseDefault).HasDefaultValue(false);
                        b.Property(e => e.NullableBackedIntZeroDefault).HasDefaultValue(0);
                    });

                modelBuilder.Entity<NonStoreGenDependent>().Property(e => e.HasTemp).HasDefaultValue(777);

                //base.OnModelCreating(modelBuilder, context);
                modelBuilder.Entity<IntToString>().Property(e => e.Id).HasConversion<string>();
                modelBuilder.Entity<GuidToString>().Property(e => e.Id).HasConversion<string>();
                modelBuilder.Entity<GuidToBytes>().HasNoKey().Property(e => e.Id).HasConversion<byte[]>();
                modelBuilder.Entity<ShortToBytes>().HasNoKey().Property(e => e.Id).HasConversion<byte[]>();

                modelBuilder.Entity<Gumball>(
                    b =>
                    {
                        var property = b.Property(e => e.Id).ValueGeneratedOnAdd().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.Identity).ValueGeneratedOnAdd().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.IdentityReadOnlyBeforeSave).ValueGeneratedOnAdd().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.IdentityReadOnlyAfterSave).ValueGeneratedOnAdd().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.AlwaysIdentity).ValueGeneratedOnAdd().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.AlwaysIdentityReadOnlyBeforeSave).ValueGeneratedOnAdd().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.AlwaysIdentityReadOnlyAfterSave).ValueGeneratedOnAdd().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.Computed).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.ComputedReadOnlyBeforeSave).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.ComputedReadOnlyAfterSave).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.AlwaysComputed).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.AlwaysComputedReadOnlyBeforeSave).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.AlwaysComputedReadOnlyAfterSave).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    });

                modelBuilder.Entity<Anais>(
                    b =>
                    {
                        b.Property(e => e.Never).ValueGeneratedNever();

                        var property = b.Property(e => e.NeverUseBeforeUseAfter).ValueGeneratedNever().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.NeverIgnoreBeforeUseAfter).ValueGeneratedNever().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.NeverThrowBeforeUseAfter).ValueGeneratedNever().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.NeverUseBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.NeverIgnoreBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.NeverThrowBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.NeverUseBeforeThrowAfter).ValueGeneratedNever().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.NeverIgnoreBeforeThrowAfter).ValueGeneratedNever().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.NeverThrowBeforeThrowAfter).ValueGeneratedNever().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        b.Property(e => e.OnAdd).ValueGeneratedOnAdd();

                        property = b.Property(e => e.OnAddUseBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.OnAddIgnoreBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.OnAddThrowBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.OnAddUseBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.OnAddIgnoreBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.OnAddThrowBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.OnAddUseBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.OnAddIgnoreBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.OnAddThrowBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        b.Property(e => e.OnAddOrUpdate).ValueGeneratedOnAddOrUpdate();

                        property = b.Property(e => e.OnAddOrUpdateUseBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.OnAddOrUpdateThrowBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.OnAddOrUpdateUseBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.OnAddOrUpdateThrowBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.OnAddOrUpdateUseBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.OnAddOrUpdateThrowBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        b.Property(e => e.OnUpdate).ValueGeneratedOnUpdate();

                        property = b.Property(e => e.OnUpdateUseBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.OnUpdateIgnoreBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.OnUpdateThrowBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                        property = b.Property(e => e.OnUpdateUseBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.OnUpdateIgnoreBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.OnUpdateThrowBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                        property = b.Property(e => e.OnUpdateUseBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.OnUpdateIgnoreBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                        property = b.Property(e => e.OnUpdateThrowBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                        property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                        property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                    });

                modelBuilder.Entity<Darwin>();

                modelBuilder.Entity<WithBackingFields>(
                    b =>
                    {
                        b.Property(e => e.Id).HasField("_id");
                        b.Property(e => e.NullableAsNonNullable).HasField("_nullableAsNonNullable").ValueGeneratedOnAddOrUpdate();
                        b.Property(e => e.NonNullableAsNullable).HasField("_nonNullableAsNullable").ValueGeneratedOnAddOrUpdate();
                    });

                modelBuilder.Entity<OptionalProduct>();
                modelBuilder.Entity<StoreGenPrincipal>();

                modelBuilder.Entity<NonStoreGenDependent>()
                    .Property(e => e.HasTemp)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasValueGenerator<TemporaryIntValueGenerator>();
            }
        }
    }
}
