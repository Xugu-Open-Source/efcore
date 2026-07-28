// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.FunctionalTests.Query;

public class GearsOfWarFromSqlQueryXGTest : GearsOfWarFromSqlQueryTestBase<GearsOfWarQueryXuguFixture>
{
    public GearsOfWarFromSqlQueryXGTest(GearsOfWarQueryXuguFixture fixture)
        : base(fixture)
    {
    }

    public override void From_sql_queryable_simple_columns_out_of_order()
    {
        using var context = CreateContext();
        var weapons = XuguTestStoreFactory.Instance.FormatTableName(Fixture.TestStore.Name, "Weapons");
        var actual = context.Set<Weapon>().FromSqlRaw(
                $"""
SELECT `Id`, `Name`, `IsAutomatic`, `AmmunitionType`, `OwnerFullName`, `SynergyWithId`
FROM `{weapons}`
ORDER BY `Name`
""")
            .ToArray();

        Assert.Equal(10, actual.Length);

        var first = actual.First();

        Assert.Equal(AmmunitionType.Shell, first.AmmunitionType);
        Assert.Equal("Baird's Gnasher", first.Name);
    }
}
