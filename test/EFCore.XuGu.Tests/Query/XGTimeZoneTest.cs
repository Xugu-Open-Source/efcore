// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.Query
{
    public sealed class XGTimeZoneTest : TestWithFixture<XGTimeZoneTest.XGTimeZoneFixture>
    {
        public XGTimeZoneTest(XGTimeZoneFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public void ConvertTimeZone()
        {
            using var context = Fixture.CreateContext();
            SetSessionTimeZone(context);
            Fixture.ClearSql();

            var metalContainer = context.Set<Model.Container>()
                .Where(c => c.DeliveredDateTimeOffset == c.DeliveredDateTimeUtc &&
                            EF.Functions.ConvertTimeZone(c.DeliveredDateTimeOffset, c.DeliveredTimeZone) == c.DeliveredDateTimeLocal)
                .Select(
                    c => new
                    {
                        c.DeliveredDateTimeUtc,
                        c.DeliveredDateTimeLocal,
                        c.DeliveredDateTimeOffset,
                        c.DeliveredTimeZone,
                        DeliveredWithAppliedTimeZone = EF.Functions.ConvertTimeZone(c.DeliveredDateTimeOffset, c.DeliveredTimeZone),
                        DeliveredConvertedToDifferent = EF.Functions.ConvertTimeZone(c.DeliveredDateTimeLocal, c.DeliveredTimeZone, "+06:00"),
                    })
                .Single();

            Assert.Equal(XGTimeZoneFixture.OriginalDateTimeUtc, metalContainer.DeliveredDateTimeUtc);
            Assert.Equal(XGTimeZoneFixture.OriginalDateTime, metalContainer.DeliveredDateTimeLocal);
            Assert.Equal(XGTimeZoneFixture.OriginalDateTimeOffset, metalContainer.DeliveredDateTimeOffset);
            Assert.Equal(XGTimeZoneFixture.OriginalDateTimeOffset.UtcDateTime, metalContainer.DeliveredDateTimeOffset.DateTime);
            Assert.Equal(TimeSpan.Zero, metalContainer.DeliveredDateTimeOffset.Offset);
            Assert.Equal(XGTimeZoneFixture.OriginalDateTime, metalContainer.DeliveredWithAppliedTimeZone);
            Assert.Equal(XGTimeZoneFixture.OriginalDateTimeUtc.AddHours(6), metalContainer.DeliveredConvertedToDifferent);

            Assert.Equal(
                """
SELECT `c`.`DeliveredDateTimeUtc`, `c`.`DeliveredDateTimeLocal`, `c`.`DeliveredDateTimeOffset`, `c`.`DeliveredTimeZone`, CONVERT_TZ(`c`.`DeliveredDateTimeOffset`, '+00:00', `c`.`DeliveredTimeZone`) AS `DeliveredWithAppliedTimeZone`, CONVERT_TZ(`c`.`DeliveredDateTimeLocal`, `c`.`DeliveredTimeZone`, '+06:00') AS `DeliveredConvertedToDifferent`
FROM `Container` AS `c`
WHERE (`c`.`DeliveredDateTimeOffset` = `c`.`DeliveredDateTimeUtc`) AND (CONVERT_TZ(`c`.`DeliveredDateTimeOffset`, '+00:00', `c`.`DeliveredTimeZone`) = `c`.`DeliveredDateTimeLocal`)
LIMIT 2
""",
                Fixture.Sql);
        }

        [ConditionalFact]
        public void DateTimeOffset_LocalDateTime()
        {
            using var context = Fixture.CreateContext();
            SetSessionTimeZone(context);
            Fixture.ClearSql();

            var metalContainer = context.Set<Model.Container>()
                .Where(c => c.DeliveredDateTimeOffset.LocalDateTime == XGTimeZoneFixture.OriginalDateTimeUtc.AddHours(XGTimeZoneFixture.SessionOffset))
                .Select(c => new { c.DeliveredDateTimeOffset.LocalDateTime })
                .Single();

            Assert.Equal(XGTimeZoneFixture.OriginalDateTimeUtc.AddHours(XGTimeZoneFixture.SessionOffset), metalContainer.LocalDateTime);

            Assert.Equal(
                """
SELECT CONVERT_TZ(`c`.`DeliveredDateTimeOffset`, '+00:00', @@session.time_zone) AS `LocalDateTime`
FROM `Container` AS `c`
WHERE CONVERT_TZ(`c`.`DeliveredDateTimeOffset`, '+00:00', @@session.time_zone) = TIMESTAMP '2023-12-31 15:00:00'
LIMIT 2
""",
                Fixture.Sql);
        }

        private static void SetSessionTimeZone(XGTimeZoneFixture.XGTimeZoneContext context)
        {
            context.Database.OpenConnection();
            var connection = context.Database.GetDbConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SET @@session.time_zone = '-08:00';";
            command.ExecuteNonQuery();
        }

        public class XGTimeZoneFixture : XGTestFixtureBase<XGTimeZoneFixture.XGTimeZoneContext>
        {
            public const int SessionOffset = -8; // UTC-8
            public const int OriginalOffset = 2; // UTC+2
            public static readonly DateTime OriginalDateTimeUtc = new DateTime(2023, 12, 31, 23, 0, 0);
            public static readonly DateTime OriginalDateTime = OriginalDateTimeUtc.AddHours(OriginalOffset);
            public static readonly DateTimeOffset OriginalDateTimeOffset = new DateTimeOffset(OriginalDateTime, TimeSpan.FromHours(OriginalOffset));

            public void ClearSql()
                => base.SqlCommands.Clear();

            public new string Sql
                => base.Sql;

            public class XGTimeZoneContext : ContextBase
            {
                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    modelBuilder.Entity<Model.Container>(
                        entity =>
                        {
                            entity.HasData(
                                new Model.Container
                                {
                                    Id = 1,
                                    Name = "Heavymetal",
                                    DeliveredDateTimeUtc = OriginalDateTimeUtc,
                                    DeliveredDateTimeLocal = OriginalDateTime,
                                    DeliveredDateTimeOffset = OriginalDateTimeOffset,
                                    DeliveredTimeZone = "+02:00",
                                });
                        });
                }
            }
        }

        private static class Model
        {
            public class Container
            {
                public int Id { get ; set; }
                public string Name { get ; set; }
                public DateTime DeliveredDateTimeUtc { get; set; }
                public DateTime DeliveredDateTimeLocal { get; set; }
                public DateTimeOffset DeliveredDateTimeOffset { get; set; }
                public string DeliveredTimeZone { get; set; }
            }
        }
    }
}
