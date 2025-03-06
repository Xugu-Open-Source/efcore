// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     <para>
    ///         A builder for building conventions for SQL Server.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" /> and multiple registrations
    ///         are allowed. This means that each <see cref="DbContext" /> instance will use its own
    ///         set of instances of this service.
    ///         The implementations may depend on other services registered with any lifetime.
    ///         The implementations do not need to be thread-safe.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-xugu">Accessing SQL Server and SQL Azure databases with EF Core</see>
    ///     for more information.
    /// </remarks>
    public class XuguConventionSetBuilder : RelationalConventionSetBuilder
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        /// <summary>
        ///     Creates a new <see cref="XuguConventionSetBuilder" /> instance.
        /// </summary>
        /// <param name="dependencies">The core dependencies for this service.</param>
        /// <param name="relationalDependencies">The relational dependencies for this service.</param>
        /// <param name="sqlGenerationHelper">The SQL generation helper to use.</param>
        public XuguConventionSetBuilder(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies,
            ISqlGenerationHelper sqlGenerationHelper)
            : base(dependencies, relationalDependencies)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        /// <summary>
        ///     Builds and returns the convention set for the current database provider.
        /// </summary>
        /// <returns>The convention set for the current database provider.</returns>
        public override ConventionSet CreateConventionSet()
        {
            var conventionSet = base.CreateConventionSet();

            var valueGenerationStrategyConvention = new XuguValueGenerationStrategyConvention(Dependencies, RelationalDependencies);
            conventionSet.ModelInitializedConventions.Add(valueGenerationStrategyConvention);
            conventionSet.ModelInitializedConventions.Add(
                new RelationalMaxIdentifierLengthConvention(128, Dependencies, RelationalDependencies));

            ValueGenerationConvention valueGenerationConvention =
                new XuguValueGenerationConvention(Dependencies, RelationalDependencies);
            var xuguIndexConvention = new XuguIndexConvention(Dependencies, RelationalDependencies, _sqlGenerationHelper);
            ReplaceConvention(conventionSet.EntityTypeBaseTypeChangedConventions, valueGenerationConvention);
            conventionSet.EntityTypeBaseTypeChangedConventions.Add(xuguIndexConvention);

            var xuguInMemoryTablesConvention = new XuguMemoryOptimizedTablesConvention(Dependencies, RelationalDependencies);
            conventionSet.EntityTypeAnnotationChangedConventions.Add(xuguInMemoryTablesConvention);
            ReplaceConvention(
                conventionSet.EntityTypeAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);

            var xuguTemporalConvention = new XuguTemporalConvention(Dependencies, RelationalDependencies);
            ConventionSet.AddBefore(
                conventionSet.EntityTypeAnnotationChangedConventions,
                xuguTemporalConvention,
                typeof(XuguValueGenerationConvention));

            ReplaceConvention(conventionSet.EntityTypePrimaryKeyChangedConventions, valueGenerationConvention);

            conventionSet.KeyAddedConventions.Add(xuguInMemoryTablesConvention);

            var xuguOnDeleteConvention = new XuguOnDeleteConvention(Dependencies, RelationalDependencies);
            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, valueGenerationConvention);
            ReplaceConvention(conventionSet.ForeignKeyAddedConventions, (CascadeDeleteConvention)xuguOnDeleteConvention);

            ReplaceConvention(conventionSet.ForeignKeyRemovedConventions, valueGenerationConvention);

            ReplaceConvention(conventionSet.ForeignKeyRequirednessChangedConventions, (CascadeDeleteConvention)xuguOnDeleteConvention);

            conventionSet.SkipNavigationForeignKeyChangedConventions.Add(xuguOnDeleteConvention);

            conventionSet.IndexAddedConventions.Add(xuguInMemoryTablesConvention);
            conventionSet.IndexAddedConventions.Add(xuguIndexConvention);

            conventionSet.IndexUniquenessChangedConventions.Add(xuguIndexConvention);

            conventionSet.IndexAnnotationChangedConventions.Add(xuguIndexConvention);

            conventionSet.PropertyNullabilityChangedConventions.Add(xuguIndexConvention);

            StoreGenerationConvention storeGenerationConvention =
                new XuguStoreGenerationConvention(Dependencies, RelationalDependencies);
            conventionSet.PropertyAnnotationChangedConventions.Add(xuguIndexConvention);
            ReplaceConvention(conventionSet.PropertyAnnotationChangedConventions, storeGenerationConvention);
            ReplaceConvention(
                conventionSet.PropertyAnnotationChangedConventions, (RelationalValueGenerationConvention)valueGenerationConvention);

            conventionSet.ModelFinalizingConventions.Add(valueGenerationStrategyConvention);
            ReplaceConvention(conventionSet.ModelFinalizingConventions, storeGenerationConvention);
            ReplaceConvention(
                conventionSet.ModelFinalizingConventions,
                (SharedTableConvention)new XuguSharedTableConvention(Dependencies, RelationalDependencies));
            conventionSet.ModelFinalizingConventions.Add(new XuguDbFunctionConvention(Dependencies, RelationalDependencies));

            ReplaceConvention(
                conventionSet.ModelFinalizedConventions,
                (RuntimeModelConvention)new XuguRuntimeModelConvention(Dependencies, RelationalDependencies));

            conventionSet.SkipNavigationForeignKeyChangedConventions.Add(xuguTemporalConvention);

            return conventionSet;
        }

        /// <summary>
        ///     <para>
        ///         Call this method to build a <see cref="ConventionSet" /> for SQL Server when using
        ///         the <see cref="ModelBuilder" /> outside of <see cref="DbContext.OnModelCreating" />.
        ///     </para>
        ///     <para>
        ///         Note that it is unusual to use this method.
        ///         Consider using <see cref="DbContext" /> in the normal way instead.
        ///     </para>
        /// </summary>
        /// <returns>The convention set.</returns>
        public static ConventionSet Build()
        {
            using var serviceScope = CreateServiceScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
            return ConventionSet.CreateConventionSet(context);
        }

        /// <summary>
        ///     <para>
        ///         Call this method to build a <see cref="ModelBuilder" /> for SQL Server outside of <see cref="DbContext.OnModelCreating" />.
        ///     </para>
        ///     <para>
        ///         Note that it is unusual to use this method.
        ///         Consider using <see cref="DbContext" /> in the normal way instead.
        ///     </para>
        /// </summary>
        /// <returns>The convention set.</returns>
        public static ModelBuilder CreateModelBuilder()
        {
            using var serviceScope = CreateServiceScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<DbContext>();
            return new ModelBuilder(ConventionSet.CreateConventionSet(context), context.GetService<ModelDependencies>());
        }

        private static IServiceScope CreateServiceScope()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkXugu()
                .AddDbContext<DbContext>(
                    (p, o) =>
                        o.UseXugu("Server=.")
                            .UseInternalServiceProvider(p))
                .BuildServiceProvider();

            return serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }
    }
}
