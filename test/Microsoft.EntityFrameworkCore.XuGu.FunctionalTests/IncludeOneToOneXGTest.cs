// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests
{
    public class IncludeOneToOneXGTest : IncludeOneToOneTestBase, IClassFixture<OneToOneQueryXGFixture>
    {
        public override void Include_person()
        {
            base.Include_person();

            Assert.Equal(
                @"SELECT `a`.`Id`, `a`.`City`, `a`.`Street`, `p`.`Id`, `p`.`Name`
FROM `Address` `a`
INNER JOIN `Person` `p` ON `a`.`Id` = `p`.`Id`",
                Sql);
        }

        public override void Include_person_shadow()
        {
            base.Include_person_shadow();

            Assert.Equal(
                @"SELECT `a`.`Id`, `a`.`City`, `a`.`PersonId`, `a`.`Street`, `p`.`Id`, `p`.`Name`
FROM `Address2` `a`
INNER JOIN `Person2` `p` ON `a`.`PersonId` = `p`.`Id`",
                Sql);
        }

        public override void Include_address()
        {
            base.Include_address();

            Assert.Equal(
                @"SELECT `p`.`Id`, `p`.`Name`, `a`.`Id`, `a`.`City`, `a`.`Street`
FROM `Person` `p`
LEFT JOIN `Address` `a` ON `a`.`Id` = `p`.`Id`",
                Sql);
        }

        public override void Include_address_shadow()
        {
            base.Include_address_shadow();

            Assert.Equal(
                @"SELECT `p`.`Id`, `p`.`Name`, `a`.`Id`, `a`.`City`, `a`.`PersonId`, `a`.`Street`
FROM `Person2` `p`
LEFT JOIN `Address2` `a` ON `a`.`PersonId` = `p`.`Id`",
                Sql);
        }

        private readonly OneToOneQueryXGFixture _fixture;

        public IncludeOneToOneXGTest(OneToOneQueryXGFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext() => _fixture.CreateContext();

        private static string Sql => TestSqlLoggerFactory.SqlStatements.Last();
    }
}
