// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace EntityFrameworkCore.XuGu.Json.Newtonsoft.Scaffolding.Internal
{
    public class XGJsonNewtonsoftCodeGeneratorPlugin : ProviderCodeGeneratorPlugin
    {
        public override MethodCallCodeFragment GenerateProviderOptions()
            => new MethodCallCodeFragment(
                nameof(XGJsonNewtonsoftDbContextOptionsBuilderExtensions.UseNewtonsoftJson));
    }
}
