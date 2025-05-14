// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetTopologySuite;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     EntityFrameworkCore.XG.NetTopologySuite extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class XGNetTopologySuiteServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the services required for NetTopologySuite support in the XG provider for Entity Framework.
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns> The same service collection so that multiple calls can be chained. </returns>
        public static IServiceCollection AddEntityFrameworkXGNetTopologySuite(
            [NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection.TryAddSingleton(NtsGeometryServices.Instance);

            new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<IRelationalTypeMappingSourcePlugin, XGNetTopologySuiteTypeMappingSourcePlugin>()
                .TryAdd<IMethodCallTranslatorPlugin, XGNetTopologySuiteMethodCallTranslatorPlugin>()
                .TryAdd<IMemberTranslatorPlugin, XGNetTopologySuiteMemberTranslatorPlugin>()
                .TryAddProviderSpecificServices(
                    x => x.TryAddSingletonEnumerable<IXGEvaluatableExpressionFilter, XGNetTopologySuiteEvaluatableExpressionFilter>());

            return serviceCollection;
        }
    }
}
