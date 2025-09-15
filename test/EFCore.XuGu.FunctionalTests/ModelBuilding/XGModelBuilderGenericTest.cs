// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ModelBuilding;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.ModelBuilding;

public class XGModelBuilderGenericTest : XGModelBuilderTestBase
{
    public class XGGenericNonRelationship(XGModelBuilderFixture fixture) : XGNonRelationship(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder> configure)
            => new ModelBuilderTest.GenericTestModelBuilder(Fixture, configure);
    }

    public class XGGenericComplexType(XGModelBuilderFixture fixture) : XGComplexType(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder> configure)
            => new ModelBuilderTest.GenericTestModelBuilder(Fixture, configure);
    }

    public class XGGenericInheritance(XGModelBuilderFixture fixture) : XGInheritance(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder> configure)
            => new ModelBuilderTest.GenericTestModelBuilder(Fixture, configure);
    }

    public class XGGenericOneToMany(XGModelBuilderFixture fixture) : XGOneToMany(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder> configure)
            => new ModelBuilderTest.GenericTestModelBuilder(Fixture, configure);
    }

    public class XGGenericManyToOne(XGModelBuilderFixture fixture) : XGManyToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder> configure)
            => new ModelBuilderTest.GenericTestModelBuilder(Fixture, configure);
    }

    public class XGGenericOneToOne(XGModelBuilderFixture fixture) : XGOneToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder> configure)
            => new ModelBuilderTest.GenericTestModelBuilder(Fixture, configure);
    }

    public class XGGenericManyToMany(XGModelBuilderFixture fixture) : XGManyToMany(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder> configure)
            => new ModelBuilderTest.GenericTestModelBuilder(Fixture, configure);
    }

    internal class XGGenericOwnedTypes(XGModelBuilderFixture fixture) : XGOwnedTypes(fixture)
    {
        // XuGu stored procedures do not support result columns.
        public override void Can_use_sproc_mapping_with_owned_reference()
            => Assert.Throws<InvalidOperationException>(() => base.Can_use_sproc_mapping_with_owned_reference());

        protected override TestModelBuilder CreateModelBuilder(
            Action<ModelConfigurationBuilder> configure)
            => new ModelBuilderTest.GenericTestModelBuilder(Fixture, configure);
    }
}
