// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.EntityFrameworkCore.XuGu.Design.Internal
{
    public class XGDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IRelationalTypeMappingSource, XGTypeMappingSource>()
                .AddSingleton<IDatabaseModelFactory, XGDatabaseModelFactory>()
                .AddSingleton<IProviderConfigurationCodeGenerator, XGCodeGenerator>()
                .AddSingleton<IAnnotationCodeGenerator, XGAnnotationCodeGenerator>()
                .TryAddSingleton<IXGOptions, XGOptions>();
        }
    }
}
