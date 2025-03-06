// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Xugu.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Xugu.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class XuguAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        #region MethodInfos

        private static readonly MethodInfo _modelUseIdentityColumnsMethodInfo
            = typeof(XuguModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguModelBuilderExtensions.UseIdentityColumns), typeof(ModelBuilder), typeof(long), typeof(int));

        private static readonly MethodInfo _modelUseHiLoMethodInfo
            = typeof(XuguModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguModelBuilderExtensions.UseHiLo), typeof(ModelBuilder), typeof(string), typeof(string));

        private static readonly MethodInfo _modelHasDatabaseMaxSizeMethodInfo
            = typeof(XuguModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguModelBuilderExtensions.HasDatabaseMaxSize), typeof(ModelBuilder), typeof(string));

        private static readonly MethodInfo _modelHasServiceTierSqlMethodInfo
            = typeof(XuguModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguModelBuilderExtensions.HasServiceTierSql), typeof(ModelBuilder), typeof(string));

        private static readonly MethodInfo _modelHasPerformanceLevelSqlMethodInfo
            = typeof(XuguModelBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguModelBuilderExtensions.HasPerformanceLevelSql), typeof(ModelBuilder), typeof(string));

        private static readonly MethodInfo _modelHasAnnotationMethodInfo
            = typeof(ModelBuilder).GetRequiredRuntimeMethod(
                nameof(ModelBuilder.HasAnnotation), typeof(string), typeof(object));

        private static readonly MethodInfo _entityTypeToTableMethodInfo
            = typeof(RelationalEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(RelationalEntityTypeBuilderExtensions.ToTable), typeof(EntityTypeBuilder), typeof(string));

        private static readonly MethodInfo _entityTypeIsMemoryOptimizedMethodInfo
            = typeof(XuguEntityTypeBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguEntityTypeBuilderExtensions.IsMemoryOptimized), typeof(EntityTypeBuilder), typeof(bool));

        private static readonly MethodInfo _propertyIsSparseMethodInfo
            = typeof(XuguPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguPropertyBuilderExtensions.IsSparse), typeof(PropertyBuilder), typeof(bool));

        private static readonly MethodInfo _propertyUseIdentityColumnsMethodInfo
            = typeof(XuguPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguPropertyBuilderExtensions.UseIdentityColumn), typeof(PropertyBuilder), typeof(long), typeof(int));

        private static readonly MethodInfo _propertyUseHiLoMethodInfo
            = typeof(XuguPropertyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguPropertyBuilderExtensions.UseHiLo), typeof(PropertyBuilder), typeof(string), typeof(string));

        private static readonly MethodInfo _indexIsClusteredMethodInfo
            = typeof(XuguIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguIndexBuilderExtensions.IsClustered), typeof(IndexBuilder), typeof(bool));

        private static readonly MethodInfo _indexIncludePropertiesMethodInfo
            = typeof(XuguIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguIndexBuilderExtensions.IncludeProperties), typeof(IndexBuilder), typeof(string[]));

        private static readonly MethodInfo _indexHasFillFactorMethodInfo
            = typeof(XuguIndexBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguIndexBuilderExtensions.HasFillFactor), typeof(IndexBuilder), typeof(int));

        private static readonly MethodInfo _keyIsClusteredMethodInfo
            = typeof(XuguKeyBuilderExtensions).GetRequiredRuntimeMethod(
                nameof(XuguKeyBuilderExtensions.IsClustered), typeof(KeyBuilder), typeof(bool));



        #endregion MethodInfos

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public XuguAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IModel model,
            IDictionary<string, IAnnotation> annotations)
        {
            var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(model, annotations));

            if (GenerateValueGenerationStrategy(annotations, model, onModel: true) is MethodCallCodeFragment valueGenerationStrategy)
            {
                fragments.Add(valueGenerationStrategy);
            }

            GenerateSimpleFluentApiCall(
                annotations,
                XuguAnnotationNames.MaxDatabaseSize, _modelHasDatabaseMaxSizeMethodInfo,
                fragments);

            GenerateSimpleFluentApiCall(
                annotations,
                XuguAnnotationNames.ServiceTierSql, _modelHasServiceTierSqlMethodInfo,
                fragments);

            GenerateSimpleFluentApiCall(
                annotations,
                XuguAnnotationNames.PerformanceLevelSql, _modelHasPerformanceLevelSqlMethodInfo,
                fragments);

            return fragments;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
            IProperty property,
            IDictionary<string, IAnnotation> annotations)
        {
            var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(property, annotations));

            if (GenerateValueGenerationStrategy(annotations, property.DeclaringEntityType.Model, onModel: false) is MethodCallCodeFragment
                valueGenerationStrategy)
            {
                fragments.Add(valueGenerationStrategy);
            }

            if (GetAndRemove<bool?>(annotations, XuguAnnotationNames.Sparse) is bool isSparse)
            {
                fragments.Add(isSparse ? new(_propertyIsSparseMethodInfo) : new(_propertyIsSparseMethodInfo, false));
            }

            return fragments;
        }


        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == RelationalAnnotationNames.DefaultSchema)
            {
                return (string?)annotation.Value == "dbo";
            }

            return annotation.Name == XuguAnnotationNames.ValueGenerationStrategy
                && (XuguValueGenerationStrategy)annotation.Value! == XuguValueGenerationStrategy.IdentityColumn;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override MethodCallCodeFragment? GenerateFluentApi(IKey key, IAnnotation annotation)
            => annotation.Name == XuguAnnotationNames.Clustered
                ? (bool)annotation.Value! == false
                    ? new MethodCallCodeFragment(_keyIsClusteredMethodInfo, false)
                    : new MethodCallCodeFragment(_keyIsClusteredMethodInfo)
                : null;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override MethodCallCodeFragment? GenerateFluentApi(IIndex index, IAnnotation annotation)
            => annotation.Name switch
            {
                XuguAnnotationNames.Clustered => (bool)annotation.Value! == false
                    ? new MethodCallCodeFragment(_indexIsClusteredMethodInfo, false)
                    : new MethodCallCodeFragment(_indexIsClusteredMethodInfo),

                XuguAnnotationNames.Include => new MethodCallCodeFragment(_indexIncludePropertiesMethodInfo, annotation.Value),
                XuguAnnotationNames.FillFactor => new MethodCallCodeFragment(_indexHasFillFactorMethodInfo, annotation.Value),

                _ => null
            };

        private MethodCallCodeFragment? GenerateValueGenerationStrategy(
            IDictionary<string, IAnnotation> annotations,
            IModel model,
            bool onModel)
        {
            XuguValueGenerationStrategy strategy;
            if (annotations.TryGetValue(XuguAnnotationNames.ValueGenerationStrategy, out var strategyAnnotation)
                && strategyAnnotation.Value != null)
            {
                annotations.Remove(XuguAnnotationNames.ValueGenerationStrategy);
                strategy = (XuguValueGenerationStrategy)strategyAnnotation.Value;
            }
            else
            {
                return null;
            }

            switch (strategy)
            {
                case XuguValueGenerationStrategy.IdentityColumn:
                    // Support pre-6.0 IdentitySeed annotations, which contained an int rather than a long
                    if (annotations.TryGetValue(XuguAnnotationNames.IdentitySeed, out var seedAnnotation)
                        && seedAnnotation.Value != null)
                    {
                        annotations.Remove(XuguAnnotationNames.IdentitySeed);
                    }
                    else
                    {
                        seedAnnotation = model.FindAnnotation(XuguAnnotationNames.IdentitySeed);
                    }

                    var seed = seedAnnotation is null
                        ? 1
                        : seedAnnotation.Value is int intValue
                            ? intValue
                            : (long?)seedAnnotation.Value ?? 1;

                    var increment = GetAndRemove<int?>(annotations, XuguAnnotationNames.IdentityIncrement)
                        ?? model.FindAnnotation(XuguAnnotationNames.IdentityIncrement)?.Value as int?
                        ?? 1;
                    return new(
                        onModel ? _modelUseIdentityColumnsMethodInfo : _propertyUseIdentityColumnsMethodInfo,
                        seed,
                        increment);

                case XuguValueGenerationStrategy.SequenceHiLo:
                    var name = GetAndRemove<string>(annotations, XuguAnnotationNames.HiLoSequenceName);
                    var schema = GetAndRemove<string>(annotations, XuguAnnotationNames.HiLoSequenceSchema);
                    return new(
                        onModel ? _modelUseHiLoMethodInfo : _propertyUseHiLoMethodInfo,
                        (name, schema) switch
                        {
                            (null, null) => Array.Empty<object>(),
                            (_, null) => new object[] { name! },
                            _ => new object[] { name!, schema! }
                        });

                case XuguValueGenerationStrategy.None:
                    return new(
                        _modelHasAnnotationMethodInfo,
                        XuguAnnotationNames.ValueGenerationStrategy,
                        XuguValueGenerationStrategy.None);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static T? GetAndRemove<T>(IDictionary<string, IAnnotation> annotations, string annotationName)
        {
            if (annotations.TryGetValue(annotationName, out var annotation)
                && annotation.Value != null)
            {
                annotations.Remove(annotationName);
                return (T)annotation.Value;
            }

            return default;
        }

        private static void GenerateSimpleFluentApiCall(
            IDictionary<string, IAnnotation> annotations,
            string annotationName,
            MethodInfo methodInfo,
            List<MethodCallCodeFragment> methodCallCodeFragments)
        {
            if (annotations.TryGetValue(annotationName, out var annotation))
            {
                annotations.Remove(annotationName);
                if (annotation.Value is object annotationValue)
                {
                    methodCallCodeFragments.Add(
                        new MethodCallCodeFragment(methodInfo, annotationValue));
                }
            }
        }
    }
}
