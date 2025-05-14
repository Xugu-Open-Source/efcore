// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class QueryFilterFuncletizationXGTest
        : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationXGTest.QueryFilterFuncletizationXGFixture>
    {
        public QueryFilterFuncletizationXGTest(
            QueryFilterFuncletizationXGFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override void DbContext_list_is_parameterized()
        {
            base.DbContext_list_is_parameterized();

            if (XGTestHelpers.HasPrimitiveCollectionsSupport(Fixture))
            {
                AssertSql(
"""
SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE `l`.`Tenant` IN (
    SELECT `e`.`value`
    FROM JSON_TABLE(NULL, '$[*]' COLUMNS (
        `key` FOR ORDINALITY,
        `value` int PATH '$[0]'
    )) AS `e`
)
""",
                //
                """
SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE `l`.`Tenant` IN (
    SELECT `e`.`value`
    FROM JSON_TABLE('[]', '$[*]' COLUMNS (
        `key` FOR ORDINALITY,
        `value` int PATH '$[0]'
    )) AS `e`
)
""",
                //
                """
SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE `l`.`Tenant` IN (
    SELECT `e`.`value`
    FROM JSON_TABLE('[1]', '$[*]' COLUMNS (
        `key` FOR ORDINALITY,
        `value` int PATH '$[0]'
    )) AS `e`
)
""",
                //
                """
SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE `l`.`Tenant` IN (
    SELECT `e`.`value`
    FROM JSON_TABLE('[2,3]', '$[*]' COLUMNS (
        `key` FOR ORDINALITY,
        `value` int PATH '$[0]'
    )) AS `e`
)
""");
            }
            else
            {
                AssertSql(
"""
SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE FALSE
""",
                //
                """
SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE FALSE
""",
                //
                """
SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE `l`.`Tenant` = 1
""",
                //
                """
SELECT `l`.`Id`, `l`.`Tenant`
FROM `ListFilter` AS `l`
WHERE `l`.`Tenant` IN (2, 3)
""");
            }
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class QueryFilterFuncletizationXGFixture : QueryFilterFuncletizationRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }
}




