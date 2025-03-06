// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    [MonoVersionCondition(Min = "4.2.0", SkipReason = "Queries fail on Mono < 4.2.0 due to differences in the implementation of LINQ")]
    public class GearsOfWarQueryXuGuDbTest : GearsOfWarQueryTestBase<XuGuDbTestStore, GearsOfWarQueryXuGuDbFixture>
    {
        [Fact]
        public override void Entity_equality_empty()
        {
            base.Entity_equality_empty();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND (`g`.`Nickname` IS NULL AND (`g`.`SquadId` = 0))",
                Sql);
        }

        [Fact]
        public override void Include_multiple_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_many();

            Assert.Equal(
                @"SELECT `t`.`Id`, `t`.`GearNickName`, `t`.`GearSquadId`, `t`.`Note`, `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `CogTag` `t`
LEFT JOIN (
    SELECT `g`.*
    FROM `Gear` `g`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g` ON (`t`.`GearNickName` = `g`.`Nickname`) AND (`t`.`GearSquadId` = `g`.`SquadId`)
ORDER BY `g`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE EXISTS (
    SELECT 1
    FROM `CogTag` `t`
    LEFT JOIN (
        SELECT `g`.*
        FROM `Gear` `g`
        WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
    ) `g` ON (`t`.`GearNickName` = `g`.`Nickname`) AND (`t`.`GearSquadId` = `g`.`SquadId`)
    WHERE `w`.`OwnerFullName` = `g`.`FullName`)
ORDER BY `w`.`OwnerFullName`",
                Sql);
        }

        [Fact]
        public override void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            base.Include_multiple_one_to_one_and_one_to_many_self_reference();

            Assert.Equal(
                @"SELECT `t`.`Id`, `t`.`GearNickName`, `t`.`GearSquadId`, `t`.`Note`, `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `CogTag` `t`
LEFT JOIN (
    SELECT `g`.*
    FROM `Gear` `g`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g` ON (`t`.`GearNickName` = `g`.`Nickname`) AND (`t`.`GearSquadId` = `g`.`SquadId`)
ORDER BY `g`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE EXISTS (
    SELECT 1
    FROM `CogTag` `t`
    LEFT JOIN (
        SELECT `g`.*
        FROM `Gear` `g`
        WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
    ) `g` ON (`t`.`GearNickName` = `g`.`Nickname`) AND (`t`.`GearSquadId` = `g`.`SquadId`)
    WHERE `w`.`OwnerFullName` = `g`.`FullName`)
ORDER BY `w`.`OwnerFullName`",
                Sql);
        }

        [Fact]
        public override void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many();

            Assert.Equal(
                @"SELECT `t`.`Id`, `t`.`GearNickName`, `t`.`GearSquadId`, `t`.`Note`, `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `s`.`Id`, `s`.`InternalNumber`, `s`.`Name`
FROM `CogTag` `t`
LEFT JOIN (
    SELECT `g`.*
    FROM `Gear` `g`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g` ON (`t`.`GearNickName` = `g`.`Nickname`) AND (`t`.`GearSquadId` = `g`.`SquadId`)
LEFT JOIN `Squad` `s` ON `g`.`SquadId` = `s`.`Id`
ORDER BY `s`.`Id`

SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
FROM `Gear` `g0`
WHERE `g0`.`Discriminator` IN ('Officer', 'Gear') AND EXISTS (
    SELECT 1
    FROM `CogTag` `t`
    LEFT JOIN (
        SELECT `g`.*
        FROM `Gear` `g`
        WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
    ) `g` ON (`t`.`GearNickName` = `g`.`Nickname`) AND (`t`.`GearSquadId` = `g`.`SquadId`)
    LEFT JOIN `Squad` `s` ON `g`.`SquadId` = `s`.`Id`
    WHERE `g0`.`SquadId` = `s`.`Id`)
ORDER BY `g0`.`SquadId`", Sql);
        }

        [Fact]
        public override void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            base.Include_multiple_one_to_one_optional_and_one_to_one_required();

            Assert.Equal(
                @"SELECT `t`.`Id`, `t`.`GearNickName`, `t`.`GearSquadId`, `t`.`Note`, `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `s`.`Id`, `s`.`InternalNumber`, `s`.`Name`
FROM `CogTag` `t`
LEFT JOIN (
    SELECT `g`.*
    FROM `Gear` `g`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g` ON (`t`.`GearNickName` = `g`.`Nickname`) AND (`t`.`GearSquadId` = `g`.`SquadId`)
LEFT JOIN `Squad` `s` ON `g`.`SquadId` = `s`.`Id`",
                Sql);
        }

        [Fact]
        public override void Include_multiple_circular()
        {
            base.Include_multiple_circular();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `c`.`Name`, `c`.`Location`
FROM `Gear` `g`
INNER JOIN `City` `c` ON `g`.`CityOrBirthName` = `c`.`Name`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `c`.`Name`

SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
FROM `Gear` `g0`
WHERE `g0`.`Discriminator` IN ('Officer', 'Gear') AND EXISTS (
    SELECT 1
    FROM `Gear` `g`
    INNER JOIN `City` `c` ON `g`.`CityOrBirthName` = `c`.`Name`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND (`g0`.`AssignedCityName` = `c`.`Name`))
ORDER BY `g0`.`AssignedCityName`",
                Sql);
        }

        [Fact]
        public override void Include_multiple_circular_with_filter()
        {
            base.Include_multiple_circular_with_filter();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `c`.`Name`, `c`.`Location`
FROM `Gear` `g`
INNER JOIN `City` `c` ON `g`.`CityOrBirthName` = `c`.`Name`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND (`g`.`Nickname` = 'Marcus')
ORDER BY `c`.`Name`

SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
FROM `Gear` `g0`
WHERE `g0`.`Discriminator` IN ('Officer', 'Gear') AND EXISTS (
    SELECT 1
    FROM `Gear` `g`
    INNER JOIN `City` `c` ON `g`.`CityOrBirthName` = `c`.`Name`
    WHERE (`g`.`Discriminator` IN ('Officer', 'Gear') AND (`g`.`Nickname` = 'Marcus')) AND (`g0`.`AssignedCityName` = `c`.`Name`))
ORDER BY `g0`.`AssignedCityName`",
                Sql);
        }

        [Fact]
        public override void Include_using_alternate_key()
        {
            base.Include_using_alternate_key();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND (`g`.`Nickname` = 'Marcus')
ORDER BY `g`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE EXISTS (
    SELECT 1
    FROM `Gear` `g`
    WHERE (`g`.`Discriminator` IN ('Officer', 'Gear') AND (`g`.`Nickname` = 'Marcus')) AND (`w`.`OwnerFullName` = `g`.`FullName`))
ORDER BY `w`.`OwnerFullName`",
                Sql);
        }

        [Fact]
        public override void Include_multiple_include_then_include()
        {
            base.Include_multiple_include_then_include();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `c`.`Name`, `c`.`Location`, `c2`.`Name`, `c2`.`Location`, `c4`.`Name`, `c4`.`Location`, `c6`.`Name`, `c6`.`Location`
FROM `Gear` `g`
LEFT JOIN `City` `c` ON `g`.`AssignedCityName` = `c`.`Name`
LEFT JOIN `City` `c2` ON `g`.`AssignedCityName` = `c2`.`Name`
INNER JOIN `City` `c4` ON `g`.`CityOrBirthName` = `c4`.`Name`
INNER JOIN `City` `c6` ON `g`.`CityOrBirthName` = `c6`.`Name`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `g`.`Nickname`, `c`.`Name`, `c2`.`Name`, `c4`.`Name`, `c6`.`Name`

SELECT `g3`.`Nickname`, `g3`.`SquadId`, `g3`.`AssignedCityName`, `g3`.`CityOrBirthName`, `g3`.`Discriminator`, `g3`.`FullName`, `g3`.`LeaderNickname`, `g3`.`LeaderSquadId`, `g3`.`Rank`, `c7`.`Id`, `c7`.`GearNickName`, `c7`.`GearSquadId`, `c7`.`Note`
FROM `Gear` `g3`
INNER JOIN (
    SELECT DISTINCT `g`.`Nickname`, `c`.`Name`, `c2`.`Name` `Name0`, `c4`.`Name` `Name1`, `c6`.`Name` `Name2`
    FROM `Gear` `g`
    LEFT JOIN `City` `c` ON `g`.`AssignedCityName` = `c`.`Name`
    LEFT JOIN `City` `c2` ON `g`.`AssignedCityName` = `c2`.`Name`
    INNER JOIN `City` `c4` ON `g`.`CityOrBirthName` = `c4`.`Name`
    INNER JOIN `City` `c6` ON `g`.`CityOrBirthName` = `c6`.`Name`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `c60` ON `g3`.`AssignedCityName` = `c60`.`Name2`
LEFT JOIN `CogTag` `c7` ON (`c7`.`GearNickName` = `g3`.`Nickname`) AND (`c7`.`GearSquadId` = `g3`.`SquadId`)
WHERE `g3`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `c60`.`Nickname`, `c60`.`Name`, `c60`.`Name0`, `c60`.`Name1`, `c60`.`Name2`

SELECT `g2`.`Nickname`, `g2`.`SquadId`, `g2`.`AssignedCityName`, `g2`.`CityOrBirthName`, `g2`.`Discriminator`, `g2`.`FullName`, `g2`.`LeaderNickname`, `g2`.`LeaderSquadId`, `g2`.`Rank`, `c5`.`Id`, `c5`.`GearNickName`, `c5`.`GearSquadId`, `c5`.`Note`
FROM `Gear` `g2`
INNER JOIN (
    SELECT DISTINCT `g`.`Nickname`, `c`.`Name`, `c2`.`Name` `Name0`, `c4`.`Name` `Name1`
    FROM `Gear` `g`
    LEFT JOIN `City` `c` ON `g`.`AssignedCityName` = `c`.`Name`
    LEFT JOIN `City` `c2` ON `g`.`AssignedCityName` = `c2`.`Name`
    INNER JOIN `City` `c4` ON `g`.`CityOrBirthName` = `c4`.`Name`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `c40` ON `g2`.`CityOrBirthName` = `c40`.`Name1`
LEFT JOIN `CogTag` `c5` ON (`c5`.`GearNickName` = `g2`.`Nickname`) AND (`c5`.`GearSquadId` = `g2`.`SquadId`)
WHERE `g2`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `c40`.`Nickname`, `c40`.`Name`, `c40`.`Name0`, `c40`.`Name1`

SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`, `c3`.`Id`, `c3`.`GearNickName`, `c3`.`GearSquadId`, `c3`.`Note`
FROM `Gear` `g1`
INNER JOIN (
    SELECT DISTINCT `g`.`Nickname`, `c`.`Name`, `c2`.`Name` `Name0`
    FROM `Gear` `g`
    LEFT JOIN `City` `c` ON `g`.`AssignedCityName` = `c`.`Name`
    LEFT JOIN `City` `c2` ON `g`.`AssignedCityName` = `c2`.`Name`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `c20` ON `g1`.`AssignedCityName` = `c20`.`Name0`
LEFT JOIN `CogTag` `c3` ON (`c3`.`GearNickName` = `g1`.`Nickname`) AND (`c3`.`GearSquadId` = `g1`.`SquadId`)
WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `c20`.`Nickname`, `c20`.`Name`, `c20`.`Name0`

SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`, `c1`.`Id`, `c1`.`GearNickName`, `c1`.`GearSquadId`, `c1`.`Note`
FROM `Gear` `g0`
INNER JOIN (
    SELECT DISTINCT `g`.`Nickname`, `c`.`Name`
    FROM `Gear` `g`
    LEFT JOIN `City` `c` ON `g`.`AssignedCityName` = `c`.`Name`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `c0` ON `g0`.`CityOrBirthName` = `c0`.`Name`
LEFT JOIN `CogTag` `c1` ON (`c1`.`GearNickName` = `g0`.`Nickname`) AND (`c1`.`GearSquadId` = `g0`.`SquadId`)
WHERE `g0`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `c0`.`Nickname`, `c0`.`Name`",
                Sql);
        }

        [Fact]
        public override void Include_navigation_on_derived_type()
        {
            base.Include_navigation_on_derived_type();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` = 'Officer'
ORDER BY `g`.`Nickname`, `g`.`SquadId`

SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
FROM `Gear` `g0`
WHERE `g0`.`Discriminator` IN ('Officer', 'Gear') AND EXISTS (
    SELECT 1
    FROM `Gear` `g`
    WHERE (`g`.`Discriminator` = 'Officer') AND ((`g0`.`LeaderNickname` = `g`.`Nickname`) AND (`g0`.`LeaderSquadId` = `g`.`SquadId`)))
ORDER BY `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();

            Assert.Equal(
                @"SELECT `o`.`Id`, `o`.`GearNickName`, `o`.`GearSquadId`, `o`.`Note`, `o.Gear`.`Nickname`, `o.Gear`.`SquadId`, `o.Gear`.`AssignedCityName`, `o.Gear`.`CityOrBirthName`, `o.Gear`.`Discriminator`, `o.Gear`.`FullName`, `o.Gear`.`LeaderNickname`, `o.Gear`.`LeaderSquadId`, `o.Gear`.`Rank`, `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `CogTag` `o`
LEFT JOIN `Gear` `o.Gear` ON (`o`.`GearNickName` = `o.Gear`.`Nickname`) AND (`o`.`GearSquadId` = `o.Gear`.`SquadId`)
LEFT JOIN (
    SELECT `g`.*
    FROM `Gear` `g`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g` ON (`o`.`GearNickName` = `g`.`Nickname`) AND (`o`.`GearSquadId` = `g`.`SquadId`)
ORDER BY `o`.`GearNickName`, `o`.`GearSquadId`",
                Sql);
        }

        [Fact]
        public override void Include_with_join_reference1()
        {
            base.Include_with_join_reference1();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `c`.`Name`, `c`.`Location`
FROM `Gear` `g`
INNER JOIN `CogTag` `t` ON (`g`.`SquadId` = `t`.`GearSquadId`) AND (`g`.`Nickname` = `t`.`GearNickName`)
INNER JOIN `City` `c` ON `g`.`CityOrBirthName` = `c`.`Name`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')",
                Sql);
        }

        [Fact]
        public override void Include_with_join_reference2()
        {
            base.Include_with_join_reference2();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `c`.`Name`, `c`.`Location`
FROM `CogTag` `t`
INNER JOIN `Gear` `g` ON (`t`.`GearSquadId` = `g`.`SquadId`) AND (`t`.`GearNickName` = `g`.`Nickname`)
INNER JOIN `City` `c` ON `g`.`CityOrBirthName` = `c`.`Name`",
                Sql);
        }

        [Fact]
        public override void Include_with_join_collection1()
        {
            base.Include_with_join_collection1();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
INNER JOIN `CogTag` `t` ON (`g`.`SquadId` = `t`.`GearSquadId`) AND (`g`.`Nickname` = `t`.`GearNickName`)
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `g`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE EXISTS (
    SELECT 1
    FROM `Gear` `g`
    INNER JOIN `CogTag` `t` ON (`g`.`SquadId` = `t`.`GearSquadId`) AND (`g`.`Nickname` = `t`.`GearNickName`)
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND (`w`.`OwnerFullName` = `g`.`FullName`))
ORDER BY `w`.`OwnerFullName`",
                Sql);
        }

        [Fact]
        public override void Include_with_join_collection2()
        {
            base.Include_with_join_collection2();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `CogTag` `t`
INNER JOIN `Gear` `g` ON (`t`.`GearSquadId` = `g`.`SquadId`) AND (`t`.`GearNickName` = `g`.`Nickname`)
ORDER BY `g`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE EXISTS (
    SELECT 1
    FROM `CogTag` `t`
    INNER JOIN `Gear` `g` ON (`t`.`GearSquadId` = `g`.`SquadId`) AND (`t`.`GearNickName` = `g`.`Nickname`)
    WHERE `w`.`OwnerFullName` = `g`.`FullName`)
ORDER BY `w`.`OwnerFullName`",
                Sql);
        }

        [Fact]
        public override void Include_where_list_contains_navigation()
        {
            base.Include_where_list_contains_navigation();

            Assert.Contains(
                @"SELECT `t`.`Id`
FROM `CogTag` `t`",
                Sql);

            Assert.Contains(
                @"SELECT `g.Tag0`.`Id`, `g.Tag0`.`GearNickName`, `g.Tag0`.`GearSquadId`, `g.Tag0`.`Note`
FROM `CogTag` `g.Tag0`", Sql);

            Assert.Contains(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `g.Tag`.`Id`, `g.Tag`.`GearNickName`, `g.Tag`.`GearSquadId`, `g.Tag`.`Note`, `c`.`Id`, `c`.`GearNickName`, `c`.`GearSquadId`, `c`.`Note`
FROM `Gear` `g`
LEFT JOIN `CogTag` `g.Tag` ON (`g`.`Nickname` = `g.Tag`.`GearNickName`) AND (`g`.`SquadId` = `g.Tag`.`GearSquadId`)
LEFT JOIN `CogTag` `c` ON (`c`.`GearNickName` = `g`.`Nickname`) AND (`c`.`GearSquadId` = `g`.`SquadId`)
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `g`.`Nickname`, `g`.`SquadId`",
                Sql);
        }

        [Fact]
        public override void Include_with_join_multi_level()
        {
            base.Include_with_join_multi_level();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `c`.`Name`, `c`.`Location`
FROM `Gear` `g`
INNER JOIN `CogTag` `t` ON (`g`.`SquadId` = `t`.`GearSquadId`) AND (`g`.`Nickname` = `t`.`GearNickName`)
INNER JOIN `City` `c` ON `g`.`CityOrBirthName` = `c`.`Name`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `c`.`Name`

SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
FROM `Gear` `g0`
WHERE `g0`.`Discriminator` IN ('Officer', 'Gear') AND EXISTS (
    SELECT 1
    FROM `Gear` `g`
    INNER JOIN `CogTag` `t` ON (`g`.`SquadId` = `t`.`GearSquadId`) AND (`g`.`Nickname` = `t`.`GearNickName`)
    INNER JOIN `City` `c` ON `g`.`CityOrBirthName` = `c`.`Name`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND (`g0`.`AssignedCityName` = `c`.`Name`))
ORDER BY `g0`.`AssignedCityName`",
                Sql);
        }

        [Fact]
        public override void Include_with_join_and_inheritance1()
        {
            base.Include_with_join_and_inheritance1();

            Assert.Equal(
                @"SELECT `t0`.`Nickname`, `t0`.`SquadId`, `t0`.`AssignedCityName`, `t0`.`CityOrBirthName`, `t0`.`Discriminator`, `t0`.`FullName`, `t0`.`LeaderNickname`, `t0`.`LeaderSquadId`, `t0`.`Rank`, `c`.`Name`, `c`.`Location`
FROM `CogTag` `t`
INNER JOIN (
    SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
    FROM `Gear` `g0`
    WHERE `g0`.`Discriminator` = 'Officer'
) `t0` ON (`t`.`GearSquadId` = `t0`.`SquadId`) AND (`t`.`GearNickName` = `t0`.`Nickname`)
INNER JOIN `City` `c` ON `t0`.`CityOrBirthName` = `c`.`Name`",
                Sql);
        }

        [Fact]
        public override void Include_with_join_and_inheritance2()
        {
            base.Include_with_join_and_inheritance2();

            Assert.Equal(
                @"SELECT `t`.`Nickname`, `t`.`SquadId`, `t`.`AssignedCityName`, `t`.`CityOrBirthName`, `t`.`Discriminator`, `t`.`FullName`, `t`.`LeaderNickname`, `t`.`LeaderSquadId`, `t`.`Rank`
FROM (
    SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
    FROM `Gear` `g0`
    WHERE `g0`.`Discriminator` = 'Officer'
) `t`
INNER JOIN `CogTag` `t0` ON (`t`.`SquadId` = `t0`.`GearSquadId`) AND (`t`.`Nickname` = `t0`.`GearNickName`)
ORDER BY `t`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
        FROM `Gear` `g0`
        WHERE `g0`.`Discriminator` = 'Officer'
    ) `t`
    INNER JOIN `CogTag` `t0` ON (`t`.`SquadId` = `t0`.`GearSquadId`) AND (`t`.`Nickname` = `t0`.`GearNickName`)
    WHERE `w`.`OwnerFullName` = `t`.`FullName`)
ORDER BY `w`.`OwnerFullName`",
                Sql);
        }

        [Fact]
        public override void Include_with_join_and_inheritance3()
        {
            base.Include_with_join_and_inheritance3();

            Assert.Equal(
                @"SELECT `t0`.`Nickname`, `t0`.`SquadId`, `t0`.`AssignedCityName`, `t0`.`CityOrBirthName`, `t0`.`Discriminator`, `t0`.`FullName`, `t0`.`LeaderNickname`, `t0`.`LeaderSquadId`, `t0`.`Rank`
FROM `CogTag` `t`
INNER JOIN (
    SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
    FROM `Gear` `g0`
    WHERE `g0`.`Discriminator` = 'Officer'
) `t0` ON (`t`.`GearSquadId` = `t0`.`SquadId`) AND (`t`.`GearNickName` = `t0`.`Nickname`)
ORDER BY `t0`.`Nickname`, `t0`.`SquadId`

SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
FROM `Gear` `g1`
WHERE `g1`.`Discriminator` IN ('Officer', 'Gear') AND EXISTS (
    SELECT 1
    FROM `CogTag` `t`
    INNER JOIN (
        SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
        FROM `Gear` `g0`
        WHERE `g0`.`Discriminator` = 'Officer'
    ) `t0` ON (`t`.`GearSquadId` = `t0`.`SquadId`) AND (`t`.`GearNickName` = `t0`.`Nickname`)
    WHERE (`g1`.`LeaderNickname` = `t0`.`Nickname`) AND (`g1`.`LeaderSquadId` = `t0`.`SquadId`))
ORDER BY `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`",
                Sql);
        }

        [Fact]
        public override void Include_with_nested_navigation_in_order_by()
        {
            base.Include_with_nested_navigation_in_order_by();

            Assert.Contains(
                @"SELECT `w.Owner.CityOfBirth`.`Name`, `w.Owner.CityOfBirth`.`Location`
FROM `City` `w.Owner.CityOfBirth`",
                Sql);

            Assert.Contains(
                @"SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`, `w.Owner`.`Nickname`, `w.Owner`.`SquadId`, `w.Owner`.`AssignedCityName`, `w.Owner`.`CityOrBirthName`, `w.Owner`.`Discriminator`, `w.Owner`.`FullName`, `w.Owner`.`LeaderNickname`, `w.Owner`.`LeaderSquadId`, `w.Owner`.`Rank`, `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Weapon` `w`
LEFT JOIN `Gear` `w.Owner` ON `w`.`OwnerFullName` = `w.Owner`.`FullName`
LEFT JOIN (
    SELECT `g`.*
    FROM `Gear` `g`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g` ON `w`.`OwnerFullName` = `g`.`FullName`
ORDER BY `w`.`OwnerFullName`",
                Sql);
        }

        [Fact]
        public override void Where_enum()
        {
            base.Where_enum();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND (`g`.`Rank` = 2)",
                Sql);
        }

        [Fact]
        public override void Where_nullable_enum_with_constant()
        {
            base.Where_nullable_enum_with_constant();

            Assert.Equal(
                @"SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` = 1",
                Sql);
        }

        [Fact]
        public override void Where_nullable_enum_with_null_constant()
        {
            base.Where_nullable_enum_with_null_constant();

            Assert.Equal(
                @"SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` IS NULL",
                Sql);
        }

        [Fact]
        public override void Where_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_nullable_enum_with_non_nullable_parameter();

            Assert.Equal(
                @":__ammunitionType_0: Cartridge

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` = :__ammunitionType_0",
                Sql);
        }

        [Fact]
        public override void Where_nullable_enum_with_nullable_parameter()
        {
            base.Where_nullable_enum_with_nullable_parameter();

            Assert.Equal(
                @":__ammunitionType_0: Cartridge (Nullable = true)

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` = :__ammunitionType_0

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` IS NULL",
                Sql);
        }

        [Fact]
        public override void Where_bitwise_and_enum()
        {
            base.Where_bitwise_and_enum();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND (`g`.`Rank` & 1 > 0)

SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND (`g`.`Rank` & 1 = 1)",
                Sql);
        }

        [Fact]
        public override void Where_bitwise_and_integral()
        {
            base.Where_bitwise_and_integral();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND ((`g`.`Rank` & 1) = 1)

SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND ((`g`.`Rank` & 1) = 1)

SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND ((`g`.`Rank` & 1) = 1)

SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND ((`g`.`Rank` & 1) = 1)",
                Sql);
        }

        [Fact]
        public override void Where_bitwise_and_nullable_enum_with_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_constant();

            Assert.Equal(
                @"SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` & 1 > 0",
                Sql);
        }

        [Fact]
        public override void Where_bitwise_and_nullable_enum_with_null_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_null_constant();

            Assert.Equal(
                @"SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` & NULL > 0",
                Sql);
        }

        [Fact]
        public override void Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter();

            Assert.Equal(
                @":__ammunitionType_0: Cartridge

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` & :__ammunitionType_0 > 0",
                Sql);
        }

        [Fact]
        public override void Where_bitwise_and_nullable_enum_with_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_nullable_parameter();

            Assert.Equal(
                @":__ammunitionType_0: Cartridge (Nullable = true)

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` & :__ammunitionType_0 > 0

:__ammunitionType_0:  (DbType = Int32)

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` & :__ammunitionType_0 > 0",
                Sql);
        }

        [Fact]
        public override void Where_bitwise_or_enum()
        {
            base.Where_bitwise_or_enum();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND (`g`.`Rank` | 1 > 0)",
                Sql);
        }

        [Fact]
        public override void Bitwise_projects_values_in_select()
        {
            base.Bitwise_projects_values_in_select();

            Assert.Equal(
                @"SELECT TOP(1) CASE
    WHEN `g`.`Rank` & 1 = 1
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END, CASE
    WHEN `g`.`Rank` & 1 = 2
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END, `g`.`Rank` & 1
FROM `Gear` `g`
WHERE ((`g`.`Discriminator` = 'Officer') OR (`g`.`Discriminator` = 'Gear')) AND (`g`.`Rank` & 1 = 1)",
                Sql);
        }

        [Fact]
        public override void Where_count_subquery_without_collision()
        {
            base.Where_count_subquery_without_collision();

            Assert.Equal(
                @"SELECT `w`.`Nickname`, `w`.`SquadId`, `w`.`AssignedCityName`, `w`.`CityOrBirthName`, `w`.`Discriminator`, `w`.`FullName`, `w`.`LeaderNickname`, `w`.`LeaderSquadId`, `w`.`Rank`
FROM `Gear` `w`
WHERE `w`.`Discriminator` IN ('Officer', 'Gear') AND ((
    SELECT COUNT(*)
    FROM `Weapon` `w0`
    WHERE `w`.`FullName` = `w0`.`OwnerFullName`
) = 2)",
                Sql);
        }

        [Fact]
        public override void Where_any_subquery_without_collision()
        {
            base.Where_any_subquery_without_collision();

            Assert.Equal(
                @"SELECT `w`.`Nickname`, `w`.`SquadId`, `w`.`AssignedCityName`, `w`.`CityOrBirthName`, `w`.`Discriminator`, `w`.`FullName`, `w`.`LeaderNickname`, `w`.`LeaderSquadId`, `w`.`Rank`
FROM `Gear` `w`
WHERE `w`.`Discriminator` IN ('Officer', 'Gear') AND EXISTS (
    SELECT 1
    FROM `Weapon` `w0`
    WHERE `w`.`FullName` = `w0`.`OwnerFullName`)",
                Sql);
        }

        [Fact]
        public override void Select_inverted_boolean()
        {
            base.Select_inverted_boolean();

            Assert.Equal(
                @"SELECT `w`.`Id`, CASE
    WHEN `w`.`IsAutomatic` = 0
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END
FROM `Weapon` `w`
WHERE `w`.`IsAutomatic` = 1",
                Sql);
        }

        [Fact]
        public override void Select_comparison_with_null()
        {
            base.Select_comparison_with_null();

            Assert.Equal(
                @":__ammunitionType_1: Cartridge (Nullable = true)
:__ammunitionType_0: Cartridge (Nullable = true)

SELECT `w`.`Id`, CASE
    WHEN `w`.`AmmunitionType` = :__ammunitionType_1
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` = :__ammunitionType_0

SELECT `w`.`Id`, CASE
    WHEN `w`.`AmmunitionType` IS NULL
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` IS NULL",
                Sql);
        }

        [Fact]
        public override void Select_ternary_operation_with_boolean()
        {
            base.Select_ternary_operation_with_boolean();

            Assert.Equal(
                @"SELECT `w`.`Id`, CASE
    WHEN `w`.`IsAutomatic` = 1
    THEN 1 ELSE 0
END
FROM `Weapon` `w`",
                Sql);
        }

        [Fact]
        public override void Select_ternary_operation_with_inverted_boolean()
        {
            base.Select_ternary_operation_with_inverted_boolean();

            Assert.Equal(
                @"SELECT `w`.`Id`, CASE
    WHEN `w`.`IsAutomatic` = 0
    THEN 1 ELSE 0
END
FROM `Weapon` `w`",
                Sql);
        }

        [Fact]
        public override void Select_ternary_operation_with_has_value_not_null()
        {
            // TODO: Optimize this query (See #4267)
            base.Select_ternary_operation_with_has_value_not_null();

            Assert.Equal(
                @"SELECT `w`.`Id`, CASE
    WHEN `w`.`AmmunitionType` IS NOT NULL AND ((`w`.`AmmunitionType` = 1) AND `w`.`AmmunitionType` IS NOT NULL)
    THEN 'Yes' ELSE 'No'
END
FROM `Weapon` `w`
WHERE `w`.`AmmunitionType` IS NOT NULL AND ((`w`.`AmmunitionType` = 1) AND `w`.`AmmunitionType` IS NOT NULL)",
                Sql);
        }

        [Fact]
        public override void Select_ternary_operation_multiple_conditions()
        {
            base.Select_ternary_operation_multiple_conditions();

            Assert.Equal(
                @"SELECT `w`.`Id`, CASE
    WHEN (`w`.`AmmunitionType` = 2) AND (`w`.`SynergyWithId` = 1)
    THEN 'Yes' ELSE 'No'
END
FROM `Weapon` `w`",
                Sql);
        }

        [Fact]
        public override void Select_ternary_operation_multiple_conditions_2()
        {
            base.Select_ternary_operation_multiple_conditions_2();

            Assert.Equal(
                @"SELECT `w`.`Id`, CASE
    WHEN (`w`.`IsAutomatic` = 0) AND ((`w`.`SynergyWithId` = 1) AND `w`.`SynergyWithId` IS NOT NULL)
    THEN 'Yes' ELSE 'No'
END
FROM `Weapon` `w`",
                Sql);
        }

        [Fact]
        public override void Select_multiple_conditions()
        {
            base.Select_multiple_conditions();

            Assert.Equal(
                @"SELECT `w`.`Id`, CASE
    WHEN (`w`.`IsAutomatic` = 0) AND ((`w`.`SynergyWithId` = 1) AND `w`.`SynergyWithId` IS NOT NULL)
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END
FROM `Weapon` `w`",
                Sql);
        }

        [Fact]
        public override void Select_nested_ternary_operations()
        {
            base.Select_nested_ternary_operations();

            Assert.Equal(
                @"SELECT `w`.`Id`, CASE
    WHEN `w`.`IsAutomatic` = 0
    THEN CASE
        WHEN (`w`.`AmmunitionType` = 1) AND `w`.`AmmunitionType` IS NOT NULL
        THEN 'ManualCartridge' ELSE 'Manual'
    END ELSE 'Auto'
END
FROM `Weapon` `w`",
                Sql);
        }

        [Fact]
        public override void Null_propagation_optimization1()
        {
            base.Null_propagation_optimization1();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND `g`.`LeaderNickname` = 'Marcus'",
                Sql);
        }

        [Fact]
        public override void Null_propagation_optimization2()
        {
            base.Null_propagation_optimization2();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND `g`.`LeaderNickname` LIKE '%' + 'us'",
                Sql);
        }

        [Fact]
        public override void Null_propagation_optimization3()
        {
            base.Null_propagation_optimization3();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND `g`.`LeaderNickname` LIKE '%' + 'us'",
                Sql);
        }

        [Fact]
        public override void Null_propagation_optimization4()
        {
            base.Null_propagation_optimization4();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND LEN(`g`.`LeaderNickname`) = 5",
                Sql);
        }

        [Fact]
        public override void Null_propagation_optimization5()
        {
            base.Null_propagation_optimization5();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND LEN(`g`.`LeaderNickname`) = 5",
                Sql);
        }

        [Fact]
        public override void Select_null_propagation_negative1()
        {
            base.Select_null_propagation_negative1();

            Assert.Equal(
                @"SELECT CASE
    WHEN `g`.`LeaderNickname` IS NOT NULL
    THEN CASE
        WHEN LEN(`g`.`Nickname`) = 5
        THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
    END ELSE NULL
END
FROM `Gear` `g`
WHERE (`g`.`Discriminator` = 'Officer') OR (`g`.`Discriminator` = 'Gear')",
                Sql);
        }

        [Fact]
        public override void Select_null_propagation_negative2()
        {
            base.Select_null_propagation_negative2();

            Assert.Equal(
                @"SELECT CASE
    WHEN `g1`.`LeaderNickname` IS NOT NULL
    THEN `g2`.`LeaderNickname` ELSE NULL
END
FROM `Gear` `g1`
CROSS JOIN `Gear` `g2`
WHERE (`g1`.`Discriminator` = 'Officer') OR (`g1`.`Discriminator` = 'Gear')",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();

            Assert.Equal(
                @"",
                Sql);
        }

        [Fact]
        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT `ct`.`Id`, `ct`.`GearNickName`, `ct`.`GearSquadId`, `ct`.`Note`, `ct.Gear`.`Nickname`, `ct.Gear`.`SquadId`, `ct.Gear`.`AssignedCityName`, `ct.Gear`.`CityOrBirthName`, `ct.Gear`.`Discriminator`, `ct.Gear`.`FullName`, `ct.Gear`.`LeaderNickname`, `ct.Gear`.`LeaderSquadId`, `ct.Gear`.`Rank`
FROM `CogTag` `ct`
LEFT JOIN `Gear` `ct.Gear` ON (`ct`.`GearNickName` = `ct.Gear`.`Nickname`) AND (`ct`.`GearSquadId` = `ct.Gear`.`SquadId`)
ORDER BY `ct`.`GearNickName`, `ct`.`GearSquadId`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();

            Assert.Equal(
                @"SELECT `ct`.`Id`, `ct`.`GearNickName`, `ct`.`GearSquadId`, `ct`.`Note`, `ct.Gear`.`Nickname`, `ct.Gear`.`SquadId`, `ct.Gear`.`AssignedCityName`, `ct.Gear`.`CityOrBirthName`, `ct.Gear`.`Discriminator`, `ct.Gear`.`FullName`, `ct.Gear`.`LeaderNickname`, `ct.Gear`.`LeaderSquadId`, `ct.Gear`.`Rank`
FROM `CogTag` `ct`
LEFT JOIN `Gear` `ct.Gear` ON (`ct`.`GearNickName` = `ct.Gear`.`Nickname`) AND (`ct`.`GearSquadId` = `ct.Gear`.`SquadId`)
ORDER BY `ct`.`GearNickName`, `ct`.`GearSquadId`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

            Assert.Equal(
                @"SELECT `o`.`Id`, `o`.`GearNickName`, `o`.`GearSquadId`, `o`.`Note`, `o.Gear`.`Nickname`, `o.Gear`.`SquadId`, `o.Gear`.`AssignedCityName`, `o.Gear`.`CityOrBirthName`, `o.Gear`.`Discriminator`, `o.Gear`.`FullName`, `o.Gear`.`LeaderNickname`, `o.Gear`.`LeaderSquadId`, `o.Gear`.`Rank`
FROM `CogTag` `o`
LEFT JOIN `Gear` `o.Gear` ON (`o`.`GearNickName` = `o.Gear`.`Nickname`) AND (`o`.`GearSquadId` = `o.Gear`.`SquadId`)
WHERE `o`.`GearNickName` IS NOT NULL OR `o`.`GearSquadId` IS NOT NULL
ORDER BY `o`.`GearNickName`, `o`.`GearSquadId`",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

            Assert.Equal(
                @"SELECT `ct1`.`Id`, `ct1`.`GearNickName`, `ct1`.`GearSquadId`, `ct1`.`Note`, `ct2`.`Id`, `ct2`.`GearNickName`, `ct2`.`GearSquadId`, `ct2`.`Note`
FROM `CogTag` `ct1`
CROSS JOIN `CogTag` `ct2`
WHERE ((`ct1`.`GearNickName` = `ct2`.`GearNickName`) OR (`ct1`.`GearNickName` IS NULL AND `ct2`.`GearNickName` IS NULL)) AND ((`ct1`.`GearSquadId` = `ct2`.`GearSquadId`) OR (`ct1`.`GearSquadId` IS NULL AND `ct2`.`GearSquadId` IS NULL))",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();

            Assert.Equal(
                @"SELECT `ct`.`Id`, `ct`.`GearNickName`, `ct`.`GearSquadId`, `ct`.`Note`
FROM `CogTag` `ct`
WHERE `ct`.`GearNickName` IS NULL AND `ct`.`GearSquadId` IS NULL",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();

            Assert.Equal(
                @"SELECT `ct`.`Id`, `ct`.`GearNickName`, `ct`.`GearSquadId`, `ct`.`Note`
FROM `CogTag` `ct`
WHERE `ct`.`GearNickName` IS NULL AND `ct`.`GearSquadId` IS NULL",
                Sql);
        }

        [Fact]
        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();

            Assert.StartsWith(
                @"SELECT `ct2.Gear`.`Nickname`, `ct2.Gear`.`SquadId`, `ct2.Gear`.`AssignedCityName`, `ct2.Gear`.`CityOrBirthName`, `ct2.Gear`.`Discriminator`, `ct2.Gear`.`FullName`, `ct2.Gear`.`LeaderNickname`, `ct2.Gear`.`LeaderSquadId`, `ct2.Gear`.`Rank`
FROM `Gear` `ct2.Gear`
WHERE (`ct2.Gear`.`Discriminator` = 'Officer') OR (`ct2.Gear`.`Discriminator` = 'Gear')

SELECT `ct1`.`Id`, `ct1`.`GearNickName`, `ct1`.`GearSquadId`, `ct1`.`Note`, `ct1.Gear`.`Nickname`, `ct1.Gear`.`SquadId`, `ct1.Gear`.`AssignedCityName`, `ct1.Gear`.`CityOrBirthName`, `ct1.Gear`.`Discriminator`, `ct1.Gear`.`FullName`, `ct1.Gear`.`LeaderNickname`, `ct1.Gear`.`LeaderSquadId`, `ct1.Gear`.`Rank`
FROM `CogTag` `ct1`
LEFT JOIN `Gear` `ct1.Gear` ON (`ct1`.`GearNickName` = `ct1.Gear`.`Nickname`) AND (`ct1`.`GearSquadId` = `ct1.Gear`.`SquadId`)
ORDER BY `ct1`.`GearNickName`, `ct1`.`GearSquadId`",
                Sql);
        }

        [Fact]
        public override void Optional_Navigation_Null_Coalesce_To_Clr_Type()
        {
            base.Optional_Navigation_Null_Coalesce_To_Clr_Type();

            Assert.Equal(@"SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`, `w.SynergyWith`.`Id`, `w.SynergyWith`.`AmmunitionType`, `w.SynergyWith`.`IsAutomatic`, `w.SynergyWith`.`Name`, `w.SynergyWith`.`OwnerFullName`, `w.SynergyWith`.`SynergyWithId`
FROM `Weapon` `w`
LEFT JOIN `Weapon` `w.SynergyWith` ON `w`.`SynergyWithId` = `w.SynergyWith`.`Id`
ORDER BY `w`.`SynergyWithId`", Sql);
        }

        [Fact]
        public override void Where_subquery_boolean()
        {
            base.Where_subquery_boolean();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND ((
    SELECT TOP(1) `w`.`IsAutomatic`
    FROM `Weapon` `w`
    WHERE `g`.`FullName` = `w`.`OwnerFullName`
) = 1)",
                Sql);
        }

        [Fact]
        public override void Singleton_Navigation_With_Member_Access()
        {
            base.Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT `ct`.`Id`, `ct`.`GearNickName`, `ct`.`GearSquadId`, `ct`.`Note`, `ct.Gear`.`Nickname`, `ct.Gear`.`SquadId`, `ct.Gear`.`AssignedCityName`, `ct.Gear`.`CityOrBirthName`, `ct.Gear`.`Discriminator`, `ct.Gear`.`FullName`, `ct.Gear`.`LeaderNickname`, `ct.Gear`.`LeaderSquadId`, `ct.Gear`.`Rank`
FROM `CogTag` `ct`
LEFT JOIN `Gear` `ct.Gear` ON (`ct`.`GearNickName` = `ct.Gear`.`Nickname`) AND (`ct`.`GearSquadId` = `ct.Gear`.`SquadId`)
ORDER BY `ct`.`GearNickName`, `ct`.`GearSquadId`",
                Sql);
        }

        [Fact]
        public override void GroupJoin_Composite_Key()
        {
            base.GroupJoin_Composite_Key();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `CogTag` `ct`
LEFT JOIN `Gear` `g` ON (`ct`.`GearNickName` = `g`.`Nickname`) AND (`ct`.`GearSquadId` = `g`.`SquadId`)
ORDER BY `ct`.`GearNickName`, `ct`.`GearSquadId`",
                Sql);
        }

        [Fact]
        public override void Join_navigation_translated_to_subquery_composite_key()
        {
            base.Join_navigation_translated_to_subquery_composite_key();

            Assert.Equal(
                @"SELECT `g`.`FullName`, `t`.`Note`
FROM `Gear` `g`
INNER JOIN `CogTag` `t` ON `g`.`FullName` = (
    SELECT TOP(1) `subQuery0`.`FullName`
    FROM `Gear` `subQuery0`
    WHERE ((`subQuery0`.`Discriminator` = 'Officer') OR (`subQuery0`.`Discriminator` = 'Gear')) AND ((`subQuery0`.`Nickname` = `t`.`GearNickName`) AND (`subQuery0`.`SquadId` = `t`.`GearSquadId`))
)
WHERE (`g`.`Discriminator` = 'Officer') OR (`g`.`Discriminator` = 'Gear')",
                Sql);
        }

        [Fact]
        public override void Collection_with_inheritance_and_join_include_joined()
        {
            base.Collection_with_inheritance_and_join_include_joined();

            Assert.Equal(
                @"SELECT `t0`.`Nickname`, `t0`.`SquadId`, `t0`.`AssignedCityName`, `t0`.`CityOrBirthName`, `t0`.`Discriminator`, `t0`.`FullName`, `t0`.`LeaderNickname`, `t0`.`LeaderSquadId`, `t0`.`Rank`, `c`.`Id`, `c`.`GearNickName`, `c`.`GearSquadId`, `c`.`Note`
FROM `CogTag` `t`
INNER JOIN (
    SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
    FROM `Gear` `g0`
    WHERE `g0`.`Discriminator` = 'Officer'
) `t0` ON (`t`.`GearSquadId` = `t0`.`SquadId`) AND (`t`.`GearNickName` = `t0`.`Nickname`)
LEFT JOIN `CogTag` `c` ON (`c`.`GearNickName` = `t0`.`Nickname`) AND (`c`.`GearSquadId` = `t0`.`SquadId`)",
                Sql);
        }

        [Fact]
        public override void Collection_with_inheritance_and_join_include_source()
        {
            base.Collection_with_inheritance_and_join_include_source();

            Assert.Equal(
                @"SELECT `t`.`Nickname`, `t`.`SquadId`, `t`.`AssignedCityName`, `t`.`CityOrBirthName`, `t`.`Discriminator`, `t`.`FullName`, `t`.`LeaderNickname`, `t`.`LeaderSquadId`, `t`.`Rank`, `c`.`Id`, `c`.`GearNickName`, `c`.`GearSquadId`, `c`.`Note`
FROM (
    SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
    FROM `Gear` `g0`
    WHERE `g0`.`Discriminator` = 'Officer'
) `t`
INNER JOIN `CogTag` `t0` ON (`t`.`SquadId` = `t0`.`GearSquadId`) AND (`t`.`Nickname` = `t0`.`GearNickName`)
LEFT JOIN `CogTag` `c` ON (`c`.`GearNickName` = `t`.`Nickname`) AND (`c`.`GearSquadId` = `t`.`SquadId`)",
                Sql);
        }

        [Fact]
        public override void Non_unicode_string_literal_is_used_for_non_unicode_column()
        {
            base.Non_unicode_string_literal_is_used_for_non_unicode_column();

            Assert.Equal(
                @"SELECT `c`.`Name`, `c`.`Location`
FROM `City` `c`
WHERE `c`.`Location` = 'Unknown'",
                Sql);
        }

        [Fact]
        public override void Non_unicode_string_literal_is_used_for_non_unicode_column_right()
        {
            base.Non_unicode_string_literal_is_used_for_non_unicode_column_right();

            Assert.Equal(
    @"SELECT `c`.`Name`, `c`.`Location`
FROM `City` `c`
WHERE 'Unknown' = `c`.`Location`",
                Sql);
        }

        [Fact]
        public override void Non_unicode_parameter_is_used_for_non_unicode_column()
        {
            base.Non_unicode_parameter_is_used_for_non_unicode_column();

            Assert.Equal(
                @":__value_0: Unknown (Size = 100) (DbType = AnsiString)

SELECT `c`.`Name`, `c`.`Location`
FROM `City` `c`
WHERE `c`.`Location` = :__value_0",
                Sql);
        }

        [Fact]
        public override void Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column()
        {
            base.Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column();

            Assert.Equal(
                @"SELECT `c`.`Name`, `c`.`Location`
FROM `City` `c`
WHERE `c`.`Location` IN ('Unknown', 'Jacinto''s location', 'Ephyra''s location')",
                Sql);
        }

        [Fact]
        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery();

            Assert.Equal(
    @"SELECT `c`.`Name`, `c`.`Location`
FROM `City` `c`
WHERE (`c`.`Location` = 'Unknown') AND ((
    SELECT COUNT(*)
    FROM `Gear` `g`
    WHERE (((`g`.`Discriminator` = 'Officer') OR (`g`.`Discriminator` = 'Gear')) AND (`g`.`Nickname` = 'Paduk')) AND (`c`.`Name` = `g`.`CityOrBirthName`)
) = 1)", Sql);
        }

        [Fact]
        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery();

            Assert.Equal(
    @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`
FROM `Gear` `g`
INNER JOIN `City` `g.CityOfBirth` ON `g`.`CityOrBirthName` = `g.CityOfBirth`.`Name`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear') AND ((`g`.`Nickname` = 'Marcus') AND (`g.CityOfBirth`.`Location` = 'Jacinto''s location'))",
                Sql);
        }

        [Fact]
        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains();

            Assert.Equal(
                @"SELECT `c`.`Name`, `c`.`Location`
FROM `City` `c`
WHERE `c`.`Location` LIKE ('%' + 'Jacinto') + '%'",
                Sql);
        }

        [Fact]
        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat();

            Assert.Equal(
                @"SELECT `c`.`Name`, `c`.`Location`
FROM `City` `c`
WHERE `c`.`Location` + 'Added' LIKE ('%' + 'Add') + '%'",
                Sql);
        }

        [Fact]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `g2`.`Nickname`, `g2`.`SquadId`, `g2`.`AssignedCityName`, `g2`.`CityOrBirthName`, `g2`.`Discriminator`, `g2`.`FullName`, `g2`.`LeaderNickname`, `g2`.`LeaderSquadId`, `g2`.`Rank`
FROM `Gear` `g`
LEFT JOIN `Gear` `g2` ON `g`.`LeaderNickname` = `g2`.`Nickname`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `g`.`LeaderNickname`, `g`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
INNER JOIN (
    SELECT DISTINCT `g`.`LeaderNickname`, `g`.`FullName`
    FROM `Gear` `g`
    LEFT JOIN `Gear` `g2` ON `g`.`LeaderNickname` = `g2`.`Nickname`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g0` ON `w`.`OwnerFullName` = `g0`.`FullName`
ORDER BY `g0`.`LeaderNickname`, `g0`.`FullName`",
                Sql);
        }

        [Fact]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2();

            Assert.Equal(
                @"SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`, `t`.`Nickname`, `t`.`SquadId`, `t`.`AssignedCityName`, `t`.`CityOrBirthName`, `t`.`Discriminator`, `t`.`FullName`, `t`.`LeaderNickname`, `t`.`LeaderSquadId`, `t`.`Rank`
FROM `Gear` `g1`
LEFT JOIN (
    SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
    FROM `Gear` `g0`
    WHERE `g0`.`Discriminator` IN ('Officer', 'Gear')
) `t` ON `g1`.`LeaderNickname` = `t`.`Nickname`
WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `g1`.`LeaderNickname`, `t`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
INNER JOIN (
    SELECT DISTINCT `g1`.`LeaderNickname`, `t`.`FullName`
    FROM `Gear` `g1`
    LEFT JOIN (
        SELECT `g0`.`Nickname`, `g0`.`SquadId`, `g0`.`AssignedCityName`, `g0`.`CityOrBirthName`, `g0`.`Discriminator`, `g0`.`FullName`, `g0`.`LeaderNickname`, `g0`.`LeaderSquadId`, `g0`.`Rank`
        FROM `Gear` `g0`
        WHERE `g0`.`Discriminator` IN ('Officer', 'Gear')
    ) `t` ON `g1`.`LeaderNickname` = `t`.`Nickname`
    WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
) `t0` ON `w`.`OwnerFullName` = `t0`.`FullName`
ORDER BY `t0`.`LeaderNickname`, `t0`.`FullName`",
                Sql);
        }

        [Fact]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `t`.`Nickname`, `t`.`SquadId`, `t`.`AssignedCityName`, `t`.`CityOrBirthName`, `t`.`Discriminator`, `t`.`FullName`, `t`.`LeaderNickname`, `t`.`LeaderSquadId`, `t`.`Rank`
FROM `Gear` `g`
LEFT JOIN (
    SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
    FROM `Gear` `g1`
    WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `g`.`LeaderNickname`, `g`.`FullName`, `t`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
INNER JOIN (
    SELECT DISTINCT `g`.`LeaderNickname`, `g`.`FullName`
    FROM `Gear` `g`
    LEFT JOIN (
        SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
        FROM `Gear` `g1`
        WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
    ) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g2` ON `w`.`OwnerFullName` = `g2`.`FullName`
ORDER BY `g2`.`LeaderNickname`, `g2`.`FullName`

SELECT `w0`.`Id`, `w0`.`AmmunitionType`, `w0`.`IsAutomatic`, `w0`.`Name`, `w0`.`OwnerFullName`, `w0`.`SynergyWithId`
FROM `Weapon` `w0`
INNER JOIN (
    SELECT DISTINCT `g`.`LeaderNickname`, `g`.`FullName`, `t`.`FullName` `FullName0`
    FROM `Gear` `g`
    LEFT JOIN (
        SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
        FROM `Gear` `g1`
        WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
    ) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `t0` ON `w0`.`OwnerFullName` = `t0`.`FullName0`
ORDER BY `t0`.`LeaderNickname`, `t0`.`FullName`, `t0`.`FullName0`",
                Sql);
        }

        [Fact]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `t`.`Nickname`, `t`.`SquadId`, `t`.`AssignedCityName`, `t`.`CityOrBirthName`, `t`.`Discriminator`, `t`.`FullName`, `t`.`LeaderNickname`, `t`.`LeaderSquadId`, `t`.`Rank`
FROM `Gear` `g`
LEFT JOIN (
    SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
    FROM `Gear` `g1`
    WHERE `g1`.`Discriminator` = 'Officer'
) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `g`.`LeaderNickname`, `g`.`FullName`, `t`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
INNER JOIN (
    SELECT DISTINCT `g`.`LeaderNickname`, `g`.`FullName`
    FROM `Gear` `g`
    LEFT JOIN (
        SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
        FROM `Gear` `g1`
        WHERE `g1`.`Discriminator` = 'Officer'
    ) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g2` ON `w`.`OwnerFullName` = `g2`.`FullName`
ORDER BY `g2`.`LeaderNickname`, `g2`.`FullName`

SELECT `w0`.`Id`, `w0`.`AmmunitionType`, `w0`.`IsAutomatic`, `w0`.`Name`, `w0`.`OwnerFullName`, `w0`.`SynergyWithId`
FROM `Weapon` `w0`
INNER JOIN (
    SELECT DISTINCT `g`.`LeaderNickname`, `g`.`FullName`, `t`.`FullName` `FullName0`
    FROM `Gear` `g`
    LEFT JOIN (
        SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
        FROM `Gear` `g1`
        WHERE `g1`.`Discriminator` = 'Officer'
    ) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `t0` ON `w0`.`OwnerFullName` = `t0`.`FullName0`
ORDER BY `t0`.`LeaderNickname`, `t0`.`FullName`, `t0`.`FullName0`",
                Sql);
        }

        [Fact]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `t`.`Nickname`, `t`.`SquadId`, `t`.`AssignedCityName`, `t`.`CityOrBirthName`, `t`.`Discriminator`, `t`.`FullName`, `t`.`LeaderNickname`, `t`.`LeaderSquadId`, `t`.`Rank`
FROM `Gear` `g`
LEFT JOIN (
    SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
    FROM `Gear` `g1`
    WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `g`.`LeaderNickname`, `g`.`FullName`, `t`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
INNER JOIN (
    SELECT DISTINCT `g`.`LeaderNickname`, `g`.`FullName`
    FROM `Gear` `g`
    LEFT JOIN (
        SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
        FROM `Gear` `g1`
        WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
    ) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g2` ON `w`.`OwnerFullName` = `g2`.`FullName`
ORDER BY `g2`.`LeaderNickname`, `g2`.`FullName`

SELECT `w0`.`Id`, `w0`.`AmmunitionType`, `w0`.`IsAutomatic`, `w0`.`Name`, `w0`.`OwnerFullName`, `w0`.`SynergyWithId`
FROM `Weapon` `w0`
INNER JOIN (
    SELECT DISTINCT `g`.`LeaderNickname`, `g`.`FullName`, `t`.`FullName` `FullName0`
    FROM `Gear` `g`
    LEFT JOIN (
        SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
        FROM `Gear` `g1`
        WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
    ) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `t0` ON `w0`.`OwnerFullName` = `t0`.`FullName0`
ORDER BY `t0`.`LeaderNickname`, `t0`.`FullName`, `t0`.`FullName0`",
                Sql);
        }

        [Fact]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result();

            Assert.Equal(
                @"SELECT `g`.`Nickname`, `g`.`SquadId`, `g`.`AssignedCityName`, `g`.`CityOrBirthName`, `g`.`Discriminator`, `g`.`FullName`, `g`.`LeaderNickname`, `g`.`LeaderSquadId`, `g`.`Rank`, `t`.`Nickname`, `t`.`SquadId`, `t`.`AssignedCityName`, `t`.`CityOrBirthName`, `t`.`Discriminator`, `t`.`FullName`, `t`.`LeaderNickname`, `t`.`LeaderSquadId`, `t`.`Rank`
FROM `Gear` `g`
LEFT JOIN (
    SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
    FROM `Gear` `g1`
    WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
ORDER BY `g`.`LeaderNickname`, `g`.`FullName`, `t`.`FullName`

SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
INNER JOIN (
    SELECT DISTINCT `g`.`LeaderNickname`, `g`.`FullName`
    FROM `Gear` `g`
    LEFT JOIN (
        SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
        FROM `Gear` `g1`
        WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
    ) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `g2` ON `w`.`OwnerFullName` = `g2`.`FullName`
ORDER BY `g2`.`LeaderNickname`, `g2`.`FullName`

SELECT `w0`.`Id`, `w0`.`AmmunitionType`, `w0`.`IsAutomatic`, `w0`.`Name`, `w0`.`OwnerFullName`, `w0`.`SynergyWithId`
FROM `Weapon` `w0`
INNER JOIN (
    SELECT DISTINCT `g`.`LeaderNickname`, `g`.`FullName`, `t`.`FullName` `FullName0`
    FROM `Gear` `g`
    LEFT JOIN (
        SELECT `g1`.`Nickname`, `g1`.`SquadId`, `g1`.`AssignedCityName`, `g1`.`CityOrBirthName`, `g1`.`Discriminator`, `g1`.`FullName`, `g1`.`LeaderNickname`, `g1`.`LeaderSquadId`, `g1`.`Rank`
        FROM `Gear` `g1`
        WHERE `g1`.`Discriminator` IN ('Officer', 'Gear')
    ) `t` ON `g`.`LeaderNickname` = `t`.`Nickname`
    WHERE `g`.`Discriminator` IN ('Officer', 'Gear')
) `t0` ON `w0`.`OwnerFullName` = `t0`.`FullName0`
ORDER BY `t0`.`LeaderNickname`, `t0`.`FullName`, `t0`.`FullName0`",
                Sql);
        }

        [Fact]
        public override void Coalesce_operator_in_predicate()
        {
            base.Coalesce_operator_in_predicate();

            Assert.Equal(
                @"SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE COALESCE(`w`.`IsAutomatic`, 0) = 1",
                Sql);
        }

        [Fact]
        public override void Coalesce_operator_in_predicate_with_other_conditions()
        {
            base.Coalesce_operator_in_predicate_with_other_conditions();

            Assert.Equal(
                @"SELECT `w`.`Id`, `w`.`AmmunitionType`, `w`.`IsAutomatic`, `w`.`Name`, `w`.`OwnerFullName`, `w`.`SynergyWithId`
FROM `Weapon` `w`
WHERE (`w`.`AmmunitionType` = 1) AND (COALESCE(`w`.`IsAutomatic`, 0) = 1)",
                Sql);
        }

        [Fact]
        public override void Coalesce_operator_in_projection_with_other_conditions()
        {
            base.Coalesce_operator_in_projection_with_other_conditions();

            Assert.Equal(
                @"SELECT CASE
    WHEN (`w`.`AmmunitionType` = 1) AND (COALESCE(`w`.`IsAutomatic`, 0) = 1)
    THEN CAST('1' AS BIT) ELSE CAST('0' AS BIT)
END
FROM `Weapon` `w`",
                Sql);
        }

        public GearsOfWarQueryXuGuDbTest(GearsOfWarQueryXuGuDbFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
