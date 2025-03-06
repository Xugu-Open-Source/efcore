// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Xugu.Internal;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Property extension methods for SQL Server-specific metadata.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
    ///     <see href="https://aka.ms/efcore-docs-xugu">Accessing SQL Server and SQL Azure databases with EF Core</see>
    ///     for more information.
    /// </remarks>
    public static class XuguPropertyExtensions
    {
        /// <summary>
        ///     Returns the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The name to use for the hi-lo sequence.</returns>
        public static string? GetHiLoSequenceName(this IReadOnlyProperty property)
            => (string?)property[XuguAnnotationNames.HiLoSequenceName];

        /// <summary>
        ///     Returns the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The name to use for the hi-lo sequence.</returns>
        public static string? GetHiLoSequenceName(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(XuguAnnotationNames.HiLoSequenceName);
            if (annotation != null)
            {
                return (string?)annotation.Value;
            }

            return property.FindSharedStoreObjectRootProperty(storeObject)?.GetHiLoSequenceName(storeObject);
        }

        /// <summary>
        ///     Sets the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="name">The sequence name to use.</param>
        public static void SetHiLoSequenceName(this IMutableProperty property, string? name)
            => property.SetOrRemoveAnnotation(
                XuguAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(name, nameof(name)));

        /// <summary>
        ///     Sets the name to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="name">The sequence name to use.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>The configured value.</returns>
        public static string? SetHiLoSequenceName(
            this IConventionProperty property,
            string? name,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                XuguAnnotationNames.HiLoSequenceName,
                Check.NullButNotEmpty(name, nameof(name)),
                fromDataAnnotation);

            return name;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the hi-lo sequence name.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the hi-lo sequence name.</returns>
        public static ConfigurationSource? GetHiLoSequenceNameConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(XuguAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The schema to use for the hi-lo sequence.</returns>
        public static string? GetHiLoSequenceSchema(this IReadOnlyProperty property)
            => (string?)property[XuguAnnotationNames.HiLoSequenceSchema];

        /// <summary>
        ///     Returns the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The schema to use for the hi-lo sequence.</returns>
        public static string? GetHiLoSequenceSchema(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var annotation = property.FindAnnotation(XuguAnnotationNames.HiLoSequenceSchema);
            if (annotation != null)
            {
                return (string?)annotation.Value;
            }

            return property.FindSharedStoreObjectRootProperty(storeObject)?.GetHiLoSequenceSchema(storeObject);
        }

        /// <summary>
        ///     Sets the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="schema">The schema to use.</param>
        public static void SetHiLoSequenceSchema(this IMutableProperty property, string? schema)
            => property.SetOrRemoveAnnotation(
                XuguAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(schema, nameof(schema)));

        /// <summary>
        ///     Sets the schema to use for the hi-lo sequence.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="schema">The schema to use.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>The configured value.</returns>
        public static string? SetHiLoSequenceSchema(
            this IConventionProperty property,
            string? schema,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                XuguAnnotationNames.HiLoSequenceSchema,
                Check.NullButNotEmpty(schema, nameof(schema)),
                fromDataAnnotation);

            return schema;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the hi-lo sequence schema.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the hi-lo sequence schema.</returns>
        public static ConfigurationSource? GetHiLoSequenceSchemaConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(XuguAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

        /// <summary>
        ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The sequence to use, or <see langword="null" /> if no sequence exists in the model.</returns>
        public static IReadOnlySequence? FindHiLoSequence(this IReadOnlyProperty property)
        {
            var model = property.DeclaringEntityType.Model;

            var sequenceName = property.GetHiLoSequenceName()
                ?? model.GetHiLoSequenceName();

            var sequenceSchema = property.GetHiLoSequenceSchema()
                ?? model.GetHiLoSequenceSchema();

            return model.FindSequence(sequenceName, sequenceSchema);
        }

        /// <summary>
        ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The sequence to use, or <see langword="null" /> if no sequence exists in the model.</returns>
        public static IReadOnlySequence? FindHiLoSequence(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var model = property.DeclaringEntityType.Model;

            var sequenceName = property.GetHiLoSequenceName(storeObject)
                ?? model.GetHiLoSequenceName();

            var sequenceSchema = property.GetHiLoSequenceSchema(storeObject)
                ?? model.GetHiLoSequenceSchema();

            return model.FindSequence(sequenceName, sequenceSchema);
        }

        /// <summary>
        ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The sequence to use, or <see langword="null" /> if no sequence exists in the model.</returns>
        public static ISequence? FindHiLoSequence(this IProperty property)
            => (ISequence?)((IReadOnlyProperty)property).FindHiLoSequence();

        /// <summary>
        ///     Finds the <see cref="ISequence" /> in the model to use for the hi-lo pattern.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The sequence to use, or <see langword="null" /> if no sequence exists in the model.</returns>
        public static ISequence? FindHiLoSequence(this IProperty property, in StoreObjectIdentifier storeObject)
            => (ISequence?)((IReadOnlyProperty)property).FindHiLoSequence(storeObject);

        /// <summary>
        ///     Returns the identity seed.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The identity seed.</returns>
        public static long? GetIdentitySeed(this IReadOnlyProperty property)
        {
            if (property is RuntimeProperty)
            {
                throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
            }

            // Support pre-6.0 IdentitySeed annotations, which contained an int rather than a long
            var annotation = property.FindAnnotation(XuguAnnotationNames.IdentitySeed);
            return annotation is null
                ? null
                : annotation.Value is int intValue
                    ? intValue
                    : (long?)annotation.Value;
        }

        /// <summary>
        ///     Returns the identity seed.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The identity seed.</returns>
        public static long? GetIdentitySeed(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            if (property is RuntimeProperty)
            {
                throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
            }

            var annotation = property.FindAnnotation(XuguAnnotationNames.IdentitySeed);
            if (annotation is not null)
            {
                // Support pre-6.0 IdentitySeed annotations, which contained an int rather than a long
                return annotation.Value is int intValue
                    ? intValue
                    : (long?)annotation.Value;
            }

            var sharedProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedProperty == null
                ? property.DeclaringEntityType.Model.GetIdentitySeed()
                : sharedProperty.GetIdentitySeed(storeObject);
        }

        /// <summary>
        ///     Sets the identity seed.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="seed">The value to set.</param>
        public static void SetIdentitySeed(this IMutableProperty property, long? seed)
            => property.SetOrRemoveAnnotation(
                XuguAnnotationNames.IdentitySeed,
                seed);

        /// <summary>
        ///     Sets the identity seed.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="seed">The value to set.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>The configured value.</returns>
        public static long? SetIdentitySeed(
            this IConventionProperty property,
            long? seed,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                XuguAnnotationNames.IdentitySeed,
                seed,
                fromDataAnnotation);

            return seed;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the identity seed.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the identity seed.</returns>
        public static ConfigurationSource? GetIdentitySeedConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(XuguAnnotationNames.IdentitySeed)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the identity increment.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The identity increment.</returns>
        public static int? GetIdentityIncrement(this IReadOnlyProperty property)
            => (property is RuntimeProperty)
                ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
                : (int?)property[XuguAnnotationNames.IdentityIncrement]
                ?? property.DeclaringEntityType.Model.GetIdentityIncrement();

        /// <summary>
        ///     Returns the identity increment.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The identity increment.</returns>
        public static int? GetIdentityIncrement(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            if (property is RuntimeProperty)
            {
                throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
            }

            var annotation = property.FindAnnotation(XuguAnnotationNames.IdentityIncrement);
            if (annotation != null)
            {
                return (int?)annotation.Value;
            }

            var sharedProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedProperty == null
                ? property.DeclaringEntityType.Model.GetIdentityIncrement()
                : sharedProperty.GetIdentityIncrement(storeObject);
        }

        /// <summary>
        ///     Sets the identity increment.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="increment">The value to set.</param>
        public static void SetIdentityIncrement(this IMutableProperty property, int? increment)
            => property.SetOrRemoveAnnotation(
                XuguAnnotationNames.IdentityIncrement,
                increment);

        /// <summary>
        ///     Sets the identity increment.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="increment">The value to set.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>The configured value.</returns>
        public static int? SetIdentityIncrement(
            this IConventionProperty property,
            int? increment,
            bool fromDataAnnotation = false)
        {
            property.SetOrRemoveAnnotation(
                XuguAnnotationNames.IdentityIncrement,
                increment,
                fromDataAnnotation);

            return increment;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the identity increment.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the identity increment.</returns>
        public static ConfigurationSource? GetIdentityIncrementConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(XuguAnnotationNames.IdentityIncrement)?.GetConfigurationSource();

        /// <summary>
        ///     <para>
        ///         Returns the <see cref="XuguValueGenerationStrategy" /> to use for the property.
        ///     </para>
        ///     <para>
        ///         If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />.
        ///     </para>
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The strategy, or <see cref="XuguValueGenerationStrategy.None" /> if none was set.</returns>
        public static XuguValueGenerationStrategy GetValueGenerationStrategy(this IReadOnlyProperty property)
        {
            var annotation = property.FindAnnotation(XuguAnnotationNames.ValueGenerationStrategy);
            if (annotation != null)
            {
                return (XuguValueGenerationStrategy?)annotation.Value ?? XuguValueGenerationStrategy.None;
            }

            if (property.ValueGenerated != ValueGenerated.OnAdd
                || property.IsForeignKey()
                || property.TryGetDefaultValue(out _)
                || property.GetDefaultValueSql() != null
                || property.GetComputedColumnSql() != null)
            {
                return XuguValueGenerationStrategy.None;
            }

            return GetDefaultValueGenerationStrategy(property);
        }

        /// <summary>
        ///     <para>
        ///         Returns the <see cref="XuguValueGenerationStrategy" /> to use for the property.
        ///     </para>
        ///     <para>
        ///         If no strategy is set for the property, then the strategy to use will be taken from the <see cref="IModel" />.
        ///     </para>
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The strategy, or <see cref="XuguValueGenerationStrategy.None" /> if none was set.</returns>
        public static XuguValueGenerationStrategy GetValueGenerationStrategy(
            this IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject)
            => GetValueGenerationStrategy(property, storeObject, null);

        internal static XuguValueGenerationStrategy GetValueGenerationStrategy(
            this IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            ITypeMappingSource? typeMappingSource)
        {
            var annotation = property.FindAnnotation(XuguAnnotationNames.ValueGenerationStrategy);
            if (annotation != null)
            {
                return (XuguValueGenerationStrategy?)annotation.Value ?? XuguValueGenerationStrategy.None;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            if (sharedTableRootProperty != null)
            {
                return sharedTableRootProperty.GetValueGenerationStrategy(storeObject)
                    == XuguValueGenerationStrategy.IdentityColumn
                    && !property.GetContainingForeignKeys().Any(fk => !fk.IsBaseLinking())
                        ? XuguValueGenerationStrategy.IdentityColumn
                        : XuguValueGenerationStrategy.None;
            }

            if (property.ValueGenerated != ValueGenerated.OnAdd
                || property.GetContainingForeignKeys().Any(fk => !fk.IsBaseLinking())
                || property.TryGetDefaultValue(storeObject, out _)
                || property.GetDefaultValueSql(storeObject) != null
                || property.GetComputedColumnSql(storeObject) != null)
            {
                return XuguValueGenerationStrategy.None;
            }

            return GetDefaultValueGenerationStrategy(property, storeObject, typeMappingSource);
        }

        private static XuguValueGenerationStrategy GetDefaultValueGenerationStrategy(IReadOnlyProperty property)
        {
            var modelStrategy = property.DeclaringEntityType.Model.GetValueGenerationStrategy();

            if (modelStrategy == XuguValueGenerationStrategy.SequenceHiLo
                && IsCompatibleWithValueGeneration(property))
            {
                return XuguValueGenerationStrategy.SequenceHiLo;
            }

            return modelStrategy == XuguValueGenerationStrategy.IdentityColumn
                && IsCompatibleWithValueGeneration(property)
                    ? XuguValueGenerationStrategy.IdentityColumn
                    : XuguValueGenerationStrategy.None;
        }

        private static XuguValueGenerationStrategy GetDefaultValueGenerationStrategy(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            ITypeMappingSource? typeMappingSource)
        {
            var modelStrategy = property.DeclaringEntityType.Model.GetValueGenerationStrategy();

            if (modelStrategy == XuguValueGenerationStrategy.SequenceHiLo
                && IsCompatibleWithValueGeneration(property, storeObject, typeMappingSource))
            {
                return XuguValueGenerationStrategy.SequenceHiLo;
            }

            return modelStrategy == XuguValueGenerationStrategy.IdentityColumn
                && IsCompatibleWithValueGeneration(property, storeObject, typeMappingSource)
                    ? XuguValueGenerationStrategy.IdentityColumn
                    : XuguValueGenerationStrategy.None;
        }

        /// <summary>
        ///     Sets the <see cref="XuguValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The strategy to use.</param>
        public static void SetValueGenerationStrategy(
            this IMutableProperty property,
            XuguValueGenerationStrategy? value)
        {
            CheckValueGenerationStrategy(property, value);

            property.SetOrRemoveAnnotation(XuguAnnotationNames.ValueGenerationStrategy, value);
        }

        /// <summary>
        ///     Sets the <see cref="XuguValueGenerationStrategy" /> to use for the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The strategy to use.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>The configured value.</returns>
        public static XuguValueGenerationStrategy? SetValueGenerationStrategy(
            this IConventionProperty property,
            XuguValueGenerationStrategy? value,
            bool fromDataAnnotation = false)
        {
            CheckValueGenerationStrategy(property, value);

            property.SetOrRemoveAnnotation(XuguAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);

            return value;
        }

        private static void CheckValueGenerationStrategy(IReadOnlyProperty property, XuguValueGenerationStrategy? value)
        {
            if (value != null)
            {
                var propertyType = property.ClrType;

                if (value == XuguValueGenerationStrategy.IdentityColumn
                    && !IsCompatibleWithValueGeneration(property))
                {
                    throw new ArgumentException(
                        XuguStrings.IdentityBadType(
                            property.Name, property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
                }

                if (value == XuguValueGenerationStrategy.SequenceHiLo
                    && !IsCompatibleWithValueGeneration(property))
                {
                    throw new ArgumentException(
                        XuguStrings.SequenceBadType(
                            property.Name, property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
                }
            }
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the <see cref="XuguValueGenerationStrategy" />.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for the <see cref="XuguValueGenerationStrategy" />.</returns>
        public static ConfigurationSource? GetValueGenerationStrategyConfigurationSource(
            this IConventionProperty property)
            => property.FindAnnotation(XuguAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

        /// <summary>
        ///     Returns a value indicating whether the property is compatible with any <see cref="XuguValueGenerationStrategy" />.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns><see langword="true" /> if compatible.</returns>
        public static bool IsCompatibleWithValueGeneration(IReadOnlyProperty property)
        {
            var valueConverter = property.GetValueConverter()
                ?? property.FindTypeMapping()?.Converter;

            var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

            return (type.IsInteger()
                || type.IsEnum
                || type == typeof(decimal));
        }

        private static bool IsCompatibleWithValueGeneration(
            IReadOnlyProperty property,
            in StoreObjectIdentifier storeObject,
            ITypeMappingSource? typeMappingSource)
        {
            var valueConverter = property.GetValueConverter()
                ?? (property.FindRelationalTypeMapping(storeObject)
                    ?? typeMappingSource?.FindMapping((IProperty)property))?.Converter;

            var type = (valueConverter?.ProviderClrType ?? property.ClrType).UnwrapNullableType();

            return (type.IsInteger()
                || type.IsEnum
                || type == typeof(decimal));
        }

        /// <summary>
        ///     Returns a value indicating whether the property's column is sparse.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns><see langword="true" /> if the property's column is sparse.</returns>
        public static bool? IsSparse(this IReadOnlyProperty property)
            => (property is RuntimeProperty)
                ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
                : (bool?)property[XuguAnnotationNames.Sparse];

        /// <summary>
        ///     Returns a value indicating whether the property's column is sparse.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns><see langword="true" /> if the property's column is sparse.</returns>
        public static bool? IsSparse(this IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            if (property is RuntimeProperty)
            {
                throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
            }

            var annotation = property.FindAnnotation(XuguAnnotationNames.Sparse);
            if (annotation != null)
            {
                return (bool?)annotation.Value;
            }

            var sharedTableRootProperty = property.FindSharedStoreObjectRootProperty(storeObject);
            return sharedTableRootProperty != null
                ? sharedTableRootProperty.IsSparse(storeObject)
                : null;
        }

        /// <summary>
        ///     Sets a value indicating whether the property's column is sparse.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="sparse">The value to set.</param>
        public static void SetIsSparse(this IMutableProperty property, bool? sparse)
            => property.SetAnnotation(XuguAnnotationNames.Sparse, sparse);

        /// <summary>
        ///     Sets a value indicating whether the property's column is sparse.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="sparse">The value to set.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>The configured value.</returns>
        public static bool? SetIsSparse(
            this IConventionProperty property,
            bool? sparse,
            bool fromDataAnnotation = false)
        {
            property.SetAnnotation(
                XuguAnnotationNames.Sparse,
                sparse,
                fromDataAnnotation);

            return sparse;
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for whether the property's column is sparse.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>The <see cref="ConfigurationSource" /> for whether the property's column is sparse.</returns>
        public static ConfigurationSource? GetIsSparseConfigurationSource(this IConventionProperty property)
            => property.FindAnnotation(XuguAnnotationNames.Sparse)?.GetConfigurationSource();
    }
}
