// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using EntityFrameworkCore.XuGu.Diagnostics.Internal;
using EntityFrameworkCore.XuGu.Infrastructure.Internal;
using EntityFrameworkCore.XuGu.Internal;
using EntityFrameworkCore.XuGu.Scaffolding.Internal;
using EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EntityFrameworkCore.XuGu.Design.Internal
{
    public class XGDesignTimeServices : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<LoggingDefinitions, XGLoggingDefinitions>()
                .AddSingleton<IRelationalTypeMappingSource, XGTypeMappingSource>()
                .AddSingleton<IDatabaseModelFactory, XGDatabaseModelFactory>()
                .AddSingleton<IProviderConfigurationCodeGenerator, XGCodeGenerator>()
                .AddSingleton<IAnnotationCodeGenerator, XGAnnotationCodeGenerator>();

            serviceCollection.TryAddSingleton<IXGOptions, XGOptions>();
        }
    }
}
