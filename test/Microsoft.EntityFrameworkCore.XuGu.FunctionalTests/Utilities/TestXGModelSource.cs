// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XG.FunctionalTests.Utilities
{
    public class TestXGModelSource : XGModelSource
    {
        private readonly TestModelSource _testModelSource;

        public TestXGModelSource(
            Action<ModelBuilder> onModelCreating,
            IDbSetFinder setFinder,
            ICoreConventionSetBuilder coreConventionSetBuilder)
            : base(setFinder, coreConventionSetBuilder, new ModelCustomizer(), new ModelCacheKeyFactory())
        {
            _testModelSource = new TestModelSource(onModelCreating, setFinder, coreConventionSetBuilder, new ModelCustomizer(), new ModelCacheKeyFactory());
        }

        public override IModel GetModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
            => _testModelSource.GetModel(context, conventionSetBuilder, validator);

        public static Func<IServiceProvider, XGModelSource> GetFactory(Action<ModelBuilder> onModelCreating)
            => p => new TestXGModelSource(
                onModelCreating,
                p.GetRequiredService<IDbSetFinder>(),
                p.GetRequiredService<ICoreConventionSetBuilder>());
    }
}
