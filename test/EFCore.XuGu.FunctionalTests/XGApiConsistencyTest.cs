// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class XGApiConsistencyTest : ApiConsistencyTestBase<XGApiConsistencyTest.XGApiConsistencyFixture>
    {
        public XGApiConsistencyTest(XGApiConsistencyFixture fixture)
            : base(fixture)
        {
        }

        protected override void AddServices(ServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkXG();

        protected override Assembly TargetAssembly => typeof(XGRelationalConnection).Assembly;

        public class XGApiConsistencyFixture : ApiConsistencyFixtureBase
        {
            public override HashSet<Type> FluentApiTypes { get; } = new()
            {
                typeof(XGDbContextOptionsBuilder),
                typeof(XGDbContextOptionsBuilderExtensions),
                typeof(XGMigrationBuilderExtensions),
                typeof(XGIndexBuilderExtensions),
                typeof(XGModelBuilderExtensions),
                typeof(XGPropertyBuilderExtensions),
                typeof(XGEntityTypeBuilderExtensions),
                typeof(XGServiceCollectionExtensions)
            };

            public override
                Dictionary<Type,
                    (Type ReadonlyExtensions,
                    Type MutableExtensions,
                    Type ConventionExtensions,
                    Type ConventionBuilderExtensions,
                    Type RuntimeExtensions)> MetadataExtensionTypes { get; }
                = new()
                {
                    {
                        typeof(IReadOnlyModel),
                        (
                            typeof(XGModelExtensions),
                            typeof(XGModelExtensions),
                            typeof(XGModelExtensions),
                            typeof(XGModelBuilderExtensions),
                            null
                        )
                    },
                    {
                        typeof(IReadOnlyEntityType),
                        (
                            typeof(XGEntityTypeExtensions),
                            typeof(XGEntityTypeExtensions),
                            typeof(XGEntityTypeExtensions),
                            typeof(XGEntityTypeBuilderExtensions),
                            null
                        )
                    },
                    {
                        typeof(IReadOnlyProperty),
                        (
                            typeof(XGPropertyExtensions),
                            typeof(XGPropertyExtensions),
                            typeof(XGPropertyExtensions),
                            typeof(XGPropertyBuilderExtensions),
                            null
                        )
                    },
                    {
                        typeof(IReadOnlyIndex),
                        (
                            typeof(XGIndexExtensions),
                            typeof(XGIndexExtensions),
                            typeof(XGIndexExtensions),
                            typeof(XGIndexBuilderExtensions),
                            null
                        )
                    },
                };

            public override HashSet<MethodInfo> UnmatchedMetadataMethods { get; } = new()
            {
                typeof(XGModelBuilderExtensions).GetMethod(
                    nameof(XGModelBuilderExtensions.UseCollation),
                    new[] {typeof(IConventionModelBuilder), typeof(string), typeof(DelegationModes?), typeof(bool)}),
                typeof(XGModelBuilderExtensions).GetMethod(
                    nameof(XGModelBuilderExtensions.UseCollation),
                    new[] {typeof(IConventionModelBuilder), typeof(string), typeof(bool?), typeof(bool)}),
            };
        }
    }
}
