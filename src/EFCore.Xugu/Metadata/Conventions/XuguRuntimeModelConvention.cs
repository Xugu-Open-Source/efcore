// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that creates an optimized copy of the mutable model.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-xugu">Accessing SQL Server and SQL Azure databases with EF Core</see>
    ///     for more information.
    /// </remarks>
    public class XuguRuntimeModelConvention : RelationalRuntimeModelConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalModelConvention" />.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
        /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
        public XuguRuntimeModelConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     Updates the model annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations">The annotations to be processed.</param>
        /// <param name="model">The source model.</param>
        /// <param name="runtimeModel">The target model that will contain the annotations.</param>
        /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
        protected override void ProcessModelAnnotations(
            Dictionary<string, object?> annotations,
            IModel model,
            RuntimeModel runtimeModel,
            bool runtime)
        {
            base.ProcessModelAnnotations(annotations, model, runtimeModel, runtime);

            if (!runtime)
            {
                annotations.Remove(XuguAnnotationNames.IdentityIncrement);
                annotations.Remove(XuguAnnotationNames.IdentitySeed);
                annotations.Remove(XuguAnnotationNames.MaxDatabaseSize);
                annotations.Remove(XuguAnnotationNames.PerformanceLevelSql);
                annotations.Remove(XuguAnnotationNames.ServiceTierSql);
            }
        }

        /// <summary>
        ///     Updates the property annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations">The annotations to be processed.</param>
        /// <param name="property">The source property.</param>
        /// <param name="runtimeProperty">The target property that will contain the annotations.</param>
        /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
        protected override void ProcessPropertyAnnotations(
            Dictionary<string, object?> annotations,
            IProperty property,
            RuntimeProperty runtimeProperty,
            bool runtime)
        {
            base.ProcessPropertyAnnotations(annotations, property, runtimeProperty, runtime);

            if (!runtime)
            {
                annotations.Remove(XuguAnnotationNames.IdentityIncrement);
                annotations.Remove(XuguAnnotationNames.IdentitySeed);
                annotations.Remove(XuguAnnotationNames.Sparse);

                if (!annotations.ContainsKey(XuguAnnotationNames.ValueGenerationStrategy))
                {
                    annotations[XuguAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
                }
            }
        }

        /// <summary>
        ///     Updates the index annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations">The annotations to be processed.</param>
        /// <param name="index">The source index.</param>
        /// <param name="runtimeIndex">The target index that will contain the annotations.</param>
        /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
        protected override void ProcessIndexAnnotations(
            Dictionary<string, object?> annotations,
            IIndex index,
            RuntimeIndex runtimeIndex,
            bool runtime)
        {
            base.ProcessIndexAnnotations(annotations, index, runtimeIndex, runtime);

            if (!runtime)
            {
                annotations.Remove(XuguAnnotationNames.Clustered);
                annotations.Remove(XuguAnnotationNames.CreatedOnline);
                annotations.Remove(XuguAnnotationNames.Include);
                annotations.Remove(XuguAnnotationNames.FillFactor);
            }
        }

        /// <summary>
        ///     Updates the key annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations">The annotations to be processed.</param>
        /// <param name="key">The source key.</param>
        /// <param name="runtimeKey">The target key that will contain the annotations.</param>
        /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
        protected override void ProcessKeyAnnotations(
            IDictionary<string, object?> annotations,
            IKey key,
            RuntimeKey runtimeKey,
            bool runtime)
        {
            base.ProcessKeyAnnotations(annotations, key, runtimeKey, runtime);

            if (!runtime)
            {
                annotations.Remove(XuguAnnotationNames.Clustered);
            }
        }

        /// <summary>
        ///     Updates the entity type annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations">The annotations to be processed.</param>
        /// <param name="entityType">The source entity type.</param>
        /// <param name="runtimeEntityType">The target entity type that will contain the annotations.</param>
        /// <param name="runtime">Indicates whether the given annotations are runtime annotations.</param>
        protected override void ProcessEntityTypeAnnotations(
            IDictionary<string, object?> annotations,
            IEntityType entityType,
            RuntimeEntityType runtimeEntityType,
            bool runtime)
        {
            base.ProcessEntityTypeAnnotations(annotations, entityType, runtimeEntityType, runtime);

            if (!runtime)
            {
                annotations.Remove(XuguAnnotationNames.TemporalHistoryTableName);
                annotations.Remove(XuguAnnotationNames.TemporalHistoryTableSchema);
                annotations.Remove(XuguAnnotationNames.TemporalPeriodEndColumnName);
                annotations.Remove(XuguAnnotationNames.TemporalPeriodEndPropertyName);
                annotations.Remove(XuguAnnotationNames.TemporalPeriodStartColumnName);
                annotations.Remove(XuguAnnotationNames.TemporalPeriodStartPropertyName);
            }
        }
    }
}
