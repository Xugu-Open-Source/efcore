// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query;

public class FunkyDataQueryXGTest : FunkyDataQueryTestBase<FunkyDataQueryXGTest.FunkyDataQueryXGFixture>
{
    public FunkyDataQueryXGTest(FunkyDataQueryXGFixture fixture)
        : base(fixture)
    {
        ClearLog();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task String_contains_on_argument_with_wildcard_constant(bool async)
    {
        await base.String_contains_on_argument_with_wildcard_constant(async);

        AssertSql(
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` LIKE '%\\%B%'
""",
            //
            """
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` LIKE '%a\\_%'
""",
            //
            """
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE FALSE
""",
            //
            """
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` IS NOT NULL
""",
            //
            """
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` LIKE '%\\_Ba\\_%'
""",
            //
            """
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` NOT LIKE '%\\%B\\%a\\%r%' OR (`f`.`FirstName` IS NULL)
""",
            //
            """
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` IS NULL
""",
            //
            """
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
""");
    }

    public override async Task String_starts_with_on_argument_with_wildcard_constant(bool async)
    {
        await base.String_starts_with_on_argument_with_wildcard_constant(async);

        AssertSql(
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` LIKE '\\%B%'
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` LIKE '\\_B%'
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE FALSE
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` IS NOT NULL
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` LIKE '\\_Ba\\_%'
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` NOT LIKE '\\%B\\%a\\%r%' OR (`f`.`FirstName` IS NULL)
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` IS NULL
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
""");
    }

    public override async Task String_ends_with_on_argument_with_wildcard_constant(bool async)
    {
        await base.String_ends_with_on_argument_with_wildcard_constant(async);

        AssertSql(
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` LIKE '%\\%r'
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` LIKE '%r\\_'
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE FALSE
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` IS NOT NULL
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` LIKE '%\\_r\\_'
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` NOT LIKE '%a\\%r\\%' OR (`f`.`FirstName` IS NULL)
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
WHERE `f`.`FirstName` IS NULL
""",
            //
"""
SELECT `f`.`FirstName`
FROM `FunkyCustomers` AS `f`
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class FunkyDataQueryXGFixture : FunkyDataQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        protected override ITestStoreFactory TestStoreFactory
            => XGTestStoreFactory.Instance;
    }
}
