// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestProviderCodeGenerator : ProviderCodeGenerator
    {
        private static readonly MethodInfo _useXGMethodInfo = typeof(XGDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
            nameof(XGDbContextOptionsBuilderExtensions.UseXG),
            typeof(string),
            typeof(ServerVersion),
            typeof(Action<XGDbContextOptionsBuilder>));

        public TestProviderCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override MethodCallCodeFragment GenerateUseProvider(
            string connectionString,
            MethodCallCodeFragment providerOptions)
            => new MethodCallCodeFragment(
                _useXGMethodInfo,
                providerOptions == null
                    ? new object[] { connectionString }
                    : new object[] { connectionString, new NestedClosureCodeFragment("x", providerOptions) });
    }
}
