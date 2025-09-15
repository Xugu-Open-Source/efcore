// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests
{
    public class XGNetTopologySuiteApiConsistencyTest : ApiConsistencyTestBase<XGNetTopologySuiteApiConsistencyTest.XGNetTopologySuiteApiConsistencyFixture>
    {
        public XGNetTopologySuiteApiConsistencyTest(XGNetTopologySuiteApiConsistencyFixture fixture)
            : base(fixture)
        {
        }

        protected override void AddServices(ServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkXGNetTopologySuite();

        protected override Assembly TargetAssembly
            => typeof(XGNetTopologySuiteServiceCollectionExtensions).Assembly;

        public class XGNetTopologySuiteApiConsistencyFixture : ApiConsistencyFixtureBase
        {
            public override HashSet<Type> FluentApiTypes { get; } = new()
            {
                typeof(XGNetTopologySuiteDbContextOptionsBuilderExtensions),
                typeof(XGNetTopologySuiteServiceCollectionExtensions)
            };
        }
    }
}
