// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ModelBuilding;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class XGComplianceTest : RelationalComplianceTestBase
    {
        // TODO: Implement remaining 3.x tests.
        protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
        {
            // There are two classes that can lead to a XGEndOfStreamException, if *both* test classes are included in the run:
            //     - RelationalModelBuilderTest.RelationalComplexTypeTestBase
            //     - RelationalModelBuilderTest.RelationalOwnedTypesTestBase
            //
            // The exception is thrown for XuGu most of the time, though in rare cases also for MariaDB.
            // We disable `RelationalModelBuilderTest.RelationalOwnedTypesTestBase` for now.

            // typeof(RelationalModelBuilderTest.RelationalNonRelationshipTestBase),
            // typeof(RelationalModelBuilderTest.RelationalComplexTypeTestBase),
            // typeof(RelationalModelBuilderTest.RelationalInheritanceTestBase),
            // typeof(RelationalModelBuilderTest.RelationalOneToManyTestBase),
            // typeof(RelationalModelBuilderTest.RelationalManyToOneTestBase),
            // typeof(RelationalModelBuilderTest.RelationalOneToOneTestBase),
            // typeof(RelationalModelBuilderTest.RelationalManyToManyTestBase),
            typeof(RelationalModelBuilderTest.RelationalOwnedTypesTestBase),
            typeof(ModelBuilderTest.OwnedTypesTestBase), // base class of RelationalModelBuilderTest.RelationalOwnedTypesTestBase

            typeof(UdfDbFunctionTestBase<>),
            typeof(TransactionInterceptionTestBase),
            typeof(CommandInterceptionTestBase),
            typeof(NorthwindQueryTaggingQueryTestBase<>),

            // TODO: 9.0
            typeof(AdHocComplexTypeQueryTestBase),
            typeof(AdHocPrecompiledQueryRelationalTestBase),
            typeof(PrecompiledQueryRelationalTestBase),
            typeof(PrecompiledSqlPregenerationQueryRelationalTestBase),

            // TODO: Reenable LoggingXGTest once its issue has been fixed in EF Core upstream.
            typeof(LoggingTestBase),
            typeof(LoggingRelationalTestBase<,>),

            // We have our own JSON support for now
            typeof(AdHocJsonQueryTestBase),
            typeof(JsonQueryRelationalTestBase<>),
            typeof(JsonQueryTestBase<>),
            typeof(JsonTypesRelationalTestBase),
            typeof(JsonTypesTestBase),
            typeof(JsonUpdateTestBase<>),
            typeof(OptionalDependentQueryTestBase<>),
        };

        protected override Assembly TargetAssembly { get; } = typeof(XGComplianceTest).Assembly;
    }
}
