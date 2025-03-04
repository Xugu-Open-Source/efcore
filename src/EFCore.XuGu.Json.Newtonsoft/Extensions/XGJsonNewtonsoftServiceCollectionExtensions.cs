// Copyright (c) Pomelo Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using EntityFrameworkCore.XuGu.Json.Newtonsoft.Query.Internal;
using EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     EntityFrameworkCore.XuGu.Json.Newtonsoft extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class XGJsonNewtonsoftServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the services required for JSON.NET support (Newtonsoft.Json) in Pomelo's XG provider for Entity Framework Core.
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns> The same service collection so that multiple calls can be chained. </returns>
        public static IServiceCollection AddEntityFrameworkXGJsonNewtonsoft(
            [NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAddProviderSpecificServices(
                    x => x
                        .TryAddSingletonEnumerable<IRelationalTypeMappingSourcePlugin, XGJsonNewtonsoftTypeMappingSourcePlugin>()
                        .TryAddSingletonEnumerable<IMethodCallTranslatorPlugin, XGJsonNewtonsoftMethodCallTranslatorPlugin>()
                        .TryAddSingletonEnumerable<IMemberTranslatorPlugin, XGJsonNewtonsoftMemberTranslatorPlugin>()
                        .TryAddSingleton<IXGJsonPocoTranslator, XGJsonNewtonsoftPocoTranslator>());

            return serviceCollection;
        }
    }
}
