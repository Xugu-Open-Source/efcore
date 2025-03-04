using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EntityFrameworkCore.XuGu.Infrastructure;
using EntityFrameworkCore.XuGu.Tests;
using EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace EntityFrameworkCore.XuGu.FunctionalTests
{
    [SupportedServerVersionCondition(nameof(ServerVersionSupport.Json))]
    public class BuiltInDataTypesXGTest :
        XGBuiltInDataTypesTestBase<BuiltInDataTypesXGTest.BuiltInDataTypesXGFixture>
    {
        public BuiltInDataTypesXGTest(BuiltInDataTypesXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void Object_to_string_conversion()
        {
            using var context = CreateContext();
            var expected = context.Set<BuiltInDataTypes>()
                .Where(e => e.Id == 13)
                .AsEnumerable()
                .Select(
                    b => new
                    {
                        Sbyte = b.TestSignedByte.ToString(),
                        Byte = b.TestByte.ToString(),
                        Short = b.TestInt16.ToString(),
                        Ushort = b.TestUnsignedInt16.ToString(),
                        Int = b.TestInt32.ToString(),
                        Uint = b.TestUnsignedInt32.ToString(),
                        Long = b.TestInt64.ToString(),
                        Ulong = b.TestUnsignedInt64.ToString(),
                        Decimal = b.TestDecimal.ToString(CultureInfo.InvariantCulture), // needed for cultures with decimal separator other than point
                        Char = b.TestCharacter.ToString()
                    })
                .First();

            Fixture.ListLoggerFactory.Clear();

            var query = context.Set<BuiltInDataTypes>()
                .Where(e => e.Id == 13)
                .Select(
                    b => new
                    {
                        Sbyte = b.TestSignedByte.ToString(),
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
                        DateTimeOffset = b.TestDateTimeOffset.ToString(),
                        TimeSpan = b.TestTimeSpan.ToString()*/
                    })
                .ToList();

            var actual = Assert.Single(query);
            Assert.Equal(expected.Sbyte, actual.Sbyte);
            Assert.Equal(expected.Byte, actual.Byte);
            Assert.Equal(expected.Short, actual.Short);
            Assert.Equal(expected.Ushort, actual.Ushort);
            Assert.Equal(expected.Int, actual.Int);
            Assert.Equal(expected.Uint, actual.Uint);
            Assert.Equal(expected.Long, actual.Long);
            Assert.Equal(expected.Ulong, actual.Ulong);
            Assert.Equal(expected.Decimal.TrimEnd('0'), actual.Decimal.TrimEnd('0')); // might have different scales
            Assert.Equal(expected.Char, actual.Char);
        }

        [Fact]
        public void Sql_translation_uses_type_mapper_when_constant()
        {
            using (var context = CreateContext())
            {
                var results
                    = context.Set<MappedNullableDataTypes>()
                        .Where(e => e.DateTimeAsDate == new DateTime())
                        .Select(e => e.Int)
                        .ToList();

                Assert.Empty(results);
                Assert.Equal(
                    @"SELECT `m`.`Int`
FROM `MappedNullableDataTypes` AS `m`
WHERE `m`.`DateTimeAsDate` = '0001-01-01'",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public void Sql_translation_uses_type_mapper_when_parameter()
        {
            using (var context = CreateContext())
            {
                var timeSpan = new TimeSpan(2, 1, 0);

                var results
                    = context.Set<MappedNullableDataTypes>()
                        .Where(e => e.TimeSpanAsTime == timeSpan)
                        .Select(e => e.Int)
                        .ToList();

                Assert.Empty(results);
                Assert.Equal(
                    @":__timeSpan_0='02:01:00.000' (DbType = Time)

SELECT `m`.`Int`
FROM `MappedNullableDataTypes` AS `m`
WHERE `m`.`TimeSpanAsTime` = :__timeSpan_0",
                    Sql,
                    ignoreLineEndingDifferences: true);
            }
        }

        [Fact]
        public virtual void Can_query_using_any_mapped_data_type()
        {
            int id = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Int = id,
                        LongAsBigint = 78L,
                        ShortAsSmallint = 79,
                        ByteAsTinyint = 80,
                        UintAsInt = int.MaxValue,
                        UlongAsBigint = long.MaxValue,
                        UShortAsSmallint = short.MaxValue,
                        SbyteAsTinyint = sbyte.MinValue,
                        BoolAsBit = true,
                        DecimalAsDecimal = 81.1m,
                        DecimalAsFixed = 82.2m,
                        DoubleAsReal = 83.3,
                        FloatAsFloat = 84.4f,
                        DoubleAsDoublePrecision = 85.5,
                        DateTimeAsDate = new DateTime(1605, 1, 2, 10, 11, 12),
                        //DateTimeOffsetAsDatetime = new DateTimeOffset(new DateTime(), TimeSpan.Zero),
                        //DateTimeOffsetAsTimestamp = new DateTimeOffset(new DateTime(2018, 1, 2, 14, 11, 12), TimeSpan.Zero),
                        DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
                        TimeSpanAsTime = new TimeSpan(0, 11, 15, 12, 2),
                        StringAsChar = "C",
                        StringAsNChar = "Your",
                        StringAsVarchar = "strong",
                        StringAsNvarchar = "don't",
                        StringAsTinytext = "help",
                        StringAsMediumtext = "anyone!",
                        StringAsText = "Gumball Rules!",
                        StringAsNtext = "Gumball Rules OK!",
                        BytesAsVarbinary = new byte[] { 89, 90, 91, 92 },
                        BytesAsBinary = new byte[] { 93, 94, 95, 96 },
                        BytesAsBlob = new byte[] { 97, 98, 99, 100 },
                        GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
                        UintAsBigint = int.MaxValue,
                        UlongAsDecimal200 = long.MaxValue,
                        UShortAsInt = short.MaxValue,
                        //SByteAsSmallint = sbyte.MinValue,
                        CharAsVarchar = 'A',
                        CharAsNvarchar = 'D',
                        CharAsText = 'G',
                        CharAsInt = 'I',
                        EnumAsNvarchar20 = StringEnumU16.Value4,
                        EnumAsVarchar20 = StringEnum16.Value2,
                        UShortAsYear = 42,
                        IntAsYear = 2011,
                        StringAsJson = @"{""a"": ""b""}",
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == id);

                long? param1 = 78L;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.LongAsBigint == param1));

                short? param2 = 79;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.ShortAsSmallint == param2));

                byte? param3 = 80;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.ByteAsTinyint == param3));

                bool? param4 = true;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.BoolAsBit == param4));

                decimal? param5 = 81.1m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.DecimalAsDecimal == param5));

                decimal? param6 = 82.2m;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.DecimalAsFixed == param6));

                double? param7a = 83.29;
                double? param7aa = 83.31;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(
                        e => e.Int == id && e.DoubleAsReal >= param7a && e.DoubleAsReal <= param7aa));

                float? param7b = 84.39f;
                float? param7bb = 84.41f;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(
                        e => e.Int == id && e.FloatAsFloat >= param7b && e.FloatAsFloat <= param7bb));

                double? param7c = 85.49;
                double? param7cc = 85.51;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(
                    e => e.Int == id && e.DoubleAsDoublePrecision >= param7c && e.DoubleAsDoublePrecision <= param7cc));

                DateTime? param8 = new DateTime(1605, 1, 2);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.DateTimeAsDate == param8));

                //DateTimeOffset? param9 = new DateTimeOffset(new DateTime(), TimeSpan.Zero);
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.DateTimeOffsetAsDatetime == param9));

                //DateTimeOffset? param10 = new DateTimeOffset(new DateTime(2018, 1, 2, 14, 11, 12), TimeSpan.Zero);
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.DateTimeOffsetAsTimestamp == param10));

                DateTime? param11 = new DateTime(2019, 1, 2, 14, 11, 12);
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.DateTimeAsDatetime == param11));

                //TimeSpan? param13 = new TimeSpan(0, 11, 15, 12, 2);
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.TimeSpanAsTime == param13));

                var param19 = "C";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.StringAsChar == param19));

                var param20 = "Your";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.StringAsNChar == param20));

                var param21 = "strong";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.StringAsVarchar == param21));

                var param27 = "don't";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.StringAsNvarchar == param27));

                var param28 = "help";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.StringAsTinytext == param28));

                var param29 = "anyone!";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.StringAsMediumtext == param29));

                //var param35 = null;//new byte[] { 89, 90, 91, 92 };
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.BytesAsVarbinary == param35));

                //var param36 = null;//new byte[] { 93, 94, 95, 96, 0, 0 };
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.BytesAsBinary == param36));

                int? param41 = int.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.UintAsInt == param41));

                long? param42 = long.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.UlongAsBigint == param42));

                short? param43 = short.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.UShortAsSmallint == param43));

                sbyte? param44 = sbyte.MinValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.SbyteAsTinyint == param44));

                int? param45 = int.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.UintAsBigint == param45));

                long? param46 = long.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.UlongAsDecimal200 == param46));

                short? param47 = short.MaxValue;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.UShortAsInt == param47));

                //sbyte? param48 = sbyte.MinValue;
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.SByteAsSmallint == param48));

                Guid? param49 = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47");
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.GuidAsUniqueidentifier == param49));

                char? param50 = 'A';
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.CharAsVarchar == param50));

                char? param53 = 'D';
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.CharAsNvarchar == param53));

                char? param58 = 'I';
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.CharAsInt == param58));

                StringEnumU16? param59 = StringEnumU16.Value4;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.EnumAsNvarchar20 == param59));

                StringEnum16? param60 = StringEnum16.Value2;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.EnumAsVarchar20 == param60));

                short? param61 = 42;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.UShortAsYear == param61));

                int? param62 = 2011;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.IntAsYear == param62));

                var param63 = @"{""a"": ""b""}";
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == id && e.StringAsJson == (XGJsonString)param63));
            }
        }

        [Fact]
        public virtual void Can_query_using_any_mapped_data_types_with_nulls()
        {
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(
                    new MappedNullableDataTypes
                    {
                        Int = 911
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911);

                long? param1 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.LongAsBigint == param1));

                short? param2 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ShortAsSmallint == param2));
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && (long?)(int?)e.ShortAsSmallint == param2));

                byte? param3 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.ByteAsTinyint == param3));

                bool? param4 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BoolAsBit == param4));

                decimal? param5 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsDecimal == param5));

                decimal? param6 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DecimalAsFixed == param6));

                double? param7a = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DoubleAsReal == param7a));

                float? param7b = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.FloatAsFloat == param7b));

                double? param7c = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DoubleAsDoublePrecision == param7c));

                DateTime? param8 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsDate == param8));

                //DateTimeOffset? param9 = null;
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeOffsetAsDatetime == param9));

                //DateTime? param10 = null;
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeOffsetAsTimestamp == param10));

                DateTime? param11 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.DateTimeAsDatetime == param11));

                //TimeSpan? param13 = null;
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.TimeSpanAsTime == param13));

                string param19 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsChar == param19));

                string param20 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNChar == param20));

                string param21 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsVarchar == param21));

                string param27 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNvarchar == param27));

                string param28 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsTinytext == param28));

                string param29 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsMediumtext == param29));

                string param30 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsText == param30));

                string param31 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsNtext == param31));

                byte[] param35 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsVarbinary == param35));

                byte[] param36 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsBinary == param36));

                byte[] param37 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.BytesAsBlob == param37));

                int? param41 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsInt == param41));

                long? param42 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UlongAsBigint == param42));

                short? param43 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsSmallint == param43));

                sbyte? param44 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SbyteAsTinyint == param44));

                int? param45 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UintAsBigint == param45));

                long? param46 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UlongAsDecimal200 == param46));

                short? param47 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsInt == param47));

                //sbyte? param48 = null;
                //Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.SByteAsSmallint == param48));

                Guid? param49 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.GuidAsUniqueidentifier == param49));

                char? param50 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsVarchar == param50));

                char? param53 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsNvarchar == param53));

                char? param56 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsText == param56));

                char? param58 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.CharAsInt == param58));

                StringEnumU16? param59 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.EnumAsNvarchar20 == param59));

                StringEnum16? param60 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.EnumAsVarchar20 == param60));

                short? param61 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.UShortAsYear == param61));

                short? param62 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.IntAsYear == param62));

                string param63 = null;
                Assert.Same(entity, context.Set<MappedNullableDataTypes>().Single(e => e.Int == 911 && e.StringAsJson == param63));
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types()
        {
            int id = new Random().Next();
            var entity = CreateMappedDataTypes(id);
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(entity);

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
//            Assert.Equal(
//                @"@p0='77'
//@p1='1'
//@p2='80'
//@p3='0x5D5E5F60' (Nullable = false) (Size = 5)
//@p4='0x61626365' (Nullable = false) (Size = 8000)
//@p5='0x61626367' (Nullable = false) (Size = 8000)
//@p6='0x61626366' (Nullable = false) (Size = 8000)
//@p7='0x61626364' (Nullable = false) (Size = 8000)
//@p8='0x595A5B5C' (Nullable = false) (Size = 255)
//@p9='73'
//@p10='D' (Nullable = false) (Size = 20)
//@p11='A' (Nullable = false) (Size = 1)
//@p12='2015-01-02T10:11:12.0000000' (DbType = Date)
//@p13='2019-01-02T14:11:12.0000000' (DbType = DateTime)
//@p14='2016-01-02T11:11:12.0000000+00:00'
//@p15='2017-01-02T12:11:12.0000000+02:00'
//@p16='81.1'
//@p17='85.5'
//@p18='83.3'
//@p19='Value4' (Nullable = false) (Size = 20)
//@p20='Value2' (Nullable = false) (Size = 20)
//@p21='a8f9f951-145f-4545-ac60-b92ff57ada47'
//@p22='2011' (DbType = Int32)
//@p23='78'
//@p24='-128'
//@p25='128'
//@p26='79'
//@p27='Your' (Nullable = false) (Size = 10) (DbType = StringFixedLength)
//@p28='{""a"": ""b""}' (Nullable = false) (Size = 4000)
//@p29='arm' (Nullable = false) (Size = 4000)
//@p30='anyone!' (Nullable = false) (Size = 4000)
//@p31='strong' (Nullable = false) (Size = 10) (DbType = StringFixedLength)
//@p32='Gumball Rules OK!' (Nullable = false) (Size = 4000)
//@p33='" + entity.StringAsNvarchar + @"' (Nullable = false) (Size = -1)
//@p34='Gumball Rules!' (Nullable = false) (Size = 4000)
//@p35='help' (Nullable = false) (Size = 4000)
//@p36='" + entity.StringAsVarchar + @"' (Nullable = false) (Size = -1)
//@p37='11:15:12'
//@p38='65535'
//@p39='65535'
//@p40='42' (DbType = Int32)
//@p41='4294967295'
//@p42='4294967295'
//@p43='18446744073709551615'
//@p44='18446744073709551615'",
//                parameters,
//                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == id), id);
            }
        }

        private string DumpParameters()
            => Fixture.TestSqlLoggerFactory.Parameters.Single().Replace(", ", eol);

        private static void AssertMappedDataTypes(MappedDataTypes entity, int id)
        {
            var expected = CreateMappedDataTypes(id);
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.LongAsBigInt);
            Assert.Equal(79, entity.ShortAsSmallint);
            Assert.Equal(80, entity.ByteAsTinyint);
            Assert.Equal(int.MinValue, entity.UintAsInt);
            Assert.Equal(long.MinValue, entity.UlongAsBigint);
            Assert.Equal(short.MinValue, entity.UShortAsSmallint);
            Assert.Equal(sbyte.MinValue, entity.SByteAsTinyint);
            Assert.True(entity.BoolAsBit);
            Assert.Equal(81.1m, entity.DecimalAsDecimal);
            Assert.Equal(83.3, entity.DoubleAsFloat, 1);
            Assert.Equal(85.5, entity.DoubleAsDouble, 1);
            //Assert.Equal(new DateTime(2015, 1, 2, 10, 11, 12), entity.DateTimeAsDate);
            Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.DateTimeOffsetAsDatetime);
            Assert.Equal(new DateTimeOffset(new DateTime(2017, 1, 2, 12, 11, 12), TimeSpan.FromHours(2)), entity.DateTimeOffsetAsTimestamp);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
            Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
            Assert.Equal(expected.StringAsVarchar, entity.StringAsVarchar);
            Assert.Equal("Your", entity.StringAsChar);
            Assert.Equal("strong", entity.StringAsNChar);
            Assert.Equal(expected.StringAsNvarchar, entity.StringAsNvarchar);
            Assert.Equal("help", entity.StringAsTinytext);
            Assert.Equal("anyone!", entity.StringAsMediumtext);
            Assert.Equal("arm", entity.StringAsLongtext);
            Assert.Equal("Gumball Rules!", entity.StringAsText);
            Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinary);
            Assert.Equal(new byte[] { 93, 94, 95, 96 }, entity.BytesAsBinary);
            Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsTinyblob);
            Assert.Equal(new byte[] { 97, 98, 99, 101 }, entity.BytesAsBlob);
            Assert.Equal(new byte[] { 97, 98, 99, 102 }, entity.BytesAsMediumblob);
            //Assert.Equal(new byte[] { 97, 98, 99, 103 }, entity.BytesAsLongblob);
            Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
            Assert.Equal(int.MinValue, entity.UintAsBigint);
            Assert.Equal(long.MinValue, entity.UlongAsDecimal200);
            Assert.Equal(short.MinValue, entity.UShortAsInt);
            //Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
            Assert.Equal('A', entity.CharAsVarchar);
            Assert.Equal('D', entity.CharAsNvarchar);
            Assert.Equal('I', entity.CharAsInt);
            Assert.Equal(StringEnum16.Value2, entity.EnumAsVarchar20);
            Assert.Equal(StringEnumU16.Value4, entity.EnumAsNvarchar20);
            Assert.Equal(42, entity.UShortAsYear);
            Assert.Equal(2011, entity.IntAsYear);
            Assert.Equal(@"{""a"": ""b""}", entity.StringAsJson);
        }

        private static MappedDataTypes CreateMappedDataTypes(int id)
            => new MappedDataTypes
            {
                Int = id,
                LongAsBigInt = 78L,
                ShortAsSmallint = 79,
                ByteAsTinyint = 80,
                UintAsInt = int.MinValue,
                UlongAsBigint = long.MinValue,
                UShortAsSmallint = short.MinValue,
                SByteAsTinyint = sbyte.MinValue,
                BoolAsBit = true,
                DecimalAsDecimal = 81.1m,
                DoubleAsFloat = 83.3,
                DoubleAsDouble = 85.5,
                DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
                DateTimeOffsetAsDatetime = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                DateTimeOffsetAsTimestamp = new DateTimeOffset(new DateTime(2017, 1, 2, 12, 11, 12), TimeSpan.FromHours(2)),
                DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
                TimeSpanAsTime = new TimeSpan(11, 15, 12),
                StringAsVarchar = string.Concat(Enumerable.Repeat("C", 8001)),
                StringAsChar = "Your",
                StringAsNChar = "strong",
                StringAsNvarchar = string.Concat(Enumerable.Repeat("D", 4001)),
                StringAsTinytext = "help",
                StringAsMediumtext = "anyone!",
                StringAsLongtext = "arm",
                StringAsText = "Gumball Rules!",
                StringAsNtext = "Gumball Rules OK!",
                BytesAsVarbinary = new byte[] {89, 90, 91, 92},
                BytesAsBinary = new byte[] {93, 94, 95, 96},
                BytesAsTinyblob = new byte[] {97, 98, 99, 100},
                BytesAsBlob = new byte[] {97, 98, 99, 101},
                BytesAsMediumblob = new byte[] {97, 98, 99, 102},
                //BytesAsLongblob = new byte[] {97, 98, 99, 103},
                GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
                UintAsBigint = int.MinValue,
                UlongAsDecimal200 = long.MinValue,
                UShortAsInt = short.MinValue,
                //SByteAsSmallint = sbyte.MinValue,
                CharAsVarchar = 'A',
                CharAsNvarchar = 'D',
                CharAsInt = 'I',
                EnumAsNvarchar20 = StringEnumU16.Value4,
                EnumAsVarchar20 = StringEnum16.Value2,
                UShortAsYear = 42,
                IntAsYear = 2011,
                StringAsJson = @"{""a"": ""b""}",
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types()
        {
            int id= new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(id));

                Assert.Equal(1, context.SaveChanges());
            }

            var parameters = DumpParameters();
//            Assert.Equal(
//                @"@p0='"+id+@"'
//@p1='True' (Nullable = true)
//@p2='80' (Nullable = true) (DbType = Binary)
//@p3=NULL (Size = 6) (DbType = Binary)
//@p4=NULL (Size = 8000) (DbType = Binary)
//@p5=NULL (Size = 255) (DbType = Binary)
//@p6='73' (Nullable = true)
//@p7='D' (Size = 1)
//@p8='G' (Size = 1)
//@p9='A' (Size = 1)
//@p10='2015-01-02T10:11:12.0000000' (Nullable = true) (DbType = Date)
//@p11='2019-01-02T14:11:12.0000000' (Nullable = true) (DbType = DateTime)
//@p12='81.1' (Nullable = true) (DbType = Object)
//@p13='82.2' (Nullable = true) (DbType = Object)
//@p14='85.5' (Nullable = true)
//@p15='83.3' (Nullable = true)
//@p16='Value4' (Size = 20)
//@p17='Value2' (Size = 20)
//@p18='84.4' (Nullable = true) (DbType = Binary)
//@p19='a8f9f951-145f-4545-ac60-b92ff57ada47' (Nullable = true) (DbType = Binary)
//@p20='2011' (Nullable = true)
//@p21='78' (Nullable = true)
//@p22='-128' (Nullable = true) (DbType = Binary)
//@p23='79' (Nullable = true)
//@p24='C' (Size = 20) (DbType = Binary)
//@p25='{""a"": ""b""}' (Size = 4000)
//@p26='anyone!' (Size = 4000)
//@p27='Your' (Size = 20)(DbType = Binary)
//@p28='Gumball Rules OK!'(Size = 4000)
//@p29='don't' (Size = 55)
//@p30='Gumball Rules!'(Size = 4000)
//@p31='help'(Size = 4000)
//@p32='strong'(Size = 55)
//@p33='32767'(Nullable = true)
//@p34='32767'(Nullable = true)
//@p35='42'(Nullable = true)
//@p36='2147483647'(Nullable = true)
//@p37='2147483647'(Nullable = true)
//@p38='9223372036854775807'(Nullable = true)
//@p39='9223372036854775807'(Nullable = true)(DbType = Object)",
//                parameters,
//                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == id), id);
            }
        }

        private static void AssertMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Equal(78, entity.LongAsBigint);
            Assert.Equal(79, entity.ShortAsSmallint.Value);
            Assert.Equal(80, entity.ByteAsTinyint.Value);
            Assert.Equal(int.MaxValue, entity.UintAsInt);
            Assert.Equal(long.MaxValue, entity.UlongAsBigint);
            Assert.Equal(short.MaxValue, entity.UShortAsSmallint);
            Assert.Equal(sbyte.MinValue, entity.SbyteAsTinyint);
            Assert.True(entity.BoolAsBit);
            Assert.Equal(81.1m, entity.DecimalAsDecimal);
            Assert.Equal(82.2m, entity.DecimalAsFixed);
            Assert.Equal(83.3, entity.DoubleAsReal.Value, 1);
            Assert.Equal(84.4f, entity.FloatAsFloat.Value, 1);
            Assert.Equal(85.5, entity.DoubleAsDoublePrecision.Value, 1);
            //Assert.Equal(DateTime.Parse("2025-01-09T17:38:43.2564890+08:00"), entity.DateTimeAsDate);
            //Assert.Equal(new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero), entity.DateTimeOffsetAsDatetime);
            //Assert.Equal(new DateTimeOffset(new DateTime(2017, 1, 2, 12, 11, 12), TimeSpan.FromHours(6)),
            //    entity.DateTimeOffsetAsTimestamp);
            Assert.Equal(new DateTime(2019, 1, 2, 14, 11, 12), entity.DateTimeAsDatetime);
            //Assert.Equal(new TimeSpan(11, 15, 12), entity.TimeSpanAsTime);
            Assert.Equal("C", entity.StringAsChar);
            Assert.Equal("Your", entity.StringAsNChar);
            Assert.Equal("strong", entity.StringAsVarchar);
            Assert.Equal("don't", entity.StringAsNvarchar);
            Assert.Equal("help", entity.StringAsTinytext);
            Assert.Equal("anyone!", entity.StringAsMediumtext);
            Assert.Equal("Gumball Rules!", entity.StringAsText);
            Assert.Equal("Gumball Rules OK!", entity.StringAsNtext);
            Assert.Equal(new byte[] { 89, 90, 91, 92 }, entity.BytesAsVarbinary);
            Assert.Equal(new byte[] { 93, 94, 95, 96}, entity.BytesAsBinary);
            Assert.Equal(new byte[] { 97, 98, 99, 100 }, entity.BytesAsBlob);
            Assert.Equal(new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"), entity.GuidAsUniqueidentifier);
            Assert.Equal(int.MaxValue, entity.UintAsBigint);
            Assert.Equal(long.MaxValue, entity.UlongAsDecimal200);
            Assert.Equal(short.MaxValue, entity.UShortAsInt);
            //Assert.Equal(sbyte.MinValue, entity.SByteAsSmallint);
            Assert.Equal('A', entity.CharAsVarchar);
            Assert.Equal('D', entity.CharAsNvarchar);
            Assert.Equal('G', entity.CharAsText);
            Assert.Equal('I', entity.CharAsInt);
            Assert.Equal(StringEnum16.Value2, entity.EnumAsVarchar20);
            Assert.Equal(StringEnumU16.Value4, entity.EnumAsNvarchar20);
            Assert.Equal((short)42, entity.UShortAsYear);
            Assert.Equal(2011, entity.IntAsYear);
            Assert.Equal(@"{""a"": ""b""}", entity.StringAsJson);
        }

        private static MappedNullableDataTypes CreateMappedNullableDataTypes(int id)
            => new MappedNullableDataTypes
            {
                Int = id,
                LongAsBigint = 78L,
                ShortAsSmallint = 79,
                ByteAsTinyint = 80,
                UintAsInt = int.MaxValue,
                UlongAsBigint = long.MaxValue,
                UShortAsSmallint = short.MaxValue,
                SbyteAsTinyint = sbyte.MinValue,
                BoolAsBit = true,
                DecimalAsDecimal = 81.1m,
                DecimalAsFixed = 82.2m,
                DoubleAsReal = 83.3,
                FloatAsFloat = 84.4f,
                DoubleAsDoublePrecision = 85.5,
                DateTimeAsDate = new DateTime(2015, 1, 2, 10, 11, 12),
                //DateTimeOffsetAsDatetime = new DateTimeOffset(new DateTime(2016, 1, 2, 11, 11, 12), TimeSpan.Zero),
                //DateTimeOffsetAsTimestamp = new DateTimeOffset(new DateTime(2017, 1, 2, 12, 11, 12), TimeSpan.FromHours(6)),
                DateTimeAsDatetime = new DateTime(2019, 1, 2, 14, 11, 12),
                TimeSpanAsTime = new TimeSpan(11, 15, 12),
                StringAsChar = "C",
                StringAsNChar = "Your",
                StringAsVarchar = "strong",
                StringAsNvarchar = "don't",
                StringAsTinytext = "help",
                StringAsMediumtext = "anyone!",
                StringAsText = "Gumball Rules!",
                StringAsNtext = "Gumball Rules OK!",
                BytesAsVarbinary = new byte[] { 89, 90, 91, 92 },
                BytesAsBinary = new byte[] { 93, 94, 95, 96, 0, 0 },
                BytesAsBlob = new byte[] { 97, 98, 99, 100 },
                GuidAsUniqueidentifier = new Guid("A8F9F951-145F-4545-AC60-B92FF57ADA47"),
                UintAsBigint = int.MaxValue,
                UlongAsDecimal200 = long.MaxValue,
                UShortAsInt = short.MaxValue,
                //SByteAsSmallint = sbyte.MinValue,
                CharAsVarchar = 'A',
                CharAsNvarchar = 'D',
                CharAsText = 'G',
                CharAsInt = 'I',
                EnumAsNvarchar20 = StringEnumU16.Value4,
                EnumAsVarchar20 = StringEnum16.Value2,
                UShortAsYear = 42,
                IntAsYear = 2011,
                StringAsJson = @"{""a"": ""b""}",
            };

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null()
        {
            int id = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = id });

                Assert.Equal(1, context.SaveChanges());
            }


            var parameters = DumpParameters();
//            Assert.Equal(
//                @"@p0='78'
//@p1=NULL (DbType = UInt64)
//@p2=NULL (DbType = SByte)
//@p3=NULL (Size = 6) (DbType = Binary)
//@p4=NULL (Size = 8000) (DbType = Binary)
//@p5=NULL (Size = 255) (DbType = Binary)
//@p6=NULL (DbType = Int32)
//@p7=NULL (Size = 1)
//@p8=NULL (Size = 1)
//@p9=NULL (Size = 1)
//@p10=NULL (DbType = Date)
//@p11=NULL (DbType = DateTime)
//@p12=NULL (Size = 6) (DbType = DateTimeOffset)
//@p13=NULL (Size = 6) (DbType = DateTimeOffset)
//@p14=NULL
//@p15=NULL
//@p16=NULL (DbType = Double)
//@p17=NULL (DbType = Double)
//@p18=NULL (Size = 20)
//@p19=NULL (Size = 20)
//@p20=NULL (DbType = Single)
//@p21=NULL (DbType = Guid)
//@p22=NULL (DbType = Int32)
//@p23=NULL (DbType = Int64)
//@p24=NULL (DbType = Int16)
//@p25=NULL (DbType = SByte)
//@p26=NULL (DbType = Int16)
//@p27=NULL (Size = 20) (DbType = StringFixedLength)
//@p28=NULL (Size = 4000)
//@p29=NULL (Size = 4000)
//@p30=NULL (Size = 20) (DbType = StringFixedLength)
//@p31=NULL (Size = 4000)
//@p32=NULL (Size = 55)
//@p33=NULL (Size = 55)
//@p34=NULL (Size = 4000)
//@p35=NULL (Size = 55)
//@p36=NULL (DbType = Time)
//@p37=NULL (DbType = Int32)
//@p38=NULL (DbType = Int16)
//@p39=NULL (DbType = Int32)
//@p40=NULL (DbType = Int64)
//@p41=NULL (DbType = Int32)
//@p42=NULL (DbType = Int64)
//@p43=NULL",
//                parameters,
//                ignoreLineEndingDifferences: true);

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().FirstOrDefault(e => e.Int == id), id);
            }
        }

        private static void AssertNullMappedNullableDataTypes(MappedNullableDataTypes entity, int id)
        {
            Assert.Equal(id, entity.Int);
            Assert.Null(entity.LongAsBigint);
            Assert.Null(entity.ShortAsSmallint);
            Assert.Null(entity.ByteAsTinyint);
            Assert.Null(entity.UintAsInt);
            Assert.Null(entity.UlongAsBigint);
            Assert.Null(entity.UShortAsSmallint);
            Assert.Null(entity.SbyteAsTinyint);
            Assert.Null(entity.BoolAsBit);
            Assert.Null(entity.DecimalAsDecimal);
            Assert.Null(entity.DecimalAsFixed);
            Assert.Null(entity.DoubleAsReal);
            Assert.Null(entity.FloatAsFloat);
            Assert.Null(entity.DoubleAsDoublePrecision);
            Assert.Null(entity.DateTimeAsDate);
            //Assert.Null(entity.DateTimeOffsetAsDatetime);
            //Assert.Null(entity.DateTimeOffsetAsTimestamp);
            Assert.Null(entity.DateTimeAsDatetime);
            //Assert.Null(entity.TimeSpanAsTime);
            Assert.Null(entity.StringAsChar);
            Assert.Null(entity.StringAsNChar);
            Assert.Null(entity.StringAsVarchar);
            Assert.Null(entity.StringAsNvarchar);
            Assert.Null(entity.StringAsTinytext);
            Assert.Null(entity.StringAsMediumtext);
            Assert.Null(entity.StringAsText);
            Assert.Null(entity.StringAsNtext);
            Assert.Null(entity.BytesAsVarbinary);
            Assert.Null(entity.BytesAsBinary);
            Assert.Null(entity.BytesAsBlob);
            Assert.Null(entity.GuidAsUniqueidentifier);
            Assert.Null(entity.UintAsBigint);
            Assert.Null(entity.UlongAsDecimal200);
            Assert.Null(entity.UShortAsInt);
            //Assert.Null(entity.SByteAsSmallint);
            Assert.Null(entity.CharAsVarchar);
            Assert.Null(entity.CharAsNvarchar);
            Assert.Null(entity.CharAsText);
            Assert.Null(entity.CharAsInt);
            Assert.Null(entity.EnumAsNvarchar20);
            Assert.Null(entity.EnumAsVarchar20);
            Assert.Null(entity.UShortAsYear);
            Assert.Null(entity.IntAsYear);
            Assert.Null(entity.StringAsJson);
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_in_batch()
        {
            int id1 = new Random().Next();
            int id2 = new Random().Next();
            int id3 = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(id1));
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(id2));
                context.Set<MappedDataTypes>().Add(CreateMappedDataTypes(id3));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == id1), id1);
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == id2), id2);
                AssertMappedDataTypes(context.Set<MappedDataTypes>().Single(e => e.Int == id3), id3);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_nullable_data_types_in_batch()
        {
            int id1 = new Random().Next();
            int id2 = new Random().Next();
            int id3 = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(id1));
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(id2));
                context.Set<MappedNullableDataTypes>().Add(CreateMappedNullableDataTypes(id3));

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                //var test = context.Set<MappedNullableDataTypes>().Select(e => new MappedNullableDataTypes
                //{
                //    Int = e.Int,
                //    LongAsBigint = e.LongAsBigint,
                //    ShortAsSmallint = e.ShortAsSmallint,
                //    ByteAsTinyint = e.ByteAsTinyint,
                //    UintAsInt = e.UintAsInt,
                //    UlongAsBigint = e.UlongAsBigint,
                //    UShortAsSmallint = e.UShortAsSmallint,
                //    SbyteAsTinyint = e.SbyteAsTinyint,
                //    BoolAsBit = e.BoolAsBit,
                //    DecimalAsDecimal = e.DecimalAsDecimal,
                //    DecimalAsFixed = e.DecimalAsFixed,
                //    FloatAsFloat = e.FloatAsFloat,
                //    DoubleAsReal = e.DoubleAsReal,
                //    DoubleAsDoublePrecision = e.DoubleAsDoublePrecision,
                //    DateTimeAsDate = e.DateTimeAsDate,
                //    DateTimeAsDatetime = e.DateTimeAsDatetime,
                //    TimeSpanAsTime = e.TimeSpanAsTime,
                //    StringAsChar = e.StringAsChar,
                //    StringAsNChar = e.StringAsNChar,
                //    StringAsVarchar = e.StringAsVarchar,
                //    StringAsNvarchar = e.StringAsNvarchar,
                //    StringAsText = e.StringAsText,
                //    StringAsNtext = e.StringAsNtext,
                //    StringAsTinytext = e.StringAsTinytext,
                //    StringAsMediumtext = e.StringAsMediumtext,
                //    BytesAsVarbinary = e.BytesAsVarbinary,
                //    BytesAsBinary = e.BytesAsBinary,
                //    BytesAsBlob = e.BytesAsBlob,
                //    GuidAsUniqueidentifier = e.GuidAsUniqueidentifier,
                //    UintAsBigint = e.UintAsBigint,
                //    UlongAsDecimal200 = e.UlongAsDecimal200,
                //    UShortAsInt = e.UShortAsInt,
                //    UShortAsYear = e.UShortAsYear,
                //    IntAsYear = e.IntAsYear,
                //    SByteAsSmallint = e.SByteAsSmallint,
                //    CharAsVarchar = e.CharAsVarchar,
                //    CharAsNvarchar = e.CharAsNvarchar,
                //    CharAsText = e.CharAsText,
                //    CharAsInt = e.CharAsInt,
                //    EnumAsVarchar20 = e.EnumAsVarchar20,
                //    EnumAsNvarchar20 = e.EnumAsNvarchar20,
                //    StringAsJson = e.StringAsJson
                //});
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == id1), id1);
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == id2), id2);
                AssertMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == id3), id3);
            }
        }

        [Fact]
        public virtual void Can_insert_and_read_back_all_mapped_data_types_set_to_null_in_batch()
        {
            int id1 = new Random().Next();
            int id2 = new Random().Next();
            int id3 = new Random().Next();
            using (var context = CreateContext())
            {
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = id1 });
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = id2 });
                context.Set<MappedNullableDataTypes>().Add(new MappedNullableDataTypes { Int = id3 });

                Assert.Equal(3, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == id1), id1);
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == id2), id2);
                AssertNullMappedNullableDataTypes(context.Set<MappedNullableDataTypes>().Single(e => e.Int == id3), id3);
            }
        }

        [ConditionalFact]
        public virtual void Columns_have_expected_data_types()
        {
            var actual = QueryForColumnTypes(CreateContext());

            var expected = $@"Animal.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
AnimalDetails.AnimalId ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
AnimalDetails.BoolField ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
AnimalDetails.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
AnimalIdentification.AnimalId ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
AnimalIdentification.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
AnimalIdentification.Method ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BinaryForeignKeyDataType.BinaryKeyDataTypeId ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BinaryForeignKeyDataType.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BinaryKeyDataType.Ex ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BinaryKeyDataType.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.Enum16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.Enum32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.Enum64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.Enum8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.EnumS8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.EnumU16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.EnumU32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.EnumU64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.PartitionId ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestBoolean ---> [BOOLEAN] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestCharacter ---> [CHAR] [MaxLength = 1] [Precision = 0 [Precision = 1 Scale = 1]
BuiltInDataTypes.TestDateTime ---> [DATETIME] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
BuiltInDataTypes.TestDecimal ---> [NUMERIC] [MaxLength = 2490385] [Precision = 38 [Precision = 2490385 Scale = 17]
BuiltInDataTypes.TestDouble ---> [DOUBLE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestSignedByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestSingle ---> [FLOAT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestUnsignedInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestUnsignedInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypes.TestUnsignedInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.Enum16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.Enum32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.Enum64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.Enum8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.EnumS8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.EnumU16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.EnumU32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.EnumU64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.PartitionId ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestBoolean ---> [BOOLEAN] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestCharacter ---> [CHAR] [MaxLength = 1] [Precision = 0 [Precision = 1 Scale = 1]
BuiltInDataTypesShadow.TestDateTime ---> [DATETIME] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
BuiltInDataTypesShadow.TestDecimal ---> [NUMERIC] [MaxLength = 2490385] [Precision = 38 [Precision = 2490385 Scale = 17]
BuiltInDataTypesShadow.TestDouble ---> [DOUBLE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestSignedByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestSingle ---> [FLOAT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestUnsignedInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestUnsignedInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInDataTypesShadow.TestUnsignedInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.Enum16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.Enum32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.Enum64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.Enum8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.EnumS8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.EnumU16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.EnumU32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.EnumU64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.PartitionId ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestByteArray ---> [BINARY] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableBoolean ---> [BOOLEAN] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableCharacter ---> [CHAR] [MaxLength = 1] [Precision = 0 [Precision = 1 Scale = 1]
BuiltInNullableDataTypes.TestNullableDateTime ---> [DATETIME] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
BuiltInNullableDataTypes.TestNullableDateTimeOffset ---> [DATETIME WITH TIME ZONE] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
BuiltInNullableDataTypes.TestNullableDecimal ---> [NUMERIC] [MaxLength = 2490385] [Precision = 38 [Precision = 2490385 Scale = 17]
BuiltInNullableDataTypes.TestNullableDouble ---> [DOUBLE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableSignedByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableSingle ---> [FLOAT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableTimeSpan ---> [TIME] [MaxLength = 3] [Precision = 0 [Precision = 3 Scale = 3]
BuiltInNullableDataTypes.TestNullableUnsignedInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableUnsignedInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestNullableUnsignedInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypes.TestString ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.Enum16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.Enum32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.Enum64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.Enum8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.EnumS8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.EnumU16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.EnumU32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.EnumU64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.PartitionId ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestByteArray ---> [BINARY] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableBoolean ---> [BOOLEAN] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableCharacter ---> [CHAR] [MaxLength = 1] [Precision = 0 [Precision = 1 Scale = 1]
BuiltInNullableDataTypesShadow.TestNullableDateTime ---> [DATETIME] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
BuiltInNullableDataTypesShadow.TestNullableDateTimeOffset ---> [DATETIME WITH TIME ZONE] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
BuiltInNullableDataTypesShadow.TestNullableDecimal ---> [NUMERIC] [MaxLength = 2490385] [Precision = 38 [Precision = 2490385 Scale = 17]
BuiltInNullableDataTypesShadow.TestNullableDouble ---> [DOUBLE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableSignedByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableSingle ---> [FLOAT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableTimeSpan ---> [TIME] [MaxLength = 3] [Precision = 0 [Precision = 3 Scale = 3]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestNullableUnsignedInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
BuiltInNullableDataTypesShadow.TestString ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
DateTimeEnclosure.DateTimeOffset ---> [DATETIME WITH TIME ZONE] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
DateTimeEnclosure.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
EmailTemplate.Id ---> [GUID] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
EmailTemplate.TemplateType ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.BoolAsBit ---> [BOOLEAN] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.ByteAsTinyint ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.BytesAsBinary ---> [BINARY] [MaxLength = 5] [Precision = 0 [Precision = 5 Scale = 5]
MappedDataTypes.BytesAsBlob ---> [BINARY] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.BytesAsMediumblob ---> [BINARY] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.BytesAsTinyblob ---> [BINARY] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.BytesAsVarbinary ---> [BINARY] [MaxLength = 255] [Precision = 0 [Precision = 255 Scale = 255]
MappedDataTypes.CharAsInt ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.CharAsNvarchar ---> [CHAR] [MaxLength = 20] [Precision = 0 [Precision = 20 Scale = 20]
MappedDataTypes.CharAsVarchar ---> [CHAR] [MaxLength = 1] [Precision = 0 [Precision = 1 Scale = 1]
MappedDataTypes.DateTimeAsDate ---> [DATE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.DateTimeAsDatetime ---> [DATETIME] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
MappedDataTypes.DateTimeOffsetAsDatetime ---> [CHAR] [MaxLength = 48] [Precision = 0 [Precision = 48 Scale = 48]
MappedDataTypes.DateTimeOffsetAsTimestamp ---> [CHAR] [MaxLength = 48] [Precision = 0 [Precision = 48 Scale = 48]
MappedDataTypes.DecimalAsDecimal ---> [NUMERIC] [MaxLength = 524290] [Precision = 8 [Precision = 524290 Scale = 2]
MappedDataTypes.DoubleAsDouble ---> [DOUBLE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.DoubleAsFloat ---> [FLOAT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.EnumAsNvarchar20 ---> [CHAR] [MaxLength = 20] [Precision = 0 [Precision = 20 Scale = 20]
MappedDataTypes.EnumAsVarchar20 ---> [CHAR] [MaxLength = 20] [Precision = 0 [Precision = 20 Scale = 20]
MappedDataTypes.GuidAsUniqueidentifier ---> [CHAR] [MaxLength = 36] [Precision = 0 [Precision = 36 Scale = 36]
MappedDataTypes.Int ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.IntAsYear ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.LongAsBigInt ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.SByteAsTinyint ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.ShortAsSmallint ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.StringAsChar ---> [CHAR] [MaxLength = 10] [Precision = 0 [Precision = 10 Scale = 10]
MappedDataTypes.StringAsJson ---> [CHAR] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.StringAsLongtext ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.StringAsMediumtext ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.StringAsNChar ---> [CHAR] [MaxLength = 10] [Precision = 0 [Precision = 10 Scale = 10]
MappedDataTypes.StringAsNtext ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.StringAsNvarchar ---> [CHAR] [MaxLength = 4001] [Precision = 0 [Precision = 4001 Scale = 4001]
MappedDataTypes.StringAsText ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.StringAsTinytext ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.StringAsVarchar ---> [CHAR] [MaxLength = 8001] [Precision = 0 [Precision = 8001 Scale = 8001]
MappedDataTypes.TimeSpanAsTime ---> [TIME] [MaxLength = 3] [Precision = 0 [Precision = 3 Scale = 3]
MappedDataTypes.UintAsBigint ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.UintAsInt ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.UlongAsBigint ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.UlongAsDecimal200 ---> [NUMERIC] [MaxLength = 1310720] [Precision = 20 [Precision = 1310720 Scale = 0]
MappedDataTypes.UShortAsInt ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.UShortAsSmallint ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedDataTypes.UShortAsYear ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.BoolAsBit ---> [BOOLEAN] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.ByteAsTinyint ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.BytesAsBinary ---> [BINARY] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
MappedNullableDataTypes.BytesAsBlob ---> [BINARY] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.BytesAsVarbinary ---> [BINARY] [MaxLength = 255] [Precision = 0 [Precision = 255 Scale = 255]
MappedNullableDataTypes.CharAsInt ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.CharAsNvarchar ---> [CHAR] [MaxLength = 1] [Precision = 0 [Precision = 1 Scale = 1]
MappedNullableDataTypes.CharAsText ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.CharAsVarchar ---> [CHAR] [MaxLength = 1] [Precision = 0 [Precision = 1 Scale = 1]
MappedNullableDataTypes.DateTimeAsDate ---> [DATE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.DateTimeAsDatetime ---> [DATETIME] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
MappedNullableDataTypes.DecimalAsDecimal ---> [NUMERIC] [MaxLength = 524290] [Precision = 8 [Precision = 524290 Scale = 2]
MappedNullableDataTypes.DecimalAsFixed ---> [NUMERIC] [MaxLength = 524290] [Precision = 8 [Precision = 524290 Scale = 2]
MappedNullableDataTypes.DoubleAsDoublePrecision ---> [DOUBLE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.DoubleAsReal ---> [FLOAT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.EnumAsNvarchar20 ---> [CHAR] [MaxLength = 20] [Precision = 0 [Precision = 20 Scale = 20]
MappedNullableDataTypes.EnumAsVarchar20 ---> [CHAR] [MaxLength = 20] [Precision = 0 [Precision = 20 Scale = 20]
MappedNullableDataTypes.FloatAsFloat ---> [FLOAT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.GuidAsUniqueidentifier ---> [GUID] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.Int ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.IntAsYear ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.LongAsBigint ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.SbyteAsTinyint ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.ShortAsSmallint ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.StringAsChar ---> [CHAR] [MaxLength = 20] [Precision = 0 [Precision = 20 Scale = 20]
MappedNullableDataTypes.StringAsJson ---> [CHAR] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.StringAsMediumtext ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.StringAsNChar ---> [CHAR] [MaxLength = 20] [Precision = 0 [Precision = 20 Scale = 20]
MappedNullableDataTypes.StringAsNtext ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.StringAsNvarchar ---> [CHAR] [MaxLength = 55] [Precision = 0 [Precision = 55 Scale = 55]
MappedNullableDataTypes.StringAsText ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.StringAsTinytext ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.StringAsVarchar ---> [CHAR] [MaxLength = 55] [Precision = 0 [Precision = 55 Scale = 55]
MappedNullableDataTypes.TimeSpanAsTime ---> [TIME] [MaxLength = 3] [Precision = 0 [Precision = 3 Scale = 3]
MappedNullableDataTypes.UintAsBigint ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.UintAsInt ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.UlongAsBigint ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.UlongAsDecimal200 ---> [NUMERIC] [MaxLength = 1310720] [Precision = 20 [Precision = 1310720 Scale = 0]
MappedNullableDataTypes.UShortAsInt ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.UShortAsSmallint ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MappedNullableDataTypes.UShortAsYear ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MaxLengthDataTypes.ByteArray5 ---> [BINARY] [MaxLength = 5] [Precision = 0 [Precision = 5 Scale = 5]
MaxLengthDataTypes.ByteArray9000 ---> [BINARY] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MaxLengthDataTypes.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
MaxLengthDataTypes.String3 ---> [CHAR] [MaxLength = 3] [Precision = 0 [Precision = 3 Scale = 3]
MaxLengthDataTypes.String9000 ---> [CHAR] [MaxLength = 9000] [Precision = 0 [Precision = 9000 Scale = 9000]
NonNullableBackedDataTypes.Boolean ---> [BOOLEAN] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Byte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Character ---> [CHAR] [MaxLength = 1] [Precision = 0 [Precision = 1 Scale = 1]
NonNullableBackedDataTypes.DateTime ---> [DATETIME] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
NonNullableBackedDataTypes.Decimal ---> [NUMERIC] [MaxLength = 2490385] [Precision = 38 [Precision = 2490385 Scale = 17]
NonNullableBackedDataTypes.Double ---> [DOUBLE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Enum16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Enum32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Enum64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Enum8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.EnumS8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.EnumU16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.EnumU32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.EnumU64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Int16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Int32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Int64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.PartitionId ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.SignedByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.Single ---> [FLOAT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.UnsignedInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.UnsignedInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NonNullableBackedDataTypes.UnsignedInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Boolean ---> [BOOLEAN] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Byte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Character ---> [CHAR] [MaxLength = 1] [Precision = 0 [Precision = 1 Scale = 1]
NullableBackedDataTypes.DateTime ---> [DATETIME] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
NullableBackedDataTypes.DateTimeOffset ---> [DATETIME WITH TIME ZONE] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
NullableBackedDataTypes.Decimal ---> [NUMERIC] [MaxLength = 2490385] [Precision = 38 [Precision = 2490385 Scale = 17]
NullableBackedDataTypes.Double ---> [DOUBLE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Enum16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Enum32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Enum64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Enum8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.EnumS8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.EnumU16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.EnumU32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.EnumU64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Int16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Int32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Int64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.PartitionId ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.SignedByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.Single ---> [FLOAT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.TimeSpan ---> [TIME] [MaxLength = 3] [Precision = 0 [Precision = 3 Scale = 3]
NullableBackedDataTypes.UnsignedInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.UnsignedInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
NullableBackedDataTypes.UnsignedInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Boolean ---> [BOOLEAN] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Byte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Bytes ---> [BINARY] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Character ---> [CHAR] [MaxLength = 1] [Precision = 0 [Precision = 1 Scale = 1]
ObjectBackedDataTypes.DateTime ---> [DATETIME] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
ObjectBackedDataTypes.DateTimeOffset ---> [DATETIME WITH TIME ZONE] [MaxLength = 6] [Precision = 0 [Precision = 6 Scale = 6]
ObjectBackedDataTypes.Decimal ---> [NUMERIC] [MaxLength = 2490385] [Precision = 38 [Precision = 2490385 Scale = 17]
ObjectBackedDataTypes.Double ---> [DOUBLE] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Enum16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Enum32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Enum64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Enum8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.EnumS8 ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.EnumU16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.EnumU32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.EnumU64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Int16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Int32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Int64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.PartitionId ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.SignedByte ---> [TINYINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.Single ---> [FLOAT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.String ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.TimeSpan ---> [TIME] [MaxLength = 3] [Precision = 0 [Precision = 3 Scale = 3]
ObjectBackedDataTypes.UnsignedInt16 ---> [SMALLINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.UnsignedInt32 ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
ObjectBackedDataTypes.UnsignedInt64 ---> [BIGINT] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
StringEnclosure.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
StringEnclosure.Value ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
StringForeignKeyDataType.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
StringForeignKeyDataType.StringKeyDataTypeId ---> [CHAR] [MaxLength = 255] [Precision = 0 [Precision = 255 Scale = 255]
StringKeyDataType.Id ---> [CHAR] [MaxLength = 255] [Precision = 0 [Precision = 255 Scale = 255]
UnicodeDataTypes.Id ---> [INTEGER] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
UnicodeDataTypes.StringAnsi ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
UnicodeDataTypes.StringAnsi3 ---> [CHAR] [MaxLength = 3] [Precision = 0 [Precision = 3 Scale = 3]
UnicodeDataTypes.StringAnsi9000 ---> [CHAR] [MaxLength = 9000] [Precision = 0 [Precision = 9000 Scale = 9000]
UnicodeDataTypes.StringDefault ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
UnicodeDataTypes.StringUnicode ---> [CLOB] [MaxLength = -1] [Precision = 0 [Precision = -1 Scale = -1]
";

            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true, ignoreCase: true, ignoreWhiteSpaceDifferences: true);
        }

        public static string QueryForColumnTypes(DbContext context)
        {
            const string query
                = @"SELECT
TABLE_NAME,
COL_NAME AS `COLUMN_NAME`,
TYPE_NAME AS `DATA_TYPE`,
`NOT_NULL` AS `IS_NULLABLE`,
`SCALE` AS CHARACTER_MAXIMUM_LENGTH,
(SCALE/65536)::INT AS `NUMERIC_PRECISION`,
CAST(MOD(SCALE,65536) AS INT) AS `NUMERIC_SCALE`,
`SCALE` AS DATETIME_PRECISION
FROM ALL_COLUMNS AS c
LEFT JOIN
ALL_TABLES AS t ON c.TABLE_ID=t.TABLE_ID;";


            var columns = new List<ColumnInfo>();

            using (context)
            {
                var connection = context.Database.GetDbConnection();

                var command = connection.CreateCommand();
                command.CommandText = query;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var columnInfo = new ColumnInfo
                        {
                            TableName = reader.GetString(0),
                            ColumnName = reader.GetString(1),
                            DataType = reader.GetString(2),
                            IsNullable = reader.IsDBNull(3) ? null : (bool?)(reader.GetString(3) == "YES"),
                            MaxLength = reader.IsDBNull(4) ? null : (int?)reader.GetInt64(4),
                            NumericPrecision = reader.IsDBNull(5) ? null : (int?)reader.GetInt32(5),
                            NumericScale = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6),
                            DateTimePrecision = reader.IsDBNull(7) ? null : (int?)reader.GetInt32(7)
                        };

                        columns.Add(columnInfo);
                    }
                }
            }

            var builder = new StringBuilder();

            foreach (var column in columns.OrderBy(e => e.TableName).ThenBy(e => e.ColumnName))
            {
                builder.Append(column.TableName);
                builder.Append(".");
                builder.Append(column.ColumnName);
                builder.Append(" ---> [");

                if (column.IsNullable == true)
                {
                    builder.Append("nullable ");
                }

                builder.Append(column.DataType);
                builder.Append("]");

                if (column.MaxLength.HasValue)
                {
                    builder.Append(" [MaxLength = ");
                    builder.Append(column.MaxLength);
                    builder.Append("]");
                }

                if (column.NumericPrecision.HasValue)
                {
                    builder.Append(" [Precision = ");
                    builder.Append(column.NumericPrecision);
                }

                if (column.DateTimePrecision.HasValue)
                {
                    builder.Append(" [Precision = ");
                    builder.Append(column.DateTimePrecision);
                }

                if (column.NumericScale.HasValue)
                {
                    builder.Append(" Scale = ");
                    builder.Append(column.NumericScale);
                }

                if (column.NumericPrecision.HasValue
                    || column.DateTimePrecision.HasValue
                    || column.NumericScale.HasValue)
                {
                    builder.Append("]");
                }

                builder.AppendLine();
            }

            var actual = builder.ToString();
            return actual;
        }

        [Fact]
        public void Can_get_column_types_from_built_model()
        {
            using (var context = CreateContext())
            {
                var mappingSource = context.GetService<IRelationalTypeMappingSource>();

                foreach (var property in context.Model.GetEntityTypes().SelectMany(e => e.GetDeclaredProperties()))
                {
                    var columnType = property.GetColumnType();
                    Assert.NotNull(columnType);

                    if (property[RelationalAnnotationNames.ColumnType] == null)
                    {
                        Assert.Equal(
                            columnType.ToLowerInvariant(),
                            mappingSource.FindMapping(property).StoreType.ToLowerInvariant());
                    }
                }
            }
        }

        private static readonly string eol = Environment.NewLine;
        private string Sql => Fixture.TestSqlLoggerFactory.Sql;

        public class BuiltInDataTypesXGFixture : BuiltInDataTypesFixtureBase
        {
            public override bool StrictEquality => false;

            public override bool SupportsAnsi => true;

            public override bool SupportsUnicodeToAnsiConversion => false;

            public override bool SupportsLargeStringComparisons => true;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            public override bool SupportsDecimalComparisons => false;

            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder);

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<MappedDataTypes>(
                    b =>
                        {
                            b.HasKey(e => e.Int);
                            b.Property(e => e.Int).ValueGeneratedNever();
                        });
                modelBuilder.Entity<MappedDataTypes>(entity =>
                {
                    entity.Property(e => e.ByteAsTinyint)
                        .HasColumnType("tinyint");
                });

                modelBuilder.Entity<MappedNullableDataTypes>(
                    b =>
                        {
                            b.HasKey(e => e.Int);
                            b.Property(e => e.Int).ValueGeneratedNever();
                        });

                MakeRequired<MappedDataTypes>(modelBuilder);
            }
        }

        [Flags]
        protected enum StringEnum16 : short
        {
            Value1 = 1,
            Value2 = 2,
            Value4 = 4
        }

        [Flags]
        protected enum StringEnumU16 : short
        {
            Value1 = 1,
            Value2 = 2,
            Value4 = 4
        }

        protected class MappedDataTypes
        {
            [Column(TypeName = "int")]
            public int Int { get; set; }

            [Column(TypeName = "bigint")]
            public long LongAsBigInt { get; set; }

            [Column(TypeName = "smallint")]
            public short ShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public byte ByteAsTinyint { get; set; }

            [Column(TypeName = "int")]
            public int UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public long UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short UShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte SByteAsTinyint { get; set; }

            [Column(TypeName = "boolean")]
            public bool BoolAsBit { get; set; }

            [Column(TypeName = "decimal(8,2)")]
            public decimal DecimalAsDecimal { get; set; }

            [Column(TypeName = "float")]
            public double DoubleAsFloat { get; set; }

            [Column(TypeName = "double")]
            public double DoubleAsDouble { get; set; }

            [Column(TypeName = "date")]
            public DateTime DateTimeAsDate { get; set; }

            [Column(TypeName = "varchar")]
            public DateTimeOffset DateTimeOffsetAsDatetime { get; set; }

            [Column(TypeName = "varchar")]
            public DateTimeOffset DateTimeOffsetAsTimestamp { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime DateTimeAsDatetime { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan TimeSpanAsTime { get; set; }

            [Column(TypeName = "char(10)")]
            public string StringAsChar { get; set; }

            [Column(TypeName = "char(10)")]
            public string StringAsNChar { get; set; }

            [Column(TypeName = "varchar(8001)")]
            public string StringAsVarchar { get; set; }

            [Column(TypeName = "varchar(4001)")]
            public string StringAsNvarchar { get; set; }

            [Column(TypeName = "clob")]
            public string StringAsText { get; set; }

            [Column(TypeName = "clob")]
            public string StringAsNtext { get; set; }

            [Column(TypeName = "clob")]
            public string StringAsTinytext { get; set; }

            [Column(TypeName = "clob")]
            public string StringAsMediumtext { get; set; }

            [Column(TypeName = "clob")]
            public string StringAsLongtext { get; set; }

            [Column(TypeName = "binary(255)")]
            public byte[] BytesAsVarbinary { get; set; }

            [Column(TypeName = "binary(5)")]
            public byte[] BytesAsBinary { get; set; }

            [Column(TypeName = "binary")]
            public byte[] BytesAsTinyblob { get; set; }

            [Column(TypeName = "binary")]
            public byte[] BytesAsBlob { get; set; }

            [Column(TypeName = "binary")]
            public byte[] BytesAsMediumblob { get; set; }

            //[Column(TypeName = "blob")]
            //public byte[] BytesAsLongblob { get; set; }

            [Column(TypeName = "char(36)")]
            public Guid GuidAsUniqueidentifier { get; set; }

            [Column(TypeName = "bigint")]
            public int UintAsBigint { get; set; }

            [Column(TypeName = "decimal(20,0)")]
            public long UlongAsDecimal200 { get; set; }

            [Column(TypeName = "int")]
            public short UShortAsInt { get; set; }

            [Column(TypeName = "int")]
            public short UShortAsYear { get; set; }

            [Column(TypeName = "int")]
            public int IntAsYear { get; set; }

            //[Column(TypeName = "smallint")]
            //public sbyte SByteAsSmallint { get; set; }

            [Column(TypeName = "varchar(1)")]
            public char CharAsVarchar { get; set; }

            [Column(TypeName = "varchar(20)")]
            public char CharAsNvarchar { get; set; }

            [Column(TypeName = "int")]
            public char CharAsInt { get; set; }

            [Column(TypeName = "varchar(20)")]
            public StringEnum16 EnumAsVarchar20 { get; set; }

            [Column(TypeName = "varchar(20)")]
            public StringEnumU16 EnumAsNvarchar20 { get; set; }

            [Column(TypeName = "varchar")]
            public string StringAsJson { get; set; }
        }

        protected class MappedNullableDataTypes
        {
            [Column(TypeName = "int")]
            public int? Int { get; set; }

            [Column(TypeName = "bigint")]
            public long? LongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short? ShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte? ByteAsTinyint { get; set; }

            [Column(TypeName = "int")]
            public int? UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public long? UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short? UShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte? SbyteAsTinyint { get; set; }

            [Column(TypeName = "boolean")]
            public bool? BoolAsBit { get; set; }

            [Column(TypeName = "decimal(8,2)")]
            public decimal? DecimalAsDecimal { get; set; }

            [Column(TypeName = "decimal(8,2)")]
            public decimal? DecimalAsFixed { get; set; }

            [Column(TypeName = "float")]
            public float? FloatAsFloat { get; set; }

            [Column(TypeName = "real")]
            public double? DoubleAsReal { get; set; }

            [Column(TypeName = "double")]
            public double? DoubleAsDoublePrecision { get; set; }

            [Column(TypeName = "date")]
            public DateTime? DateTimeAsDate { get; set; }

            //[Column(TypeName = "date")]
            //public DateTimeOffset? DateTimeOffsetAsDatetime { get; set; }

            //[Column(TypeName = "timestamp")]
            //public DateTimeOffset? DateTimeOffsetAsTimestamp { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime? DateTimeAsDatetime { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan? TimeSpanAsTime { get; set; }

            [Column(TypeName = "char(20)")]
            public string StringAsChar { get; set; }

            [Column(TypeName = "char(20)")]
            public string StringAsNChar { get; set; }

            [Column(TypeName = "varchar(55)")]
            public string StringAsVarchar { get; set; }

            [Column(TypeName = "varchar(55)")]
            public string StringAsNvarchar { get; set; }

            [Column(TypeName = "clob")]
            public string StringAsText { get; set; }

            [Column(TypeName = "clob")]
            public string StringAsNtext { get; set; }

            [Column(TypeName = "clob")]
            public string StringAsTinytext { get; set; }

            [Column(TypeName = "clob")]
            public string StringAsMediumtext { get; set; }

            [Column(TypeName = "binary(255)")]
            public byte[] BytesAsVarbinary { get; set; }

            [Column(TypeName = "binary(6)")]
            public byte[] BytesAsBinary { get; set; }

            [Column(TypeName = "binary")]
            public byte[] BytesAsBlob { get; set; }

            [Column(TypeName = "guid")]
            public Guid? GuidAsUniqueidentifier { get; set; }

            [Column(TypeName = "bigint")]
            public int? UintAsBigint { get; set; }

            [Column(TypeName = "decimal(20,0)")]
            public long? UlongAsDecimal200 { get; set; }

            [Column(TypeName = "int")]
            public short? UShortAsInt { get; set; }

            [Column(TypeName = "int")]
            public short? UShortAsYear { get; set; }

            [Column(TypeName = "int")]
            public int? IntAsYear { get; set; }

            //[Column(TypeName = "smallint")]
            //public sbyte? SByteAsSmallint { get; set; }

            [Column(TypeName = "varchar(1)")]
            public char? CharAsVarchar { get; set; }

            [Column(TypeName = "varchar(1)")]
            public char? CharAsNvarchar { get; set; }

            [Column(TypeName = "clob")]
            public char? CharAsText { get; set; }

            [Column(TypeName = "int")]
            public char? CharAsInt { get; set; }

            [Column(TypeName = "varchar(20)")]
            public StringEnum16? EnumAsVarchar20 { get; set; }

            [Column(TypeName = "varchar(20)")]
            public StringEnumU16? EnumAsNvarchar20 { get; set; }

            [Column(TypeName = "varchar")]
            public string StringAsJson { get; set; }
        }

        public class ColumnInfo
        {
            public string TableName { get; set; }
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public bool? IsNullable { get; set; }
            public int? MaxLength { get; set; }
            public int? NumericPrecision { get; set; }
            public int? NumericScale { get; set; }
            public int? DateTimePrecision { get; set; }
        }
    }
}
