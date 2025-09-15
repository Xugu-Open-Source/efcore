// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.EntityFrameworkCore.ModelBuilding;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.ModelBuilding;

public class XGModelBuilderTestBase : RelationalModelBuilderTest
{
    public abstract class XGNonRelationship(XGModelBuilderFixture fixture)
        : RelationalNonRelationshipTestBase(fixture), IClassFixture<XGModelBuilderFixture>;

    public abstract class XGComplexType(XGModelBuilderFixture fixture)
        : RelationalComplexTypeTestBase(fixture), IClassFixture<XGModelBuilderFixture>;

    public abstract class XGInheritance(XGModelBuilderFixture fixture)
        : RelationalInheritanceTestBase(fixture), IClassFixture<XGModelBuilderFixture>;

    public abstract class XGOneToMany(XGModelBuilderFixture fixture)
        : RelationalOneToManyTestBase(fixture), IClassFixture<XGModelBuilderFixture>;

    public abstract class XGManyToOne(XGModelBuilderFixture fixture)
        : RelationalManyToOneTestBase(fixture), IClassFixture<XGModelBuilderFixture>;

    public abstract class XGOneToOne(XGModelBuilderFixture fixture)
        : RelationalOneToOneTestBase(fixture), IClassFixture<XGModelBuilderFixture>;

    public abstract class XGManyToMany(XGModelBuilderFixture fixture)
        : RelationalManyToManyTestBase(fixture), IClassFixture<XGModelBuilderFixture>;

    public abstract class XGOwnedTypes(XGModelBuilderFixture fixture)
        : RelationalOwnedTypesTestBase(fixture), IClassFixture<XGModelBuilderFixture>;

    public class XGModelBuilderFixture : RelationalModelBuilderFixture
    {
        public override TestHelpers TestHelpers
            => XGTestHelpers.Instance;
    }
}
