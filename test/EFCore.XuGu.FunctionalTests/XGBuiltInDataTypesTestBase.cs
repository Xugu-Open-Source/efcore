// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable InconsistentNaming
namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    public abstract class XGBuiltInDataTypesTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : XGBuiltInDataTypesTestBase<TFixture>.BuiltInDataTypesFixtureBase, new()
    {
        protected XGBuiltInDataTypesTestBase(TFixture fixture)
            => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected DbContext CreateContext()
            => Fixture.CreateContext();

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_filter_projection_with_captured_enum_variable(bool async)
        {
            using var context = CreateContext();
            var templateType = EmailTemplateTypeDto.PasswordResetRequest;

            var query = context
                .Set<EmailTemplate>()
                .Select(
                    t => new EmailTemplateDto { Id = t.Id, TemplateType = (EmailTemplateTypeDto)t.TemplateType })
                .Where(t => t.TemplateType == templateType);

            var results = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Single(results);
            Assert.Equal(EmailTemplateTypeDto.PasswordResetRequest, results.Single().TemplateType);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_filter_projection_with_inline_enum_variable(bool async)
        {
            using var context = CreateContext();
            var query = context
                .Set<EmailTemplate>()
                .Select(
                    t => new EmailTemplateDto { Id = t.Id, TemplateType = (EmailTemplateTypeDto)t.TemplateType })
                .Where(t => t.TemplateType == EmailTemplateTypeDto.PasswordResetRequest);

            var results = async
                ? await query.ToListAsync()
                : query.ToList();

            Assert.Single(results);
            Assert.Equal(EmailTemplateTypeDto.PasswordResetRequest, results.Single().TemplateType);
        }

        [ConditionalFact]
        public virtual void Can_perform_query_with_max_length()
        {
            var shortString = "Sky";
            var shortBinary = new byte[] { 8, 8, 7, 8, 7 };
            var longString = new string('X', Fixture.LongStringLength);
            var longBinary = new byte[Fixture.LongStringLength];
            for (var i = 0; i < longBinary.Length; i++)
            {
                longBinary[i] = (byte)i;
            }

            using (var context = CreateContext())
            {
                context.Set<MaxLengthDataTypes>().Add(
                    new MaxLengthDataTypes
                    {
                        Id = 799,
                        String3 = shortString,
                        ByteArray5 = shortBinary,
                        String9000 = longString,
                        ByteArray9000 = longBinary
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                Assert.NotNull(
                    context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.String3 == shortString).ToList().SingleOrDefault());

                Assert.NotNull(
                    context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.String9000 == longString).ToList().SingleOrDefault());

                Assert.NotNull(
                    context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.ByteArray5 == shortBinary).ToList()
                        .SingleOrDefault());

                Assert.NotNull(
                    context.Set<MaxLengthDataTypes>().Where(e => e.Id == 799 && e.ByteArray9000 == longBinary).ToList()
                        .SingleOrDefault());
            }
        }

        [ConditionalFact]
        public virtual void Can_perform_query_with_ansi_strings_test()
        {
            var shortString = Fixture.SupportsUnicodeToAnsiConversion ? "Ϩky" : "sky";
            var longString = Fixture.SupportsUnicodeToAnsiConversion
                ? new string('Ϩ', Fixture.LongStringLength)
                : new string('s', Fixture.LongStringLength);

            using (var context = CreateContext())
            {
                context.Set<UnicodeDataTypes>().Add(
                    new UnicodeDataTypes
                    {
                        Id = 799,
                        StringDefault = shortString,
                        StringAnsi = shortString,
                        StringAnsi3 = shortString,
                        StringAnsi9000 = longString,
                        StringUnicode = shortString
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                Assert.NotNull(
                    context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringDefault == shortString).ToList().SingleOrDefault());
                Assert.NotNull(
                    context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringAnsi == shortString).ToList().SingleOrDefault());
                Assert.NotNull(
                    context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringAnsi3 == shortString).ToList().SingleOrDefault());

                if (Fixture.SupportsLargeStringComparisons)
                {
                    Assert.NotNull(
                        context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringAnsi9000 == longString).ToList()
                            .SingleOrDefault());
                }

                Assert.NotNull(
                    context.Set<UnicodeDataTypes>().Where(e => e.Id == 799 && e.StringUnicode == shortString).ToList().SingleOrDefault());

                var entity = context.Set<UnicodeDataTypes>().Where(e => e.Id == 799).ToList().Single();

                Assert.Equal(shortString, entity.StringDefault);
                Assert.Equal(shortString, entity.StringUnicode);

                if (Fixture.SupportsAnsi
                    && Fixture.SupportsUnicodeToAnsiConversion)
                {
                    Assert.NotEqual(shortString, entity.StringAnsi);
                    Assert.NotEqual(shortString, entity.StringAnsi3);
                    Assert.NotEqual(longString, entity.StringAnsi9000);
                }
                else
                {
                    Assert.Equal(shortString, entity.StringAnsi);
                    Assert.Equal(shortString, entity.StringAnsi3);
                    Assert.Equal(longString, entity.StringAnsi9000);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_query_using_any_data_type()
        {
            using var context = CreateContext();
            var source = AddTestBuiltInDataTypes(context.Set<BuiltInDataTypes>());

            Assert.Equal(1, context.SaveChanges());

            QueryBuiltInDataTypesTest(source);
        }

        [ConditionalFact]
        public virtual void Can_query_using_any_data_type_shadow()
        {
            using var context = CreateContext();
            var source = AddTestBuiltInDataTypes(context.Set<BuiltInDataTypesShadow>());

            Assert.Equal(1, context.SaveChanges());

            QueryBuiltInDataTypesTest(source);
        }

        private void QueryBuiltInDataTypesTest<TEntity>(EntityEntry<TEntity> source)
            where TEntity : BuiltInDataTypesBase
        {
            using var context = CreateContext();
            var set = context.Set<TEntity>();
            var entity = set.Where(e => e.Id == 11).ToList().Single();
            var entityType = context.Model.FindEntityType(typeof(TEntity));

            var param1 = (short)-1234;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<short>(e, nameof(BuiltInDataTypes.TestInt16)) == param1).ToList().Single());

            var param2 = -123456789;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<int>(e, nameof(BuiltInDataTypes.TestInt32)) == param2).ToList().Single());

            var param3 = -1234567890123456789L;
            if (Fixture.IntegerPrecision == 64)
            {
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<long>(e, nameof(BuiltInDataTypes.TestInt64)) == param3).ToList().Single());
            }

            double? param4 = -1.23456789;
            if (Fixture.StrictEquality)
            {
                Assert.Same(
                    entity, set.Where(
                        e => e.Id == 11
                            && EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) == param4).ToList().Single());
            }
            else if (Fixture.SupportsDecimalComparisons)
            {
                double? param4l = -1.234567891;
                double? param4h = -1.234567889;
                Assert.Same(
                    entity, set.Where(
                            e => e.Id == 11
                                && (EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) == param4
                                    || (EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) > param4l
                                        && EF.Property<double>(e, nameof(BuiltInDataTypes.TestDouble)) < param4h)))
                        .ToList().Single());
            }

            var param5 = -1234567890.01M;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<decimal>(e, nameof(BuiltInDataTypes.TestDecimal)) == param5).ToList()
                    .Single());

            var param6 = Fixture.DefaultDateTime;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<DateTime>(e, nameof(BuiltInDataTypes.TestDateTime)) == param6).ToList()
                    .Single());

            //if (entityType.FindProperty(nameof(BuiltInDataTypes.TestDateTimeOffset)) != null)
            //{
            //    var param7 = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0));
            //    Assert.Same(
            //        entity,
            //        set.Where(e => e.Id == 11 && EF.Property<DateTimeOffset>(e, nameof(BuiltInDataTypes.TestDateTimeOffset)) == param7)
            //            .ToList().Single());
            //}

            

            var param9 = -1.234F;
            if (Fixture.StrictEquality)
            {
                Assert.Same(
                    entity, set.Where(
                        e => e.Id == 11
                            && EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) == param9).ToList().Single());
            }
            else if (Fixture.SupportsDecimalComparisons)
            {
                var param9l = -1.2341F;
                var param9h = -1.2339F;
                Assert.Same(
                    entity, set.Where(
                        e => e.Id == 11
                            && (EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) == param9
                                || (EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) > param9l
                                    && EF.Property<float>(e, nameof(BuiltInDataTypes.TestSingle)) < param9h))).ToList().Single());
            }

            var param10 = true;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<bool>(e, nameof(BuiltInDataTypes.TestBoolean)) == param10).ToList().Single());

            if (entityType.FindProperty(nameof(BuiltInDataTypes.TestByte)) != null)
            {
                var param11 = 127;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<sbyte>(e, nameof(BuiltInDataTypes.TestByte)) == param11).ToList().Single());
            }

            var param12 = Enum64.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == param12).ToList().Single());

            var param13 = Enum32.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == param13).ToList().Single());

            var param14 = Enum16.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == 11 && EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == param14).ToList().Single());

            if (entityType.FindProperty(nameof(BuiltInDataTypes.Enum8)) != null)
            {
                var param15 = Enum8.SomeValue;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == param15).ToList().Single());
            }

            if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt16)) != null)
            {
                var param16 = (short)1234;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<short>(e, nameof(BuiltInDataTypes.TestUnsignedInt16)) == param16).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt32)) != null)
            {
                var param17 = 1234565789U;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<int>(e, nameof(BuiltInDataTypes.TestUnsignedInt32)) == param17).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInDataTypes.TestUnsignedInt64)) != null)
            {
                var param18 = 1234567890123456789L;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<long>(e, nameof(BuiltInDataTypes.TestUnsignedInt64)) == param18).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInDataTypes.TestCharacter)) != null)
            {
                var param19 = 'a';
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<char>(e, nameof(BuiltInDataTypes.TestCharacter)) == param19).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInDataTypes.TestSignedByte)) != null)
            {
                var param20 = (sbyte)-128;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<sbyte>(e, nameof(BuiltInDataTypes.TestSignedByte)) == param20).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU64)) != null)
            {
                var param21 = EnumU64.SomeValue;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<EnumU64>(e, nameof(BuiltInDataTypes.EnumU64)) == param21).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU32)) != null)
            {
                var param22 = EnumU32.SomeValue;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<EnumU32>(e, nameof(BuiltInDataTypes.EnumU32)) == param22).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumU16)) != null)
            {
                var param23 = EnumU16.SomeValue;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<EnumU16>(e, nameof(BuiltInDataTypes.EnumU16)) == param23).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInDataTypes.EnumS8)) != null)
            {
                var param24 = EnumS8.SomeValue;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<EnumS8>(e, nameof(BuiltInDataTypes.EnumS8)) == param24).ToList().Single());
            }

            if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum64))?.GetProviderClrType()) == typeof(long))
            {
                var param25 = 1;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == (Enum64)param25).ToList()
                        .Single());
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && (int)EF.Property<Enum64>(e, nameof(BuiltInDataTypes.Enum64)) == param25).ToList()
                        .Single());
            }

            if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum32))?.GetProviderClrType()) == typeof(int))
            {
                var param26 = 1;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == (Enum32)param26).ToList()
                        .Single());
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && (int)EF.Property<Enum32>(e, nameof(BuiltInDataTypes.Enum32)) == param26).ToList()
                        .Single());
            }

            if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum16))?.GetProviderClrType()) == typeof(short))
            {
                var param27 = 1;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == (Enum16)param27).ToList()
                        .Single());
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && (int)EF.Property<Enum16>(e, nameof(BuiltInDataTypes.Enum16)) == param27).ToList()
                        .Single());
            }

            if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInDataTypes.Enum8))?.GetProviderClrType()) == typeof(sbyte))
            {
                var param28 = 1;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == (Enum8)param28).ToList()
                        .Single());
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == 11 && (int)EF.Property<Enum8>(e, nameof(BuiltInDataTypes.Enum8)) == param28).ToList()
                        .Single());
            }

            foreach (var propertyEntry in context.Entry(entity).Properties)
            {
                if (propertyEntry.Metadata.ValueGenerated != ValueGenerated.Never)
                {
                    continue;
                }

                Assert.Equal(
                    source.Property(propertyEntry.Metadata.Name).CurrentValue,
                    propertyEntry.CurrentValue);
            }
        }

        protected EntityEntry<TEntity> AddTestBuiltInDataTypes<TEntity>(DbSet<TEntity> set)
            where TEntity : BuiltInDataTypesBase, new()
        {
            var entityEntry = set.Add(
                new TEntity { Id = 11 });

            entityEntry.CurrentValues.SetValues(
                new BuiltInDataTypes
                {
                    Id = 11,
                    PartitionId = 1,
                    TestInt16 = -1234,
                    TestInt32 = -123456789,
                    TestInt64 = -1234567890123456789L,
                    TestDouble = -1.23456789,
                    TestDecimal = -1234567890.01M,
                    TestDateTime = Fixture.DefaultDateTime,
                    //TestDateTimeOffset = new DateTime(),
                    //TestTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                    TestSingle = -1.234F,
                    TestBoolean = true,
                    TestByte = 127,
                    TestUnsignedInt16 = 1234,
                    TestUnsignedInt32 = 1234565789,
                    TestUnsignedInt64 = 1234567890123456789L,
                    TestCharacter = 'a',
                    TestSignedByte = -128,
                    Enum64 = Enum64.SomeValue,
                    Enum32 = Enum32.SomeValue,
                    Enum16 = Enum16.SomeValue,
                    Enum8 = Enum8.SomeValue,
                    EnumU64 = EnumU64.SomeValue,
                    EnumU32 = EnumU32.SomeValue,
                    EnumU16 = EnumU16.SomeValue,
                    EnumS8 = EnumS8.SomeValue
                });

            return entityEntry;
        }

        [ConditionalFact]
        public virtual void Can_query_using_any_nullable_data_type()
        {
            int id = new Random().Next();
            using var context = CreateContext();
            var source = AddTestBuiltInNullableDataTypes(context.Set<BuiltInNullableDataTypes>(),id);

            Assert.Equal(1, context.SaveChanges());

            QueryBuiltInNullableDataTypesTest(source,id);
        }

        [ConditionalFact]
        public virtual void Can_query_using_any_data_type_nullable_shadow()
        {
            int id = new Random().Next();
            using var context = CreateContext();
            var source = AddTestBuiltInNullableDataTypes(context.Set<BuiltInNullableDataTypesShadow>(),id);

            Assert.Equal(1, context.SaveChanges());

            QueryBuiltInNullableDataTypesTest(source,id);
        }

        private void QueryBuiltInNullableDataTypesTest<TEntity>(EntityEntry<TEntity> source,int id)
            where TEntity : BuiltInNullableDataTypesBase
        {
            using var context = CreateContext();
            var set = context.Set<TEntity>();
            var entity = set.Where(e => e.Id == id).ToList().Single();
            var entityType = context.Model.FindEntityType(typeof(TEntity));

            short? param1 = -1234;
            Assert.Same(
                entity,
                set.Where(e => e.Id == id && EF.Property<short?>(e, nameof(BuiltInNullableDataTypes.TestNullableInt16)) == param1)
                    .ToList().Single());

            int? param2 = -123456789;
            Assert.Same(
                entity,
                set.Where(e => e.Id == id && EF.Property<int?>(e, nameof(BuiltInNullableDataTypes.TestNullableInt32)) == param2)
                    .ToList().Single());

            long? param3 = -1234567890123456789L;
            Assert.Same(
                entity,
                set.Where(e => e.Id == id && EF.Property<long?>(e, nameof(BuiltInNullableDataTypes.TestNullableInt64)) == param3)
                    .ToList().Single());

            double? param4 = -1.23456789;
            if (Fixture.StrictEquality)
            {
                Assert.Same(
                    entity, set.Where(
                            e => e.Id == id
                                && EF.Property<double?>(e, nameof(BuiltInNullableDataTypes.TestNullableDouble)) == param4).ToList()
                        .Single());
            }
            else if (Fixture.SupportsDecimalComparisons)
            {
                double? param4l = -1.234567891;
                double? param4h = -1.234567889;
                Assert.Same(
                    entity, set.Where(
                            e => e.Id == id
                                && (EF.Property<double?>(e, nameof(BuiltInNullableDataTypes.TestNullableDouble)) == param4
                                    || (EF.Property<double?>(e, nameof(BuiltInNullableDataTypes.TestNullableDouble)) > param4l
                                        && EF.Property<double?>(e, nameof(BuiltInNullableDataTypes.TestNullableDouble)) < param4h)))
                        .ToList().Single());
            }

            decimal? param5 = -1234567890.01M;
            Assert.Same(
                entity,
                set.Where(e => e.Id == id && EF.Property<decimal?>(e, nameof(BuiltInNullableDataTypes.TestNullableDecimal)) == param5)
                    .ToList().Single());

            DateTime? param6 = Fixture.DefaultDateTime;
            Assert.Same(
                entity,
                set.Where(e => e.Id == id && EF.Property<DateTime?>(e, nameof(BuiltInNullableDataTypes.TestNullableDateTime)) == param6)
                    .ToList().Single());

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)) != null)
            {
                DateTimeOffset? param7 = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0));
                Assert.Same(
                    entity,
                    set.Where(
                        e => e.Id == id
                            && EF.Property<DateTimeOffset?>(e, nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset))
                            == param7).ToList().Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableTimeSpan)) != null)
            {
                TimeSpan? param8 = new TimeSpan(0, 10, 9, 8, 7);
                Assert.Same(
                    entity,
                    set.Where(
                            e => e.Id == id
                                && EF.Property<TimeSpan?>(e, nameof(BuiltInNullableDataTypes.TestNullableTimeSpan))
                                == param8)
                        .ToList().Single());
            }

            float? param9 = -1.234F;
            if (Fixture.StrictEquality)
            {
                Assert.Same(
                    entity, set.Where(
                            e => e.Id == id
                                && EF.Property<float?>(e, nameof(BuiltInNullableDataTypes.TestNullableSingle)) == param9).ToList()
                        .Single());
            }
            else if (Fixture.SupportsDecimalComparisons)
            {
                float? param9l = -1.2341F;
                float? param9h = -1.2339F;
                Assert.Same(
                    entity, set.Where(
                            e => e.Id == id
                                && (EF.Property<float?>(e, nameof(BuiltInNullableDataTypes.TestNullableSingle)) == param9
                                    || (EF.Property<float?>(e, nameof(BuiltInNullableDataTypes.TestNullableSingle)) > param9l
                                        && EF.Property<float?>(e, nameof(BuiltInNullableDataTypes.TestNullableSingle)) < param9h)))
                        .ToList().Single());
            }

            bool? param10 = true;
            Assert.Same(
                entity,
                set.Where(e => e.Id == id && EF.Property<bool?>(e, nameof(BuiltInNullableDataTypes.TestNullableBoolean)) == param10)
                    .ToList().Single());

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableByte)) != null)
            {
                sbyte? param11 = 127;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && EF.Property<sbyte?>(e, nameof(BuiltInNullableDataTypes.TestNullableByte)) == param11)
                        .ToList().Single());
            }

            Enum64? param12 = Enum64.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == id && EF.Property<Enum64?>(e, nameof(BuiltInNullableDataTypes.Enum64)) == param12).ToList()
                    .Single());

            Enum32? param13 = Enum32.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == id && EF.Property<Enum32?>(e, nameof(BuiltInNullableDataTypes.Enum32)) == param13).ToList()
                    .Single());

            Enum16? param14 = Enum16.SomeValue;
            Assert.Same(
                entity,
                set.Where(e => e.Id == id && EF.Property<Enum16?>(e, nameof(BuiltInNullableDataTypes.Enum16)) == param14).ToList()
                    .Single());

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum8)) != null)
            {
                Enum8? param15 = Enum8.SomeValue;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && EF.Property<Enum8?>(e, nameof(BuiltInNullableDataTypes.Enum8)) == param15).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) != null)
            {
                short? param16 = 1234;
                Assert.Same(
                    entity,
                    set.Where(
                        e => e.Id == id
                            && EF.Property<short?>(e, nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16))
                            == param16).ToList().Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) != null)
            {
                int? param17 = 1234565789;
                Assert.Same(
                    entity,
                    set.Where(
                            e => e.Id == id
                                && EF.Property<int?>(e, nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32))
                                == param17)
                        .ToList().Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) != null)
            {
                long? param18 = 1234567890123456789L;
                Assert.Same(
                    entity,
                    set.Where(
                        e => e.Id == id
                            && EF.Property<long?>(
                                e, nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64))
                            == param18).ToList().Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableCharacter)) != null)
            {
                char? param19 = 'a';
                Assert.Same(
                    entity,
                    set.Where(
                            e => e.Id == id && EF.Property<char?>(e, nameof(BuiltInNullableDataTypes.TestNullableCharacter)) == param19)
                        .ToList().Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) != null)
            {
                sbyte? param20 = -128;
                Assert.Same(
                    entity,
                    set.Where(
                            e => e.Id == id
                                && EF.Property<sbyte?>(e, nameof(BuiltInNullableDataTypes.TestNullableSignedByte))
                                == param20)
                        .ToList().Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU64)) != null)
            {
                var param21 = EnumU64.SomeValue;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && EF.Property<EnumU64?>(e, nameof(BuiltInNullableDataTypes.EnumU64)) == param21).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU32)) != null)
            {
                var param22 = EnumU32.SomeValue;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && EF.Property<EnumU32?>(e, nameof(BuiltInNullableDataTypes.EnumU32)) == param22).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU16)) != null)
            {
                var param23 = EnumU16.SomeValue;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && EF.Property<EnumU16?>(e, nameof(BuiltInNullableDataTypes.EnumU16)) == param23).ToList()
                        .Single());
            }

            if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumS8)) != null)
            {
                var param24 = EnumS8.SomeValue;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && EF.Property<EnumS8?>(e, nameof(BuiltInNullableDataTypes.EnumS8)) == param24).ToList()
                        .Single());
            }

            if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum64))?.GetProviderClrType())
                == typeof(long))
            {
                int? param25 = 1;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && EF.Property<Enum64?>(e, nameof(BuiltInNullableDataTypes.Enum64)) == (Enum64)param25)
                        .ToList().Single());
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && (int)EF.Property<Enum64?>(e, nameof(BuiltInNullableDataTypes.Enum64)) == param25)
                        .ToList().Single());
            }

            if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum32))?.GetProviderClrType())
                == typeof(int))
            {
                int? param26 = 1;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && EF.Property<Enum32?>(e, nameof(BuiltInNullableDataTypes.Enum32)) == (Enum32)param26)
                        .ToList().Single());
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && (int)EF.Property<Enum32?>(e, nameof(BuiltInNullableDataTypes.Enum32)) == param26)
                        .ToList().Single());
            }

            if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum16))?.GetProviderClrType())
                == typeof(short))
            {
                int? param27 = 1;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && EF.Property<Enum16?>(e, nameof(BuiltInNullableDataTypes.Enum16)) == (Enum16)param27)
                        .ToList().Single());
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && (int)EF.Property<Enum16?>(e, nameof(BuiltInNullableDataTypes.Enum16)) == param27)
                        .ToList().Single());
            }

            if (UnwrapNullableType(entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum8))?.GetProviderClrType())
                == typeof(sbyte))
            {
                int? param28 = 1;
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && EF.Property<Enum8?>(e, nameof(BuiltInNullableDataTypes.Enum8)) == (Enum8)param28)
                        .ToList().Single());
                Assert.Same(
                    entity,
                    set.Where(e => e.Id == id && (int)EF.Property<Enum8?>(e, nameof(BuiltInNullableDataTypes.Enum8)) == param28)
                        .ToList().Single());
            }

            foreach (var propertyEntry in context.Entry(entity).Properties)
            {
                if (propertyEntry.Metadata.ValueGenerated != ValueGenerated.Never)
                {
                    continue;
                }

                Assert.Equal(
                    source.Property(propertyEntry.Metadata.Name).CurrentValue,
                    propertyEntry.CurrentValue);
            }
        }

        protected virtual EntityEntry<TEntity> AddTestBuiltInNullableDataTypes<TEntity>(DbSet<TEntity> set,int id)
            where TEntity : BuiltInNullableDataTypesBase, new()
        {
            
            var entityEntry = set.Add(
                new TEntity { Id = id });

            entityEntry.CurrentValues.SetValues(
                new BuiltInNullableDataTypes
                {
                    Id = id,
                    PartitionId = 1,
                    TestNullableInt16 = -1234,
                    TestNullableInt32 = -123456789,
                    TestNullableInt64 = -1234567890123456789L,
                    TestNullableDouble = -1.23456789,
                    TestNullableDecimal = -1234567890.01M,
                    TestNullableDateTime = Fixture.DefaultDateTime,
                    TestNullableDateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                    TestNullableTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                    TestNullableSingle = -1.234F,
                    TestNullableBoolean = true,
                    TestNullableByte = 127,
                    TestNullableUnsignedInt16 = 1234,
                    TestNullableUnsignedInt32 = 1234565789,
                    TestNullableUnsignedInt64 = 1234567890123456789L,
                    TestNullableCharacter = 'a',
                    TestNullableSignedByte = -128,
                    Enum64 = Enum64.SomeValue,
                    Enum32 = Enum32.SomeValue,
                    Enum16 = Enum16.SomeValue,
                    Enum8 = Enum8.SomeValue,
                    EnumU64 = EnumU64.SomeValue,
                    EnumU32 = EnumU32.SomeValue,
                    EnumU16 = EnumU16.SomeValue,
                    EnumS8 = EnumS8.SomeValue
                });

            return entityEntry;
        }

        [ConditionalFact]
        public virtual void Can_query_using_any_nullable_data_type_as_literal()
        {
            var id=new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes
                    {
                        Id = id,
                        PartitionId = 1,
                        TestNullableInt16 = -1234,
                        TestNullableInt32 = -123456789,
                        TestNullableInt64 = -1234567890123456789L,
                        TestNullableDouble = -1.23456789,
                        TestNullableDecimal = -1234567890.01M,
                        TestNullableDateTime = Fixture.DefaultDateTime,
                        TestNullableDateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                        TestNullableTimeSpan = new TimeSpan(),
                        TestNullableSingle = -1.234F,
                        TestNullableBoolean = true,
                        TestNullableByte = 127,
                        TestNullableUnsignedInt16 = 1234,
                        TestNullableUnsignedInt32 = 1234565789,
                        TestNullableUnsignedInt64 = 1234567890123456789L,
                        TestNullableCharacter = 'a',
                        TestNullableSignedByte = -128,
                        Enum64 = Enum64.SomeValue,
                        Enum32 = Enum32.SomeValue,
                        Enum16 = Enum16.SomeValue,
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id).ToList().Single();
                var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableInt16 == -1234).ToList().Single());

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableInt32 == -123456789).ToList().Single());

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableInt64 == -1234567890123456789L).ToList()
                        .Single());

                if (Fixture.StrictEquality)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableDouble == -1.23456789).ToList()
                            .Single());
                }
                else if (Fixture.SupportsDecimalComparisons)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(
                            e => e.Id == id
                                && -e.TestNullableDouble + -1.23456789 < 1E-5
                                && -e.TestNullableDouble + -1.23456789 > -1E-5).ToList().Single());
                }

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableDouble != 1E18).ToList().Single());

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableDecimal == -1234567890.01M).ToList()
                        .Single());

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableDateTime == Fixture.DefaultDateTime)
                        .ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableDateTimeOffset)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(
                                e => e.Id == id
                                    && e.TestNullableDateTimeOffset
                                    == new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)))
                            .ToList().Single());
                }

                //if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableTimeSpan)) != null)
                //{
                //    Assert.Same(
                //        entity,
                //        context.Set<BuiltInNullableDataTypes>()
                //            .Where(e => e.Id == id && e.TestNullableTimeSpan == new TimeSpan(0, 10, 9, 8, 7)).ToList().Single());
                //}

                if (Fixture.StrictEquality)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableSingle == -1.234F).ToList()
                            .Single());
                }
                else if (Fixture.SupportsDecimalComparisons)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && -e.TestNullableSingle + -1.234F < 1E-5).ToList()
                            .Single());
                }

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableSingle != 1E-8).ToList().Single());

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableBoolean == true).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableByte)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableByte == 255).ToList().Single());
                }

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.Enum64 == Enum64.SomeValue).ToList().Single());

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.Enum32 == Enum32.SomeValue).ToList().Single());

                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.Enum16 == Enum16.SomeValue).ToList().Single());

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.Enum8)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.Enum8 == Enum8.SomeValue).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableUnsignedInt16 == 1234).ToList()
                            .Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableUnsignedInt32 == 1234565789U)
                            .ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>()
                            .Where(e => e.Id == id && e.TestNullableUnsignedInt64 == 1234567890123456789L).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableCharacter)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableCharacter == 'a').ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.TestNullableSignedByte == -128).ToList()
                            .Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU64)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.EnumU64 == EnumU64.SomeValue).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU32)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.EnumU32 == EnumU32.SomeValue).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU16)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.EnumU16 == EnumU16.SomeValue).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumS8)) != null)
                {
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == id && e.EnumS8 == EnumS8.SomeValue).ToList().Single());
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_query_with_null_parameters_using_any_nullable_data_type()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes { Id = 711 });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var test = context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711).ToList();
                var entity = context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711).ToList().Single();

                short? param1 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt16 == param1).ToList().Single());
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && (long?)(int?)e.TestNullableInt16 == param1).ToList()
                        .Single());

                int? param2 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt32 == param2).ToList().Single());

                long? param3 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableInt64 == param3).ToList().Single());

                double? param4 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDouble == param4).ToList().Single());

                decimal? param5 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDecimal == param5).ToList().Single());

                DateTime? param6 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDateTime == param6).ToList().Single());

                DateTimeOffset? param7 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableDateTimeOffset == param7).ToList()
                        .Single());

                //TimeSpan? param8 = null;
                //Assert.Same(
                //    entity,
                //    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableTimeSpan == param8).ToList().Single());

                float? param9 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableSingle == param9).ToList().Single());

                bool? param10 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableBoolean == param10).ToList().Single());

                sbyte? param11 = null;
                Assert.Same(
                    entity,
                    context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableByte == param11).ToList().Single());

                Enum64? param12 = null;
                Assert.Same(
                    entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum64 == param12).ToList().Single());

                Enum32? param13 = null;
                Assert.Same(
                    entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum32 == param13).ToList().Single());

                Enum16? param14 = null;
                Assert.Same(
                    entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum16 == param14).ToList().Single());

                Enum8? param15 = null;
                Assert.Same(
                    entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.Enum8 == param15).ToList().Single());

                var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));
                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt16)) != null)
                {
                    short? param16 = null;
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt16 == param16).ToList()
                            .Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt32)) != null)
                {
                    int? param17 = null;
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt32 == param17).ToList()
                            .Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableUnsignedInt64)) != null)
                {
                    long? param18 = null;
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableUnsignedInt64 == param18).ToList()
                            .Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableCharacter)) != null)
                {
                    char? param19 = null;
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableCharacter == param19).ToList()
                            .Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.TestNullableSignedByte)) != null)
                {
                    sbyte? param20 = null;
                    Assert.Same(
                        entity,
                        context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.TestNullableSignedByte == param20).ToList()
                            .Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU64)) != null)
                {
                    EnumU64? param21 = null;
                    Assert.Same(
                        entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU64 == param21).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU32)) != null)
                {
                    EnumU32? param22 = null;
                    Assert.Same(
                        entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU32 == param22).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumU16)) != null)
                {
                    EnumU16? param23 = null;
                    Assert.Same(
                        entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumU16 == param23).ToList().Single());
                }

                if (entityType.FindProperty(nameof(BuiltInNullableDataTypes.EnumS8)) != null)
                {
                    EnumS8? param24 = null;
                    Assert.Same(
                        entity, context.Set<BuiltInNullableDataTypes>().Where(e => e.Id == 711 && e.EnumS8 == param24).ToList().Single());
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_all_non_nullable_data_types()
        {
            int id = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<BuiltInDataTypes>().Add(
                    new BuiltInDataTypes
                    {
                        Id = id,
                        PartitionId = id+1,
                        TestInt16 = -1234,
                        TestInt32 = -123456789,
                        TestInt64 = -1234567890123456789L,
                        TestDouble = -1.23456789,
                        TestDecimal = -1234567890.01M,
                        TestDateTime = DateTime.Parse("2000-1-1 12:34:56"),
                        //TestDateTimeOffset = DateTime.Parse("2000-1-1 12:34:56"),
                        //TestTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                        TestSingle = -1.234F,
                        TestBoolean = true,
                        TestByte = 127,
                        TestUnsignedInt16 = 1234,
                        TestUnsignedInt32 = 1234565789,
                        TestUnsignedInt64 = 1234567890123456789L,
                        TestCharacter = 'a',
                        TestSignedByte = -128,
                        Enum64 = Enum64.SomeValue,
                        Enum32 = Enum32.SomeValue,
                        Enum16 = Enum16.SomeValue,
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInDataTypes>().Where(e => e.Id == id).ToList().FirstOrDefault();

                var entityType = context.Model.FindEntityType(typeof(BuiltInDataTypes));
                AssertEqualIfMapped(entityType, (short)-1234, () => dt.TestInt16);
                AssertEqualIfMapped(entityType, -123456789, () => dt.TestInt32);
                AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.TestInt64);
                AssertEqualIfMapped(entityType, -1.23456789, () => dt.TestDouble);
                AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.TestDecimal);
                AssertEqualIfMapped(entityType, DateTime.Parse("2000-1-1 12:34:56"), () => dt.TestDateTime);
                //AssertEqualIfMapped(
                //    entityType, new DateTimeOffset(DateTime.Parse("2000-1-1 12:34:56"), TimeSpan.FromHours(-8.0)),
                //    () => dt.TestDateTimeOffset);
                //AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TestTimeSpan);
                AssertEqualIfMapped(entityType, -1.234F, () => dt.TestSingle);
                AssertEqualIfMapped(entityType, true, () => dt.TestBoolean);
                AssertEqualIfMapped(entityType, (sbyte)127, () => dt.TestByte);
                AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
                AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
                AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
                AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
                AssertEqualIfMapped(entityType, (short)1234, () => dt.TestUnsignedInt16);
                AssertEqualIfMapped(entityType, 1234565789, () => dt.TestUnsignedInt32);
                AssertEqualIfMapped(entityType, 1234567890123456789L, () => dt.TestUnsignedInt64);
                AssertEqualIfMapped(entityType, 'a', () => dt.TestCharacter);
                AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.TestSignedByte);
                AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
                AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
                AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
                AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_with_max_length_set()
        {
            const string shortString = "Sky";
            var shortBinary = new byte[] { 8, 8, 7, 8, 7 };

            var longString = new string('X', Fixture.LongStringLength);
            var longBinary = new byte[Fixture.LongStringLength];
            for (var i = 0; i < longBinary.Length; i++)
            {
                longBinary[i] = (byte)i;
            }

            using (var context = CreateContext())
            {
                context.Set<MaxLengthDataTypes>().Add(
                    new MaxLengthDataTypes
                    {
                        Id = 79,
                        String3 = shortString,
                        ByteArray5 = shortBinary,
                        String9000 = longString,
                        ByteArray9000 = longBinary
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<MaxLengthDataTypes>().Where(e => e.Id == 79).ToList().Single();

                Assert.Equal(shortString, dt.String3);
                Assert.Equal(shortBinary, dt.ByteArray5);
                Assert.Equal(longString, dt.String9000);
                Assert.Equal(longBinary, dt.ByteArray9000);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_binary_key()
        {
            if (!Fixture.SupportsBinaryKeys)
            {
                return;
            }
            var id1 = new Random().Next();
            var id2 = new Random().Next();
            var id3 = new Random().Next();
            var id4 = new Random().Next();
            var id5 = new Random().Next();
            var id6 = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<BinaryKeyDataType>().AddRange(
                    new BinaryKeyDataType { Id = id1, Ex = "X1" },
                    new BinaryKeyDataType { Id = id2, Ex = "X3" },
                    new BinaryKeyDataType { Id = id3, Ex = "X2" });

                context.Set<BinaryForeignKeyDataType>().AddRange(
                    new BinaryForeignKeyDataType { Id = id4, BinaryKeyDataTypeId = id2 },
                    new BinaryForeignKeyDataType { Id = id5, BinaryKeyDataTypeId = id1 },
                    new BinaryForeignKeyDataType { Id = id6, BinaryKeyDataTypeId = id3 });

                Assert.Equal(6, context.SaveChanges());
            }

            BinaryKeyDataType QueryByBinaryKey(DbContext context, int id)
            {
                return context
                    .Set<BinaryKeyDataType>()
                    .Include(e => e.Dependents)
                    .Where(e => e.Id == id)
                    .ToList().Single();
            }

            using (var context = CreateContext())
            {
                var entity1 = QueryByBinaryKey(context, id1);
                Assert.Equal(id1, entity1.Id);
                Assert.Equal(1, entity1.Dependents.Count);

                var entity2 = QueryByBinaryKey(context, id2);
                Assert.Equal(id2, entity2.Id);
                Assert.Equal(1, entity2.Dependents.Count);

                var entity3 = QueryByBinaryKey(context, id3);
                Assert.Equal(id3, entity3.Id);
                Assert.Equal(1, entity3.Dependents.Count);

                entity3.Ex = "Xx1";
                entity2.Ex = "Xx3";
                entity1.Ex = "Xx7";

                entity1.Dependents.Single().BinaryKeyDataTypeId = id3;

                entity2.Dependents.Single().BinaryKeyDataTypeId = id3;

                context.SaveChanges();
            }

            using (var context = CreateContext())
            {
                var entity1 = QueryByBinaryKey(context, id1);
                Assert.Equal("Xx7", entity1.Ex);
                Assert.Equal(0, entity1.Dependents.Count);

                var entity2 = QueryByBinaryKey(context, id2);
                Assert.Equal("Xx3", entity2.Ex);
                Assert.Equal(0, entity2.Dependents.Count);

                var entity3 = QueryByBinaryKey(context, id3);
                Assert.Equal("Xx1", entity3.Ex);
                Assert.Equal(3, entity3.Dependents.Count);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_null_binary_foreign_key()
        {
            int id = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<BinaryForeignKeyDataType>().Add(
                    new BinaryForeignKeyDataType { Id = id });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<BinaryForeignKeyDataType>().Where(e => e.Id == id).ToList().Single();

                Assert.Null(entity.BinaryKeyDataTypeId);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_string_key()
        {
            using (var context = CreateContext())
            {
                var principal = context.Set<StringKeyDataType>().Add(
                    new StringKeyDataType { Id = "Gumball!" }).Entity;

                var dependent = context.Set<StringForeignKeyDataType>().Add(
                    new StringForeignKeyDataType { Id = 77, StringKeyDataTypeId = "Gumball!" }).Entity;

                Assert.Same(principal, dependent.Principal);

                Assert.Equal(2, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context
                    .Set<StringKeyDataType>()
                    .Include(e => e.Dependents)
                    .Where(e => e.Id == "Gumball!")
                    .ToList().Single();

                Assert.Equal("Gumball!", entity.Id);
                Assert.Equal("Gumball!", entity.Dependents.First().StringKeyDataTypeId);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_with_null_string_foreign_key()
        {
            using (var context = CreateContext())
            {
                context.Set<StringForeignKeyDataType>().Add(
                    new StringForeignKeyDataType { Id = 78 });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<StringForeignKeyDataType>().Where(e => e.Id == 78).ToList().Single();

                Assert.Null(entity.StringKeyDataTypeId);
            }
        }

        private void AssertEqualIfMapped<T>(IEntityType entityType, T expected, Expression<Func<T>> actualExpression)
        {
            if (entityType.FindProperty(((MemberExpression)actualExpression.Body).Member.Name) != null)
            {
                var actual = actualExpression.Compile()();
                var type = UnwrapNullableEnumType(typeof(T));
                //if (IsSignedInteger(type))
                //{
                //    Assert.True(Equal(Convert.ToInt64(expected), Convert.ToInt64(actual)), $"Expected:\t{expected}\r\nActual:\t{actual}");
                //}
                ////else if (IsUnsignedInteger(type))
                ////{
                ////    Assert.True(Equal(Convert.ToUInt64(expected), Convert.ToUInt64(actual)), $"Expected:\t{expected}\r\nActual:\t{actual}");
                ////}
                //else
                //{
                //    Assert.Equal(expected, actual);
                //}
                Assert.Equal(expected, actual);
            }
        }

        private bool Equal(long left, long right)
        {
            if (left >= 0
                && right >= 0)
            {
                return Equal((long)left, (long)right);
            }

            if (left < 0
                && right < 0)
            {
                return Equal((long)-left, (long)-right);
            }

            return false;
        }

        private bool Equal1(long left, long right)
        {
            if (Fixture.IntegerPrecision < 64)
            {
                var largestPrecise = 1l << Fixture.IntegerPrecision;
                while (left > largestPrecise)
                {
                    left >>= 1;
                    right >>= 1;
                }
            }

            return left == right;
        }

        private static Type UnwrapNullableType(Type type)
            => type == null ? null : Nullable.GetUnderlyingType(type) ?? type;

        public static Type UnwrapNullableEnumType(Type type)
        {
            var underlyingNonNullableType = UnwrapNullableType(type);
            if (!underlyingNonNullableType.IsEnum)
            {
                return underlyingNonNullableType;
            }

            return Enum.GetUnderlyingType(underlyingNonNullableType);
        }

        private static bool IsSignedInteger(Type type)
            => type == typeof(int)
                || type == typeof(long)
                || type == typeof(short)
                || type == typeof(sbyte);

        private static bool IsUnsignedInteger(Type type)
            => type == typeof(sbyte)
                || type == typeof(int)
                || type == typeof(long)
                || type == typeof(short)
                || type == typeof(char);

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_null()
        {
            int id = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes { Id = id, PartitionId = id });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id == id).ToList().Single();

                Assert.Null(dt.TestString);
                Assert.Null(dt.TestByteArray);
                Assert.Null(dt.TestNullableInt16);
                Assert.Null(dt.TestNullableInt32);
                Assert.Null(dt.TestNullableInt64);
                Assert.Null(dt.TestNullableDouble);
                Assert.Null(dt.TestNullableDecimal);
                Assert.Null(dt.TestNullableDateTime);
                Assert.Null(dt.TestNullableDateTimeOffset);
                Assert.Null(dt.TestNullableTimeSpan);
                Assert.Null(dt.TestNullableSingle);
                Assert.Null(dt.TestNullableBoolean);
                Assert.Null(dt.TestNullableByte);
                Assert.Null(dt.TestNullableUnsignedInt16);
                Assert.Null(dt.TestNullableUnsignedInt32);
                Assert.Null(dt.TestNullableUnsignedInt64);
                Assert.Null(dt.TestNullableCharacter);
                Assert.Null(dt.TestNullableSignedByte);
                Assert.Null(dt.Enum64);
                Assert.Null(dt.Enum32);
                Assert.Null(dt.Enum16);
                Assert.Null(dt.Enum8);
                Assert.Null(dt.EnumU64);
                Assert.Null(dt.EnumU32);
                Assert.Null(dt.EnumU16);
                Assert.Null(dt.EnumS8);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_non_null()
        {
            int id = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes
                    {
                        Id = id,
                        PartitionId = id,
                        TestString = "TestString",
                        TestByteArray = new byte[] { 10, 9, 8, 7, 6 },
                        TestNullableInt16 = -1234,
                        TestNullableInt32 = -123456789,
                        TestNullableInt64 = -1234567890123456789L,
                        TestNullableDouble = -1.23456789,
                        TestNullableDecimal = -1234567890.01M,
                        TestNullableDateTime = DateTime.Parse("2000-1-1 12:34:56"),
                        TestNullableDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                        TestNullableTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                        TestNullableSingle = -1.234F,
                        TestNullableBoolean = false,
                        TestNullableByte = 127,
                        TestNullableUnsignedInt16 = 1234,
                        TestNullableUnsignedInt32 = 1234565789,
                        TestNullableUnsignedInt64 = 1234567890123456789L,
                        TestNullableCharacter = 'a',
                        TestNullableSignedByte = -128,
                        Enum64 = Enum64.SomeValue,
                        Enum32 = Enum32.SomeValue,
                        Enum16 = Enum16.SomeValue,
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id == id).ToList().Single();

                var entityType = context.Model.FindEntityType(typeof(BuiltInNullableDataTypes));
                AssertEqualIfMapped(entityType, "TestString", () => dt.TestString);
                AssertEqualIfMapped(entityType, new byte[] { 10, 9, 8, 7, 6 }, () => dt.TestByteArray);
                AssertEqualIfMapped(entityType, (short)-1234, () => dt.TestNullableInt16);
                AssertEqualIfMapped(entityType, -123456789, () => dt.TestNullableInt32);
                AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.TestNullableInt64);
                AssertEqualIfMapped(entityType, -1.23456789, () => dt.TestNullableDouble);
                AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.TestNullableDecimal);
                AssertEqualIfMapped(entityType, DateTime.Parse("01/01/2000 12:34:56"), () => dt.TestNullableDateTime);
                AssertEqualIfMapped(
                    entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                    () => dt.TestNullableDateTimeOffset);
                //AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TestNullableTimeSpan);
                AssertEqualIfMapped(entityType, -1.234F, () => dt.TestNullableSingle);
                AssertEqualIfMapped(entityType, false, () => dt.TestNullableBoolean);
                AssertEqualIfMapped(entityType, (sbyte)127, () => dt.TestNullableByte);
                AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
                AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
                AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
                AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
                AssertEqualIfMapped(entityType, (short)1234, () => dt.TestNullableUnsignedInt16);
                AssertEqualIfMapped(entityType, 1234565789, () => dt.TestNullableUnsignedInt32);
                AssertEqualIfMapped(entityType, 1234567890123456789L, () => dt.TestNullableUnsignedInt64);
                AssertEqualIfMapped(entityType, 'a', () => dt.TestNullableCharacter);
                AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.TestNullableSignedByte);
                AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
                AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
                AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
                AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_object_backed_data_types()
        {
            int id = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<ObjectBackedDataTypes>().Add(
                    new ObjectBackedDataTypes
                    {
                        Id = id,
                        PartitionId = id,
                        String = "TestString",
                        Bytes = new byte[] { 10, 9, 8, 7, 6 },
                        Int16 = -1234,
                        Int32 = -123456789,
                        Int64 = -1234567890123456789L,
                        Double = -1.23456789,
                        Decimal = -1234567890.01M,
                        DateTime = DateTime.Parse("01/01/2000 12:34:56"),
                        DateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                        TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                        Single = -1.234F,
                        Boolean = false,
                        Byte = 127,
                        UnsignedInt16 = 1234,
                        UnsignedInt32 = 1234565789,
                        UnsignedInt64 = 1234567890123456789L,
                        Character = 'a',
                        SignedByte = -128,
                        Enum64 = Enum64.SomeValue,
                        Enum32 = Enum32.SomeValue,
                        Enum16 = Enum16.SomeValue,
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<ObjectBackedDataTypes>().Where(ndt => ndt.Id == id).ToList().Single();

                var entityType = context.Model.FindEntityType(typeof(ObjectBackedDataTypes));
                AssertEqualIfMapped(entityType, "TestString", () => dt.String);
                AssertEqualIfMapped(entityType, new byte[] { 10, 9, 8, 7, 6 }, () => dt.Bytes);
                AssertEqualIfMapped(entityType, (short)-1234, () => dt.Int16);
                AssertEqualIfMapped(entityType, -123456789, () => dt.Int32);
                AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.Int64);
                AssertEqualIfMapped(entityType, -1.23456789, () => dt.Double);
                AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.Decimal);
                AssertEqualIfMapped(entityType, DateTime.Parse("01/01/2000 12:34:56"), () => dt.DateTime);
                AssertEqualIfMapped(
                    entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                    () => dt.DateTimeOffset);
                //AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TimeSpan);
                AssertEqualIfMapped(entityType, -1.234F, () => dt.Single);
                AssertEqualIfMapped(entityType, false, () => dt.Boolean);
                AssertEqualIfMapped(entityType, (sbyte)127, () => dt.Byte);
                AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
                AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
                AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
                AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
                AssertEqualIfMapped(entityType, (short)1234, () => dt.UnsignedInt16);
                AssertEqualIfMapped(entityType, 1234565789, () => dt.UnsignedInt32);
                AssertEqualIfMapped(entityType, 1234567890123456789L, () => dt.UnsignedInt64);
                AssertEqualIfMapped(entityType, 'a', () => dt.Character);
                AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.SignedByte);
                AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
                AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
                AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
                AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_nullable_backed_data_types()
        {
            int id = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<NullableBackedDataTypes>().Add(
                    new NullableBackedDataTypes
                    {
                        Id = id,
                        PartitionId = id,
                        Int16 = -1234,
                        Int32 = -123456789,
                        Int64 = -1234567890123456789L,
                        Double = -1.23456789,
                        Decimal = -1234567890.01M,
                        DateTime = DateTime.Parse("2000-01-01 12:34:56"),
                        DateTimeOffset = new DateTimeOffset(DateTime.Parse("2000-01-01 12:34:56"), TimeSpan.FromHours(-8)),
                        TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                        Single = -1.234F,
                        Boolean = false,
                        Byte = 127,
                        UnsignedInt16 = 1234,
                        UnsignedInt32 = 1234565789,
                        UnsignedInt64 = 1234567890123456789L,
                        Character = 'a',
                        SignedByte = -128,
                        Enum64 = Enum64.SomeValue,
                        Enum32 = Enum32.SomeValue,
                        Enum16 = Enum16.SomeValue,
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<NullableBackedDataTypes>().Where(ndt => ndt.Id == id).ToList().Single();

                var entityType = context.Model.FindEntityType(typeof(NullableBackedDataTypes));
                AssertEqualIfMapped(entityType, (short)-1234, () => dt.Int16);
                AssertEqualIfMapped(entityType, -123456789, () => dt.Int32);
                AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.Int64);
                AssertEqualIfMapped(entityType, -1.23456789, () => dt.Double);
                AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.Decimal);
                AssertEqualIfMapped(entityType, DateTime.Parse("01/01/2000 12:34:56"), () => dt.DateTime);
                AssertEqualIfMapped(
                    entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                    () => dt.DateTimeOffset);
                //AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TimeSpan);
                AssertEqualIfMapped(entityType, -1.234F, () => dt.Single);
                AssertEqualIfMapped(entityType, false, () => dt.Boolean);
                AssertEqualIfMapped(entityType, (sbyte)127, () => dt.Byte);
                AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
                AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
                AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
                AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
                AssertEqualIfMapped(entityType, (short)1234, () => dt.UnsignedInt16);
                AssertEqualIfMapped(entityType, 1234565789, () => dt.UnsignedInt32);
                AssertEqualIfMapped(entityType, 1234567890123456789L, () => dt.UnsignedInt64);
                AssertEqualIfMapped(entityType, 'a', () => dt.Character);
                AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.SignedByte);
                AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
                AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
                AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
                AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_and_read_back_non_nullable_backed_data_types()
        {
            int id = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<NonNullableBackedDataTypes>().Add(
                    new NonNullableBackedDataTypes
                    {
                        Id = id,
                        PartitionId = id,
                        Int16 = -1234,
                        Int32 = -123456789,
                        Int64 = -1234567890123456789L,
                        Double = -1.23456789,
                        Decimal = -1234567890.01M,
                        DateTime = DateTime.Parse("2000-01-01 12:34:56"),
                        //DateTimeOffset = DateTime.Parse("2000-01-01 12:34:56"),
                        //TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                        Single = -1.234F,
                        Boolean = true,
                        Byte = 127,
                        UnsignedInt16 = 1234,
                        UnsignedInt32 = 1234565789,
                        UnsignedInt64 = 1234567890123456789L,
                        Character = 'a',
                        SignedByte = -128,
                        Enum64 = Enum64.SomeValue,
                        Enum32 = Enum32.SomeValue,
                        Enum16 = Enum16.SomeValue,
                        Enum8 = Enum8.SomeValue,
                        EnumU64 = EnumU64.SomeValue,
                        EnumU32 = EnumU32.SomeValue,
                        EnumU16 = EnumU16.SomeValue,
                        EnumS8 = EnumS8.SomeValue
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<NonNullableBackedDataTypes>().Where(ndt => ndt.Id == id).ToList().Single();

                var entityType = context.Model.FindEntityType(typeof(NonNullableBackedDataTypes));
                AssertEqualIfMapped(entityType, (short)-1234, () => dt.Int16);
                AssertEqualIfMapped(entityType, -123456789, () => dt.Int32);
                AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.Int64);
                AssertEqualIfMapped(entityType, -1234567890123456789L, () => dt.Int64);
                AssertEqualIfMapped(entityType, -1.23456789, () => dt.Double);
                AssertEqualIfMapped(entityType, -1234567890.01M, () => dt.Decimal);
                AssertEqualIfMapped(entityType, DateTime.Parse("01/01/2000 12:34:56"), () => dt.DateTime);
                //AssertEqualIfMapped(
                //    entityType, new DateTimeOffset(DateTime.Parse("01/01/2000 12:34:56"), TimeSpan.FromHours(-8.0)),
                //    () => dt.DateTimeOffset);
                //AssertEqualIfMapped(entityType, new TimeSpan(0, 10, 9, 8, 7), () => dt.TimeSpan);
                AssertEqualIfMapped(entityType, -1.234F, () => dt.Single);
                AssertEqualIfMapped(entityType, true, () => dt.Boolean);
                AssertEqualIfMapped(entityType, (sbyte)127, () => dt.Byte);
                AssertEqualIfMapped(entityType, Enum64.SomeValue, () => dt.Enum64);
                AssertEqualIfMapped(entityType, Enum32.SomeValue, () => dt.Enum32);
                AssertEqualIfMapped(entityType, Enum16.SomeValue, () => dt.Enum16);
                AssertEqualIfMapped(entityType, Enum8.SomeValue, () => dt.Enum8);
                AssertEqualIfMapped(entityType, (short)1234, () => dt.UnsignedInt16);
                AssertEqualIfMapped(entityType, 1234565789, () => dt.UnsignedInt32);
                AssertEqualIfMapped(entityType, 1234567890123456789L, () => dt.UnsignedInt64);
                AssertEqualIfMapped(entityType, 'a', () => dt.Character);
                AssertEqualIfMapped(entityType, (sbyte)-128, () => dt.SignedByte);
                AssertEqualIfMapped(entityType, EnumU64.SomeValue, () => dt.EnumU64);
                AssertEqualIfMapped(entityType, EnumU32.SomeValue, () => dt.EnumU32);
                AssertEqualIfMapped(entityType, EnumU16.SomeValue, () => dt.EnumU16);
                AssertEqualIfMapped(entityType, EnumS8.SomeValue, () => dt.EnumS8);
            }
        }

        [ConditionalFact]
        public virtual void Can_read_back_mapped_enum_from_collection_first_or_default()
        {
            using var context = CreateContext();
            var query = from animal in context.Set<Animal>()
                        select new { animal.Id, animal.IdentificationMethods.FirstOrDefault().Method };

            var result = query.SingleOrDefault();
            Assert.Equal(IdentificationMethod.EarTag, result.Method);
        }

        [ConditionalFact]
        public virtual void Can_read_back_bool_mapped_as_int_through_navigation()
        {
            using var context = CreateContext();
            var query = from animal in context.Set<Animal>()
                        where animal.Details != null
                        select new { animal.Details.BoolField };

            var result = Assert.Single(query.ToList());
            Assert.True(result.BoolField);
        }

        [ConditionalFact]
        public virtual void Can_compare_enum_to_constant()
        {
            using var context = CreateContext();
            var query = context.Set<AnimalIdentification>()
                .Where(a => a.Method == IdentificationMethod.EarTag)
                .ToList();

            var result = Assert.Single(query);
            Assert.Equal(IdentificationMethod.EarTag, result.Method);
        }

        [ConditionalFact]
        public virtual void Can_compare_enum_to_parameter()
        {
            var method = IdentificationMethod.EarTag;
            using var context = CreateContext();
            var query = context.Set<AnimalIdentification>()
                .Where(a => a.Method == method)
                .ToList();

            var result = Assert.Single(query);
            Assert.Equal(IdentificationMethod.EarTag, result.Method);
        }

        [ConditionalFact]
        public virtual void Object_to_string_conversion()
        {
            using var context = CreateContext();
            var expected = context.Set<BuiltInDataTypes>()
                .Where(e => e.Id == 13)
                .AsEnumerable()
                .Select(
                    b => new
                    {
                        Ssbyte = b.TestSignedByte.ToString(),
                        Byte = b.TestByte.ToString(),
                        Short = b.TestInt16.ToString(),
                        Ushort = b.TestUnsignedInt16.ToString(),
                        Int = b.TestInt32.ToString(),
                        Uint = b.TestUnsignedInt32.ToString(),
                        Long = b.TestInt64.ToString(),
                        Ulong = b.TestUnsignedInt64.ToString(),
                        Decimal = b.TestDecimal.ToString(),
                        Char = b.TestCharacter.ToString()
                    })
                .First();

            Fixture.ListLoggerFactory.Clear();

            var query = context.Set<BuiltInDataTypes>()
                .Where(e => e.Id == 13)
                .Select(
                    b => new
                    {
                        Ssbyte = b.TestSignedByte.ToString(),
                        Byte = b.TestByte.ToString(),
                        Short = b.TestInt16.ToString(),
                        Ushort = b.TestUnsignedInt16.ToString(),
                        Int = b.TestInt32.ToString(),
                        Uint = b.TestUnsignedInt32.ToString(),
                        Long = b.TestInt64.ToString(),
                        Ulong = b.TestUnsignedInt64.ToString(),
                        Float = b.TestSingle.ToString(),
                        Double = b.TestDouble.ToString(),
                        Decimal = b.TestDecimal.ToString(),
                        Char = b.TestCharacter.ToString(),
                        DateTime = b.TestDateTime.ToString()/*,
                        DateTimeOffset = b.TestDateTimeOffset.ToString()*/
                    })
                .ToList();

            var actual = Assert.Single(query);
            Assert.Equal(expected.Ssbyte, actual.Ssbyte);
            Assert.Equal(expected.Byte, actual.Byte);
            Assert.Equal(expected.Short, actual.Short);
            Assert.Equal(expected.Ushort, actual.Ushort);
            Assert.Equal(expected.Int, actual.Int);
            Assert.Equal(expected.Uint, actual.Uint);
            Assert.Equal(expected.Long, actual.Long);
            Assert.Equal(expected.Ulong, actual.Ulong);
            Assert.Equal(expected.Decimal, actual.Decimal);
            Assert.Equal(expected.Char, actual.Char);
        }

        [ConditionalFact]
        public virtual void Optional_datetime_reading_null_from_database()
        {
            using var context = CreateContext();
            var expected = context.Set<DateTimeEnclosure>().ToList()
                .Select(e => new { DT = e.DateTimeOffset == null ? (DateTime?)null : e.DateTimeOffset.Value.DateTime.Date }).ToList();

            var actual = context.Set<DateTimeEnclosure>()
                .Select(e => new { DT = e.DateTimeOffset == null ? (DateTime?)null : e.DateTimeOffset.Value.DateTime.Date }).ToList();

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].DT, actual[i].DT);
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_query_multiline_string()
        {
            using var context = CreateContext();

            Assert.Equal(Fixture.ReallyLargeString, Assert.Single(context.Set<StringEnclosure>()).Value);
        }

        public abstract class BuiltInDataTypesFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
        {
            protected override string StoreName { get; } = "BuiltInDataTypes";

            public virtual int LongStringLength
                => 9000;

            public virtual string ReallyLargeString
                => string.Join("", Enumerable.Repeat(Environment.NewLine, 1001));

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<BinaryKeyDataType>();
                modelBuilder.Entity<StringKeyDataType>();
                modelBuilder.Entity<BuiltInDataTypes>(
                    eb =>
                    {
                        eb.HasData(
                            new BuiltInDataTypes
                            {
                                Id = 13,
                                PartitionId = 1,
                                TestInt16 = -1234,
                                TestInt32 = -123456789,
                                TestInt64 = -1234567890123456789L,
                                TestDouble = -1.23456789,
                                TestDecimal = -1234567890.01M,
                                TestDateTime = DateTime.Parse("2000-1-1 12:34:56"),
                                //TestDateTimeOffset = DateTime.Parse("2000-1-1 12:34:56"),
                                //TestTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                                TestSingle = -1.234F,
                                TestBoolean = true,
                                TestByte = 127,
                                TestUnsignedInt16 = 1234,
                                TestUnsignedInt32 = 1234565789,
                                TestUnsignedInt64 = 1234567890123456789L,
                                TestCharacter = 'a',
                                TestSignedByte = -128,
                                Enum64 = Enum64.SomeValue,
                                Enum32 = Enum32.SomeValue,
                                Enum16 = Enum16.SomeValue,
                                Enum8 = Enum8.SomeValue,
                                EnumU64 = EnumU64.SomeValue,
                                EnumU32 = EnumU32.SomeValue,
                                EnumU16 = EnumU16.SomeValue,
                                EnumS8 = EnumS8.SomeValue
                            });
                        eb.Property(e => e.Id).ValueGeneratedNever();
                    });
                modelBuilder.Entity<BuiltInDataTypesShadow>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<BuiltInNullableDataTypes>(
                    eb =>
                    {
                        eb.HasData(
                            new BuiltInNullableDataTypes
                            {
                                Id = 13,
                                PartitionId = 1,
                                TestNullableInt16 = -1234,
                                TestNullableInt32 = -123456789,
                                TestNullableInt64 = -1234567890123456789L,
                                TestNullableDouble = -1.23456789,
                                TestNullableDecimal = -1234567890.01M,
                                TestNullableDateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                                TestNullableTimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                                TestNullableSingle = -1.234F,
                                TestNullableBoolean = true,
                                TestNullableByte = 127,
                                TestNullableUnsignedInt16 = 1234,
                                TestNullableUnsignedInt32 = 1234565789,
                                TestNullableUnsignedInt64 = 1234567890123456789L,
                                TestNullableCharacter = 'a',
                                TestNullableSignedByte = -128,
                                Enum64 = Enum64.SomeValue,
                                Enum32 = Enum32.SomeValue,
                                Enum16 = Enum16.SomeValue,
                                Enum8 = Enum8.SomeValue,
                                EnumU64 = EnumU64.SomeValue,
                                EnumU32 = EnumU32.SomeValue,
                                EnumU16 = EnumU16.SomeValue,
                                EnumS8 = EnumS8.SomeValue
                            });
                        eb.Property(e => e.Id).ValueGeneratedNever();
                    });
                modelBuilder.Entity<BuiltInNullableDataTypesShadow>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<BinaryForeignKeyDataType>().Property(e => e.Id).ValueGeneratedNever();
                modelBuilder.Entity<StringForeignKeyDataType>().Property(e => e.Id).ValueGeneratedNever();
                MakeRequired<BuiltInDataTypes>(modelBuilder);
                MakeRequired<BuiltInDataTypesShadow>(modelBuilder);

                modelBuilder.Entity<MaxLengthDataTypes>(
                    b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.ByteArray5).HasMaxLength(5);
                        b.Property(e => e.String3).HasMaxLength(3);
                        b.Property(e => e.ByteArray9000).HasMaxLength(LongStringLength);
                        b.Property(e => e.String9000).HasMaxLength(LongStringLength);
                    });

                modelBuilder.Entity<UnicodeDataTypes>(
                    b =>
                    {
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property(e => e.StringAnsi).IsUnicode(false);
                        b.Property(e => e.StringAnsi3).HasMaxLength(3).IsUnicode(false);
                        b.Property(e => e.StringAnsi9000).IsUnicode(false).HasMaxLength(LongStringLength);
                        b.Property(e => e.StringUnicode).IsUnicode();
                    });

                modelBuilder.Entity<BuiltInDataTypesShadow>(
                    b =>
                    {
                        foreach (var property in modelBuilder.Entity<BuiltInDataTypes>().Metadata
                            .GetProperties().Where(p => p.Name != "Id"))
                        {
                            b.Property(property.ClrType, property.Name);
                        }
                    });

                modelBuilder.Entity<BuiltInNullableDataTypesShadow>(
                    b =>
                    {
                        foreach (var property in modelBuilder.Entity<BuiltInNullableDataTypes>().Metadata
                            .GetProperties().Where(p => p.Name != "Id"))
                        {
                            b.Property(property.ClrType, property.Name);
                        }
                    });

                modelBuilder.Entity<EmailTemplate>(
                    b =>
                    {
                        b.HasData(
                            new EmailTemplate
                            {
                                Id = Guid.Parse("3C56082A-005A-4FFB-A9CF-F5EBD641E07D"),
                                TemplateType = EmailTemplateType.PasswordResetRequest
                            });
                    });

                modelBuilder.Entity<ObjectBackedDataTypes>()
                    .HasData(
                        new ObjectBackedDataTypes
                        {
                            Id = 13,
                            PartitionId = 1,
                            String = "string",
                            Bytes = null,//new byte[] { 4, 20 },
                            Int16 = -1234,
                            Int32 = -123456789,
                            Int64 = -1234567890123456789L,
                            Double = -1.23456789,
                            Decimal = -1234567890.01M,
                            DateTime = new DateTime(1973, 9, 3),
                            DateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                            TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                            Single = -1.234F,
                            Boolean = true,
                            Byte = 127,
                            UnsignedInt16 = 1234,
                            UnsignedInt32 = 1234565789,
                            UnsignedInt64 = 1234567890123456789L,
                            Character = 'a',
                            SignedByte = -128,
                            Enum64 = Enum64.SomeValue,
                            Enum32 = Enum32.SomeValue,
                            Enum16 = Enum16.SomeValue,
                            Enum8 = Enum8.SomeValue,
                            EnumU64 = EnumU64.SomeValue,
                            EnumU32 = EnumU32.SomeValue,
                            EnumU16 = EnumU16.SomeValue,
                            EnumS8 = EnumS8.SomeValue
                        });

                modelBuilder.Entity<NullableBackedDataTypes>()
                    .HasData(
                        new NullableBackedDataTypes
                        {
                            Id = 13,
                            PartitionId = 1,
                            Int16 = -1234,
                            Int32 = -123456789,
                            Int64 = -1234567890123456789L,
                            Double = -1.23456789,
                            Decimal = -1234567890.01M,
                            DateTime = new DateTime(1973, 9, 3),
                            DateTimeOffset = new DateTimeOffset(new DateTime(), TimeSpan.FromHours(-8.0)),
                            TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                            Single = -1.234F,
                            Boolean = true,
                            Byte = 127,
                            UnsignedInt16 = 1234,
                            UnsignedInt32 = 1234565789,
                            UnsignedInt64 = 1234567890123456789L,
                            Character = 'a',
                            SignedByte = -128,
                            Enum64 = Enum64.SomeValue,
                            Enum32 = Enum32.SomeValue,
                            Enum16 = Enum16.SomeValue,
                            Enum8 = Enum8.SomeValue,
                            EnumU64 = EnumU64.SomeValue,
                            EnumU32 = EnumU32.SomeValue,
                            EnumU16 = EnumU16.SomeValue,
                            EnumS8 = EnumS8.SomeValue
                        });

                modelBuilder.Entity<NonNullableBackedDataTypes>()
                    .HasData(
                        new NonNullableBackedDataTypes
                        {
                            Id = 13,
                            PartitionId = 1,
                            Int16 = -1234,
                            Int32 = -123456789,
                            Int64 = -1234567890123456789L,
                            Double = -1.23456789,
                            Decimal = -1234567890.01M,
                            DateTime = new DateTime(1973, 9, 3),
                            //DateTimeOffset = new DateTime(),
                            //TimeSpan = new TimeSpan(0, 10, 9, 8, 7),
                            Single = -1.234F,
                            Boolean = true,
                            Byte = 127,
                            UnsignedInt16 = 1234,
                            UnsignedInt32 = 1234565789,
                            UnsignedInt64 = 1234567890123456789L,
                            Character = 'a',
                            SignedByte = -128,
                            Enum64 = Enum64.SomeValue,
                            Enum32 = Enum32.SomeValue,
                            Enum16 = Enum16.SomeValue,
                            Enum8 = Enum8.SomeValue,
                            EnumU64 = EnumU64.SomeValue,
                            EnumU32 = EnumU32.SomeValue,
                            EnumU16 = EnumU16.SomeValue,
                            EnumS8 = EnumS8.SomeValue
                        });

                modelBuilder.Entity<Animal>()
                    .HasData(
                        new Animal { Id = 1 });

                modelBuilder.Entity<AnimalDetails>()
                    .HasData(
                        new AnimalDetails
                        {
                            Id = 1,
                            AnimalId = 1,
                            BoolField = true
                        });

                modelBuilder.Entity<AnimalIdentification>()
                    .HasData(
                        new AnimalIdentification
                        {
                            Id = 1,
                            AnimalId = 1,
                            Method = IdentificationMethod.EarTag
                        });

                modelBuilder.Entity<DateTimeEnclosure>()
                    .HasData(
                        new DateTimeEnclosure { Id = 1, DateTimeOffset = new DateTimeOffset(2020, 3, 12, 1, 1, 1, new TimeSpan(3, 0, 0)) },
                        new DateTimeEnclosure { Id = 2 });

                modelBuilder.Entity<StringEnclosure>()
                    .HasData(
                        new StringEnclosure
                        {
                            Id = 1/*,
                            Value = ReallyLargeString*/
                        });
            }

            protected static void MakeRequired<TEntity>(ModelBuilder modelBuilder)
                where TEntity : class
            {
                foreach (var property in modelBuilder.Entity<TEntity>().Metadata.GetDeclaredProperties())
                {
                    property.IsNullable = false;
                }
            }

            public abstract bool StrictEquality { get; }

            public virtual int IntegerPrecision
                => 19;

            public abstract bool SupportsAnsi { get; }

            public abstract bool SupportsUnicodeToAnsiConversion { get; }

            public abstract bool SupportsLargeStringComparisons { get; }

            public abstract bool SupportsBinaryKeys { get; }

            public abstract bool SupportsDecimalComparisons { get; }

            public abstract DateTime DefaultDateTime { get; }
        }

        protected class BuiltInDataTypesBase
        {
            public int Id { get; set; }
        }

        protected class BuiltInDataTypes : BuiltInDataTypesBase
        {
            public int PartitionId { get; set; }
            public short TestInt16 { get; set; }
            public int TestInt32 { get; set; }
            public long TestInt64 { get; set; }
            public double TestDouble { get; set; }
            public decimal TestDecimal { get; set; }
            public DateTime TestDateTime { get; set; }
            //[Column(TypeName = "varchar")]
            //public DateTimeOffset TestDateTimeOffset { get; set; }
            public float TestSingle { get; set; }
            public bool TestBoolean { get; set; }
            public sbyte TestByte { get; set; }
            public short TestUnsignedInt16 { get; set; }
            public int TestUnsignedInt32 { get; set; }
            public long TestUnsignedInt64 { get; set; }
            public char TestCharacter { get; set; }
            public sbyte TestSignedByte { get; set; }
            public Enum64 Enum64 { get; set; }
            public Enum32 Enum32 { get; set; }
            public Enum16 Enum16 { get; set; }
            public Enum8 Enum8 { get; set; }
            public EnumU64 EnumU64 { get; set; }
            public EnumU32 EnumU32 { get; set; }
            public EnumU16 EnumU16 { get; set; }
            public EnumS8 EnumS8 { get; set; }
        }

        protected class BuiltInDataTypesShadow : BuiltInDataTypesBase
        {
        }

        protected enum Enum64 : long
        {
            SomeValue = 1
        }

        protected enum Enum32
        {
            SomeValue = 1
        }

        protected enum Enum16 : short
        {
            SomeValue = 1
        }

        protected enum Enum8 : sbyte
        {
            SomeValue = 1
        }

        protected enum EnumU64 : long
        {
            SomeValue = long.MaxValue
        }

        protected enum EnumU32 : int
        {
            SomeValue = int.MaxValue
        }

        protected enum EnumU16 : short
        {
            SomeValue = short.MaxValue
        }

        protected enum EnumS8 : sbyte
        {
            SomeValue = sbyte.MinValue
        }

        protected class MaxLengthDataTypes
        {
            public int Id { get; set; }
            public string String3 { get; set; }
            public byte[] ByteArray5 { get; set; }
            public string String9000 { get; set; }
            public byte[] ByteArray9000 { get; set; }
        }

        protected class UnicodeDataTypes
        {
            public int Id { get; set; }
            public string StringDefault { get; set; }
            public string StringAnsi { get; set; }
            public string StringAnsi3 { get; set; }
            public string StringAnsi9000 { get; set; }
            public string StringUnicode { get; set; }
        }

        protected class BinaryKeyDataType
        {
            public int Id { get; set; }

            public string Ex { get; set; }

            public ICollection<BinaryForeignKeyDataType> Dependents { get; set; }
        }

        protected class BinaryForeignKeyDataType
        {
            public int Id { get; set; }
            public int BinaryKeyDataTypeId { get; set; }

            public BinaryKeyDataType Principal { get; set; }
        }

        protected class StringKeyDataType
        {
            public string Id { get; set; }

            public ICollection<StringForeignKeyDataType> Dependents { get; set; }
        }

        protected class StringForeignKeyDataType
        {
            public int Id { get; set; }
            public string StringKeyDataTypeId { get; set; }

            public StringKeyDataType Principal { get; set; }
        }

        protected class BuiltInNullableDataTypesBase
        {
            public int Id { get; set; }
        }

        protected class BuiltInNullableDataTypes : BuiltInNullableDataTypesBase
        {
            public int PartitionId { get; set; }
            public string TestString { get; set; }
            public byte[] TestByteArray { get; set; }
            public short? TestNullableInt16 { get; set; }
            public int? TestNullableInt32 { get; set; }
            public long? TestNullableInt64 { get; set; }
            public double? TestNullableDouble { get; set; }
            public decimal? TestNullableDecimal { get; set; }
            public DateTime? TestNullableDateTime { get; set; }
            public DateTimeOffset? TestNullableDateTimeOffset { get; set; }
            public TimeSpan? TestNullableTimeSpan { get; set; }
            public float? TestNullableSingle { get; set; }
            public bool? TestNullableBoolean { get; set; }
            public sbyte? TestNullableByte { get; set; }
            public short? TestNullableUnsignedInt16 { get; set; }
            public int? TestNullableUnsignedInt32 { get; set; }
            public long? TestNullableUnsignedInt64 { get; set; }
            public char? TestNullableCharacter { get; set; }
            public sbyte? TestNullableSignedByte { get; set; }

            // ReSharper disable MemberHidesStaticFromOuterClass
            public Enum64? Enum64 { get; set; }
            public Enum32? Enum32 { get; set; }
            public Enum16? Enum16 { get; set; }
            public Enum8? Enum8 { get; set; }
            public EnumU64? EnumU64 { get; set; }
            public EnumU32? EnumU32 { get; set; }
            public EnumU16? EnumU16 { get; set; }

            public EnumS8? EnumS8 { get; set; }
            // ReSharper restore MemberHidesStaticFromOuterClass
        }

        protected class BuiltInNullableDataTypesShadow : BuiltInNullableDataTypesBase
        {
        }

        protected class EmailTemplate
        {
            public Guid Id { get; set; }
            public EmailTemplateType TemplateType { get; set; }
        }

        protected enum EmailTemplateType
        {
            PasswordResetRequest = 0,
            EmailConfirmation = 1
        }

        protected class EmailTemplateDto
        {
            public Guid Id { get; set; }
            public EmailTemplateTypeDto TemplateType { get; set; }
        }

        protected enum EmailTemplateTypeDto
        {
            PasswordResetRequest = 0,
            EmailConfirmation = 1
        }

        protected class ObjectBackedDataTypes
        {
            private object _string;
            private object _sbytes;
            private object _int16;
            private object _int32;
            private object _int64;
            private object _double;
            private object _decimal;
            private object _dateTime;
            private object _dateTimeOffset;
            private object _timeSpan;
            private object _single;
            private object _boolean;
            private object _sbyte;
            private object _unsignedInt16;
            private object _unsignedInt32;
            private object _unsignedInt64;
            private object _character;
            private object _signedByte;
            private object _enum64;
            private object _enum32;
            private object _enum16;
            private object _enum8;
            private object _enumU64;
            private object _enumU32;
            private object _enumU16;
            private object _enumS8;

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int PartitionId { get; set; }

            public string String
            {
                get => (string)_string;
                set => _string = value;
            }

            public byte[] Bytes
            {
                get => (byte[])_sbytes;
                set => _sbytes = value;
            }

            public short Int16
            {
                get => (short)_int16;
                set => _int16 = value;
            }

            public int Int32
            {
                get => (int)_int32;
                set => _int32 = value;
            }

            public long Int64
            {
                get => (long)_int64;
                set => _int64 = value;
            }

            public double Double
            {
                get => (double)_double;
                set => _double = value;
            }

            public decimal Decimal
            {
                get => (decimal)_decimal;
                set => _decimal = value;
            }

            public DateTime DateTime
            {
                get => (DateTime)_dateTime;
                set => _dateTime = value;
            }

            public DateTimeOffset DateTimeOffset
            {
                get => (DateTimeOffset)_dateTimeOffset;
                set => _dateTimeOffset = value;
            }

            public TimeSpan TimeSpan
            {
                get => (TimeSpan)_timeSpan;
                set => _timeSpan = value;
            }

            public float Single
            {
                get => (float)_single;
                set => _single = value;
            }

            public bool Boolean
            {
                get => (bool)_boolean;
                set => _boolean = value;
            }

            public sbyte Byte
            {
                get => (sbyte)_sbyte;
                set => _sbyte = value;
            }

            public short UnsignedInt16
            {
                get => (short)_unsignedInt16;
                set => _unsignedInt16 = value;
            }

            public int UnsignedInt32
            {
                get => (int)_unsignedInt32;
                set => _unsignedInt32 = value;
            }

            public long UnsignedInt64
            {
                get => (long)_unsignedInt64;
                set => _unsignedInt64 = value;
            }

            public char Character
            {
                get => (char)_character;
                set => _character = value;
            }

            public sbyte SignedByte
            {
                get => (sbyte)_signedByte;
                set => _signedByte = value;
            }

            public Enum64 Enum64
            {
                get => (Enum64)_enum64;
                set => _enum64 = value;
            }

            public Enum32 Enum32
            {
                get => (Enum32)_enum32;
                set => _enum32 = value;
            }

            public Enum16 Enum16
            {
                get => (Enum16)_enum16;
                set => _enum16 = value;
            }

            public Enum8 Enum8
            {
                get => (Enum8)_enum8;
                set => _enum8 = value;
            }

            public EnumU64 EnumU64
            {
                get => (EnumU64)_enumU64;
                set => _enumU64 = value;
            }

            public EnumU32 EnumU32
            {
                get => (EnumU32)_enumU32;
                set => _enumU32 = value;
            }

            public EnumU16 EnumU16
            {
                get => (EnumU16)_enumU16;
                set => _enumU16 = value;
            }

            public EnumS8 EnumS8
            {
                get => (EnumS8)_enumS8;
                set => _enumS8 = value;
            }
        }

        protected class NullableBackedDataTypes
        {
            private short? _int16;
            private int? _int32;
            private long? _int64;
            private double? _double;
            private decimal? _decimal;
            private DateTime? _dateTime;
            private DateTimeOffset? _dateTimeOffset;
            private TimeSpan? _timeSpan;
            private float? _single;
            private bool? _boolean;
            private sbyte? _sbyte;
            private short? _unsignedInt16;
            private int? _unsignedInt32;
            private long? _unsignedInt64;
            private char? _character;
            private sbyte? _signedByte;
            private Enum64? _enum64;
            private Enum32? _enum32;
            private Enum16? _enum16;
            private Enum8? _enum8;
            private EnumU64? _enumU64;
            private EnumU32? _enumU32;
            private EnumU16? _enumU16;
            private EnumS8? _enumS8;

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int PartitionId { get; set; }

            public short Int16
            {
                get => (short)_int16;
                set => _int16 = value;
            }

            public int Int32
            {
                get => (int)_int32;
                set => _int32 = value;
            }

            public long Int64
            {
                get => (long)_int64;
                set => _int64 = value;
            }

            public double Double
            {
                get => (double)_double;
                set => _double = value;
            }

            public decimal Decimal
            {
                get => (decimal)_decimal;
                set => _decimal = value;
            }

            public DateTime DateTime
            {
                get => (DateTime)_dateTime;
                set => _dateTime = value;
            }

            public DateTimeOffset DateTimeOffset
            {
                get => (DateTimeOffset)_dateTimeOffset;
                set => _dateTimeOffset = value;
            }

            public TimeSpan TimeSpan
            {
                get => (TimeSpan)_timeSpan;
                set => _timeSpan = value;
            }

            public float Single
            {
                get => (float)_single;
                set => _single = value;
            }

            public bool Boolean
            {
                get => (bool)_boolean;
                set => _boolean = value;
            }

            public sbyte Byte
            {
                get => (sbyte)_sbyte;
                set => _sbyte = value;
            }

            public short UnsignedInt16
            {
                get => (short)_unsignedInt16;
                set => _unsignedInt16 = value;
            }

            public int UnsignedInt32
            {
                get => (int)_unsignedInt32;
                set => _unsignedInt32 = value;
            }

            public long UnsignedInt64
            {
                get => (long)_unsignedInt64;
                set => _unsignedInt64 = value;
            }

            public char Character
            {
                get => (char)_character;
                set => _character = value;
            }

            public sbyte SignedByte
            {
                get => (sbyte)_signedByte;
                set => _signedByte = value;
            }

            public Enum64 Enum64
            {
                get => (Enum64)_enum64;
                set => _enum64 = value;
            }

            public Enum32 Enum32
            {
                get => (Enum32)_enum32;
                set => _enum32 = value;
            }

            public Enum16 Enum16
            {
                get => (Enum16)_enum16;
                set => _enum16 = value;
            }

            public Enum8 Enum8
            {
                get => (Enum8)_enum8;
                set => _enum8 = value;
            }

            public EnumU64 EnumU64
            {
                get => (EnumU64)_enumU64;
                set => _enumU64 = value;
            }

            public EnumU32 EnumU32
            {
                get => (EnumU32)_enumU32;
                set => _enumU32 = value;
            }

            public EnumU16 EnumU16
            {
                get => (EnumU16)_enumU16;
                set => _enumU16 = value;
            }

            public EnumS8 EnumS8
            {
                get => (EnumS8)_enumS8;
                set => _enumS8 = value;
            }
        }

        protected class NonNullableBackedDataTypes
        {
            private short _int16;
            private int _int32;
            private long _int64;
            private double _double;
            private decimal _decimal;
            private DateTime _dateTime;
            private DateTimeOffset _dateTimeOffset;
            private TimeSpan _timeSpan;
            private float _single;
            private bool _boolean;
            private sbyte _sbyte;
            private short _unsignedInt16;
            private int _unsignedInt32;
            private long _unsignedInt64;
            private char _character;
            private sbyte _signedByte;
            private Enum64 _enum64;
            private Enum32 _enum32;
            private Enum16 _enum16;
            private Enum8 _enum8;
            private EnumU64 _enumU64;
            private EnumU32 _enumU32;
            private EnumU16 _enumU16;
            private EnumS8 _enumS8;

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public int PartitionId { get; set; }

            public short? Int16
            {
                get => _int16;
                set => _int16 = (short)value;
            }

            public int? Int32
            {
                get => _int32;
                set => _int32 = (int)value;
            }

            public long? Int64
            {
                get => _int64;
                set => _int64 = (long)value;
            }

            public double? Double
            {
                get => _double;
                set => _double = (double)value;
            }

            public decimal? Decimal
            {
                get => _decimal;
                set => _decimal = (decimal)value;
            }

            public DateTime? DateTime
            {
                get => _dateTime;
                set => _dateTime = (DateTime)value;
            }

            //public DateTimeOffset? DateTimeOffset
            //{
            //    get => _dateTimeOffset;
            //    set => _dateTimeOffset = (DateTimeOffset)value;
            //}

            //public TimeSpan? TimeSpan
            //{
            //    get => _timeSpan;
            //    set => _timeSpan = (TimeSpan)value;
            //}

            public float? Single
            {
                get => _single;
                set => _single = (float)value;
            }

            public bool? Boolean
            {
                get => _boolean;
                set => _boolean = (bool)value;
            }

            public sbyte? Byte
            {
                get => _sbyte;
                set => _sbyte = (sbyte)value;
            }

            public short? UnsignedInt16
            {
                get => _unsignedInt16;
                set => _unsignedInt16 = (short)value;
            }

            public int? UnsignedInt32
            {
                get => _unsignedInt32;
                set => _unsignedInt32 = (int)value;
            }

            public long? UnsignedInt64
            {
                get => _unsignedInt64;
                set => _unsignedInt64 = (long)value;
            }

            public char? Character
            {
                get => _character;
                set => _character = (char)value;
            }

            public sbyte? SignedByte
            {
                get => _signedByte;
                set => _signedByte = (sbyte)value;
            }

            public Enum64? Enum64
            {
                get => _enum64;
                set => _enum64 = (Enum64)value;
            }

            public Enum32? Enum32
            {
                get => _enum32;
                set => _enum32 = (Enum32)value;
            }

            public Enum16? Enum16
            {
                get => _enum16;
                set => _enum16 = (Enum16)value;
            }

            public Enum8? Enum8
            {
                get => _enum8;
                set => _enum8 = (Enum8)value;
            }

            public EnumU64? EnumU64
            {
                get => _enumU64;
                set => _enumU64 = (EnumU64)value;
            }

            public EnumU32? EnumU32
            {
                get => _enumU32;
                set => _enumU32 = (EnumU32)value;
            }

            public EnumU16? EnumU16
            {
                get => _enumU16;
                set => _enumU16 = (EnumU16)value;
            }

            public EnumS8? EnumS8
            {
                get => _enumS8;
                set => _enumS8 = (EnumS8)value;
            }
        }

        protected class Animal
        {
            public int Id { get; set; }
            public ICollection<AnimalIdentification> IdentificationMethods { get; set; }
            public AnimalDetails Details { get; set; }
        }

        protected class AnimalDetails
        {
            public int Id { get; set; }
            public int? AnimalId { get; set; }

            [Column(TypeName = "int")]
            public bool BoolField { get; set; }
        }

        protected class AnimalIdentification
        {
            public int Id { get; set; }
            public int AnimalId { get; set; }
            public IdentificationMethod Method { get; set; }
        }

        protected enum IdentificationMethod
        {
            Notch,
            EarTag,
            Rfid
        }

        protected class DateTimeEnclosure
        {
            public int Id { get; set; }
            public DateTimeOffset? DateTimeOffset { get; set; }
        }

        protected class StringEnclosure
        {
            public int Id { get; set; }

            public string Value { get; set; }
        }
    }
}
