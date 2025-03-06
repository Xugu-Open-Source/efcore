// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGuDb.FunctionalTests
{
    public class PropertyEntryXuGuDbTest : PropertyEntryTestBase<XuGuDbTestStore, F1XuGuDbFixture>
    {
        public PropertyEntryXuGuDbTest(F1XuGuDbFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public override void Property_entry_original_value_is_set()
        {
            base.Property_entry_original_value_is_set();

            Assert.Contains(
                @"SELECT `e`.`Id`, `e`.`EngineSupplierId`, `e`.`Name`
FROM `Engines` `e` LIMIT 1",
                Sql);

            Assert.Contains(
                @"UPDATE `Engines` SET `Name` = @p0
WHERE `Id` = @p1 AND `EngineSupplierId` = @p2 AND `Name` = @p3;
SELECT SQL%ROWCOUNT;",
                Sql);
        }

        private static string Sql => TestSqlLoggerFactory.Sql;
    }
}
