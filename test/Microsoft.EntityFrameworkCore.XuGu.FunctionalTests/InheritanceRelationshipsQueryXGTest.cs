// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class InheritanceRelationshipsQueryXGTest : InheritanceRelationshipsQueryTestBase<XGTestStore, InheritanceRelationshipsQueryXGFixture>
    {
        public InheritanceRelationshipsQueryXGTest(InheritanceRelationshipsQueryXGFixture fixture)
            : base(fixture)
        {
        }

        public override void Include_reference_with_inheritance1()
        {
            base.Include_reference_with_inheritance1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) `b` ON `b`.`BaseParentId` = `e`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')",
                Sql);
        }

        public override void Include_reference_with_inheritance2()
        {
            base.Include_reference_with_inheritance2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_reference_with_inheritance_reverse()
        {
            base.Include_reference_with_inheritance_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`BaseParentId`, `e`.`Discriminator`, `e`.`Name`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `BaseReferenceOnBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b` ON `e`.`BaseParentId` = `b`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')",
                Sql);
        }

        public override void Include_self_refence_with_inheritence()
        {
            base.Include_self_refence_with_inheritence();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'
) `b` ON `b`.`BaseId` = `e`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')",
                Sql);
        }

        public override void Include_self_refence_with_inheritence_reverse()
        {
            base.Include_self_refence_with_inheritence_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b` ON `e`.`BaseId` = `b`.`Id`
WHERE `e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Include_reference_with_inheritance_with_filter1()
        {
            base.Include_reference_with_inheritance_with_filter1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) `b` ON `b`.`BaseParentId` = `e`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)",
                Sql);
        }

        public override void Include_reference_with_inheritance_with_filter2()
        {
            base.Include_reference_with_inheritance_with_filter2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_reference_with_inheritance_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_with_filter_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`BaseParentId`, `e`.`Discriminator`, `e`.`Name`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `BaseReferenceOnBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b` ON `e`.`BaseParentId` = `b`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)",
                Sql);
        }

        public override void Include_reference_without_inheritance()
        {
            base.Include_reference_without_inheritance();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `r`.`Id`, `r`.`Name`, `r`.`ParentId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN `ReferenceOnBase` `r` ON `r`.`ParentId` = `e`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')",
                Sql);
        }

        public override void Include_reference_without_inheritance_reverse()
        {
            base.Include_reference_without_inheritance_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Name`, `e`.`ParentId`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `ReferenceOnBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b` ON `e`.`ParentId` = `b`.`Id`",
                Sql);
        }

        public override void Include_reference_without_inheritance_with_filter()
        {
            base.Include_reference_without_inheritance_with_filter();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `r`.`Id`, `r`.`Name`, `r`.`ParentId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN `ReferenceOnBase` `r` ON `r`.`ParentId` = `e`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)",
                Sql);
        }

        public override void Include_reference_without_inheritance_with_filter_reverse()
        {
            base.Include_reference_without_inheritance_with_filter_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Name`, `e`.`ParentId`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `ReferenceOnBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b` ON `e`.`ParentId` = `b`.`Id`
WHERE (`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL",
                Sql);
        }

        public override void Include_collection_with_inheritance1()
        {
            base.Include_collection_with_inheritance1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`
FROM `BaseInheritanceRelationshipEntity` `e`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
ORDER BY `e`.`Id`

SELECT `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedProperty`
FROM `BaseCollectionOnBase` `b`
WHERE `b`.`Discriminator` IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase') AND EXISTS (
    SELECT 1
    FROM `BaseInheritanceRelationshipEntity` `e`
    WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND (`b`.`BaseParentId` = `e`.`Id`))
ORDER BY `b`.`BaseParentId`",
                Sql);
        }

        public override void Include_collection_with_inheritance2()
        {
            base.Include_collection_with_inheritance2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_collection_with_inheritance_reverse()
        {
            base.Include_collection_with_inheritance_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`BaseParentId`, `e`.`Discriminator`, `e`.`Name`, `e`.`DerivedProperty`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `BaseCollectionOnBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b` ON `e`.`BaseParentId` = `b`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase')",
                Sql);
        }

        public override void Include_collection_with_inheritance_with_filter1()
        {
            base.Include_collection_with_inheritance_with_filter1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`
FROM `BaseInheritanceRelationshipEntity` `e`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)
ORDER BY `e`.`Id`

SELECT `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedProperty`
FROM `BaseCollectionOnBase` `b`
WHERE `b`.`Discriminator` IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase') AND EXISTS (
    SELECT 1
    FROM `BaseInheritanceRelationshipEntity` `e`
    WHERE (`e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)) AND ((`b`.`BaseParentId` = `e`.`Id`) AND `b`.`BaseParentId` IS NOT NULL))
ORDER BY `b`.`BaseParentId`",
                Sql);
        }

        public override void Include_collection_with_inheritance_with_filter2()
        {
            base.Include_collection_with_inheritance_with_filter2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_collection_with_inheritance_with_filter_reverse()
        {
            base.Include_collection_with_inheritance_with_filter_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`BaseParentId`, `e`.`Discriminator`, `e`.`Name`, `e`.`DerivedProperty`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `BaseCollectionOnBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b` ON `e`.`BaseParentId` = `b`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)",
                Sql);
        }

        public override void Include_collection_without_inheritance()
        {
            base.Include_collection_without_inheritance();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`
FROM `BaseInheritanceRelationshipEntity` `e`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
ORDER BY `e`.`Id`

SELECT `c`.`Id`, `c`.`Name`, `c`.`ParentId`
FROM `CollectionOnBase` `c`
WHERE EXISTS (
    SELECT 1
    FROM `BaseInheritanceRelationshipEntity` `e`
    WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND (`c`.`ParentId` = `e`.`Id`))
ORDER BY `c`.`ParentId`",
                Sql);
        }

        public override void Include_collection_without_inheritance_reverse()
        {
            base.Include_collection_without_inheritance_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Name`, `e`.`ParentId`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `CollectionOnBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b` ON `e`.`ParentId` = `b`.`Id`",
                Sql);
        }

        public override void Include_collection_without_inheritance_with_filter()
        {
            base.Include_collection_without_inheritance_with_filter();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`
FROM `BaseInheritanceRelationshipEntity` `e`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)
ORDER BY `e`.`Id`

SELECT `c`.`Id`, `c`.`Name`, `c`.`ParentId`
FROM `CollectionOnBase` `c`
WHERE EXISTS (
    SELECT 1
    FROM `BaseInheritanceRelationshipEntity` `e`
    WHERE (`e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)) AND ((`c`.`ParentId` = `e`.`Id`) AND `c`.`ParentId` IS NOT NULL))
ORDER BY `c`.`ParentId`",
                Sql);
        }

        public override void Include_collection_without_inheritance_with_filter_reverse()
        {
            base.Include_collection_without_inheritance_with_filter_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Name`, `e`.`ParentId`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `CollectionOnBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b` ON `e`.`ParentId` = `b`.`Id`
WHERE (`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived1()
        {
            base.Include_reference_with_inheritance_on_derived1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) `b` ON `b`.`BaseParentId` = `e`.`Id`
WHERE `e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived2()
        {
            base.Include_reference_with_inheritance_on_derived2();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedInheritanceRelationshipEntityId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnDerived` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnDerived', 'BaseReferenceOnDerived')
) `b` ON `b`.`BaseParentId` = `e`.`Id`
WHERE `e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived3()
        {
            base.Include_reference_with_inheritance_on_derived3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived4()
        {
            base.Include_reference_with_inheritance_on_derived4();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedInheritanceRelationshipEntityId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnDerived` `b`
    WHERE `b`.`Discriminator` = 'DerivedReferenceOnDerived'
) `b` ON `b`.`DerivedInheritanceRelationshipEntityId` = `e`.`Id`
WHERE `e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_reverse()
        {
            base.Include_reference_with_inheritance_on_derived_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`BaseParentId`, `e`.`Discriminator`, `e`.`Name`, `e`.`DerivedInheritanceRelationshipEntityId`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `BaseReferenceOnDerived` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'
) `b` ON `e`.`BaseParentId` = `b`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedReferenceOnDerived', 'BaseReferenceOnDerived')",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter1()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) `b` ON `b`.`BaseParentId` = `e`.`Id`
WHERE (`e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter2()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter2();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedInheritanceRelationshipEntityId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnDerived` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnDerived', 'BaseReferenceOnDerived')
) `b` ON `b`.`BaseParentId` = `e`.`Id`
WHERE (`e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter3()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter4()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter4();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedInheritanceRelationshipEntityId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnDerived` `b`
    WHERE `b`.`Discriminator` = 'DerivedReferenceOnDerived'
) `b` ON `b`.`DerivedInheritanceRelationshipEntityId` = `e`.`Id`
WHERE (`e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)",
                Sql);
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`BaseParentId`, `e`.`Discriminator`, `e`.`Name`, `e`.`DerivedInheritanceRelationshipEntityId`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `BaseReferenceOnDerived` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'
) `b` ON `e`.`BaseParentId` = `b`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedReferenceOnDerived', 'BaseReferenceOnDerived') AND ((`e`.`Name` <> 'Bar') OR `e`.`Name` IS NULL)",
                Sql);
        }

        public override void Include_reference_without_inheritance_on_derived1()
        {
            base.Include_reference_without_inheritance_on_derived1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `r`.`Id`, `r`.`Name`, `r`.`ParentId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN `ReferenceOnBase` `r` ON `r`.`ParentId` = `e`.`Id`
WHERE `e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Include_reference_without_inheritance_on_derived2()
        {
            base.Include_reference_without_inheritance_on_derived2();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `r`.`Id`, `r`.`Name`, `r`.`ParentId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN `ReferenceOnDerived` `r` ON `r`.`ParentId` = `e`.`Id`
WHERE `e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Include_reference_without_inheritance_on_derived_reverse()
        {
            base.Include_reference_without_inheritance_on_derived_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Name`, `e`.`ParentId`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `ReferenceOnDerived` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'
) `b` ON `e`.`ParentId` = `b`.`Id`",
                Sql);
        }

        public override void Include_collection_with_inheritance_on_derived1()
        {
            base.Include_collection_with_inheritance_on_derived1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`
FROM `BaseInheritanceRelationshipEntity` `e`
WHERE `e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'
ORDER BY `e`.`Id`

SELECT `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedProperty`
FROM `BaseCollectionOnBase` `b`
WHERE `b`.`Discriminator` IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase') AND EXISTS (
    SELECT 1
    FROM `BaseInheritanceRelationshipEntity` `e`
    WHERE (`e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity') AND (`b`.`BaseParentId` = `e`.`Id`))
ORDER BY `b`.`BaseParentId`",
                Sql);
        }

        public override void Include_collection_with_inheritance_on_derived2()
        {
            base.Include_collection_with_inheritance_on_derived2();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`
FROM `BaseInheritanceRelationshipEntity` `e`
WHERE `e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'
ORDER BY `e`.`Id`

SELECT `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`ParentId`, `b`.`DerivedInheritanceRelationshipEntityId`
FROM `BaseCollectionOnDerived` `b`
WHERE `b`.`Discriminator` IN ('DerivedCollectionOnDerived', 'BaseCollectionOnDerived') AND EXISTS (
    SELECT 1
    FROM `BaseInheritanceRelationshipEntity` `e`
    WHERE (`e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity') AND (`b`.`ParentId` = `e`.`Id`))
ORDER BY `b`.`ParentId`",
                Sql);
        }

        public override void Include_collection_with_inheritance_on_derived3()
        {
            base.Include_collection_with_inheritance_on_derived3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_collection_with_inheritance_on_derived4()
        {
            base.Include_collection_with_inheritance_on_derived4();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Include_collection_with_inheritance_on_derived_reverse()
        {
            base.Include_collection_with_inheritance_on_derived_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`ParentId`, `e`.`DerivedInheritanceRelationshipEntityId`, `b`.`Id`, `b`.`Discriminator`, `b`.`Name`, `b`.`BaseId`
FROM `BaseCollectionOnDerived` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseInheritanceRelationshipEntity` `b`
    WHERE `b`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'
) `b` ON `e`.`ParentId` = `b`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedCollectionOnDerived', 'BaseCollectionOnDerived')",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_reference1()
        {
            base.Nested_include_with_inheritance_reference_reference1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `n`.`Id`, `n`.`Discriminator`, `n`.`Name`, `n`.`ParentCollectionId`, `n`.`ParentReferenceId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) `b` ON `b`.`BaseParentId` = `e`.`Id`
LEFT JOIN (
    SELECT `n`.*
    FROM `NestedReferenceBase` `n`
    WHERE `n`.`Discriminator` IN ('NestedReferenceDerived', 'NestedReferenceBase')
) `n` ON `n`.`ParentReferenceId` = `b`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_reference2()
        {
            base.Nested_include_with_inheritance_reference_reference2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_reference3()
        {
            base.Nested_include_with_inheritance_reference_reference3();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `n`.`Id`, `n`.`Discriminator`, `n`.`Name`, `n`.`ParentCollectionId`, `n`.`ParentReferenceId`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) `b` ON `b`.`BaseParentId` = `e`.`Id`
LEFT JOIN (
    SELECT `n`.*
    FROM `NestedReferenceBase` `n`
    WHERE `n`.`Discriminator` IN ('NestedReferenceDerived', 'NestedReferenceBase')
) `n` ON `n`.`ParentReferenceId` = `b`.`Id`
WHERE `e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_reference4()
        {
            base.Nested_include_with_inheritance_reference_reference4();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_reference_reverse()
        {
            base.Nested_include_with_inheritance_reference_reference_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`ParentCollectionId`, `e`.`ParentReferenceId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b0`.`Id`, `b0`.`Discriminator`, `b0`.`Name`, `b0`.`BaseId`
FROM `NestedReferenceBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) `b` ON `e`.`ParentReferenceId` = `b`.`Id`
LEFT JOIN (
    SELECT `b0`.*
    FROM `BaseInheritanceRelationshipEntity` `b0`
    WHERE `b0`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b0` ON `b`.`BaseParentId` = `b0`.`Id`
WHERE `e`.`Discriminator` IN ('NestedReferenceDerived', 'NestedReferenceBase')",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_collection1()
        {
            base.Nested_include_with_inheritance_reference_collection1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) `b` ON `b`.`BaseParentId` = `e`.`Id`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
ORDER BY `b`.`Id`

SELECT `n`.`Id`, `n`.`Discriminator`, `n`.`Name`, `n`.`ParentCollectionId`, `n`.`ParentReferenceId`
FROM `NestedCollectionBase` `n`
WHERE `n`.`Discriminator` IN ('NestedCollectionDerived', 'NestedCollectionBase') AND EXISTS (
    SELECT 1
    FROM `BaseInheritanceRelationshipEntity` `e`
    LEFT JOIN (
        SELECT `b`.*
        FROM `BaseReferenceOnBase` `b`
        WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
    ) `b` ON `b`.`BaseParentId` = `e`.`Id`
    WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND (`n`.`ParentReferenceId` = `b`.`Id`))
ORDER BY `n`.`ParentReferenceId`",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_collection2()
        {
            base.Nested_include_with_inheritance_reference_collection2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_collection3()
        {
            base.Nested_include_with_inheritance_reference_collection3();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`
FROM `BaseInheritanceRelationshipEntity` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) `b` ON `b`.`BaseParentId` = `e`.`Id`
WHERE `e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity'
ORDER BY `b`.`Id`

SELECT `n`.`Id`, `n`.`Discriminator`, `n`.`Name`, `n`.`ParentCollectionId`, `n`.`ParentReferenceId`
FROM `NestedCollectionBase` `n`
WHERE `n`.`Discriminator` IN ('NestedCollectionDerived', 'NestedCollectionBase') AND EXISTS (
    SELECT 1
    FROM `BaseInheritanceRelationshipEntity` `e`
    LEFT JOIN (
        SELECT `b`.*
        FROM `BaseReferenceOnBase` `b`
        WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
    ) `b` ON `b`.`BaseParentId` = `e`.`Id`
    WHERE (`e`.`Discriminator` = 'DerivedInheritanceRelationshipEntity') AND (`n`.`ParentReferenceId` = `b`.`Id`))
ORDER BY `n`.`ParentReferenceId`",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_collection4()
        {
            base.Nested_include_with_inheritance_reference_collection4();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_reference_collection_reverse()
        {
            base.Nested_include_with_inheritance_reference_collection_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`ParentCollectionId`, `e`.`ParentReferenceId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b0`.`Id`, `b0`.`Discriminator`, `b0`.`Name`, `b0`.`BaseId`
FROM `NestedCollectionBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseReferenceOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedReferenceOnBase', 'BaseReferenceOnBase')
) `b` ON `e`.`ParentReferenceId` = `b`.`Id`
LEFT JOIN (
    SELECT `b0`.*
    FROM `BaseInheritanceRelationshipEntity` `b0`
    WHERE `b0`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b0` ON `b`.`BaseParentId` = `b0`.`Id`
WHERE `e`.`Discriminator` IN ('NestedCollectionDerived', 'NestedCollectionBase')",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_reference1()
        {
            base.Nested_include_with_inheritance_collection_reference1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`
FROM `BaseInheritanceRelationshipEntity` `e`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
ORDER BY `e`.`Id`

SELECT `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedProperty`, `n`.`Id`, `n`.`Discriminator`, `n`.`Name`, `n`.`ParentCollectionId`, `n`.`ParentReferenceId`
FROM `BaseCollectionOnBase` `b`
LEFT JOIN (
    SELECT `n`.*
    FROM `NestedReferenceBase` `n`
    WHERE `n`.`Discriminator` IN ('NestedReferenceDerived', 'NestedReferenceBase')
) `n` ON `n`.`ParentCollectionId` = `b`.`Id`
WHERE `b`.`Discriminator` IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase') AND EXISTS (
    SELECT 1
    FROM `BaseInheritanceRelationshipEntity` `e`
    WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND (`b`.`BaseParentId` = `e`.`Id`))
ORDER BY `b`.`BaseParentId`",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_reference2()
        {
            base.Nested_include_with_inheritance_collection_reference2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_reference3()
        {
            base.Nested_include_with_inheritance_collection_reference3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_reference4()
        {
            base.Nested_include_with_inheritance_collection_reference4();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_reference_reverse()
        {
            base.Nested_include_with_inheritance_collection_reference_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`ParentCollectionId`, `e`.`ParentReferenceId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedProperty`, `b0`.`Id`, `b0`.`Discriminator`, `b0`.`Name`, `b0`.`BaseId`
FROM `NestedReferenceBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseCollectionOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase')
) `b` ON `e`.`ParentCollectionId` = `b`.`Id`
LEFT JOIN (
    SELECT `b0`.*
    FROM `BaseInheritanceRelationshipEntity` `b0`
    WHERE `b0`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b0` ON `b`.`BaseParentId` = `b0`.`Id`
WHERE `e`.`Discriminator` IN ('NestedReferenceDerived', 'NestedReferenceBase')",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_collection1()
        {
            base.Nested_include_with_inheritance_collection_collection1();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`BaseId`
FROM `BaseInheritanceRelationshipEntity` `e`
WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
ORDER BY `e`.`Id`

SELECT `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedProperty`
FROM `BaseCollectionOnBase` `b`
WHERE `b`.`Discriminator` IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase') AND EXISTS (
    SELECT 1
    FROM `BaseInheritanceRelationshipEntity` `e`
    WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND (`b`.`BaseParentId` = `e`.`Id`))
ORDER BY `b`.`BaseParentId`, `b`.`Id`

SELECT `n`.`Id`, `n`.`Discriminator`, `n`.`Name`, `n`.`ParentCollectionId`, `n`.`ParentReferenceId`
FROM `NestedCollectionBase` `n`
INNER JOIN (
    SELECT DISTINCT `b`.`BaseParentId`, `b`.`Id`
    FROM `BaseCollectionOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase') AND EXISTS (
        SELECT 1
        FROM `BaseInheritanceRelationshipEntity` `e`
        WHERE `e`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity') AND (`b`.`BaseParentId` = `e`.`Id`))
) `b0` ON `n`.`ParentCollectionId` = `b0`.`Id`
WHERE `n`.`Discriminator` IN ('NestedCollectionDerived', 'NestedCollectionBase')
ORDER BY `b0`.`BaseParentId`, `b0`.`Id`",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_collection2()
        {
            base.Nested_include_with_inheritance_collection_collection2();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_collection3()
        {
            base.Nested_include_with_inheritance_collection_collection3();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_collection4()
        {
            base.Nested_include_with_inheritance_collection_collection4();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Nested_include_with_inheritance_collection_collection_reverse()
        {
            base.Nested_include_with_inheritance_collection_collection_reverse();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Discriminator`, `e`.`Name`, `e`.`ParentCollectionId`, `e`.`ParentReferenceId`, `b`.`Id`, `b`.`BaseParentId`, `b`.`Discriminator`, `b`.`Name`, `b`.`DerivedProperty`, `b0`.`Id`, `b0`.`Discriminator`, `b0`.`Name`, `b0`.`BaseId`
FROM `NestedCollectionBase` `e`
LEFT JOIN (
    SELECT `b`.*
    FROM `BaseCollectionOnBase` `b`
    WHERE `b`.`Discriminator` IN ('DerivedCollectionOnBase', 'BaseCollectionOnBase')
) `b` ON `e`.`ParentCollectionId` = `b`.`Id`
LEFT JOIN (
    SELECT `b0`.*
    FROM `BaseInheritanceRelationshipEntity` `b0`
    WHERE `b0`.`Discriminator` IN ('DerivedInheritanceRelationshipEntity', 'BaseInheritanceRelationshipEntity')
) `b0` ON `b`.`BaseParentId` = `b0`.`Id`
WHERE `e`.`Discriminator` IN ('NestedCollectionDerived', 'NestedCollectionBase')",
                Sql);
        }

        public override void Nested_include_collection_reference_on_non_entity_base()
        {
            base.Nested_include_collection_reference_on_non_entity_base();

            Assert.Equal(
                @"SELECT `e`.`Id`, `e`.`Name`
FROM `ReferencedEntity` `e`
ORDER BY `e`.`Id`

SELECT `p`.`Id`, `p`.`Name`, `p`.`ReferenceId`, `p`.`ReferencedEntityId`, `r`.`Id`, `r`.`Name`
FROM `PrincipalEntity` `p`
LEFT JOIN `ReferencedEntity` `r` ON `p`.`ReferenceId` = `r`.`Id`
WHERE EXISTS (
    SELECT 1
    FROM `ReferencedEntity` `e`
    WHERE `p`.`ReferencedEntityId` = `e`.`Id`)
ORDER BY `p`.`ReferencedEntityId`",
                Sql);
        }

        protected override void ClearLog()
        {
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
