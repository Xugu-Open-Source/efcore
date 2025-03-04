// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Storage.Internal;

namespace EntityFrameworkCore.XuGu.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class XGCodeGenerator : ProviderCodeGenerator
    {
        private readonly IXGOptions _options;

        public XGCodeGenerator(
            [NotNull] ProviderCodeGeneratorDependencies dependencies,
            IXGOptions options)
            : base(dependencies)
        {
            _options = options;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override MethodCallCodeFragment GenerateUseProvider(
            string connectionString,
            MethodCallCodeFragment providerOptions)
        {
            // Strip scaffolding specific connection string options first.
            connectionString = new XGScaffoldingConnectionSettings(connectionString).GetProviderCompatibleConnectionString();

            return new MethodCallCodeFragment(
                nameof(XGDbContextOptionsBuilderExtensions.UseXG),
                providerOptions == null
                    ? new object[] {connectionString, new XGCodeGenerationServerVersionCreation(_options.ServerVersion)}
                    : new object[] {connectionString, new XGCodeGenerationServerVersionCreation(_options.ServerVersion), new NestedClosureCodeFragment("x", providerOptions)});
        }
    }
}
