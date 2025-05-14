// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal
{
    public class XGNetTopologySuiteCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
    {
        private static readonly MethodInfo _useNetTopologySuiteMethodInfo =
            typeof(XGNetTopologySuiteDbContextOptionsBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XGNetTopologySuiteDbContextOptionsBuilderExtensions.UseNetTopologySuite),
                typeof(XGDbContextOptionsBuilder));

        public override MethodCallCodeFragment GenerateProviderOptions()
            => new MethodCallCodeFragment(_useNetTopologySuiteMethodInfo);
    }
}
