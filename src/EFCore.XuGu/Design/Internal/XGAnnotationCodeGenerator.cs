// Copyright (c) XuGu Foundation. All rights reserved.
// Licensed under the MIT. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using EntityFrameworkCore.XuGu.Metadata.Internal;

namespace EntityFrameworkCore.XuGu.Design.Internal
{
    public class XGAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        public XGAnnotationCodeGenerator([NotNull] AnnotationCodeGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        public override IEnumerable<IAnnotation> FilterIgnoredAnnotations(IEnumerable<IAnnotation> annotations)
        {
            annotations = base.FilterIgnoredAnnotations(annotations).ToArray();

            var hasCharSetAnnotation = annotations.Any(a => a.Name == XGAnnotationNames.CharSet);
            var hasCollationAnnotation = annotations.Any(a => a.Name == RelationalAnnotationNames.Collation);

            foreach (var annotation in annotations)
            {
                // Charsets and their delegation and collations and their delegation are handled in the same Fluent API call.
                // Since the GenerateFluentApi methods cannot skip annotations, we have to ignore one of them here early, if both have been
                // set, so we don't output a HasCharSet()/UseCollation() call and a CharSetDelegation/CollationDelegation annotation in
                // addition to that.
                if (annotation.Name == XGAnnotationNames.CharSetDelegation && hasCharSetAnnotation ||
                    annotation.Name == XGAnnotationNames.CollationDelegation && hasCollationAnnotation)
                {
                    continue;
                }

                yield return annotation;
            }
        }

        protected override MethodCallCodeFragment GenerateFluentApi(IModel model, IAnnotation annotation)
        {
            if (annotation.Name == XGAnnotationNames.CharSet)
            {
                var delegationModes = model[XGAnnotationNames.CharSetDelegation] as DelegationModes?;
                return new MethodCallCodeFragment(
                    nameof(XGModelBuilderExtensions.HasCharSet),
                    new[] {annotation.Value}
                        .AppendIfTrue(delegationModes.HasValue, delegationModes)
                        .ToArray());
            }

            if (annotation.Name == XGAnnotationNames.CharSetDelegation &&
                model[XGAnnotationNames.CharSet] is null)
            {
                return new MethodCallCodeFragment(
                    nameof(XGModelBuilderExtensions.HasCharSet),
                    null,
                    annotation.Value);
            }

            // EF Core currently just falls back on using the `Relational:Collation` annotation instead of generating the `UseCollation()`
            // method call (though it could), so we can return our method call fragment here, without generating an ugly duplicate.
            if (annotation.Name == RelationalAnnotationNames.Collation)
            {
                var delegationModes = model[XGAnnotationNames.CollationDelegation] as DelegationModes?;
                return new MethodCallCodeFragment(
                    nameof(XGModelBuilderExtensions.UseCollation),
                    new[] {annotation.Value}
                        .AppendIfTrue(delegationModes.HasValue, delegationModes)
                        .ToArray());
            }

            if (annotation.Name == XGAnnotationNames.CollationDelegation &&
                model[RelationalAnnotationNames.Collation] is null)
            {
                return new MethodCallCodeFragment(
                    nameof(XGModelBuilderExtensions.UseCollation),
                    null,
                    annotation.Value);
            }

            if (annotation.Name == XGAnnotationNames.GuidCollation)
            {
                return new MethodCallCodeFragment(
                    nameof(XGModelBuilderExtensions.UseGuidCollation),
                    annotation.Value);
            }

            return null;
        }

        protected override MethodCallCodeFragment GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        {
            if (annotation.Name == XGAnnotationNames.CharSet)
            {
                var delegationModes = entityType[XGAnnotationNames.CharSetDelegation] as DelegationModes?;
                return new MethodCallCodeFragment(
                    nameof(XGEntityTypeBuilderExtensions.HasCharSet),
                    new[] {annotation.Value}
                        .AppendIfTrue(delegationModes.HasValue, delegationModes)
                        .ToArray());
            }

            if (annotation.Name == XGAnnotationNames.CharSetDelegation &&
                entityType[XGAnnotationNames.CharSet] is null)
            {
                return new MethodCallCodeFragment(
                    nameof(XGEntityTypeBuilderExtensions.HasCharSet),
                    null,
                    annotation.Value);
            }

            if (annotation.Name == RelationalAnnotationNames.Collation)
            {
                var delegationModes = entityType[XGAnnotationNames.CollationDelegation] as DelegationModes?;
                return new MethodCallCodeFragment(
                    nameof(XGEntityTypeBuilderExtensions.UseCollation),
                    new[] {annotation.Value}
                        .AppendIfTrue(delegationModes.HasValue, delegationModes)
                        .ToArray());
            }

            if (annotation.Name == XGAnnotationNames.CollationDelegation &&
                entityType[RelationalAnnotationNames.Collation] is null)
            {
                return new MethodCallCodeFragment(
                    nameof(XGEntityTypeBuilderExtensions.UseCollation),
                    null,
                    annotation.Value);
            }

            return null;
        }

        protected override MethodCallCodeFragment GenerateFluentApi(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            // At this point, all legacy `XG:Collation` annotations should have been replaced by `Relational:Collation` ones.
#pragma warning disable 618
            Debug.Assert(annotation.Name != XGAnnotationNames.Collation);
#pragma warning restore 618

            switch (annotation.Name)
            {
                case XGAnnotationNames.CharSet when annotation.Value is string {Length: > 0} charSet:
                    return new MethodCallCodeFragment(nameof(XGPropertyBuilderExtensions.HasCharSet), charSet);

                default:
                    return null;
            }
        }
    }
}
