// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class DataAnnotationXuGuDbTest : DataAnnotationTestBase<XuGuDbTestStore, DataAnnotationXuGuDbFixture>
    {
        public DataAnnotationXuGuDbTest(DataAnnotationXuGuDbFixture fixture)
            : base(fixture)
        {
        }

        public override void ConcurrencyCheckAttribute_throws_if_value_in_database_changed()
        {
            base.ConcurrencyCheckAttribute_throws_if_value_in_database_changed();

            Assert.Equal(@"SELECT `r`.`UniqueNo`, `r`.`MaxLengthProperty`, `r`.`Name`, `r`.`RowVersion`
FROM `Sample` `r`
WHERE `r`.`UniqueNo` = 1 LIMIT 1

SELECT `r`.`UniqueNo`, `r`.`MaxLengthProperty`, `r`.`Name`, `r`.`RowVersion`
FROM `Sample` `r`
WHERE `r`.`UniqueNo` = 1 LIMIT 1

:p2: 1
:p0: ModifiedData (Nullable = false) (Size = 4000)
:p1: 00000000-0000-0000-0003-000000000001
:p3: 00000001-0000-0000-0000-000000000001

UPDATE `Sample` SET `Name` = :p0, `RowVersion` = :p1
WHERE `UniqueNo` = :p2 AND `RowVersion` = :p3;
SELECT SQL%ROWCOUNT;

:p2: 1
:p0: ChangedData (Nullable = false) (Size = 4000)
:p1: 00000000-0000-0000-0002-000000000001
:p3: 00000001-0000-0000-0000-000000000001


UPDATE `Sample` SET `Name` = :p0, `RowVersion` = :p1
WHERE `UniqueNo` = :p2 AND `RowVersion` = :p3;
SELECT SQL%ROWCOUNT;",
                Sql);
        }

        public override void DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity()
        {
            base.DatabaseGeneratedAttribute_autogenerates_values_when_set_to_identity();

            Assert.Equal(@":p0:  (Size = 10) (DbType = String)
:p1: Third (Nullable = false) (Size = 4000)
:p2: 00000000-0000-0000-0000-000000000003


INSERT INTO `Sample` (`MaxLengthProperty`, `Name`, `RowVersion`)
VALUES (:p0, :p1, :p2);
SELECT `UniqueNo`
FROM `Sample`
WHERE SQL%ROWCOUNT = 1 AND `UniqueNo` = scope_identity();",
                Sql);
        }

        public override void MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            base.MaxLengthAttribute_throws_while_inserting_value_longer_than_max_length();

            Assert.Equal(@":p0: Short (Size = 10)
:p1: ValidString (Nullable = false) (Size = 4000)
:p2: 00000000-0000-0000-0000-000000000001


INSERT INTO `Sample` (`MaxLengthProperty`, `Name`, `RowVersion`)
VALUES (:p0, :p1, :p2);
SELECT `UniqueNo`
FROM `Sample`
WHERE SQL%ROWCOUNT = 1 AND `UniqueNo` = scope_identity();

:p0: VeryVeryVeryVeryVeryVeryLongString (Size = -1)
:p1: ValidString (Nullable = false) (Size = 4000)
:p2: 00000000-0000-0000-0000-000000000002


INSERT INTO `Sample` (`MaxLengthProperty`, `Name`, `RowVersion`)
VALUES (:p0, :p1, :p2);
SELECT `UniqueNo`
FROM `Sample`
WHERE SQL%ROWCOUNT = 1 AND `UniqueNo` = scope_identity();",
                Sql);
        }

        public override void RequiredAttribute_for_navigation_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_navigation_throws_while_inserting_null_value();

            Assert.Contains(@":p1: Book1 (Nullable = false) (Size = 450)
",
                Sql);

            Assert.Contains(@":p1:  (Nullable = false) (Size = 450) (DbType = String)
",
                Sql);
        }

        public override void RequiredAttribute_for_property_throws_while_inserting_null_value()
        {
            base.RequiredAttribute_for_property_throws_while_inserting_null_value();

            Assert.Equal(@":p0:  (Size = 10) (DbType = String)
:p1: ValidString (Nullable = false) (Size = 4000)
:p2: 00000000-0000-0000-0000-000000000001


INSERT INTO `Sample` (`MaxLengthProperty`, `Name`, `RowVersion`)
VALUES (:p0, :p1, :p2);
SELECT `UniqueNo`
FROM `Sample`
WHERE SQL%ROWCOUNT = 1 AND `UniqueNo` = scope_identity();

:p0:  (Size = 10) (DbType = String)
:p1:  (Nullable = false) (Size = 4000) (DbType = String)
:p2: 00000000-0000-0000-0000-000000000002


INSERT INTO `Sample` (`MaxLengthProperty`, `Name`, `RowVersion`)
VALUES (:p0, :p1, :p2);
SELECT `UniqueNo`
FROM `Sample`
WHERE SQL%ROWCOUNT = 1 AND `UniqueNo` = scope_identity();",
                Sql);
        }

        public override void StringLengthAttribute_throws_while_inserting_value_longer_than_max_length()
        {
            base.StringLengthAttribute_throws_while_inserting_value_longer_than_max_length();

            Assert.Equal(@":p0: ValidString (Size = 16)


INSERT INTO `Two` (`Data`)
VALUES (:p0);
SELECT `Id`, `Timestamp`
FROM `Two`
WHERE SQL%ROWCOUNT = 1 AND `Id` = scope_identity();

:p0: ValidButLongString (Size = -1)


INSERT INTO `Two` (`Data`)
VALUES (:p0);
SELECT `Id`, `Timestamp`
FROM `Two`
WHERE SQL%ROWCOUNT = 1 AND `Id` = scope_identity();",
                Sql);
        }

        public override void TimestampAttribute_throws_if_value_in_database_changed()
        {
            base.TimestampAttribute_throws_if_value_in_database_changed();

            Assert.Equal(@"SELECT TOP(1) `r`.`Id`, `r`.`Data`, `r`.`Timestamp`
FROM `Two` `r`
WHERE `r`.`Id` = 1

SELECT TOP(1) `r`.`Id`, `r`.`Data`, `r`.`Timestamp`
FROM `Two` `r`
WHERE `r`.`Id` = 1

:p1: 1
:p0: ModifiedData (Size = 16)
:p2: 0x00000000000007D1 (Size = 8)


DECLARE @inserted0 TABLE (`Timestamp` varbinary(8));
UPDATE `Two` SET `Data` = :p0
OUTPUT INSERTED.`Timestamp`
INTO @inserted0
WHERE `Id` = :p1 AND `Timestamp` = :p2;
SELECT `Timestamp` FROM @inserted0;

:p1: 1
:p0: ChangedData (Size = 16)
:p2: 0x00000000000007D1 (Size = 8)


DECLARE @inserted0 TABLE (`Timestamp` varbinary(8));
UPDATE `Two` SET `Data` = :p0
OUTPUT INSERTED.`Timestamp`
INTO @inserted0
WHERE `Id` = :p1 AND `Timestamp` = :p2;
SELECT `Timestamp` FROM @inserted0;",
                Sql);
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
