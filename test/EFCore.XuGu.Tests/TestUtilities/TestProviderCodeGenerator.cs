// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
